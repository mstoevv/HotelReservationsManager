using System;
using System.Collections.Generic;
using System.Text;
using HotelReservationsManager.Models.Domain;

namespace HotelReservationsManager.Business.Interfaces
{
    public interface IReservationService
    {
        Task<IEnumerable<Reservation>> GetAllAsync(int pageIndex, int pageSize);
        Task<int> GetCountAsync();
        Task<Reservation> GetByIdAsync(int id);
        Task CreateAsync(Reservation reservation);
        Task DeleteAsync(int id);
        decimal CalculateFinalPrice(Reservation reservation, Room room, List<Client> clients);
    }
}