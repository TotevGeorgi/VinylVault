using Common.DTOs;
using CoreLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace VinylVaultWeb.Pages
{
    public class OrderHistoryModel : PageModel
    {
        private readonly IOrderService _orderService;

        public List<OrderDTO> Orders { get; set; } = new();

        public OrderHistoryModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/LogIn");
            }

            Orders = await _orderService.GetOrdersByUser(userId);
            return Page();
        }
    }
}
