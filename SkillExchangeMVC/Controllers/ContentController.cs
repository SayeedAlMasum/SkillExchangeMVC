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

        public IActionResult IndexContent()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var contents = _skillExchangeContext.Content.Where(c => c.UploaderEmail == email).OrderByDescending(c => c.CreatedDate).ToList(); 
            return View(contents);
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
                var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "unknown@domain.com";

                content.UploaderEmail = userEmail;
                content.CreatedBy = userName;
                content.UpdatedBy = userName;
                content.CreatedDate = DateTime.Now;
                content.UpdatedDate = DateTime.Now;

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
        [Authorize(Roles = "Teacher")]
        public IActionResult EditContent(int id)
        {
            var content = _skillExchangeContext.Content.FirstOrDefault(c => c.ContentId == id);
            if (content == null) return NotFound();

            var email = User.FindFirstValue(ClaimTypes.Email);
            var userId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;

            ViewBag.Courses = new SelectList(
                _skillExchangeContext.Course.Where(c => c.TeacherId == userId),
                "CourseId", "Title");

            return View(content);
        }
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public IActionResult EditContent(Content updated)
        {
            if (!ModelState.IsValid) return View(updated);

            var existing = _skillExchangeContext.Content.FirstOrDefault(c => c.ContentId == updated.ContentId);
            if (existing == null) return NotFound();

            var name = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            existing.Title = updated.Title;
            existing.Description = updated.Description;
            existing.Type = updated.Type;
            existing.URL = updated.URL;
            existing.CourseId = updated.CourseId;
            existing.UpdatedBy = name;
            existing.UpdatedDate = DateTime.Now;

            _skillExchangeContext.SaveChanges();
            TempData["Success"] = "Content updated successfully.";

            return RedirectToAction("IndexContent");
        }
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public IActionResult DeleteContent(int id)
        {
            var content = _skillExchangeContext.Content.FirstOrDefault(c => c.ContentId == id);
            if (content == null)
                return Json(new { success = false, message = "Content not found" });

            _skillExchangeContext.Content.Remove(content);
            _skillExchangeContext.SaveChanges();

            return Json(new { success = true, message = "Content deleted successfully" });
        }



    }
}
