// ContentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SkillExchangeMVC.Models.Context;
using SkillExchangeMVC.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace SkillExchangeMVC.Controllers
{
    public class ContentController : Controller
    {
        private readonly SkillExchangeContext _skillExchangeContext;
        private readonly IWebHostEnvironment _env;

        public ContentController(SkillExchangeContext skillExchangeContext, IWebHostEnvironment env)
        {
            _skillExchangeContext = skillExchangeContext;
            _env = env;
        }

        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult IndexContent()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            List<Content> contents;

            if (User.IsInRole("Admin"))
            {
                // Admin can see all content from all users
                contents = _skillExchangeContext.Content
                    .OrderByDescending(c => c.CreatedDate)
                    .ToList();
            }
            else
            {
                // Teachers can only see their own uploaded content
                contents = _skillExchangeContext.Content
                    .Where(c => c.UploaderEmail == email)
                    .OrderByDescending(c => c.CreatedDate)
                    .ToList();
            }

            // Get course information for display
            var courseIds = contents.Select(c => c.CourseId).Distinct().ToList();
            var courses = _skillExchangeContext.Course
                .Where(c => courseIds.Contains(c.CourseId))
                .ToDictionary(c => c.CourseId, c => c.Title);
            
            ViewBag.Courses = courses;

            return View(contents);
        }

        // View all contents for a specific course (for enrolled users or Admin)
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult CourseContents(int courseId)
        {
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null) return NotFound();

            // Admin can always see contents
            if (!User.IsInRole("Admin"))
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                var userId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
                var isEnrolled = _skillExchangeContext.Enrollments.Any(e => e.CourseId == courseId && e.UserInfoId == userId);
                // If course is premium, require enrollment; else allow
                if (course.IsPremium && !isEnrolled)
                {
                    TempData["Error"] = "You must be enrolled to view this course contents.";
                    return RedirectToAction("IndexCourse", "Course");
                }
            }

            var contents = _skillExchangeContext.Content
                            .Where(c => c.CourseId == courseId)
                            .OrderBy(c => c.Title)
                            .ToList();
            ViewBag.Course = course;
            return View("CourseContents", contents);
        }

        // Download content file
        [Authorize(Roles = "Admin,Teacher,Student")]
        public IActionResult DownloadContent(int contentId)
        {
            var content = _skillExchangeContext.Content.FirstOrDefault(c => c.ContentId == contentId);
            if (content == null)
            {
                return NotFound("Content not found.");
            }

            // Check if user has access to this content's course
            var course = _skillExchangeContext.Course.FirstOrDefault(c => c.CourseId == content.CourseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Check permissions (only for non-admin users)
            if (!User.IsInRole("Admin"))
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                var userId = _skillExchangeContext.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
                var isEnrolled = _skillExchangeContext.Enrollments.Any(e => e.CourseId == course.CourseId && e.UserInfoId == userId);
                
                // If course is premium, require enrollment
                if (course.IsPremium && !isEnrolled)
                {
                    return Forbid("You must be enrolled to download this content.");
                }
            }

            // Handle different content types
            if (content.Type == "Link")
            {
                // For links, redirect to the URL
                return Redirect(content.URL);
            }
            else if (content.Type == "PDF" || content.Type == "Video")
            {
                // For uploaded files, provide download
                try
                {
                    var filePath = Path.Combine(_env.WebRootPath, content.URL.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    
                    if (!System.IO.File.Exists(filePath))
                    {
                        return NotFound("File not found on server.");
                    }

                    var fileName = content.Title;
                    // Add extension if not present
                    if (content.Type == "PDF" && !fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += ".pdf";
                    }
                    else if (content.Type == "Video")
                    {
                        var originalExtension = Path.GetExtension(content.URL);
                        if (!string.IsNullOrEmpty(originalExtension) && !fileName.EndsWith(originalExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            fileName += originalExtension;
                        }
                    }

                    var mimeType = content.Type == "PDF" ? "application/pdf" : "application/octet-stream";
                    
                    // Read the file
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);
                    
                    return File(fileBytes, mimeType, fileName);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error downloading file: {ex.Message}");
                }
            }
            else
            {
                return BadRequest("Unknown content type.");
            }
        }

        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult UploadContent()
        {
            // Show all courses in the dropdown
            var courses = _skillExchangeContext.Course.ToList();
            ViewBag.Courses = new SelectList(courses, "CourseId", "Title");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UploadContent(Content content, IFormFile? uploadFile)
        {
            // Clear URL validation errors initially since we'll set it programmatically for file uploads
            if (content.Type == "PDF" || content.Type == "Video")
            {
                ModelState.Remove("URL");
            }

            // Handle file upload based on type
            if (content.Type == "PDF" || content.Type == "Video")
            {
                if (uploadFile == null || uploadFile.Length == 0)
                {
                    ModelState.AddModelError("", $"Please upload a {content.Type.ToLower()} file.");
                    ViewBag.Courses = new SelectList(_skillExchangeContext.Course.ToList(), "CourseId", "Title");
                    return View(content);
                }

                var allowedExts = content.Type == "PDF" 
                    ? new[] { ".pdf" } 
                    : new[] { ".mp4", ".mov", ".avi", ".mkv", ".webm" };
                
                var ext = Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
                
                if (!allowedExts.Contains(ext))
                {
                    ModelState.AddModelError("", $"Only {content.Type.ToLower()} files are allowed.");
                    ViewBag.Courses = new SelectList(_skillExchangeContext.Course.ToList(), "CourseId", "Title");
                    return View(content);
                }

                try
                {
                    // Save the uploaded file
                    var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "content");
                    Directory.CreateDirectory(uploadsRoot);

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var fullPath = Path.Combine(uploadsRoot, fileName);
                    
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await uploadFile.CopyToAsync(stream);
                    }

                    content.URL = $"/uploads/content/{fileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading file. Please try again.");
                    ViewBag.Courses = new SelectList(_skillExchangeContext.Course.ToList(), "CourseId", "Title");
                    return View(content);
                }
            }
            else if (content.Type == "Link")
            {
                // For links, URL is required and no file upload
                if (string.IsNullOrWhiteSpace(content.URL))
                {
                    ModelState.AddModelError("URL", "Please provide a valid URL.");
                    ViewBag.Courses = new SelectList(_skillExchangeContext.Course.ToList(), "CourseId", "Title");
                    return View(content);
                }
            }
            else
            {
                ModelState.AddModelError("Type", "Please select a valid content type.");
                ViewBag.Courses = new SelectList(_skillExchangeContext.Course.ToList(), "CourseId", "Title");
                return View(content);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set audit fields like other working controllers
                    var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
                    var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "unknown@domain.com";

                    content.UploaderEmail = userEmail;
                    content.CreatedBy = userName;
                    content.UpdatedBy = userName;
                    content.CreatedDate = DateTime.Now;
                    content.UpdatedDate = DateTime.Now;

                    // Save to database
                    _skillExchangeContext.Content.Add(content);
                    _skillExchangeContext.SaveChanges();

                    TempData["Success"] = "Content uploaded successfully.";
                    return RedirectToAction("IndexContent");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving to database. Please try again.");
                }
            }

            // Repopulate dropdown on validation error
            ViewBag.Courses = new SelectList(_skillExchangeContext.Course.ToList(), "CourseId", "Title");
            return View(content);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult EditContent(int id)
        {
            var content = _skillExchangeContext.Content.FirstOrDefault(c => c.ContentId == id);
            if (content == null) return NotFound();

            ViewBag.Courses = new SelectList(_skillExchangeContext.Course.ToList(), "CourseId", "Title");
            return View(content);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult EditContent(Content updated)
        {
            if (ModelState.IsValid)
            {
                var existing = _skillExchangeContext.Content.FirstOrDefault(c => c.ContentId == updated.ContentId);
                if (existing == null) return NotFound();

                var name = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

                existing.Title = updated.Title;
                existing.Description = updated.Description;
                existing.Type = updated.Type;
                existing.URL = updated.URL;
                existing.CourseId = updated.CourseId;
                existing.UpdatedBy = name;
                existing.UpdatedDate = DateTime.Now;

                _skillExchangeContext.SaveChanges();
                TempData["Success"] = "Content updated successfully.";

                return RedirectToAction("IndexContent");
            }
            
            ViewBag.Courses = new SelectList(_skillExchangeContext.Course.ToList(), "CourseId", "Title");
            return View(updated);
        }

        // Delete content file (for course materials)
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult DeleteCourseContent(int contentId)
        {
            try
            {
                var content = _skillExchangeContext.Content.FirstOrDefault(c => c.ContentId == contentId);
                if (content == null)
                {
                    return Json(new { success = false, message = "Content not found." });
                }

                // Check if the current user has permission to delete this content
                if (!User.IsInRole("Admin"))
                {
                    var userEmail = User.FindFirstValue(ClaimTypes.Email);
                    if (content.UploaderEmail != userEmail)
                    {
                        return Json(new { success = false, message = "You can only delete content you uploaded." });
                    }
                }

                // Delete the physical file if it's an uploaded file (not a link)
                if (content.Type == "PDF" || content.Type == "Video")
                {
                    try
                    {
                        var filePath = Path.Combine(_env.WebRootPath, content.URL.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't fail the deletion from database
                        // The file might already be deleted or inaccessible
                        System.Diagnostics.Debug.WriteLine($"Error deleting file: {ex.Message}");
                    }
                }

                // Remove from database
                _skillExchangeContext.Content.Remove(content);
                _skillExchangeContext.SaveChanges();

                return Json(new { success = true, message = "Content deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting content: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult DeleteContent(int id)
        {
            var content = _skillExchangeContext.Content.FirstOrDefault(c => c.ContentId == id);
            if (content == null)
                return Json(new { success = false, message = "Content not found" });

            _skillExchangeContext.Content.Remove(content);
            _skillExchangeContext.SaveChanges();

            return Json(new { success = true, message = "Content deleted successfully" });
        }
    }
}
