using Common;
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

        public List<Vinyl> Sellers { get; set; }
        public string AlbumName { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public int SelectedVinylId { get; set; }

        public BuyVinylModel(IVinylService vinylService, IOrderService orderService, IUserService userService)
        {
            _vinylService = vinylService;
            _orderService = orderService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync(string albumId)
        {

            var albumDetails = await _vinylService.GetAlbumDetailsByIdAsync(albumId);

            if (albumDetails == null)
            {
                return NotFound();
            }

            AlbumName = albumDetails.Name;
            Sellers = await _vinylService.GetAvailableVinylsByAlbum(albumId);


            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string albumId)
        {

            string? buyerEmail = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerEmail))
            {
                return RedirectToPage("/Login");
            }

            bool updated = await _vinylService.MarkAsSold(SelectedVinylId);

            if (!updated)
            {
                ErrorMessage = "Error: Could not mark vinyl as sold.";
            }
            else
            {
                await _orderService.AddOrder(buyerEmail, SelectedVinylId);
                Message = "Purchase successful! This vinyl has been added to your order history.";
            }

            var albumDetails = await _vinylService.GetAlbumDetailsByIdAsync(albumId);
            AlbumName = albumDetails?.Name ?? "";
            Sellers = await _vinylService.GetAvailableVinylsByAlbum(albumId);

            return Page();
        }
    }
}
