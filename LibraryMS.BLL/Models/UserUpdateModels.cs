using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.BLL.Models
{
    public class UserUpdateModels
    {
        public sealed record UserSearchItem(string Code, string Name)
        {
            public string Display => $"{Code} - {Name}";
            public override string ToString() => Display;
        }

        public sealed record UserEditModel(
            string Code,
            string Name,
            string Mobile,
            string GroupCode,
            bool Active,
            bool MemberStatus,
            bool SubscriptionStatus,
            string? SubscriptionId,
            int? SubscriptionDays,
            string? Email,
            string? Nic,
            string? Address,
            System.DateTime? Dob,
            System.DateTime? RegisteredDate,
            System.DateTime? ExpiredDate,
            string? Gender,
            int MaxBorrow,
            bool AllLocations,
            string? LocationCode
        );

        public sealed class UserUpdateRequest
        {
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string Mobile { get; set; } = "";
            public string GroupCode { get; set; } = "";

            public bool Active { get; set; }
            public bool MemberStatus { get; set; }
            public bool SubscriptionStatus { get; set; }

            public string? SubscriptionId { get; set; }
            public int? SubscriptionDays { get; set; }

            public string? Email { get; set; }
            public string? Nic { get; set; }
            public string? Address { get; set; }
            public System.DateTime? Dob { get; set; }
            public string? Gender { get; set; }

            public int MaxBorrow { get; set; }

            public bool AllLocations { get; set; }
            public string? LocationCode { get; set; }

            // Optional: only update password if provided
            public string? NewPassword { get; set; }
        }
    }
}
