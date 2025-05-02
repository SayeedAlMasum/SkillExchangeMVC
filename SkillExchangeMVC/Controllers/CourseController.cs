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
            // Fetch all courses from the database and pass to view
            var viewModel = new CourseViewModel
            {
                Courses = _skillExchangeContext.Course.ToList()
            };
            return View(viewModel);
        }

        public IActionResult CreateCourse()
        {
            // Initialize an empty course + list of existing courses
            var viewModel = new CourseViewModel
            {
                Courses =_skillExchangeContext.Course.ToList(),
                Course= new Course() //used to bind form input
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult CreateCourse(CourseViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Add new course to the database
                _skillExchangeContext.Course.Add(viewModel.Course);
                _skillExchangeContext.SaveChanges();

                // Redirect to clear the form (Post-Redirect-Get)
                return RedirectToAction("CreateCourse");
            }
            // Reload existing courses if form fails validation
            viewModel.Courses = _skillExchangeContext.Course.ToList();
            return View(viewModel);
        }
    }
}
