using System;
using System.Collections.Generic;
using System.Text;
using HotelReservationsManager.Models.Domain;

namespace HotelReservationsManager.Business.Interfaces
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllAsync(string filter, int pageIndex, int pageSize);
        Task<int> GetCountAsync(string filter);
        Task<Client> GetByIdAsync(int id);
        Task CreateAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(int id);
    }
}
