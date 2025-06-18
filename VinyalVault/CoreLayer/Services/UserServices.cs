using Common;
using Common.DTOs;
using Common.Repositories;
using DataLayer;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Guid?> RegisterUser(Person person)
        {
            return await _userRepository.RegisterUser(person);
        }


        public async Task<bool> EmailExists(string email)
        {
            return await _userRepository.EmailExists(email);  
        }

        public async Task<Person?> AuthenticateUser(string email, string password)
        {
            var person = await _userRepository.GetUserByEmail(email);

            if (person == null)
                return null;

            if (!PasswordHasher.Verify(password, person.PasswordHash))
                return null;

            if (person.UserId == Guid.Empty)
            {
                Console.WriteLine($"[ERROR] User found but has empty UserId for email: {email}");
                return null;
            }

            return person;
        }


        public async Task<Person?> GetUserByEmail(string email)
        {
            return await _userRepository.GetUserByEmail(email);  
        }

        public bool UpdateUserProfile(string email, string fullName, string address)
        {
            return _userRepository.UpdateUserProfile(email, fullName, address);  
        }

        public async Task<bool> UpgradeToSeller(string email)
        {
            return await _userRepository.UpgradeToSeller(email);  
        }

        public async Task<bool> DeleteUser(string email)
        {
            return await _userRepository.DeleteUser(email);  
        }

        public async Task<bool> RemoveItem(string email, string item)
        {
            return await _userRepository.RemoveItem(email, item);
        }
    }
}
