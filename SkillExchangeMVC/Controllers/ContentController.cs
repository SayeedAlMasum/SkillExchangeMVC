// ContentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SkillExchangeMVC.Models.Context;
using SkillExchangeMVC.Models;
using System.Security.Claims;

namespace SkillExchangeMVC.Controllers
{
    public class ContentController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;

        public ContentController(SkillExchangeContext skillExchangeContext)
        {
            _skillExchangeContext = skillExchangeContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult UploadContent()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var userId = _skillExchangeContext.UserInfo
                           .FirstOrDefault(u => u.Email == email)?.UserInfoId;

            var courses = _skillExchangeContext.Course
                           .Where(c => c.TeacherId == userId)
                           .ToList();

            ViewBag.Courses = new SelectList(courses, "CourseId", "Title");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public IActionResult UploadContent(Content content)
        {
            if (ModelState.IsValid)
            {
                _skillExchangeContext.Content.Add(content);
                _skillExchangeContext.SaveChanges();
                TempData["Success"] = "Content uploaded successfully.";
                return RedirectToAction("UploadContent");
            }

            var email = User.FindFirstValue(ClaimTypes.Email);
            var userId = _skillExchangeContext.UserInfo
                           .FirstOrDefault(u => u.Email == email)?.UserInfoId;

            var courses = _skillExchangeContext.Course
               .Where(c => c.TeacherId == userId)
               .ToList();

            ViewBag.Courses = new SelectList(courses, "CourseId", "Title");
            return View(content);
        }
    }
}
