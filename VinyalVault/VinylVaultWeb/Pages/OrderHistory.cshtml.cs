using Common.DTOs;
using CoreLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VinylVaultWeb.Pages
{
    public class OrderHistoryModel : PageModel
    {
        private readonly IOrderService _orders;
        public List<OrderDTO> Orders { get; set; } = new();

        public OrderHistoryModel(IOrderService orders) => _orders = orders;

        public async Task<IActionResult> OnGetAsync()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/LogIn");

            Orders = await _orders.GetOrdersByUser(email);
            return Page();
        }
    }
}
