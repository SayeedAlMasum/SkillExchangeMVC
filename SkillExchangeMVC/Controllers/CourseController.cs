//CourseController.cs
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.ViewModels;

namespace SkillExchangeMVC.Controllers
{
    public class CourseController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;
        public CourseController(SkillExchangeContext skillExchangeContext) {
            _skillExchangeContext = skillExchangeContext;
        }

        public IActionResult Index()
        {
            var viewModel = new CourseViewModel
            {
                Courses = _skillExchangeContext.Course.ToList()
            };
            return View();
        }

        public IActionResult CreateCourse()
        {
            var viewModel = new CourseViewModel
            {
                Courses =_skillExchangeContext.Course.ToList(),
                Course= new Course() // Optional, since  ViewModel sets default
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
            // Reload course list on validation failure
            viewModel.Courses = _skillExchangeContext.Course.ToList();
            return View(viewModel);
        }
    }
}
