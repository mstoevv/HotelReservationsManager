using System;
using System.Collections.Generic;
using System.Text;
using HotelReservationsManager.Models.Domain;

namespace HotelReservationsManager.Business.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllAsync();
        Task<Room> GetByIdAsync(int id);
        Task CreateAsync(Room room);
        Task UpdateAsync(Room room);
        Task DeleteAsync(int id);
    }
}