using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.BLL.Models
{
    public sealed class SubscriptionItem
    {
        public string Id { get; init; } = "";
        public string Desc { get; init; } = "";
        public int Days { get; init; }
    }

}
