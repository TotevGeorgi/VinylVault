using Common;
using Common.Repositories;
using DataLayer;
using System;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public class UserService : IAuthenticationService, IRegistrationService, IUserProfileService, ISellerService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher _hasher;

        public UserService(IUserRepository userRepo, IPasswordHasher hasher)
        {
            _userRepo = userRepo;
            _hasher = hasher;
        }

        public async Task<Person?> AuthenticateUser(string email, string password)
        {
            var p = await _userRepo.GetUserByEmail(email);
            if (p == null || !_hasher.Verify(password, p.PasswordHash) || p.UserId == Guid.Empty)
                return null;
            return p;
        }

        public Task<Person?> GetUserByEmail(string email)
            => _userRepo.GetUserByEmail(email);

        public Task<Guid?> RegisterUser(Person person)
            => _userRepo.RegisterUser(person);

        public Task<bool> EmailExists(string email)
            => _userRepo.EmailExists(email);

        public Task<bool> UpdateUserProfileAsync(string email, string fullName, string address)
            => Task.FromResult(_userRepo.UpdateUserProfile(email, fullName, address));

        public Task<bool> DeleteUserAsync(string email)
            => _userRepo.DeleteUser(email);

        public Task<bool> UpgradeToSellerAsync(string email)
            => _userRepo.UpgradeToSeller(email);

        public Task<bool> RemoveItemAsync(string email, string item)
            => _userRepo.RemoveItem(email, item);
    }
}
