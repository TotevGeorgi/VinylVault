using System;
using System.Collections.Generic;

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
