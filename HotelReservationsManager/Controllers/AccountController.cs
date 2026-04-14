using HotelReservationsManager.Business.Interfaces;
using HotelReservationsManager.Models.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelReservationsManager.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IClientService _clientService; // Добавяме това

        // Инжектираме и IClientService
        public AccountController(IUserService userService, IClientService clientService)
        {
            _userService = userService;
            _clientService = clientService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _userService.AuthenticateAsync(username, password);

            if (user != null)
            {
                string userRole = "Guest";
                if (user.Username.ToLower() == "admin")
                {
                    userRole = "Admin";
                }
                else if (user.IsActive)
                {
                    userRole = "Employee";
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, userRole),
                    new Claim(ClaimTypes.Email, user.Email) // Полезно е да имаме имейла в клеймовете
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Невалидно потребителско име или парола!");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            ModelState.Remove("DismissalDate");
            ModelState.Remove("AppointmentDate");
            ModelState.Remove("IsActive");

            if (ModelState.IsValid)
            {
                var existingUser = await _userService.GetByUsernameAsync(user.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Потребителското име вече е заето!");
                    return View(user);
                }

                // 1. СЪЗДАВАМЕ ПОТРЕБИТЕЛ (за логване)
                user.IsActive = false;
                user.AppointmentDate = DateTime.Now;
                await _userService.CreateAsync(user);

                // 2. АВТОМАТИЧНО СЪЗДАВАМЕ КЛИЕНТ (за да може да бъде добавян в резервации)
                var client = new Client
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    IsAdult = true // Приемаме, че щом се регистрира сам, е възрастен
                };

                // Провери как се казва полето за ЕГН в твоя Client модел (EGN или PersonalId)
                // Ако в модела Client имаш поле EGN, добави го тук:
                // client.EGN = user.EGN;

                await _clientService.CreateAsync(client);

                return RedirectToAction("Login");
            }

            return View(user);
        }
    }
}