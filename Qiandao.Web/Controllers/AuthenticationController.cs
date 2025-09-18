using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Qiandao.Model.Response;
using Qiandao.Model.ViewModel;
using Qiandao.Service;
using Qiandao.Web.Extensions;
using System.Collections.Generic;
using static System.Net.WebRequestMethods;

namespace Qiandao.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogger<DeviceController> _logger;
        private readonly LoginService? _loginService;

        public AuthenticationController(ILogger<DeviceController> logger, LoginService loginService)
        {
            _logger = logger;
            this._loginService = loginService;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel objUser)
        {
            try
            {
                ResponseModel rm = await _loginService.Login(objUser);


                if(rm.Code == 200)
                {
                    string dataJson = JsonConvert.SerializeObject(rm.Data);
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson);

                    if (dict.TryGetValue("TenantId", out var tenantId))
                    {
                        HttpContext.Session.SetObject("TenantId", tenantId?.ToString());
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid User Name or Password";
                    return View();
                }

            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = " Error!!! contact cms@info.in";
                return View();
            }
        }

        public IActionResult Logout()
        {
            // Delete cookies
            Response.Cookies.Delete("Username");
            Response.Cookies.Delete("Password");
            Response.Cookies.Delete("Role");
            Response.Cookies.Delete("RoleId");
            Response.Cookies.Delete("EmpId");
            Response.Cookies.Delete("Name");

            // Clear session
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Authentication");
        }

    }
}
