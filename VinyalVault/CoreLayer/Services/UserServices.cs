using Common;
using Common.DTOs;
using DataLayer;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class UserService : IUserService
    {
        private readonly DBUser _userRepository;

        public UserService(DBUser userRepository)
        {
            _userRepository = userRepository;
        }

        // Register a new user (can be User, Seller, or Admin)
        public async Task<bool> RegisterUser(Person person)
        {
            return await _userRepository.RegisterUser(person);  // Await async method in DBUser
        }

        // Check if an email exists in the database
        public async Task<bool> EmailExists(string email)
        {
            return await _userRepository.EmailExists(email);  // Await async method in DBUser
        }

        // Authenticate user by email and password
        public async Task<Person?> AuthenticateUser(string email, string password)
        {
            return await _userRepository.GetUserByEmail(email);  // Await async method in DBUser
        }

        // Get user by email
        public async Task<Person?> GetUserByEmail(string email)
        {
            return await _userRepository.GetUserByEmail(email);  // Await async method in DBUser
        }

        // Update user profile (full name, address)
        public bool UpdateUserProfile(string email, string fullName, string address)
        {
            return _userRepository.UpdateUserProfile(email, fullName, address);  // Sync operation, no await needed
        }

        // Upgrade user to seller
        public async Task<bool> UpgradeToSeller(string email)
        {
            return await _userRepository.UpgradeToSeller(email);  // Await async method in DBUser
        }

        // Admin can delete a user
        public async Task<bool> DeleteUser(string email)
        {
            return await _userRepository.DeleteUser(email);  // Await async method in DBUser
        }

        // Admin can remove a vinyl item
        public async Task<bool> RemoveItem(string email, string item)
        {
            return await _userRepository.RemoveItem(email, item);  // Await async method in DBUser
        }
    }
}
