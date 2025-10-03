// UploadedContentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models.Context;
using System.Security.Claims;

namespace SkillExchangeMVC.Controllers
{
    public class UploadedContentsController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;

        public UploadedContentsController(SkillExchangeContext skillExchangeContext)
        {
            _skillExchangeContext = skillExchangeContext;
        }

        // Check if user has access to content based on course enrollment
        private bool HasAccessToCourse(int courseId)
        {
            // Admin always has access
            if (User.IsInRole("Admin"))
                return true;

            var email = User.FindFirstValue(ClaimTypes.Email);
            var userId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
            
            if (userId == null) return false;

            // Check if user is enrolled in the course
            var isEnrolled = _skillExchangeContext.Enrollments.Any(e => e.CourseId == courseId && e.UserInfoId == userId);
            
            // Check if course is premium - if premium, user must be enrolled
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            if (course != null && course.IsPremium && !isEnrolled)
                return false;

            return true;
        }

        // C# Methods Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult CSharpMethods(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View("C#Methods");
        }

        // C# OOP Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult CSharpOOP(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View("C#OOP");
        }

        // Java Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult Java(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View();
        }

        // C++ Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult CPlusPlus(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View("C++");
        }

        // Graphics Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult Graphics(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View();
        }

        // Video Editing Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult VideoEditing(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View();
        }

        // Photography Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult Photography(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View();
        }

        // Marketing Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult Marketing(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View();
        }

        // Entrepreneurship Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult Entrepreneurship(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View();
        }

        // Leadership Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult Leadership(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View();
        }

        // Public Speaking Content
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult PublicSpeaking(int courseId)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;
            return View();
        }

        // Method to get content pages by course category/subject
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult GetContentByCategory(int courseId, string category)
        {
            if (!HasAccessToCourse(courseId))
            {
                TempData["Error"] = "You must be enrolled to view this course content.";
                return RedirectToAction("IndexCourse", "Course");
            }

            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.Course = course;
            ViewBag.CourseId = courseId;

            // Route to appropriate content based on category/subject
            return category?.ToLower() switch
            {
                "c# methods" or "csharp methods" => View("C#Methods"),
                "c# oop" or "csharp oop" => View("C#OOP"),
                "java" => View("Java"),
                "c++" or "cpp" => View("C++"),
                "graphics" or "graphics design" => View("Graphics"),
                "video editing" => View("VideoEditing"),
                "photography" => View("Photography"),
                "marketing" => View("Marketing"),
                "entrepreneurship" => View("Entrepreneurship"),
                "leadership" => View("Leadership"),
                "public speaking" => View("PublicSpeaking"),
                _ => RedirectToAction("CourseContents", "Content", new { courseId })
            };
        }
    }
}