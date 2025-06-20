using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface IUserProfileService
    {
        bool UpdateUserProfile(string email, string fullName, string address);
        Task<bool> DeleteUser(string email);
    }
}
