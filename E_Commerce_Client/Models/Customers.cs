using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Commerce_Client.Models
{
    public partial class Customers
    {
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public int StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string Suburb { get; set; }
        public string Province { get; set; }

        public virtual Users User { get; set; }
    }
}
