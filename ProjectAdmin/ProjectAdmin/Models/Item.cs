using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectAdmin.Models
{
    public class Item
    {
        public Product product { get; set; }
        public int Quantity { get; set; }
    }
}