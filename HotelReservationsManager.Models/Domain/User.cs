using System;
using System.ComponentModel.DataAnnotations;

namespace HotelReservationsManager.Models.Domain
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [StringLength(10)]
        public string EGN { get; set; }

        [Required]
        [MaxLength(10)]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public DateTime? DismissalDate { get; set; }
    }
}