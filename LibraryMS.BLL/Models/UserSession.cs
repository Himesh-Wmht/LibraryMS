using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.BLL.Models
{
    public class UserSession
    {
        public string UserCode { get; init; } = "";
        public string UserName { get; init; } = "";
        public string GroupCode { get; init; } = "";

        public string LocationCode { get; set; } = "";
        public string LocationDesc { get; set; } = "";
    }
    public sealed record LoginUserDb(string UserCode, string UserName, string GroupCode, bool Active, string PasswordStored, int FailCount, bool Locked);
}
