using EurovisionHub.Models;
using EurovisionHub.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "SuperAdmin")]
public class AdminPanelController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly EurovisionContext _context;

    public AdminPanelController(UserManager<ApplicationUser> userManager,
                                RoleManager<IdentityRole> roleManager,
                                EurovisionContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IActionResult> Requests()
    {
        var requests = await _context.RoleRequests
            .Include(r => r.User)
            .Where(r => r.Status == RequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        ViewBag.PendingRequestsCount = _context.RoleRequests.Count(r => r.Status == RequestStatus.Pending);

        return View(requests);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int requestId)
    {
        var request = await _context.RoleRequests.FindAsync(requestId);
        if (request == null) return NotFound();

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user != null)
        {
            await _userManager.RemoveFromRoleAsync(user, "User");
            await _userManager.AddToRoleAsync(user, request.RequestedRole);

            request.Status = RequestStatus.Approved;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int requestId, string adminComment)
    {
        var request = await _context.RoleRequests.FindAsync(requestId);
        if (request == null) return NotFound();

        request.Status = RequestStatus.Rejected;
        request.AdminComment = adminComment;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Requests));
    }

    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();

        var model = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserListViewModel
            {
                Id = user.Id,
                Email = user.Email,
                CurrentRole = roles.FirstOrDefault() ?? "No Role"
            });
        }

        ViewBag.PendingRequestsCount = _context.RoleRequests.Count(r => r.Status == RequestStatus.Pending);

        return View(model);
    }

    public async Task<IActionResult> EditRole(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

        var model = new ChangeRoleViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email,
            SelectedRole = roles.FirstOrDefault(),
            AllRoles = allRoles
        };

        ViewBag.PendingRequestsCount = _context.RoleRequests.Count(r => r.Status == RequestStatus.Pending);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditRole(ChangeRoleViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);

        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, model.SelectedRole);

        user.RoleChangeComment = model.Comment;
        user.ShowRoleChangeNotification = true;

        await _userManager.UpdateAsync(user);

        TempData["success"] = "Role successfully changed";
        return RedirectToAction(nameof(Users));
    }
}