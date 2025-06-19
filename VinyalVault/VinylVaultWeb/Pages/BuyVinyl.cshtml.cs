using Common;
using Common.DTOs;
using CoreLayer;
using CoreLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinylVaultWeb.Pages
{
    [Authorize]
    public class BuyVinylModel : PageModel
    {
        private readonly IVinylService _vinylService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IRatingService _ratingService;

        public List<Vinyl> Sellers { get; set; } = new();
        public string AlbumName { get; set; } = "";
        public Dictionary<string, double> SellerRatings { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string AlbumId { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "rating";

        [TempData]
        public string? Message { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SelectedVinylId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SellerEmail { get; set; }

        [BindProperty]
        public int RatingScore { get; set; }

        [BindProperty]
        public string? RatingComment { get; set; }

        public BuyVinylModel(
            IVinylService vinylService,
            IOrderService orderService,
            IUserService userService,
            IRatingService ratingService)
        {
            _vinylService = vinylService;
            _orderService = orderService;
            _userService = userService;
            _ratingService = ratingService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(AlbumId))
                return BadRequest();

            var albumDetails = await _vinylService.GetAlbumDetailsByIdAsync(AlbumId);
            if (albumDetails == null) return NotFound();
            AlbumName = albumDetails.Name;

            Sellers = await _vinylService.GetAvailableVinylsByAlbum(AlbumId);

            foreach (var v in Sellers)
                SellerRatings[v.SellerEmail] = await _ratingService.GetAverageRatingAsync(v.SellerEmail);

           
            Sellers = SortBy switch
            {
                "priceAsc" => Sellers.OrderBy(v => v.Price).ToList(),
                "priceDesc" => Sellers.OrderByDescending(v => v.Price).ToList(),
                _ => Sellers.OrderByDescending(v => SellerRatings[v.SellerEmail]).ToList(),
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(AlbumId))
                return BadRequest();

            var buyerEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerEmail))
                return RedirectToPage("/Login");

            var ok = await _vinylService.MarkAsSold(SelectedVinylId);
            if (!ok)
            {
                TempData["Error"] = "Error: Could not mark vinyl as sold.";
                return RedirectToPage(new { AlbumId });
            }
            await _orderService.AddOrder(buyerEmail, SelectedVinylId);

            var chosen = await _vinylService.GetVinylById(SelectedVinylId);
            SellerEmail = chosen?.SellerEmail;
            Message = "Purchase successful! Please leave a rating below.";

            return RedirectToPage(new { AlbumId, SelectedVinylId, SellerEmail });
        }

        public async Task<IActionResult> OnPostSubmitRatingAsync()
        {
            if (string.IsNullOrEmpty(AlbumId) || string.IsNullOrEmpty(SellerEmail) || RatingScore < 1 || RatingScore > 5)
                return RedirectToPage(new { AlbumId });

            var rating = new SellerRatingDTO
            {
                BuyerEmail = User.Identity?.Name ?? "",
                SellerEmail = SellerEmail,
                VinylId = SelectedVinylId,
                Rating = RatingScore,
                Comment = RatingComment
            };

            var success = await _ratingService.AddRatingAsync(rating);
            Message = success
                ? "Thank you! Your rating has been submitted."
                : "An error occurred while submitting the rating.";

            return RedirectToPage(new { AlbumId });
        }
    }
}
