//CourseController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.ViewModels;
using System.Linq;

namespace SkillExchangeMVC.Controllers
{
    [Authorize(Roles = "Admin")]
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
    }
}
