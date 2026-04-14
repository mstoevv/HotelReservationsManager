using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelReservationsManager.Models.Domain
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<Client> Clients { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public bool HasBreakfast { get; set; }

        [Required]
        public bool IsAllInclusive { get; set; }

        [Required]
        public decimal FinalPrice { get; set; }
    }
}