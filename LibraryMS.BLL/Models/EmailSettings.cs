using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.BLL.Models
{
    public sealed class EmailSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "";
    }
}
