//RegisterController
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using SkillExchangeMVC.Models.ViewModels;

namespace SkillExchangeMVC.Controllers
{
    public class RegisterController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;
        public RegisterController(SkillExchangeContext skillExchangeContext)
        {
            _skillExchangeContext = skillExchangeContext;
        }

        public IActionResult CreateRegister()
        {
            var viewModel = new RegisterViewModel();
            return View(viewModel);
        }
        [HttpPost]
        public IActionResult CreateRegister(RegisterViewModel viewModel)
        {

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Check if email already exists
            bool emailExists = _skillExchangeContext.UserInfo.Any(u => u.Email == viewModel.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(viewModel);
            }
            if (viewModel.Role != "Student" && viewModel.Role != "Teacher" && viewModel.Role != "Admin")
            {
                ModelState.AddModelError("", "Invalid role selected.");
                return View(viewModel);
            }


            // Map ViewModel to User entity
            var user = new UserInfo
            {
                Name = viewModel.Name,
                Email = viewModel.Email,
                PasswordHash = new PasswordHasher<UserInfo>().HashPassword(null, viewModel.Password),
                Role = viewModel.Role
            };

            // Save to database
            _skillExchangeContext.UserInfo.Add(user);
            _skillExchangeContext.SaveChanges();
            TempData["SuccessMessage"] = "Registration successful! Please log in.";


            // Redirect to login or home page
            return RedirectToAction("CreateLogin", "Login");
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult IndexRegister()
        {
            var users = _skillExchangeContext.UserInfo.ToList();
            return View(users);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult DeleteUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid user ID." });
                }

                var user = _skillExchangeContext.UserInfo.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Prevent deleting the current logged-in user
                var currentUserEmail = User.Identity.Name;
                if (user.Email == currentUserEmail)
                {
                    return Json(new { success = false, message = "You cannot delete your own account." });
                }

                // Delete related records manually to handle any potential issues
                // Since the database has cascade delete configured, we'll let EF handle it
                // but we can also manually delete if needed
                
                try
                {
                    // Remove related payments (if cascade delete is not working)
                    var payments = _skillExchangeContext.Payment.Where(p => p.UserInfoId == id).ToList();
                    if (payments.Any())
                    {
                        _skillExchangeContext.Payment.RemoveRange(payments);
                    }

                    // Remove related enrollments
                    var enrollments = _skillExchangeContext.Enrollments.Where(e => e.UserInfoId == id).ToList();
                    if (enrollments.Any())
                    {
                        _skillExchangeContext.Enrollments.RemoveRange(enrollments);
                    }

                    // Remove related quiz attempts
                    var quizAttempts = _skillExchangeContext.QuizAttempts.Where(qa => qa.UserInfoId == id).ToList();
                    if (quizAttempts.Any())
                    {
                        _skillExchangeContext.QuizAttempts.RemoveRange(quizAttempts);
                    }

                    // Remove related certificates
                    var certificates = _skillExchangeContext.Certificates.Where(c => c.UserInfoId == id).ToList();
                    if (certificates.Any())
                    {
                        _skillExchangeContext.Certificates.RemoveRange(certificates);
                    }

                    // Finally remove the user
                    _skillExchangeContext.UserInfo.Remove(user);
                    _skillExchangeContext.SaveChanges();
                    
                    return Json(new { success = true, message = "User deleted successfully!" });
                }
                catch (DbUpdateException ex)
                {
                    // Log the specific database exception
                    System.Diagnostics.Debug.WriteLine($"Database error deleting user: {ex.Message}");
                    return Json(new { success = false, message = $"Database error: {ex.InnerException?.Message ?? ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error deleting user: {ex.Message}");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
