using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.BLL.Models
{
    public static class AppSession
    {
        public static UserSession? Current { get; set; }
    }
}
