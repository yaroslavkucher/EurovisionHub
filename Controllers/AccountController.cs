using EurovisionHub.Models;
using EurovisionHub.Models.ViewModels;
using EurovisionHub.Services;
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
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public AccountController(EurovisionContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _config = config;
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
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    RegistrationDate = DateTime.UtcNow
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

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

                ModelState.AddModelError(string.Empty, "Invalid login attempt. Check your email and password.");
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

        [Authorize(Roles = "User, Admin")]
        [HttpGet]
        public async Task<IActionResult> ApplyForAdmin()
        {
            var userId = _userManager.GetUserId(User);

            var existingRequest = await _context.RoleRequests
                .AnyAsync(r => r.UserId == userId && r.Status == RequestStatus.Pending);

            if (existingRequest)
            {
                TempData["error"] = "Your request is already being reviewed by the SuperAdmin.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var latestRequest = await _context.RoleRequests
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestRequest != null && latestRequest.Status == RequestStatus.Pending)
            {
                if (roles.Contains(latestRequest.RequestedRole))
                {
                    latestRequest = null;
                }
            }

            var model = new UserProfileViewModel
            {
                Email = user.Email,
                Username = user.UserName,
                RegistrationDate = user.RegistrationDate,
                LatestRoleRequest = latestRequest,
                Roles = roles,
                ShowRoleChangeNotification = user.ShowRoleChangeNotification,
                RoleChangeComment = user.RoleChangeComment
            };

            if (user.ShowRoleChangeNotification)
            {
                user.ShowRoleChangeNotification = false;
                await _userManager.UpdateAsync(user);
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyForAdmin(string motivation)
        {
            if (string.IsNullOrWhiteSpace(motivation))
            {
                ModelState.AddModelError("motivation", "Please provide a motivation.");
                return View();
            }

            var userId = _userManager.GetUserId(User);

            var roleRequest = new RoleRequest
            {
                UserId = userId,
                RequestedRole = User.IsInRole("User") ? "Admin" : "SuperAdmin",
                Motivation = motivation,
                CreatedAt = DateTime.UtcNow,
                Status = RequestStatus.Pending
            };

            _context.RoleRequests.Add(roleRequest);
            await _context.SaveChangesAsync();

            TempData["success"] = "Your request has been successfully submitted!";
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError("OldPassword", error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["success"] = "Your password has been changed successfully.";

            return RedirectToAction("Profile");
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", "User with this email was not found.");
                    ViewBag.SuggestRegistration = true;
                    return View(model);
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                var callbackUrl = Url.Action("ResetPassword", "Account",
                        new { code = code, email = model.Email },
                        protocol: HttpContext.Request.Scheme);

                var emailSubject = "Reset Your EurovisionHub Password";
                var emailBody = $@"
                    <h3>Password Reset Request</h3>
                    <p>Hello,</p>
                    <p>We received a request to reset your password for EurovisionHub. Click the button below to set a new password(The link is valid for 15 minutes):</p>
                    <a href='{callbackUrl}' style='display:inline-block; padding:10px 20px; background-color:#0d6efd; color:#ffffff; text-decoration:none; border-radius:5px;'>Reset Password</a>
                    <p><br>If you didn't request this, you can safely ignore this email.</p>
                ";

                await _emailService.SendEmailAsync(model.Email, emailSubject, emailBody);

                TempData["success"] = "A password reset link has been sent to your email address.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                return BadRequest("A code and email must be supplied for password reset.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return View("InvalidToken");
            }

            var isTokenValid = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                code);

            if (!isTokenValid)
            {
                return View("InvalidToken");
            }

            return View(new ResetPasswordViewModel { Code = code, Email = email });
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["success"] = "Your password has been reset successfully.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                TempData["success"] = "Your password has been reset successfully. You can now log in.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
    }
}