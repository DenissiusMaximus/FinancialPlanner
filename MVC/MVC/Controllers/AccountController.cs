using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using API;
using API.Models;
using API.Utils;
using MVC.Models.ViewModels;

namespace MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _appDbContext;
        private readonly API.Utils.IPasswordHasher _passwordHasher;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> roleManager,
                                 AppDbContext appDbContext,
                                 API.Utils.IPasswordHasher passwordHasher)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _appDbContext = appDbContext;
            _passwordHasher = passwordHasher;
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Створення ролі, якщо вона не існує
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    }

                    // Призначення обраної ролі користувачу
                    await _userManager.AddToRoleAsync(user, model.Role);

                    var appUserId = await EnsureDomainUserAsync(model.Email, model.Password);

                    // Автоматичний вхід після реєстрації
                    await _signInManager.SignInWithClaimsAsync(user, isPersistent: false,
                        [new Claim("app_user_id", appUserId.ToString())]);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "/")
        {
            return View(new LoginModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await _signInManager.SignOutAsync();
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        var appUserId = await EnsureDomainUserAsync(model.Email, model.Password);

                        // Re-issue cookie with domain user id claim required by domain services.
                        await _signInManager.SignOutAsync();
                        await _signInManager.SignInWithClaimsAsync(user, isPersistent: false,
                            [new Claim("app_user_id", appUserId.ToString())]);

                        return Redirect(model.ReturnUrl ?? "/");
                    }
                }
                ModelState.AddModelError("", "Невірний email або пароль");
            }
            return View(model);
        }

        [Authorize(AuthenticationSchemes = "Identity.Application")]
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }

        [Authorize(AuthenticationSchemes = "Identity.Application")]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new ProfileModel
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Identity.Application")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Профіль успішно оновлено";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            model.Roles = await _userManager.GetRolesAsync(user);
            return View(model);
        }

        private async Task<int> EnsureDomainUserAsync(string email, string password)
        {
            var existingUserId = await _appDbContext.Users
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();

            if (existingUserId.HasValue)
            {
                return existingUserId.Value;
            }

            var domainUser = new User
            {
                Name = email,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password)
            };

            _appDbContext.Users.Add(domainUser);
            await _appDbContext.SaveChangesAsync();

            return domainUser.Id;
        }
    }
}
