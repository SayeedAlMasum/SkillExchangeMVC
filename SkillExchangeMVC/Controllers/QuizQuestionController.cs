// QuizQuestionController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using System.Linq;
using System.Security.Claims;

namespace SkillExchangeMVC.Controllers
{
    [Authorize(Roles = "Teacher,Admin")]
    public class QuizQuestionController : Controller
    {
        private readonly SkillExchangeContext _db;
        public QuizQuestionController(SkillExchangeContext db)
        {
            _db = db;
        }

        public IActionResult Manage(int quizId)
        {
            var quiz = _db.Quizzes.FirstOrDefault(q => q.QuizId == quizId);
            if (quiz == null) return NotFound();
            ViewBag.Quiz = quiz;
            var questions = _db.QuizQuestions.Where(q => q.QuizId == quizId).OrderBy(q => q.Order).ToList();
            var options = _db.QuizOptions.Where(o => questions.Select(q => q.QuizQuestionId).Contains(o.QuizQuestionId)).ToList();
            ViewBag.Options = options.GroupBy(o => o.QuizQuestionId).ToDictionary(g => g.Key, g => g.ToList());
            return View("Manage", questions);
        }

        [HttpPost]
        public IActionResult AddQuestion(int quizId, string text, int marks = 1)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Error"] = "Question text is required.";
                return RedirectToAction("Manage", new { quizId });
            }
            var order = (_db.QuizQuestions.Where(q => q.QuizId == quizId).Max(q => (int?)q.Order) ?? 0) + 1;
            var qn = new QuizQuestion { QuizId = quizId, Text = text.Trim(), Marks = marks, Order = order };
            _db.QuizQuestions.Add(qn);
            _db.SaveChanges();
            return RedirectToAction("Manage", new { quizId });
        }

        [HttpPost]
        public IActionResult DeleteQuestion(int id, int quizId)
        {
            var qn = _db.QuizQuestions.FirstOrDefault(q => q.QuizQuestionId == id);
            if (qn != null)
            {
                var opts = _db.QuizOptions.Where(o => o.QuizQuestionId == id).ToList();
                _db.QuizOptions.RemoveRange(opts);
                _db.QuizQuestions.Remove(qn);
                _db.SaveChanges();
            }
            return RedirectToAction("Manage", new { quizId });
        }

        [HttpPost]
        public IActionResult AddOption(int questionId, string text, bool isCorrect = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Error"] = "Option text is required.";
                var quizId = _db.QuizQuestions.FirstOrDefault(q => q.QuizQuestionId == questionId)?.QuizId ?? 0;
                return RedirectToAction("Manage", new { quizId });
            }
            var order = (_db.QuizOptions.Where(o => o.QuizQuestionId == questionId).Max(o => (int?)o.Order) ?? 0) + 1;
            _db.QuizOptions.Add(new QuizOption { QuizQuestionId = questionId, Text = text.Trim(), IsCorrect = isCorrect, Order = order });
            _db.SaveChanges();
            var qzId = _db.QuizQuestions.First(q => q.QuizQuestionId == questionId).QuizId;
            return RedirectToAction("Manage", new { quizId = qzId });
        }

        [HttpPost]
        public IActionResult DeleteOption(int id)
        {
            var opt = _db.QuizOptions.FirstOrDefault(o => o.QuizOptionId == id);
            if (opt == null) return NotFound();
            var quizId = _db.QuizQuestions.FirstOrDefault(q => q.QuizQuestionId == opt.QuizQuestionId)?.QuizId ?? 0;
            _db.QuizOptions.Remove(opt);
            _db.SaveChanges();
            return RedirectToAction("Manage", new { quizId });
        }
    }
}
