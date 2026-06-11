using FlightBooking.Models.Domain;
using FlightBooking.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlightBooking.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment)

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // ── LOGIN ──────────────────────────────────────

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Safe redirect — never redirect to external URLs
                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("",
                    "Account locked. Try again in 10 minutes.");
                return View(model);
            }

            ModelState.AddModelError("",
                "Invalid email or password.");
            return View(model);
        }

        // ── REGISTER ───────────────────────────────────

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(
                user, model.Password);

            if (result.Succeeded)
            {
                // Auto-assign User role on registration
                await _userManager.AddToRoleAsync(user, "User");

                // Sign in immediately after register
                await _signInManager.SignInAsync(user,
                    isPersistent: false);

                return RedirectToAction("Index", "Home");
            }

            // Surface Identity errors (e.g. duplicate email)
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // ── LOGOUT / ACCESS DENIED ─────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // ── PROFILE ────────────────────────────────────

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                CurrentPhotoUrl = user.ProfilePhotoUrl
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                model.Email = user.Email;
                model.CurrentPhotoUrl = user.ProfilePhotoUrl;
                return View(model);
            }

            // Handle photo upload
            if (model.ProfilePhoto != null &&
                model.ProfilePhoto.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(
                    model.ProfilePhoto.FileName).ToLower();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("ProfilePhoto",
                        "Only .jpg and .png files are allowed.");
                    model.Email = user.Email;
                    model.CurrentPhotoUrl = user.ProfilePhotoUrl;
                    return View(model);
                }

                // Save to wwwroot/uploads/profiles/
                var uploadDir = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "uploads", "profiles");
                Directory.CreateDirectory(uploadDir);

                var fileName = $"{user.Id}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);

                using var stream = new FileStream(
                    filePath, FileMode.Create);
                await model.ProfilePhoto.CopyToAsync(stream);

                user.ProfilePhotoUrl =
                    $"/uploads/profiles/{fileName}";
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        // ── CHANGE PASSWORD ────────────────────────────

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (result.Succeeded)
            {
                // Re-sign in so the cookie stays valid
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] =
                    "Password changed successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);

        }
    }
}

