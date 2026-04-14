using Microsoft.EntityFrameworkCore;
using HotelReservationsManager.Data;
using HotelReservationsManager.Business.Interfaces;
using HotelReservationsManager.Business.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Конфигурация на Базата данни (SQL Server)
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Конфигурация на Автентикацията и Бисквитките (Cookies)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";       // Къде отиваме за вход
        options.LogoutPath = "/Account/Logout";     // Къде отиваме за изход

        // КРИТИЧНО: Къде отива потребителят, ако няма права за дадена страница (напр. Гост в Админ панел)
        options.AccessDeniedPath = "/Account/Login";

        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// 3. Регистрация на Бизнес услугите (Dependency Injection)
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

// 4. Настройка на HTTP Middleware пайплайна
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Важно за CSS и JS файловете

app.UseRouting();

// ВАЖНО: Authentication ТРЯБВА да е преди Authorization
app.UseAuthentication();
app.UseAuthorization();

// 5. Маршрутизация (Routing)
app.MapControllerRoute(
    name: "default",
    // Настройваме началната страница да бъде Login екрана
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();