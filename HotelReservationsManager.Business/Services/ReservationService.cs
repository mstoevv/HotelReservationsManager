using HotelReservationsManager.Business.Interfaces;
using HotelReservationsManager.Data;
using HotelReservationsManager.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Business.Services
{
    public class ReservationService : IReservationService
    {
        private readonly HotelDbContext _context;

        public ReservationService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reservation>> GetAllAsync(int pageIndex, int pageSize)
        {
            return await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .Include(r => r.Clients)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync() => await _context.Reservations.CountAsync();

        public async Task<Reservation> GetByIdAsync(int id) =>
            await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(m => m.Id == id);

        public async Task CreateAsync(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            // Маркираме стаята като заета
            var room = await _context.Rooms.FindAsync(reservation.RoomId);
            if (room != null) room.IsFree = false;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                // Освобождаваме стаята при изтриване
                var room = await _context.Rooms.FindAsync(reservation.RoomId);
                if (room != null) room.IsFree = true;

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
        }

        public decimal CalculateFinalPrice(Reservation reservation, Room room, List<Client> clients)
        {
            // Използваме твоите имена: CheckOutDate и CheckInDate
            int nights = (reservation.CheckOutDate - reservation.CheckInDate).Days;

            if (nights <= 0) nights = 1;

            decimal pricePerNight = 0;
            foreach (var client in clients)
            {
                // Смятаме според това дали е възрастен или дете
                pricePerNight += client.IsAdult ? room.PriceAdultBed : room.PriceChildBed;
            }

            decimal total = nights * pricePerNight;

            // Използваме твоите имена: IsAllInclusive и HasBreakfast
            if (reservation.IsAllInclusive)
            {
                total += (nights * 20 * clients.Count); // Примерна добавка 20лв за All Inclusive
            }
            else if (reservation.HasBreakfast)
            {
                total += (nights * 10 * clients.Count); // Примерна добавка 10лв за закуска
            }

            return total;
        }
    }
}