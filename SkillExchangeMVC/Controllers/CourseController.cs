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
        public IActionResult IndexCourse(string searchTerm)
        {
            var coursesQuery = _skillExchangeContext.Course.AsQueryable();

            // Apply search filter if search term is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                coursesQuery = coursesQuery.Where(c => 
                    c.Title.ToLower().Contains(searchTerm) || 
                    c.Description.ToLower().Contains(searchTerm) ||
                    c.Category.ToLower().Contains(searchTerm) ||
                    c.SubCategory.ToLower().Contains(searchTerm));
            }

            var viewModel = new CourseViewModel
            {
                Courses = coursesQuery.OrderBy(c => c.CourseId).ToList(),
                SearchTerm = searchTerm
            };

            // Determine already enrolled courses for current user (Student/Teacher/Admin)
            var email = User.FindFirstValue(ClaimTypes.Email);
            var userId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
            if (userId != null)
            {
                viewModel.EnrolledCourseIds = _skillExchangeContext.Enrollments
                    .Where(e => e.UserInfoId == userId)
                    .Select(e => e.CourseId)
                    .ToList();
            }
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

                // Ensure price is not negative
                if (viewModel.Course.Price < 0)
                {
                    viewModel.Course.Price = 0;
                }

                _skillExchangeContext.Course.Add(viewModel.Course);
                _skillExchangeContext.SaveChanges();

                TempData["Success"] = $"Course '{viewModel.Course.Title}' created successfully with price ৳{viewModel.Course.Price ?? 0}";
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

                // Store old price for comparison
                var oldPrice = existing.Price ?? 0;

                existing.Title = updatedCourse.Title;
                existing.Description = updatedCourse.Description;
                existing.Category = updatedCourse.Category;
                existing.SubCategory = updatedCourse.SubCategory;
                existing.IsPremium = updatedCourse.IsPremium;
                existing.Price = updatedCourse.Price >= 0 ? updatedCourse.Price : 0; // Ensure price is not negative

                var name = User.FindFirstValue(ClaimTypes.Name);
                existing.UpdatedBy = name ?? "Unknown";
                existing.UpdatedDate = DateTime.Now;

                _skillExchangeContext.SaveChanges();

                // Show success message with price update info
                if (oldPrice != existing.Price)
                {
                    TempData["Success"] = $"Course '{existing.Title}' updated successfully. Price changed from ৳{oldPrice} to ৳{existing.Price ?? 0}";
                }
                else
                {
                    TempData["Success"] = $"Course '{existing.Title}' updated successfully.";
                }

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

            TempData["Success"] = $"Course '{course.Title}' deleted successfully.";
            return RedirectToAction("CreateCourse");
        }

        [Authorize(Roles = "Teacher,Admin")]
        public IActionResult TeacherCourses()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var teacherId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;

            var courses = _skillExchangeContext.Course
                            .Where(c => c.TeacherId == teacherId)
                            .ToList();

            return View(courses);
        }

        // Students enroll to attend quizzes/exams (Admin allowed as superuser)
        [Authorize(Roles = "Student,Admin")]
        [HttpPost]
        public IActionResult Enroll(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var studentId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
            if (studentId == null) return Unauthorized();
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == id);
            if (course == null) return NotFound();

            // For free courses, enroll directly and redirect to content
            if (!course.IsPremium || course.Price == 0)
            {
                var exists = _skillExchangeContext.Enrollments.Any(e => e.CourseId == id && e.UserInfoId == studentId);
                if (!exists)
                {
                    _skillExchangeContext.Enrollments.Add(new Enrollment
                    {
                        CourseId = id,
                        UserInfoId = studentId
                    });
                    _skillExchangeContext.SaveChanges();
                }
                // Redirect to course content for free courses
                return RedirectToAction("CourseContents", "Content", new { courseId = id });
            }

            // For premium courses, check if user is Admin
            if (course.IsPremium && course.Price > 0)
            {
                if (User.IsInRole("Admin"))
                {
                    // Admin can access premium courses for free
                    var exists = _skillExchangeContext.Enrollments.Any(e => e.CourseId == id && e.UserInfoId == studentId);
                    if (!exists)
                    {
                        _skillExchangeContext.Enrollments.Add(new Enrollment
                        {
                            CourseId = id,
                            UserInfoId = studentId
                        });
                        _skillExchangeContext.SaveChanges();
                    }
                    return RedirectToAction("CourseContents", "Content", new { courseId = id });
                }
                else
                {
                    // Students need to pay for premium courses
                    return RedirectToAction("CreatePayment", "Payment", new { courseId = id });
                }
            }

            // Default fallback (shouldn't reach here)
            TempData["Error"] = "Unable to process enrollment. Please try again.";
            return RedirectToAction("IndexCourse");
        }

        // Admin enroll options page
        [Authorize(Roles = "Admin")]
        public IActionResult AdminEnrollOptions(int id)
        {
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == id);
            if (course == null) return NotFound();
            
            // For free courses, directly enroll and redirect to content
            if (!course.IsPremium || course.Price == 0)
            {
                return AdminEnrollWithoutPayment(id);
            }
            
            // For premium courses, show enrollment options
            return View("AdminEnrollOptions", course);
        }

        // Admin enroll without payment and view contents
        [Authorize(Roles = "Admin")]
        public IActionResult AdminEnrollWithoutPayment(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var adminId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
            if (adminId == null) return Unauthorized();
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == id);
            if (course == null) return NotFound();

            var exists = _skillExchangeContext.Enrollments.Any(e => e.CourseId == id && e.UserInfoId == adminId);
            if (!exists)
            {
                _skillExchangeContext.Enrollments.Add(new Enrollment
                {
                    CourseId = id,
                    UserInfoId = adminId
                });
                _skillExchangeContext.SaveChanges();
            }
            return RedirectToAction("CourseContents", "Content", new { courseId = id });
        }

        // New action for quick price update
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdatePrice(int courseId, decimal price)
        {
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return Json(new { success = false, message = "Course not found" });
            }

            var oldPrice = course.Price ?? 0;
            course.Price = price >= 0 ? price : 0;
            
            var name = User.FindFirstValue(ClaimTypes.Name);
            course.UpdatedBy = name ?? "Unknown";
            course.UpdatedDate = DateTime.Now;

            _skillExchangeContext.SaveChanges();

            return Json(new { 
                success = true, 
                message = $"Price updated from ৳{oldPrice} to ৳{course.Price}",
                newPrice = course.Price
            });
        }

        // Teacher enroll action - similar to student but for teachers
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        public IActionResult TeacherEnroll(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var teacherId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
            if (teacherId == null) return Unauthorized();
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == id);
            if (course == null) return NotFound();

            // For free courses, enroll directly and redirect to content
            if (!course.IsPremium || course.Price == 0)
            {
                var exists = _skillExchangeContext.Enrollments.Any(e => e.CourseId == id && e.UserInfoId == teacherId);
                if (!exists)
                {
                    _skillExchangeContext.Enrollments.Add(new Enrollment
                    {
                        CourseId = id,
                        UserInfoId = teacherId
                    });
                    _skillExchangeContext.SaveChanges();
                }
                // Redirect to course content for free courses
                return RedirectToAction("CourseContents", "Content", new { courseId = id });
            }

            // For premium courses, check if user is Admin
            if (course.IsPremium && course.Price > 0)
            {
                if (User.IsInRole("Admin"))
                {
                    // Admin can access premium courses for free
                    var exists = _skillExchangeContext.Enrollments.Any(e => e.CourseId == id && e.UserInfoId == teacherId);
                    if (!exists)
                    {
                        _skillExchangeContext.Enrollments.Add(new Enrollment
                        {
                            CourseId = id,
                            UserInfoId = teacherId
                        });
                        _skillExchangeContext.SaveChanges();
                    }
                    return RedirectToAction("CourseContents", "Content", new { courseId = id });
                }
                else
                {
                    // Teachers need to pay for premium courses
                    return RedirectToAction("CreatePayment", "Payment", new { courseId = id });
                }
            }

            // Default fallback (shouldn't reach here)
            TempData["Error"] = "Unable to process enrollment. Please try again.";
            return RedirectToAction("IndexCourse");
        }
    }
}