using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface IRegistrationService
    {
        Task<Guid?> RegisterUser(Person person);
        Task<bool> EmailExists(string email);
    }
}
