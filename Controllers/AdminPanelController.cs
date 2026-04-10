using EurovisionHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "SuperAdmin")]
public class AdminPanelController : Controller
{
    private readonly EurovisionContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminPanelController(EurovisionContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Requests()
    {
        var requests = await _context.RoleRequests
            .Include(r => r.User)
            .Where(r => r.Status == RequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
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
}