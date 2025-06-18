using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface IOrderRepository
    {
        Task<bool> AddOrder(string buyerEmail, int vinylId);
        Task<List<OrderDTO>> GetOrdersByUser(string email);
    }

}
