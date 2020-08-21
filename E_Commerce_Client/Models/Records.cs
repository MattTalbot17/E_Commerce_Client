using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Commerce_Client.Models
{
    public partial class Records
    {
        int totalNumber;
        string category;

        public Records(int totalNumber, string category)
        {
            this.totalNumber = totalNumber;
            this.category = category;
        }

        public int TotalNumber { get => totalNumber; set => totalNumber = value; }
        public string Category { get => category; set => category = value; }
    }

}
