using HotelReservationsManager.Business.Interfaces;
using HotelReservationsManager.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelReservationsManager.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index(string filter, int pageIndex = 1, int pageSize = 10)
        {
            var users = await _userService.GetAllAsync(filter, pageIndex, pageSize);
            var totalUsers = await _userService.GetCountAsync(filter);

            ViewBag.Filter = filter;
            ViewBag.PageIndex = pageIndex;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            // 1. ПРЕМАХВАМЕ пречките пред валидацията в C#
            ModelState.Remove("DismissalDate");
            ModelState.Remove("Reservations");
            ModelState.Remove("Id");
            ModelState.Remove("MiddleName");

            if (ModelState.IsValid)
            {
                try
                {
                    user.IsActive = true;

                    // ОПРАВЯНЕ НА ГРЕШКАТА ОТ БАЗАТА:
                    // Ако базата изисква MiddleName, му даваме празен низ вместо null
                    if (string.IsNullOrEmpty(user.MiddleName))
                    {
                        user.MiddleName = string.Empty;
                    }

                    if (user.AppointmentDate == default)
                    {
                        user.AppointmentDate = DateTime.Now;
                    }

                    await _userService.CreateAsync(user);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // По-детайлно съобщение за грешка (показва InnerException)
                    var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    ModelState.AddModelError("", "Грешка при запис в базата: " + innerMessage);
                }
            }

            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            ModelState.Remove("Password");
            ModelState.Remove("DismissalDate");
            ModelState.Remove("Reservations");
            ModelState.Remove("MiddleName");

            if (ModelState.IsValid)
            {
                // Подсигуряваме MiddleName и тук
                if (string.IsNullOrEmpty(user.MiddleName)) user.MiddleName = string.Empty;

                await _userService.UpdateAsync(user);
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (user != null && (user.Username.ToLower() == "admin" || user.Id.ToString() == currentUserId))
            {
                return BadRequest("Не можете да изтриете администратора или собствения си профил!");
            }

            await _userService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}