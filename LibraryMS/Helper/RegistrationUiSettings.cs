using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.Win.Helper
{
    public class RegistrationUiSettings
    {
        public Defaults Defaults { get; set; } = new();
        public PolicyPopup PolicyPopup { get; set; } = new();
    }

    public class Defaults
    {
        public int MaxBorrow { get; set; } = 3;
    }

    public class PolicyPopup
    {
        public string Title { get; set; } = "Policies";
        public bool RequireAgreement { get; set; } = true;
        public List<string> CommonLines { get; set; } = new();
        public Dictionary<string, List<string>> GroupLines { get; set; } = new();
        public List<string> TemplateLines { get; set; } = new();
    }
}
