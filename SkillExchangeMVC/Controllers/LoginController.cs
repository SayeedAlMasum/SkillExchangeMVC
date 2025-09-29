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
        public async Task<IActionResult> CreateLogin(LoginViewModel viewModel)
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
                            new Claim(ClaimTypes.Role, user.Role ?? "Student")
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(viewModel);
        }

        // Account-related actions
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("Email", "Email is required.");
                return View();
            }

            var user = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "No user found with that email.");
                return View();
            }

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpires = DateTime.Now.AddHours(1);
            _skillExchangeContext.SaveChanges();

            return RedirectToAction("ForgotPasswordConfirmation", new { email = email, token = user.PasswordResetToken });
        }

        public IActionResult ForgotPasswordConfirmation(string email, string token)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        public IActionResult ResetPassword(string email, string token)
        {
            var user = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email && u.PasswordResetToken == token && u.ResetTokenExpires > DateTime.Now);
            if (user == null)
            {
                return RedirectToAction("CreateLogin");
            }

            var model = new ResetPasswordViewModel { Email = email, Token = token };
            return View(model);
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == model.Email && u.PasswordResetToken == model.Token && u.ResetTokenExpires > DateTime.Now);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid or expired reset token.");
                return View(model);
            }

            var hasher = new PasswordHasher<UserInfo>();
            user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            _skillExchangeContext.SaveChanges();

            return RedirectToAction("ResetPasswordConfirmation");
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}