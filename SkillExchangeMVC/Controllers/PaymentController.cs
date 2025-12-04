//PaymentController.cs
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using SkillExchangeMVC.Models.ViewModels;
using System.Linq;
using System.Security.Claims;

namespace SkillExchangeMVC.Controllers
{
    public class PaymentController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;
        private readonly ILogger<PaymentController> _logger;

   public PaymentController(SkillExchangeContext skillExchangeContext, ILogger<PaymentController> logger)
        {
            _skillExchangeContext = skillExchangeContext;
       _logger = logger;
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

            // Check if course is free
            if (course.Price == null || course.Price == 0)
      {
          TempData["Info"] = "This is a free course. No payment required.";
      return RedirectToAction("IndexCourse", "Course");
            }

       // Check if user is already enrolled
    var email = User.FindFirstValue(ClaimTypes.Email);
 var userId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
       
 if (userId != null)
            {
       var existingEnrollment = _skillExchangeContext.Enrollments
    .Any(e => e.CourseId == courseId && e.UserInfoId == userId);
   
             if (existingEnrollment)
          {
         TempData["Info"] = "You are already enrolled in this course.";
    return RedirectToAction("CourseContents", "Content", new { courseId });
       }
         }

    // Preparing the view model to show course title on the form
            var viewModel = new PaymentViewModel
   {
     Course = course,
   Amount = course.Price ?? 0 // Set amount from course price
      };

  return View("CreatePayment", viewModel);// Show payment form
     }

      // Process after submitting the payment form
    [HttpPost]
     [ValidateAntiForgeryToken]
  public async Task<IActionResult> CreatePayment(PaymentViewModel viewModel)
        {
            _logger.LogInformation($"Payment form submitted. CourseId: {viewModel.Course?.CourseId}, Amount: {viewModel.Amount}");

         // Clear Course-related validation errors since we don't validate Course properties in payment form
 var courseErrors = ModelState.Keys.Where(key => key.StartsWith("Course.")).ToList();
          foreach (var key in courseErrors)
 {
        ModelState.Remove(key);
   }

          // Reload course data from database
       var courseId = viewModel.Course?.CourseId ?? 0;
        if (courseId > 0)
            {
      viewModel.Course = _skillExchangeContext.Course
          .FirstOrDefault(c => c.CourseId == courseId);
     }

    if (viewModel.Course == null)
       {
    ModelState.AddModelError("", "Invalid course selected.");
            return View("CreatePayment", viewModel);
  }

            // Check if course is free
    if (viewModel.Course.Price == null || viewModel.Course.Price == 0)
            {
       TempData["Info"] = "This is a free course. No payment required.";
                return RedirectToAction("IndexCourse", "Course");
      }

    // Validate amount
         if (viewModel.Amount <= 0)
            {
     ModelState.AddModelError("Amount", "Amount must be greater than 0.");
        return View("CreatePayment", viewModel);
          }

        // If model state is invalid, redisplay the form with errors
            if (!ModelState.IsValid)
            {
        _logger.LogWarning("Model state is invalid. Errors: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
  return View("CreatePayment", viewModel);
            }

   var email = User.FindFirstValue(ClaimTypes.Email);
        var userId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;

      if (userId == null)
  {
           ModelState.AddModelError("", "User not found. Please login again.");
    return View("CreatePayment", viewModel);
       }

   // Process card payment (simulate success)
            var payment = new Payment
            {
         CourseId = courseId,
         UserInfoId = userId,
        PaymentMethod = "Card",
   Amount = viewModel.Amount,
          PaymentStatus = "Completed",
          CardNumber = viewModel.CardNumber?.Substring(Math.Max(0, viewModel.CardNumber.Length - 4)), // Store only last 4 digits
          ExpiryDate = viewModel.ExpiryDate,
         CVV = viewModel.CVV,
            PaymentDate = DateTime.Now
   };

            _skillExchangeContext.Payment.Add(payment);

            // Enroll the user in the course
     var existingEnrollment = _skillExchangeContext.Enrollments
.Any(e => e.CourseId == courseId && e.UserInfoId == userId);
            
            if (!existingEnrollment)
        {
     _skillExchangeContext.Enrollments.Add(new Enrollment
   {
           CourseId = courseId,
          UserInfoId = userId
    });
            }

       await _skillExchangeContext.SaveChangesAsync();

         TempData["PaymentSuccess"] = $"Card payment successful for the course: {viewModel.Course?.Title}. Amount paid: ৳{viewModel.Amount}";

            // If admin paid, optionally redirect to course contents
            if (User.IsInRole("Admin") && courseId > 0)
      {
    return RedirectToAction("CourseContents", "Content", new { courseId });
 }

       // Redirect back to course list page
        return RedirectToAction("IndexCourse", "Course");
        }
    }
}
