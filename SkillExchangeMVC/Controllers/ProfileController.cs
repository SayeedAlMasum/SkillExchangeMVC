//ProfileController.cs
// ProfileController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using System.Security.Claims;

namespace SkillExchangeMVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(SkillExchangeContext context, IWebHostEnvironment webHostEnvironment)
        {
            _skillExchangeContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult IndexProfile()
        {
            var username = User.Identity.Name;
            var user = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Name == username);

            if (user != null && !string.IsNullOrEmpty(user.ProfileImagePath))
            {
                HttpContext.Session.SetString("ProfileImage", "/images/" + user.ProfileImagePath);
            }
            else
            {
                HttpContext.Session.SetString("ProfileImage", "/images/default-user.png");
            }

            return View(user);
        }


        [HttpGet]
        public IActionResult EditProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult EditProfile(UserInfo updatedUser, IFormFile? ProfileImage)
        {
            var existingUser = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.UserInfoId == updatedUser.UserInfoId);
            if (existingUser == null) return NotFound();

            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.Location = updatedUser.Location;

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/profile");
                Directory.CreateDirectory(uploadDir); // Ensure folder exists
                string filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }

                existingUser.ProfileImagePath = $"/images/profile/{fileName}";
                HttpContext.Session.SetString("ProfileImage", existingUser.ProfileImagePath); 
            }

            _skillExchangeContext.SaveChanges();

            TempData["SweetAlert"] = "success|Profile updated successfully!";
            return RedirectToAction("IndexProfile");
        }
    }
}
