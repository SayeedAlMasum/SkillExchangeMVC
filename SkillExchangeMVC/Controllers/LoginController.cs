//LoginController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.ViewModels;

namespace SkillExchangeMVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;
        public LoginController(SkillExchangeContext skillExchangeContext)
        {
            _skillExchangeContext = skillExchangeContext;
        }

        public IActionResult Index()
        {
            // Initialize an empty login view model
            var viewModel = new LoginViewModel();
            return View(viewModel);
        }

        public IActionResult CreateLogin()
        {
            var viewModel = new LoginViewModel();
            return View(viewModel);

        }
        [HttpPost]
        public IActionResult CreateLogin(LoginViewModel viewModel)
        {
            // Step 1: Check if submitted form is valid
            if (ModelState.IsValid)
            {
                // Step 2: Search for a user with the submitted email
                var user = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == viewModel.Email);

                // Step 3: If user found, verify password
                if (user != null)
                {
                    var hasher = new PasswordHasher<UserInfo>();
                    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, viewModel.Password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        // TODO:Set user session or Add authentication logic here

                        // Step 5: Redirect user based on role
                        if (user.Role == "Admin")
                            return RedirectToAction("CreateCourse", "Course");

                        else if (user.Role == "Teacher")
                            return RedirectToPage("/Index");
                        else
                            return RedirectToAction("Index", "Course");
                    }
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(viewModel);
        }
    }
}