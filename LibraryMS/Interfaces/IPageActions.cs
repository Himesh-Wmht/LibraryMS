using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.Win.Interfaces
{
    public interface IPageActions
    {
        void OnEdit();
        Task OnSaveAsync();
        Task OnProcessAsync();
    }
}
