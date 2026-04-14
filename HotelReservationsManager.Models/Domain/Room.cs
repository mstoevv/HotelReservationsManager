using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace HotelReservationsManager.Models.Domain
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public bool IsFree { get; set; }

        [Required]
        public decimal PriceAdultBed { get; set; }

        [Required]
        public decimal PriceChildBed { get; set; }
    }
}
