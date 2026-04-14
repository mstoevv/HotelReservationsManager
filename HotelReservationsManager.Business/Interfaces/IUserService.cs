using System;
using System.Collections.Generic;
using System.Text;
using HotelReservationsManager.Models.Domain;

namespace HotelReservationsManager.Business.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync(string filter, int pageIndex, int pageSize);
        Task<int> GetCountAsync(string filter);
        Task<User> GetByIdAsync(int id);
        Task CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> AuthenticateAsync(string username, string password);
    }
}