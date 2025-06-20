using Common.DTOs;
using Common.Repositories;
using CoreLayer.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest
{
    public class OrderServiceTests
    {
        private readonly OrderService _svc;
        private readonly Mock<IOrderRepository> _repo = new();

        public OrderServiceTests()
        {
            _svc = new OrderService(_repo.Object);
        }

        [Fact]
        public async Task AddOrder_DelegatesToRepo()
        {
            _repo.Setup(r => r.AddOrder("u", 42)).ReturnsAsync(true);

            var ok = await _svc.AddOrder("u", 42);
            Assert.True(ok);
        }

        [Fact]
        public async Task GetOrdersByUser_DelegatesToRepo()
        {
            var dtos = new List<OrderDTO> { new() { Id = 1 } };
            _repo.Setup(r => r.GetOrdersByUser("u")).ReturnsAsync(dtos);

            var outList = await _svc.GetOrdersByUser("u");
            Assert.Same(dtos, outList);
        }
    }
}
