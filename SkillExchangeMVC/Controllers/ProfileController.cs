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
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email);
            if (user == null) return RedirectToAction("CreateLogin", "Login");

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

                existingUser.ProfileImagePath = "/images/profile/" + fileName;
            }

            _skillExchangeContext.SaveChanges();
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("IndexProfile");
        }
    }
}
