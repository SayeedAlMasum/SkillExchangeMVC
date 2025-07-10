// CourseController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using SkillExchangeMVC.Models.ViewModels;
using System.Linq;
using System.Security.Claims;

namespace SkillExchangeMVC.Controllers
{
    public class CourseController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;

        public CourseController(SkillExchangeContext skillExchangeContext)
        {
            _skillExchangeContext = skillExchangeContext;
        }

        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult IndexCourse()
        {
            var viewModel = new CourseViewModel
            {
                Courses = _skillExchangeContext.Course
                            .OrderBy(c => c.CourseId)
                            .ToList()
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateCourse()
        {
            var teachers = _skillExchangeContext.UserInfo
                            .Where(u => u.Role == "Teacher")
                            .Select(t => new SelectListItem
                            {
                                Value = t.UserInfoId,
                                Text = t.Name
                            }).ToList();

            var viewModel = new CourseViewModel
            {
                Courses = _skillExchangeContext.Course.OrderBy(c => c.CourseId).ToList(),
                Teachers = teachers,
                Course = new Course()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateCourse(CourseViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var name = User.FindFirstValue(ClaimTypes.Name);
                viewModel.Course.CreatedBy = name ?? "Unknown";
                viewModel.Course.UpdatedBy = name ?? "Unknown";
                viewModel.Course.CreatedDate = DateTime.Now;
                viewModel.Course.UpdatedDate = DateTime.Now;

                _skillExchangeContext.Course.Add(viewModel.Course);
                _skillExchangeContext.SaveChanges();

                return RedirectToAction("CreateCourse");
            }

            viewModel.Courses = _skillExchangeContext.Course.OrderBy(c => c.CourseId).ToList();
            viewModel.Teachers = _skillExchangeContext.UserInfo
                .Where(u => u.Role == "Teacher")
                .Select(t => new SelectListItem
                {
                    Value = t.UserInfoId,
                    Text = t.Name
                }).ToList();

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult EditCourse(int id)
        {
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult EditCourse(Course updatedCourse)
        {
            if (ModelState.IsValid)
            {
                var existing = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == updatedCourse.CourseId);
                if (existing == null) return NotFound();

                existing.Title = updatedCourse.Title;
                existing.Description = updatedCourse.Description;
                existing.Category = updatedCourse.Category;
                existing.SubCategory = updatedCourse.SubCategory;
                existing.IsPremium = updatedCourse.IsPremium;

                var name = User.FindFirstValue(ClaimTypes.Name);
                existing.UpdatedBy = name ?? "Unknown";
                existing.UpdatedDate = DateTime.Now;

                _skillExchangeContext.SaveChanges();
                return RedirectToAction("CreateCourse");
            }

            return View(updatedCourse);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }
            _skillExchangeContext.Course.Remove(course);
            _skillExchangeContext.SaveChanges();

            return RedirectToAction("CreateCourse");
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult TeacherCourses()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var teacherId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;

            var courses = _skillExchangeContext.Course
                            .Where(c => c.TeacherId == teacherId)
                            .ToList();

            return View(courses);
        }
    }
}