// QuizController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using System.Security.Claims;
using System.Linq;

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
