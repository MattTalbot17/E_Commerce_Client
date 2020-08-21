using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Commerce_Client.Models
{
    public class LogSingleton
    {
        public int LogId { get; set; }
        public string LogType { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime LogDate { get; set; }

        public virtual Product Product { get; set; }
        public virtual Users User { get; set; }
    }
}
