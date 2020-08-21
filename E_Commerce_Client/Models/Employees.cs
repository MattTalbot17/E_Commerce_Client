using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Commerce_Client.Models
{
    public class Employees
    {
        public int EmployeeId { get; set; }
        public int UserId { get; set; }
        public string EmployeePosition { get; set; }

        public virtual Users User { get; set; }
    }
}
