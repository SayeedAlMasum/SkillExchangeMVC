//ProfileController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.Context;
using System.Security.Claims;

namespace SkillExchangeMVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly SkillExchangeContext _context;

        public ProfileController(SkillExchangeContext context)
        {
            _context = context;
        }

        public IActionResult IndexProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.UserInfo.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return View(user);
        }
    }
}
