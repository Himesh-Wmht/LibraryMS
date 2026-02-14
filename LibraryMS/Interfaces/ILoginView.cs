using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.BLL.Models;

namespace LibraryMS.Win.Interfaces
{
    public interface ILoginView
    {
        string UserCode { get; }
        void SetLocationLoading(bool isLoading);
        void BindLocations(List<LocationItem> locations);
        void ShowError(string message);
    }
}
