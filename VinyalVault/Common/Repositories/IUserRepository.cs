using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface IUserRepository
    {
        Task<Guid?> RegisterUser(Person person);
        Task<bool> EmailExists(string email);
        Task<Person?> GetUserByEmail(string email);
        bool UpdateUserProfile(string email, string fullName, string address);
        Task<bool> UpgradeToSeller(string email);
        Task<bool> DeleteUser(string email);
        Task<bool> RemoveItem(string email, string item);
    }
}
