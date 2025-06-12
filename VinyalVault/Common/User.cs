using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class User : Person
    {
        public List<string> PurchaseHistory { get; set; } = new List<string>();

        public void PurchaseItem(string item)
        {
            PurchaseHistory.Add(item);
        }
    }
}
