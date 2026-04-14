using System.ComponentModel.DataAnnotations;

namespace HotelReservationsManager.Models.Domain
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [MaxLength(10)]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public bool IsAdult { get; set; }

        // --- НОВОТО ПОЛЕ ЗА СИГУРНОСТ ---
        /// <summary>
        /// Съхранява ID-то на потребителя (User), който е регистрирал този клиент.
        /// Ако е null, клиентът е добавен от администратор/служител или е основен гост.
        /// </summary>
        public int? AddedByUserId { get; set; }
    }
}