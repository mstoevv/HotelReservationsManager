using HotelReservationsManager.Business.Interfaces;
using HotelReservationsManager.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservationsManager.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee,Guest")]
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllAsync();
            return View(rooms);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            if (ModelState.IsValid)
            {
                room.IsFree = true;
                await _roomService.CreateAsync(room);
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // --- ТУК СА ПРОМЕНИТЕ ЗА EDIT ---

        // 1. GET МЕТОД: Отваря страницата с формата
        [HttpGet] // ВИНАГИ добавяй това за яснота
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        // 2. POST МЕТОД: Записва промените
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Room room)
        {
            // ПРЕПОРЪКА: Махни валидациите за сложни обекти, ако има такива
            // В твоя случай Room е прост, но е добре да знаеш

            if (ModelState.IsValid)
            {
                await _roomService.UpdateAsync(room);
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // 3. DELETE МЕТОД: Сложи го най-отдолу, за да не пречи на четенето
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _roomService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}