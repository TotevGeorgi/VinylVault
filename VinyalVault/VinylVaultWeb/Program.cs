using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using CoreLayer.Services;
using CoreLayer;
using DataLayer;
using Common;
using Common.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/LogIn";
        options.LogoutPath = "/LogOut";
        options.AccessDeniedPath = "/LogIn";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddSession();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<DBConnection>();

builder.Services.AddScoped<IUserRepository, DBUser>();
builder.Services.AddScoped<IVinylRepository, DBVinyl>();
builder.Services.AddScoped<IOrderRepository, DBOrder>();
builder.Services.AddScoped<IWishlistRepository, DBWishlist>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVinylService, VinylService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();

builder.Services.AddHttpClient<IGenreService, SpotifyGenreService>();
builder.Services.AddHttpClient<ISpotifyAlbumService, SpotifyAlbumService>();

builder.Services.AddSingleton<DBCashMarket>();
builder.Services.AddScoped<ICacheService, CacheService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();
