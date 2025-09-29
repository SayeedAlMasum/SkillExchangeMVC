//RequirementController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;

namespace SkillExchangeMVC.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class RequirementController : Controller
    {
        public readonly SkillExchangeContext _skillExchangeContext;
        private readonly IWebHostEnvironment _env;

        public RequirementController(SkillExchangeContext skillExchangeContext, IWebHostEnvironment env)
        {
            _skillExchangeContext = skillExchangeContext;
            _env = env;
        }

        public IActionResult Index()
        {
            var files = _skillExchangeContext.RequirementDocuments.ToList();
            return View(files);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(_env.WebRootPath, "requirements", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var doc = new RequirementDocument
                {
                    FileName = fileName,
                    UploadedBy = User.Identity.Name,
                    UploadDate = DateTime.Now
                };

                _skillExchangeContext.RequirementDocuments.Add(doc);
                await _skillExchangeContext.SaveChangesAsync();

                TempData["Message"] = "Document uploaded successfully!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "No file selected!";
            return View();
        }
    }
}