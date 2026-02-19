using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.BLL.Models
{
    public sealed class UserGroupItem
    {
        public string Code { get; init; } = "";
        public string Name { get; init; } = "";
        public decimal MembershipFee { get; set; }
    }
}
