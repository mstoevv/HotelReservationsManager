using HotelReservationsManager.Business.Interfaces; // За IClientService
using HotelReservationsManager.Models;
using HotelReservationsManager.Models.Domain; // За модела Client
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims; // За ClaimTypes

namespace HotelReservationsManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly IClientService _clientService;

        // Инжектираме услугата за клиенти в конструктора
        public HomeController(IClientService clientService)
        {
            _clientService = clientService;
        }

        public IActionResult Index()
        {
            // Проверка: Ако потребителят не е логнат, го пращаме към Login
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // НОВИЯТ МЕТОД ЗА ПРОФИЛА
        public async Task<IActionResult> MyProfile()
        {
            // Вземаме имейла на текущо логнатия потребител от неговата сесия (Claims)
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            // Вземаме всички клиенти (тук може да се оптимизира с метод GetByEmail в сервиза)
            var allClients = await _clientService.GetAllAsync("", 1, 1000);

            // Търсим записа в таблица Clients, който отговаря на този имейл
            var currentClient = allClients.FirstOrDefault(c => c.Email == userEmail);

            if (currentClient == null)
            {
                // Ако не намерим клиент с този имейл, показваме грешка или празна страница
                return NotFound("Не е намерен клиентски профил за този потребител.");
            }

            return View(currentClient);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}