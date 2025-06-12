using Common.DTOs;
using DataLayer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class OrderService : IOrderService
    {
        private readonly DBOrder _dbOrder;

        public OrderService(DBOrder dbOrder)
        {
            _dbOrder = dbOrder;
        }

        public async Task<bool> AddOrder(string buyerEmail, int vinylId)
        {
            return await _dbOrder.AddOrder(buyerEmail, vinylId);
        }

        public async Task<List<Order>> GetOrdersByUser(string email)
        {
            return await _dbOrder.GetOrdersByUser(email);
        }
    }
}
