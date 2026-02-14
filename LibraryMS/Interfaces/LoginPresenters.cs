using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.BLL.Services;

namespace LibraryMS.Win.Interfaces
{
    public class LoginPresenter
    {
        private readonly ILoginView _view;
        private readonly LocationService _locationService;
        private int _requestId = 0;

        public LoginPresenter(ILoginView view, LocationService locationService)
        {
            _view = view;
            _locationService = locationService;
        }

        public async Task LoadLocationsAsync()
        {
            var user = _view.UserCode?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(user) || user.Length < 2)
            {
                _view.BindLocations(new());
                return;
            }

            var myReq = ++_requestId;


            try
            {
                _view.SetLocationLoading(true);
                var locations = await _locationService.GetUserLocationsAsync(user);

                if (myReq != _requestId) return;
                _view.BindLocations(locations);
            }
            catch (Exception ex)
            {
                if (myReq != _requestId) return;
                _view.BindLocations(new());
                _view.ShowError($"Failed to load locations: {ex.Message}");
            }
            finally
            {
                if (myReq == _requestId)
                    _view.SetLocationLoading(false);
            }
        }
    }
}
