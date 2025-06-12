using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Person
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
        public string Role { get; set; } = "User";

        public virtual void UpdateProfile(string fullName, string address)
        {
            this.FullName = fullName;
            this.Address = address;
        }
    }
}
