//LogoutControler.cs
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace SkillExchangeMVC.Controllers
{
    public class LogoutController : Controller 
    {

        [HttpPost]
        public async Task<IActionResult> IndexLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
