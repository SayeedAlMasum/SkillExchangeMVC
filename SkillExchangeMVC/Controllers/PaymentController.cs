//PaymentController.cs
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using SkillExchangeMVC.Models.ViewModels;
using System.Linq;

namespace SkillExchangeMVC.Controllers
{
    public class PaymentController : Controller // Inherit from Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;

        public PaymentController(SkillExchangeContext skillExchangeContext)
        {
            _skillExchangeContext = skillExchangeContext;
        }

        // Displays the payment form for a given course
  
        public IActionResult CreatePayment(int courseId)
        {
            // Retrieve the course from the database by ID
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return NotFound();// Return 404 if course doesn't exist
            }

            // Preparing the view model to show course title on the form
            var viewModel = new PaymentViewModel
            {
                Course = course
            };

            return View("CreatePayment", viewModel);// Show payment form
        }

        // Process after submitting the payment form
        [HttpPost]
        public IActionResult CreatePayment(PaymentViewModel viewModel)
        {
            // If model state is invalid (e.g., missing or invalid input), redisplay the form with errors
            if (!ModelState.IsValid)
            {
                if (viewModel.Course?.CourseId != null)
                {
                    viewModel.Course = _skillExchangeContext.Course
                        .FirstOrDefault(c => c.CourseId == viewModel.Course.CourseId);
                }
                return View("CreatePayment", viewModel);
            }

            // Simulate successful payment (no real payment gateway here)
            TempData["PaymentSuccess"] = "Payment successful for the course: " + viewModel.Course?.Title;

            // Redirect back to course list page
            return RedirectToAction("IndexCourse", "Course");
        }
    }
}
