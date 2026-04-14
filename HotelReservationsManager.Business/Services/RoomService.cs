using HotelReservationsManager.Business.Interfaces;
using HotelReservationsManager.Data;
using HotelReservationsManager.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Business.Services
{
    public class RoomService : IRoomService
    {
        private readonly HotelDbContext _context;

        public RoomService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetAllAsync() => await _context.Rooms.ToListAsync();

        public async Task<Room> GetByIdAsync(int id) => await _context.Rooms.FindAsync(id);

        public async Task CreateAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Room room)
        {
            _context.Update(room);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
        }
    }
}