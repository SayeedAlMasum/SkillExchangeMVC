//CourseController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using SkillExchangeMVC.Models.ViewModels;
using System.Linq;

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
        [Authorize(Roles ="Admin")]
        public IActionResult CreateCourse()
        {
            var viewModel = new CourseViewModel
            {
                Courses = _skillExchangeContext.Course
                            .OrderBy(c => c.CourseId)
                            .ToList(),
                Course = new Course()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult CreateCourse(CourseViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                _skillExchangeContext.Course.Add(viewModel.Course);
                _skillExchangeContext.SaveChanges();
                return RedirectToAction("CreateCourse");
            }

            viewModel.Courses = _skillExchangeContext.Course
                                  .OrderBy(c => c.CourseId)
                                  .ToList();
            return View(viewModel);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult EditCourse(int id)
        {
            // Fetch the course from the database using the course id
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == id);

            // If the course does not exist, return a 404 error page
            if (course == null)
            {
                return NotFound();
            }

            // Return the course to the view for editing
            return View(course);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult EditCourse(Course updatedCourse)
        {
            if (ModelState.IsValid)
            {
                // Get the existing course by ID
                var existing = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == updatedCourse.CourseId);
                if (existing == null) return NotFound();

                // Update values
                existing.Title = updatedCourse.Title;
                existing.Description = updatedCourse.Description;
                existing.Category = updatedCourse.Category;
                existing.SubCategory = updatedCourse.SubCategory;
                existing.IsPremium = updatedCourse.IsPremium;

                _skillExchangeContext.SaveChanges();

                return RedirectToAction("CreateCourse");
            }

            return View(updatedCourse);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            // Find course by ID
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }
            // Remove course
            _skillExchangeContext.Course.Remove(course);
            _skillExchangeContext.SaveChanges();

            return RedirectToAction("CreateCourse");
        }

    }
}
