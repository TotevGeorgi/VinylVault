using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CoreLayer.Services;
using DataLayer;
using Common.Repositories;
using CoreLayer;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

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
builder.Services.AddScoped<IRatingRepository, DBRating>();
builder.Services.AddSingleton<ICacheRepository, DbCacheMarket>();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<IAuthenticationService, UserService>();
builder.Services.AddScoped<IRegistrationService, UserService>();
builder.Services.AddScoped<IUserProfileService, UserService>();
builder.Services.AddScoped<ISellerService, UserService>();

builder.Services.AddScoped<IVinylService, VinylService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

builder.Services.Configure<SpotifySettings>(builder.Configuration.GetSection("Spotify"));
builder.Services.AddSingleton<ISpotifySettings>(sp =>
    sp.GetRequiredService<IOptions<SpotifySettings>>().Value
);

builder.Services.AddHttpClient<ISpotifyHttpClient, SpotifyHttpClient>();
builder.Services.AddScoped<ISpotifyAlbumService, SpotifyAlbumService>();
builder.Services.AddScoped<IGenreService, SpotifyGenreService>();

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
