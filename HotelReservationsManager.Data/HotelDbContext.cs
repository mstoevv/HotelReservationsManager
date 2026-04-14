using Microsoft.EntityFrameworkCore;
using HotelReservationsManager.Models.Domain;

namespace HotelReservationsManager.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка за точност на цените
            modelBuilder.Entity<Reservation>()
                .Property(r => r.FinalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Room>()
                .Property(r => r.PriceAdultBed)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Room>()
                .Property(r => r.PriceChildBed)
                .HasColumnType("decimal(18,2)");

            // Добавяне на начален администратор
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin",
                Password = "adminpassword",
                FirstName = "Admin",
                MiddleName = "Adminov",
                LastName = "Administrator",
                EGN = "0000000000",
                PhoneNumber = "0888888888",
                Email = "admin@hotel.com",
                // Използвай фиксирана дата тук:
                AppointmentDate = new DateTime(2024, 1, 1),
                IsActive = true
            });
        }
    }
}