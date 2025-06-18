using Common;
using Common.DTOs;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface IUserService
    {
        Task<Guid?> RegisterUser(Person person);
        Task<bool> EmailExists(string email);   
        Task<Person?> AuthenticateUser(string email, string password);  
        Task<Person?> GetUserByEmail(string email);

        bool UpdateUserProfile(string email, string fullName, string address); 
        Task<bool> UpgradeToSeller(string email);  
        Task<bool> DeleteUser(string email);  
        Task<bool> RemoveItem(string email, string item);  
    }
}
