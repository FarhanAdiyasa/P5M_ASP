using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using P5M.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace P5M.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public LoginController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("LoggedInUsername")))
            {
                return View();
            }
            else
            {
                return RedirectToAction("SSO", "Login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            var response = new { success = false, message = "", role = "" };
            var cekUsn = _dbContext.Pengguna.Where(m => m.username == username).ToList();
            var cekUsn2 = _dbContext.Pengguna.Where(m => m.username == username).FirstOrDefault();

            if (cekUsn2 != null)
            {
               
                HttpContext.Session.SetString("LoggedInUsername", cekUsn2.nama_pengguna);
                response = new { success = true, message = "", role = "" };
            }
            else
            {
                Console.WriteLine("salah");
            }


            return Json(response);
        }
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("LoggedInUsername")))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                try
                {
                    HttpContext.Session.Clear();
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return RedirectToAction("Index", "Login");
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Index", "Error");
                }
            }
        }
        [HttpGet]
        public IActionResult SSO()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("LoggedInUsername")))
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var cekUsn = _dbContext.Pengguna.Where(m => m.nama_pengguna == HttpContext.Session.GetString("LoggedInUsername")).ToList();

                return View(cekUsn);
            }
           
        }
        public async Task<IActionResult> SSOLog(string username, string role)
        {
            var response = new { success = false, message = "", role = "" };
            var cekUsn = _dbContext.Pengguna.SingleOrDefault(m => m.username == username && m.role == role);
            HttpContext.Session.SetString("LoggedInNpeng", cekUsn.username);
            HttpContext.Session.SetString("LoggedInRole",role);
            HttpContext.Session.SetString("LoggedInKelas", cekUsn.kelas);
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, cekUsn.nama_pengguna),
                };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authenticationProperties = new AuthenticationProperties
            {
               
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);

            response = new { success = true, message = "", role = role };
            return RedirectToAction("Dashboard", "Home");
        }



    }
}
