using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTOs;

namespace CoreLayer.Services
{
    public interface IOrderService
    {
        Task<bool> AddOrder(string buyerEmail, int vinylId);
        Task<List<Order>> GetOrdersByUser(string email);
    }
}
