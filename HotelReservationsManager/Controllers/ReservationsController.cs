using HotelReservationsManager.Business.Interfaces;
using HotelReservationsManager.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace HotelReservationsManager.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee,Guest")]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IRoomService _roomService;
        private readonly IClientService _clientService;
        private readonly IUserService _userService;

        public ReservationsController(
            IReservationService reservationService,
            IRoomService roomService,
            IClientService clientService,
            IUserService userService)
        {
            _reservationService = reservationService;
            _roomService = roomService;
            _clientService = clientService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(int? roomId, int pageIndex = 1, int pageSize = 10)
        {
            var reservations = await _reservationService.GetAllAsync(pageIndex, pageSize);

            if (roomId.HasValue)
            {
                reservations = reservations.Where(r => r.RoomId == roomId.Value).ToList();
                ViewBag.CurrentRoomId = roomId;
            }

            if (User.IsInRole("Guest"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    int currentUserId = int.Parse(userIdClaim.Value);
                    reservations = reservations.Where(r => r.UserId == currentUserId).ToList();
                }
            }

            var total = await _reservationService.GetCountAsync();
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return View(reservations);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ByEmployee(int employeeId)
        {
            var allReservations = await _reservationService.GetAllAsync(1, 1000);
            var filtered = allReservations.Where(r => r.UserId == employeeId).ToList();
            var employee = await _userService.GetByIdAsync(employeeId);
            ViewData["Title"] = $"Резервации на {employee.FirstName} {employee.LastName}";
            return View("Index", filtered);
        }

        // ПОКАЗВАНЕ НА ФОРМАТА - ТУК Е ФИЛТЪРЪТ ЗА ПОВЕРИТЕЛНОСТ
        public async Task<IActionResult> Create()
        {
            var allRooms = await _roomService.GetAllAsync();
            var freeRooms = allRooms.Where(r => r.IsFree).ToList();
            var allClients = await _clientService.GetAllAsync("", 1, 1000);

            if (User.IsInRole("Guest"))
            {
                // Вземаме ID-то и Имейла на текущия гост (Мария)
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

                // ФИЛТЪР: Мария вижда само себе си ИЛИ хора, добавени от нея
                ViewBag.Clients = allClients.Where(c =>
                    c.Email == currentUserEmail ||
                    c.AddedByUserId == currentUserId
                ).ToList();
            }
            else
            {
                // Служителите и Админите виждат всички за оперативна работа
                ViewBag.Clients = allClients;
            }

            ViewBag.Rooms = new SelectList(freeRooms, "Id", "Number");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation, int[] selectedClients)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                reservation.UserId = int.Parse(userIdClaim.Value);
            }

            var room = await _roomService.GetByIdAsync(reservation.RoomId);

            // Проверка за капацитет
            if (room != null && selectedClients != null)
            {
                if (selectedClients.Length > room.Capacity)
                {
                    ModelState.AddModelError("", $"Грешка: Стая №{room.Number} позволява макс. {room.Capacity} души. Избрали сте {selectedClients.Length}.");
                }
            }

            if (selectedClients == null || selectedClients.Length == 0)
            {
                ModelState.AddModelError("", "Трябва да изберете поне един клиент.");
            }

            ModelState.Remove("User");
            ModelState.Remove("Room");
            ModelState.Remove("Clients");

            if (ModelState.IsValid)
            {
                reservation.Clients = new List<Client>();
                foreach (var clientId in selectedClients)
                {
                    var client = await _clientService.GetByIdAsync(clientId);
                    if (client != null) reservation.Clients.Add(client);
                }

                reservation.FinalPrice = _reservationService.CalculateFinalPrice(reservation, room, reservation.Clients.ToList());

                await _reservationService.CreateAsync(reservation);

                if (room != null)
                {
                    room.IsFree = false;
                    await _roomService.UpdateAsync(room);
                }

                return RedirectToAction(nameof(Index));
            }

            // При грешка връщаме филтрирания списък отново
            var allClients = await _clientService.GetAllAsync("", 1, 1000);
            if (User.IsInRole("Guest"))
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
                ViewBag.Clients = allClients.Where(c => c.Email == currentUserEmail || c.AddedByUserId == currentUserId).ToList();
            }
            else
            {
                ViewBag.Clients = allClients;
            }

            var allRooms = await _roomService.GetAllAsync();
            ViewBag.Rooms = new SelectList(allRooms.Where(r => r.IsFree || r.Id == reservation.RoomId), "Id", "Number", reservation.RoomId);

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee,Guest")]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _reservationService.GetByIdAsync(id);
            if (reservation == null) return NotFound();

            if (User.IsInRole("Guest"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || int.Parse(userIdClaim.Value) != reservation.UserId)
                {
                    return Forbid();
                }
            }

            var room = await _roomService.GetByIdAsync(reservation.RoomId);
            if (room != null)
            {
                room.IsFree = true;
                await _roomService.UpdateAsync(room);
            }

            await _reservationService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}