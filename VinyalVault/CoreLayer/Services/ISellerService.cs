using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface ISellerService
    {
        Task<bool> UpgradeToSeller(string email);
        Task<bool> RemoveItem(string email, string item);
    }
}
