using EurovisionHub.Models;
using EurovisionHub.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EurovisionHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly EurovisionContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(EurovisionContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

                    if (model.WantsToBeAdmin && !string.IsNullOrEmpty(model.Motivation))
                    {
                        var roleRequest = new RoleRequest
                        {
                            UserId = user.Id,
                            RequestedRole = "Admin",
                            Motivation = model.Motivation,
                            Status = RequestStatus.Pending
                        };

                        _context.RoleRequests.Add(roleRequest);
                        await _context.SaveChangesAsync();
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Невірна спроба входу. Перевірте пошту та пароль.");
            }
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ApplyForAdmin()
        {
            var userId = _userManager.GetUserId(User);

            var existingRequest = await _context.RoleRequests
                .AnyAsync(r => r.UserId == userId && r.Status == RequestStatus.Pending);

            if (existingRequest)
            {
                TempData["error"] = "Ваша заявка вже розглядається СуперАдміном.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyForAdmin(string motivation)
        {
            if (string.IsNullOrWhiteSpace(motivation))
            {
                ModelState.AddModelError("motivation", "Будь ласка, вкажіть мотивацію.");
                return View();
            }

            var userId = _userManager.GetUserId(User);

            var roleRequest = new RoleRequest
            {
                UserId = userId,
                RequestedRole = "Admin",
                Motivation = motivation,
                CreatedAt = DateTime.UtcNow,
                Status = RequestStatus.Pending
            };

            _context.RoleRequests.Add(roleRequest);
            await _context.SaveChangesAsync();

            TempData["success"] = "Заявку успішно надіслано!";
            return RedirectToAction("Index", "Home");
        }
    }
}