using Microsoft.AspNetCore.Mvc;

namespace Qiandao.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
