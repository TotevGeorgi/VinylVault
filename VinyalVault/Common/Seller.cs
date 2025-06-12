using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Seller : User
    {
        public List<string> ItemsForSale { get; set; } = new List<string>();

        public void PlaceItemForSale(string item)
        {
            ItemsForSale.Add(item);
        }

        public void RemoveItemFromSale(string item)
        {
            ItemsForSale.Remove(item);
        }
    }
}
