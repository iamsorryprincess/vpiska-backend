using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RtclightWeb.Models;
using RtclightWeb.Services;

namespace RtclightWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            
            var user = await _userService.GetByCredentials(request.Name, request.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid name or password");
                return View(request);
            }

            await Authenticate(request.Name);
            return RedirectToAction("Index", "Main");
        }

        [HttpGet]
        public IActionResult Registration() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration([FromForm] RegisterModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var user = await _userService.GetByName(request.Name);

            if (user != null)
            {
                ModelState.AddModelError("", "This name is already taken, please choose something else");
                return View(request);
            }

            await _userService.Insert(new User() {Name = request.Name, Password = request.Password});
            await Authenticate(request.Name);
            return RedirectToAction("Index", "Main");
        }
        
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private async Task Authenticate(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, username)
            };
            
            var claimsIdentity = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }
    }
}