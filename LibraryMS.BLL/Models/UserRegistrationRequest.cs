using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.BLL.Models
{
    public sealed class UserRegistrationRequest
    {
        public string Code { get; init; } = "";
        public string Name { get; init; } = "";
        public string Mobile { get; init; } = "";
        public string Uid { get; init; } = "";

        public bool Active { get; init; }
        public string GroupCode { get; init; } = "";

        public bool MemberStatus { get; init; }
        public bool SubscriptionStatus { get; init; }

        public string Password { get; init; } = "";

        public DateTime? Dob { get; init; }
        public string? Address { get; init; }
        public string? Email { get; init; }
        public string? Nic { get; init; }
        public string? Gender { get; init; }

        public string? SubscriptionId { get; init; }     // store SUB_ID
        public int? SubscriptionDays { get; init; }

        public int MaxBorrow { get; init; }

        public bool AllLocations { get; init; }
        public string? LocationCode { get; init; }
        public decimal MembershipFee { get; set; }        // from group table
        public decimal PaidAmt { get; set; }              // from popup
        public string? PaymentMethod { get; set; }
        public string? ReferenceNo { get; set; }

    }
}
