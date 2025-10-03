// QuizController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using System.Security.Claims;
using System.Linq;
using SkiaSharp;
using Microsoft.EntityFrameworkCore;

namespace SkillExchangeMVC.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly SkillExchangeContext _db;
        public QuizController(SkillExchangeContext db)
        {
            _db = db;
        }

        // Teachers create/schedule quiz
        [Authorize(Roles = "Teacher,Admin")]
        public IActionResult Create(int courseId)
        {
            var course = _db.Course.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null) return NotFound();
            var quiz = new Quiz { CourseId = courseId, StartTime = DateTime.Now.AddDays(1), EndTime = DateTime.Now.AddDays(1).AddHours(1), Title = $"{course.Title} Quiz" };
            return View("Create", quiz);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        public IActionResult Create(Quiz quiz)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", quiz);
            }
            // ensure teacher owns course (admins bypass)
            var email = User.FindFirstValue(ClaimTypes.Email);
            var teacherId = _db.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
            var course = _db.Course.FirstOrDefault(c => c.CourseId == quiz.CourseId);
            if (course == null) return NotFound();
            if (User.IsInRole("Teacher") && !User.IsInRole("Admin") && course.TeacherId != teacherId) return Forbid();

            _db.Quizzes.Add(quiz);
            _db.SaveChanges();
            return RedirectToAction("Details", new { id = quiz.QuizId });
        }

        [Authorize(Roles = "Teacher,Admin,Student")]
        public IActionResult Details(int id)
        {
            var quiz = _db.Quizzes.FirstOrDefault(q => q.QuizId == id);
            if (quiz == null) return NotFound();
            // bring questions and options count
            var questions = _db.QuizQuestions.Where(q => q.QuizId == id).ToList();
            ViewBag.QuestionCount = questions.Count;
            ViewBag.OptionCounts = _db.QuizOptions.Where(o => questions.Select(q => q.QuizQuestionId).Contains(o.QuizQuestionId))
                .GroupBy(o => o.QuizQuestionId).ToDictionary(g => g.Key, g => g.Count());
            return View("Details", quiz);
        }

        // List quizzes by course
        [Authorize(Roles = "Teacher,Admin,Student")]
        public IActionResult ListByCourse(int courseId)
        {
            var course = _db.Course.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null) return NotFound();
            ViewBag.Course = course;
            var list = _db.Quizzes.Where(q => q.CourseId == courseId).OrderByDescending(q => q.StartTime).ToList();
            return View("ListByCourse", list);
        }

        // Delete quiz functionality
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        public IActionResult DeleteQuiz(int id)
        {
            try
            {
                var quiz = _db.Quizzes.Find(id);
                if (quiz == null)
                {
                    return Json(new { success = false, message = "Quiz not found." });
                }

                // Check if user has permission to delete this quiz
                var email = User.FindFirstValue(ClaimTypes.Email);
                var teacherId = _db.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
                var course = _db.Course.FirstOrDefault(c => c.CourseId == quiz.CourseId);
                
                if (course == null)
                {
                    return Json(new { success = false, message = "Associated course not found." });
                }

                // Only the teacher who owns the course or admin can delete
                if (User.IsInRole("Teacher") && !User.IsInRole("Admin") && course.TeacherId != teacherId)
                {
                    return Json(new { success = false, message = "You don't have permission to delete this quiz." });
                }

                try
                {
                    // Delete related records manually to handle cascade delete
                    // Remove quiz responses first
                    var quizAttempts = _db.QuizAttempts.Where(qa => qa.QuizId == id).ToList();
                    if (quizAttempts.Any())
                    {
                        var attemptIds = quizAttempts.Select(qa => qa.QuizAttemptId).ToList();
                        var quizResponses = _db.QuizResponses.Where(qr => attemptIds.Contains(qr.QuizAttemptId)).ToList();
                        if (quizResponses.Any())
                        {
                            _db.QuizResponses.RemoveRange(quizResponses);
                        }
                        
                        // Remove quiz attempts
                        _db.QuizAttempts.RemoveRange(quizAttempts);
                    }

                    // Remove quiz options
                    var quizQuestions = _db.QuizQuestions.Where(qq => qq.QuizId == id).ToList();
                    if (quizQuestions.Any())
                    {
                        var questionIds = quizQuestions.Select(qq => qq.QuizQuestionId).ToList();
                        var quizOptions = _db.QuizOptions.Where(qo => questionIds.Contains(qo.QuizQuestionId)).ToList();
                        if (quizOptions.Any())
                        {
                            _db.QuizOptions.RemoveRange(quizOptions);
                        }
                        
                        // Remove quiz questions
                        _db.QuizQuestions.RemoveRange(quizQuestions);
                    }

                    // Remove certificates related to this quiz if it's an exam
                    if (quiz.IsExam)
                    {
                        var certificates = _db.Certificates.Where(c => c.CourseId == quiz.CourseId).ToList();
                        if (certificates.Any())
                        {
                            _db.Certificates.RemoveRange(certificates);
                        }
                    }

                    // Finally remove the quiz
                    _db.Quizzes.Remove(quiz);
                    _db.SaveChanges();
                    
                    return Json(new { success = true, message = "Quiz deleted successfully!" });
                }
                catch (DbUpdateException ex)
                {
                    // Log the specific database exception
                    System.Diagnostics.Debug.WriteLine($"Database error deleting quiz: {ex.Message}");
                    return Json(new { success = false, message = $"Database error: {ex.InnerException?.Message ?? ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error deleting quiz: {ex.Message}");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // Students take quiz in window (admins allowed for testing)
        [Authorize(Roles = "Student,Admin")]
        public IActionResult Take(int id)
        {
            var quiz = _db.Quizzes.FirstOrDefault(q => q.QuizId == id);
            if (quiz == null) return NotFound();
            var now = DateTime.UtcNow;
            if (now < quiz.StartTime.ToUniversalTime() || now > quiz.EndTime.ToUniversalTime())
            {
                TempData["Error"] = "Quiz is not available at this time.";
                return RedirectToAction("Details", new { id });
            }

            var currentEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = _db.UserInfo.FirstOrDefault(u => u.Email == currentEmail)?.UserInfoId;
            var enrolled = _db.Enrollments.Any(e => e.CourseId == quiz.CourseId && e.UserInfoId == userId);
            if (!enrolled && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "You are not enrolled in this course.";
                return RedirectToAction("Details", new { id });
            }

            // Load questions and options for the quiz taking view
            var questions = _db.QuizQuestions.Where(q => q.QuizId == id).OrderBy(q => q.Order).ToList();
            var optionLookup = _db.QuizOptions.Where(o => questions.Select(q => q.QuizQuestionId).Contains(o.QuizQuestionId))
                .GroupBy(o => o.QuizQuestionId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Order).ToList());
            ViewBag.Questions = questions;
            ViewBag.Options = optionLookup;

            return View("Take", quiz);
        }

        [HttpPost]
        [Authorize(Roles = "Student,Admin")]
        public IActionResult Submit(int id)
        {
            var quiz = _db.Quizzes.FirstOrDefault(q => q.QuizId == id);
            if (quiz == null) return NotFound();
            var now = DateTime.UtcNow;
            if (now < quiz.StartTime.ToUniversalTime() || now > quiz.EndTime.ToUniversalTime())
            {
                TempData["Error"] = "Submission outside allowed time window.";
                return RedirectToAction("Details", new { id });
            }

            var userId = _db.UserInfo.FirstOrDefault(u => u.Email == User.FindFirstValue(ClaimTypes.Email))?.UserInfoId;
            if (userId == null) return Unauthorized();

            // Auto-grade MCQs if questions exist
            var questions = _db.QuizQuestions.Where(q => q.QuizId == id).ToList();
            var totalPossible = questions.Sum(q => q.Marks);
            int autoScore = 0;

            // Create attempt first to store responses
            var attempt = new QuizAttempt
            {
                QuizId = id,
                UserInfoId = userId,
            };
            _db.QuizAttempts.Add(attempt);
            _db.SaveChanges();

            var responses = new List<QuizResponse>();

            if (questions.Any())
            {
                foreach (var q in questions)
                {
                    var key = $"qn_{q.QuizQuestionId}";
                    var selectedStr = Request.Form[key].FirstOrDefault();
                    int? selectedOptionId = null;
                    if (int.TryParse(selectedStr, out var parsed))
                    {
                        selectedOptionId = parsed;
                        var correct = _db.QuizOptions.Any(o => o.QuizOptionId == parsed && o.QuizQuestionId == q.QuizQuestionId && o.IsCorrect);
                        if (correct)
                        {
                            autoScore += q.Marks;
                        }
                    }
                    responses.Add(new QuizResponse
                    {
                        QuizAttemptId = attempt.QuizAttemptId,
                        QuizQuestionId = q.QuizQuestionId,
                        SelectedOptionId = selectedOptionId
                    });
                }
                _db.QuizResponses.AddRange(responses);
                _db.SaveChanges();
            }
            else
            {
                // Fallback to manual score when no MCQs exist
                var scoreStr = Request.Form["score"].FirstOrDefault();
                if (!int.TryParse(scoreStr, out autoScore)) autoScore = 0;
            }

            // Normalize against quiz.TotalMarks if set; otherwise use totalPossible
            var referenceTotal = quiz.TotalMarks > 0 ? quiz.TotalMarks : (totalPossible > 0 ? totalPossible : quiz.TotalMarks);
            attempt.Score = Math.Clamp(autoScore, 0, referenceTotal > 0 ? referenceTotal : autoScore);
            attempt.Passed = referenceTotal > 0 ? attempt.Score >= (int)Math.Ceiling(referenceTotal * 0.6) : autoScore >= 0;

            _db.SaveChanges();

            if (attempt.Passed && quiz.IsExam)
            {
                var existingCert = _db.Certificates.FirstOrDefault(c => c.CourseId == quiz.CourseId && c.UserInfoId == userId);
                if (existingCert == null)
                {
                    var grade = GetGrade(attempt.Score, referenceTotal > 0 ? referenceTotal : attempt.Score);
                    var cert = new Certificate
                    {
                        CourseId = quiz.CourseId,
                        UserInfoId = userId,
                        Score = attempt.Score,
                        Grade = grade
                    };
                    _db.Certificates.Add(cert);
                    _db.SaveChanges();
                }
            }

            TempData["Success"] = "Quiz submitted.";
            return RedirectToAction("Result", new { attemptId = attempt.QuizAttemptId });
        }

        [Authorize(Roles = "Student,Teacher,Admin")]
        public IActionResult Result(int attemptId)
        {
            var attempt = _db.QuizAttempts.FirstOrDefault(a => a.QuizAttemptId == attemptId);
            if (attempt == null) return NotFound();
            var quiz = _db.Quizzes.FirstOrDefault(q => q.QuizId == attempt.QuizId);
            ViewBag.Quiz = quiz;
            var cert = _db.Certificates.FirstOrDefault(c => c.CourseId == quiz.CourseId && c.UserInfoId == attempt.UserInfoId);
            ViewBag.Certificate = cert;
            return View("Result", attempt);
        }

        // My certificates page
        [Authorize(Roles = "Student,Admin")]
        public IActionResult MyCertificates()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var userId = _db.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
            if (userId == null) return Unauthorized();
            var certs = _db.Certificates.Where(c => c.UserInfoId == userId).OrderByDescending(c => c.IssuedOn).ToList();
            var courseLookup = _db.Course.ToDictionary(c => c.CourseId, c => c.Title);
            ViewBag.CourseTitles = courseLookup;
            return View("MyCertificates", certs);
        }

        [Authorize(Roles = "Student,Admin")]
        public IActionResult DownloadCertificate(int id)
        {
            // id is CertificateId
            var email = User.FindFirstValue(ClaimTypes.Email);
            var currentUserId = _db.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
            if (currentUserId == null) return Unauthorized();

            var cert = _db.Certificates.FirstOrDefault(c => c.CertificateId == id);
            if (cert == null) return NotFound();

            // Only owner or admin can download
            if (!User.IsInRole("Admin") && cert.UserInfoId != currentUserId)
            {
                return Forbid();
            }

            var course = _db.Course.FirstOrDefault(c => c.CourseId == cert.CourseId);
            var user = _db.UserInfo.FirstOrDefault(u => u.UserInfoId == cert.UserInfoId);
            var courseTitle = course?.Title ?? $"Course #{cert.CourseId}";
            var studentName = user?.Name ?? "Student";

            // Generate a simple PDF certificate using SkiaSharp
            using var ms = new MemoryStream();
            using (var document = SKDocument.CreatePdf(ms))
            {
                // A4 portrait at 72 DPI => 595 x 842 points
                const float pageWidth = 595f;
                const float pageHeight = 842f;
                using var canvas = document.BeginPage(pageWidth, pageHeight);

                // Background
                canvas.Clear(SKColors.White);

                // Border
                using (var borderPaint = new SKPaint
                {
                    Color = new SKColor(9, 54, 114), // #093672
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 6,
                    IsAntialias = true
                })
                {
                    var margin = 24f;
                    canvas.DrawRect(margin, margin, pageWidth - margin * 2, pageHeight - margin * 2, borderPaint);
                }

                // Header
                using var titlePaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextSize = 36,
                    TextAlign = SKTextAlign.Center,
                    Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
                };
                canvas.DrawText("Certificate of Achievement", pageWidth / 2, 120, titlePaint);

                // Subtitle
                using var subPaint = new SKPaint
                {
                    Color = new SKColor(90, 90, 90),
                    IsAntialias = true,
                    TextSize = 18,
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText("This certificate is proudly presented to", pageWidth / 2, 160, subPaint);

                // Student Name
                using var namePaint = new SKPaint
                {
                    Color = new SKColor(9, 54, 114),
                    IsAntialias = true,
                    TextSize = 30,
                    TextAlign = SKTextAlign.Center,
                    Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
                };
                canvas.DrawText(studentName, pageWidth / 2, 210, namePaint);

                // Course line
                using var bodyPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextSize = 16,
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText($"for successfully completing the course", pageWidth / 2, 250, bodyPaint);
                using var coursePaint = new SKPaint
                {
                    Color = new SKColor(0, 128, 96),
                    IsAntialias = true,
                    TextSize = 22,
                    TextAlign = SKTextAlign.Center,
                    Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
                };
                canvas.DrawText(courseTitle, pageWidth / 2, 285, coursePaint);

                // Details box
                float boxLeft = 60, boxTop = 330, boxRight = pageWidth - 60, boxBottom = 470;
                using (var boxPaint = new SKPaint { Color = new SKColor(240, 248, 255) })
                {
                    canvas.DrawRect(SKRect.Create(boxLeft, boxTop, boxRight - boxLeft, boxBottom - boxTop), boxPaint);
                }
                using (var boxBorder = new SKPaint { Color = new SKColor(200, 220, 235), Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
                {
                    canvas.DrawRect(SKRect.Create(boxLeft, boxTop, boxRight - boxLeft, boxBottom - boxTop), boxBorder);
                }

                using var labelPaint = new SKPaint { Color = new SKColor(80, 80, 80), TextSize = 14, IsAntialias = true };
                using var valuePaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true, Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold) };

                float lineY = boxTop + 35;
                float labelX = boxLeft + 20;
                float valueX = boxLeft + 170;

                void DrawPair(string label, string value)
                {
                    canvas.DrawText(label, labelX, lineY, labelPaint);
                    canvas.DrawText(value ?? string.Empty, valueX, lineY, valuePaint);
                    lineY += 32;
                }

                DrawPair("Certificate No:", cert.CertificateNumber);
                DrawPair("Issued On:", cert.IssuedOn.ToString("yyyy-MM-dd"));
                DrawPair("Score:", cert.Score.ToString());
                DrawPair("Grade:", cert.Grade);

                // Footer/signature lines
                using var sigPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 1.5f, IsAntialias = true };
                float sigY = boxBottom + 80;
                canvas.DrawLine(80, sigY, 250, sigY, sigPaint);
                canvas.DrawLine(pageWidth - 250, sigY, pageWidth - 80, sigY, sigPaint);

                using var sigText = new SKPaint { Color = new SKColor(90, 90, 90), TextSize = 12, IsAntialias = true, TextAlign = SKTextAlign.Center };
                canvas.DrawText("Student", 165, sigY + 16, sigText);
                canvas.DrawText("Authorized", pageWidth - 165, sigY + 16, sigText);

                document.EndPage();
                document.Close();
            }

            var safeCourse = string.Join("_", (courseTitle ?? "Course").Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            var fileName = $"Certificate_{safeCourse}_{cert.CertificateNumber}.pdf";
            var bytes = ms.ToArray();
            return File(bytes, "application/pdf", fileName);
        }

        private string GetGrade(int score, int total)
        {
            var pct = total == 0 ? 0 : (score * 100.0 / total);
            if (pct >= 85) return "A";
            if (pct >= 70) return "B";
            if (pct >= 60) return "C";
            return "F";
        }
    }
}
