using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface IAuthenticationService
    {
        Task<Person?> AuthenticateUser(string email, string password);
        Task<Person?> GetUserByEmail(string email);
    }
}
