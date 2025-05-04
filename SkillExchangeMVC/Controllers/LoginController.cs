//LoginController.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using SkillExchangeMVC.Models.ViewModels;
using System.Security.Claims;

namespace SkillExchangeMVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;
        public LoginController(SkillExchangeContext skillExchangeContext)
        {
            _skillExchangeContext = skillExchangeContext;
        }

        public IActionResult CreateLogin()
        {
            var viewModel = new LoginViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLogin(LoginViewModel viewModel) // Marked as async
        {
            if (ModelState.IsValid)
            {
                var user = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == viewModel.Email);
                if (user != null)
                {
                    var hasher = new PasswordHasher<UserInfo>();
                    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, viewModel.Password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Name ?? "User"),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, user.Role ?? "Student") // Role must be present in DB
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        // Sign in the user
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(viewModel);
        }
    }
}