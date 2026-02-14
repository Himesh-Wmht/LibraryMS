using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.BLL.Models
{
    public class MenuNode
    {
        public string Code { get; init; } = "";
        public string Desc { get; init; } = "";
        public string? Parent { get; init; }
        public string? Child { get; init; }
    }

}
