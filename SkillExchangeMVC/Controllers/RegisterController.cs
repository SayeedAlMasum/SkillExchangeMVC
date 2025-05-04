//RegisterController
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using SkillExchangeMVC.Models.ViewModels;

namespace SkillExchangeMVC.Controllers
{
    public class RegisterController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;
        public RegisterController(SkillExchangeContext skillExchangeContext)
        {
            _skillExchangeContext = skillExchangeContext;
        }

        public IActionResult CreateRegister()
        {
            var viewModel = new RegisterViewModel();
            return View(viewModel);
        }
        [HttpPost]
        public IActionResult CreateRegister(RegisterViewModel viewModel)
        {

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Check if email already exists
            bool emailExists = _skillExchangeContext.UserInfo.Any(u => u.Email == viewModel.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(viewModel);
            }
            if (viewModel.Role != "Student" && viewModel.Role != "Teacher" && viewModel.Role != "Admin")
            {
                ModelState.AddModelError("", "Invalid role selected.");
                return View(viewModel);
            }


            // Map ViewModel to User entity
            var user = new UserInfo
            {
                Name = viewModel.Name,
                Email = viewModel.Email,
                PasswordHash = new PasswordHasher<UserInfo>().HashPassword(null, viewModel.Password),
                Role = viewModel.Role
            };

            // Save to database
            _skillExchangeContext.UserInfo.Add(user);
            _skillExchangeContext.SaveChanges();


            // Redirect to login or home page
            return RedirectToAction("CreateLogin", "Login");
        }

    }
}
