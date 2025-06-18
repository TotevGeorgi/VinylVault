using Common.DTOs;
using Common.Repositories;
using DataLayer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _dbOrder;

        public OrderService(IOrderRepository dbOrder)
        {
            _dbOrder = dbOrder;
        }

        public async Task<bool> AddOrder(string buyerEmail, int vinylId)
        {
            return await _dbOrder.AddOrder(buyerEmail, vinylId);
        }

        public async Task<List<OrderDTO>> GetOrdersByUser(string email)
        {
            return await _dbOrder.GetOrdersByUser(email);
        }
    }
}
