using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Admin : Person
    {
        public void DeleteUser(User user)
        {
            Console.WriteLine($"User {user.Email} deleted.");
        }

        public void RemoveItem(string item)
        {
            Console.WriteLine($"Item {item} removed from the marketplace.");
        }
    }
}
