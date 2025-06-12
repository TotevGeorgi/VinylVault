using Common;
using Common.DTOs;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface IUserService
    {
        Task<bool> RegisterUser(Person person);  // async operation can be synchronous too depending on DB interaction
        Task<bool> EmailExists(string email);    // async because it involves a DB query
        Task<Person?> AuthenticateUser(string email, string password);  // async for DB query
        Task<Person?> GetUserByEmail(string email);  // async for DB query

        bool UpdateUserProfile(string email, string fullName, string address);  // sync operation
        Task<bool> UpgradeToSeller(string email);  // async for DB query
        Task<bool> DeleteUser(string email);  // async for DB query
        Task<bool> RemoveItem(string email, string item);  // async for DB query
    }
}
