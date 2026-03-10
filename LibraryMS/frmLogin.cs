using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryMS
{
    public partial class frmLogin : Form, ILoginView
    {
        private bool _pwdVisible = false;
        private readonly LoginPresenter _presenter;
        private readonly System.Windows.Forms.Timer _typingTimer;
        private readonly LocationService _locationService;
        private readonly AuthService _authService;

        public frmLogin(LocationService locationService, AuthService authService)
        {
            InitializeComponent();

            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            Opacity = 1.0;
            TransparencyKey = Color.Empty;

            txtUserName.Font = new Font("Segoe UI", 12f);
            txtPassword.Font = new Font("Segoe UI", 12f);

            txtPassword.UseSystemPasswordChar = true;

            pictureBox2.BackColor = txtPassword.BackColor;
            pictureBox2.BringToFront();
            pictureBox2.Cursor = Cursors.Hand;

            cmbLoginlocs.DropDownStyle = ComboBoxStyle.DropDownList;

            AcceptButton = btnLogin;

            _presenter = new LoginPresenter(this, locationService);

            _typingTimer = new System.Windows.Forms.Timer
            {
                Interval = 250
            };

            _typingTimer.Tick += async (_, __) =>
            {
                _typingTimer.Stop();
                await _presenter.LoadLocationsAsync();
            };

            BindLocations(new List<LocationItem>());
        }

        public string UserCode => txtUserName.Text.Trim();

        public void SetLocationLoading(bool isLoading)
        {
            cmbLoginlocs.Enabled = !isLoading;
            cmbLoginlocs.Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
            UseWaitCursor = isLoading;
        }

        public void BindLocations(List<LocationItem> locations)
        {
            var list = new List<LocationItem>
            {
                new LocationItem { Code = "", Desc = "-- Select Location --" }
            };

            if (locations != null && locations.Count > 0)
                list.AddRange(locations);

            cmbLoginlocs.DataSource = null;
            cmbLoginlocs.DataSource = list;
            cmbLoginlocs.DisplayMember = "Desc";
            cmbLoginlocs.ValueMember = "Code";
            cmbLoginlocs.SelectedIndex = 0;
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private LocationItem? SelectedLocation =>
            cmbLoginlocs.SelectedItem as LocationItem;

        private async void button1_Click(object sender, EventArgs e)
        {
            var user = txtUserName.Text.Trim();
            var pass = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                ShowError("Enter Username and Password.");
                return;
            }

            if (SelectedLocation == null || string.IsNullOrWhiteSpace(SelectedLocation.Code))
            {
                ShowError("Select a Location.");
                return;
            }

            btnLogin.Enabled = false;

            try
            {
                var (result, session, msg) = await _authService.LoginAsync(user, pass);

                if (result != AuthResult.LoginGranted || session == null)
                {
                    if (result == AuthResult.AccountLocked)
                    {
                        ShowError(msg);
                        txtPassword.Clear();
                        txtUserName.Focus();
                        return;
                    }

                    ShowError(msg);
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                bool hasLoc = await _locationService.UserHasLocationAsync(user, SelectedLocation.Code);
                if (!hasLoc)
                {
                    ShowError("You do not have permission to login to this location.");
                    return;
                }

                session.LocationCode = SelectedLocation.Code;
                session.LocationDesc = SelectedLocation.Desc;

                AppSession.Current = session;

                var pop = await _authService.GetMembershipPopupTextAsync(user);
                if (!string.IsNullOrWhiteSpace(pop))
                {
                    MessageBox.Show(pop, "Membership", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                btnLogin.Enabled = true;
                UseWaitCursor = false;
            }
        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {
            _typingTimer.Stop();

            if (string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                BindLocations(new List<LocationItem>());
                return;
            }

            _typingTimer.Start();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            int pos = txtPassword.SelectionStart;

            _pwdVisible = !_pwdVisible;
            txtPassword.UseSystemPasswordChar = !_pwdVisible;

            txtPassword.Focus();
            txtPassword.SelectionStart = pos;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Optional
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_typingTimer != null)
            {
                _typingTimer.Stop();
                _typingTimer.Dispose();
            }

            base.OnFormClosed(e);
        }
    }
}