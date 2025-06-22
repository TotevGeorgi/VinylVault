using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface ISellerService
    {
        Task<bool> UpgradeToSellerAsync(string email);
        Task<bool> RemoveItemAsync(string email, string item);
    }
}
