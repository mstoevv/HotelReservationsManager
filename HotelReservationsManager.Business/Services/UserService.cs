using HotelReservationsManager.Business.Interfaces;
using HotelReservationsManager.Data;
using HotelReservationsManager.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Business.Services
{
    public class UserService : IUserService
    {
        private readonly HotelDbContext _context;

        public UserService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync(string filter, int pageIndex, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(u => u.Username.Contains(filter) ||
                                         u.FirstName.Contains(filter) ||
                                         u.MiddleName.Contains(filter) ||
                                         u.LastName.Contains(filter) ||
                                         u.Email.Contains(filter));
            }

            return await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(string filter)
        {
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(u => u.Username.Contains(filter) ||
                                         u.FirstName.Contains(filter) ||
                                         u.Email.Contains(filter));
            }
            return await query.CountAsync();
        }

        public async Task<User> GetByIdAsync(int id) => await _context.Users.FindAsync(id);

        public async Task CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}