using HotelReservationsManager.Business.Interfaces;
using HotelReservationsManager.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelReservationsManager.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee,Guest")]
    public class ClientsController : Controller
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        // --- МЕТОД ЗА БЪРЗО ДОБАВЯНЕ СЪС ЗАПИС НА СОБСТВЕНИК ---
        [HttpPost]
        [Authorize(Roles = "Admin,Employee,Guest")]
        public async Task<IActionResult> CreateQuick([FromBody] QuickClientModel data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.FirstName) || string.IsNullOrWhiteSpace(data.LastName))
            {
                return Json(new { success = false, message = "Името и фамилията са задължителни." });
            }

            try
            {
                // Вземаме ID-то на логнатия в момента потребител (напр. Мария)
                var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int? currentUserId = !string.IsNullOrEmpty(currentUserIdClaim) ? int.Parse(currentUserIdClaim) : null;

                var newClient = new Client
                {
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    IsAdult = data.IsAdult,
                    // ТУК ЗАПИСВАМЕ КОЙ ГО ДОБАВЯ:
                    AddedByUserId = currentUserId,
                    Email = "family-member@hotel.com",
                    PhoneNumber = "0000000000"
                };

                ModelState.Clear();

                await _clientService.CreateAsync(newClient);

                return Json(new { success = true, id = newClient.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Index(string filter, int pageIndex = 1, int pageSize = 10)
        {
            var clients = await _clientService.GetAllAsync(filter, pageIndex, pageSize);
            var totalClients = await _clientService.GetCountAsync(filter);

            ViewBag.Filter = filter;
            ViewBag.PageIndex = pageIndex;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalClients / (double)pageSize);

            return View(clients);
        }

        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                await _clientService.CreateAsync(client);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();

            if (User.IsInRole("Guest"))
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                // Гостът може да едитва само себе си или хора, които той е добавил
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (client.Email != userEmail && client.AddedByUserId != currentUserId)
                {
                    return Forbid();
                }
            }

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Client client)
        {
            ModelState.Remove("Reservations");

            if (ModelState.IsValid)
            {
                await _clientService.UpdateAsync(client);

                if (User.IsInRole("Guest"))
                {
                    // Върни госта в профила му или началната страница
                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int id)
        {
            await _clientService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }

    public class QuickClientModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdult { get; set; }
    }
}