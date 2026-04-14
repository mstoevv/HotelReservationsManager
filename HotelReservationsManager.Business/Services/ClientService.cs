using HotelReservationsManager.Business.Interfaces;
using HotelReservationsManager.Data;
using HotelReservationsManager.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Business.Services
{
    public class ClientService : IClientService
    {
        private readonly HotelDbContext _context;

        public ClientService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetAllAsync(string filter, int pageIndex, int pageSize)
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(c => c.FirstName.Contains(filter) || c.LastName.Contains(filter));
            }

            return await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(string filter)
        {
            var query = _context.Clients.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(c => c.FirstName.Contains(filter) || c.LastName.Contains(filter));
            }
            return await query.CountAsync();
        }

        public async Task<Client> GetByIdAsync(int id) => await _context.Clients.FindAsync(id);

        public async Task CreateAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            _context.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
        }
    }
}