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



        // public string UserCode => throw new NotImplementedException();

        public frmLogin(LocationService locationService, AuthService authService)
        {
            InitializeComponent();

            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            this.Opacity = 1.0;
            this.TransparencyKey = Color.Empty;

            // Font size you want
            txtPassword.Font = new Font("Segoe UI", 14f);
            txtUserName.Font = new Font("Segoe UI", 14f);

            // Stop WinForms from auto-snapping the height to the font
            txtPassword.AutoSize = false;
            txtUserName.AutoSize = false;

            // Now you can keep a nice tall box
            txtPassword.Height = 30;
            txtUserName.Height = 30;

            // Password as *
            txtPassword.UseSystemPasswordChar = true; // or: textBoxPassword.PasswordChar = '*';

            pictureBox2.BackColor = txtPassword.BackColor;   // usually white (SystemColors.Window)
            pictureBox2.BringToFront();
            pictureBox2.Cursor = Cursors.Hand; 

            // ---- NEW: presenter + debounce timer
            _presenter = new LoginPresenter(this, locationService);

            _typingTimer = new System.Windows.Forms.Timer { Interval = 200 };
            _typingTimer.Tick += async (_, __) =>
            {
                _typingTimer.Stop();
                await _presenter.LoadLocationsAsync();
            };

            txtUserName.TextChanged += (_, __) =>
            {
                _typingTimer.Stop();
                _typingTimer.Start();

                if (string.IsNullOrWhiteSpace(txtUserName.Text))
                    BindLocations(new()); // clear combo quickly
            };
            cmbLoginlocs.DropDownStyle = ComboBoxStyle.DropDownList;
            // initial combo state
            BindLocations(new());
        }
        public string UserCode => txtUserName.Text;

        public void SetLocationLoading(bool isLoading)
        {
            cmbLoginlocs.Enabled = !isLoading;
            cmbLoginlocs.Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
        }
        // ILoginView implementation

        public void BindLocations(List<LocationItem> locations)
        {
            cmbLoginlocs.DropDownStyle = ComboBoxStyle.DropDownList;

            var list = new List<LocationItem>
            {
                new LocationItem { Code = "", Desc = "-- Select Location --" }
            };
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

        //private async void button1_Click(object sender, EventArgs e)
        //{
        //    var user = txtUserName.Text.Trim();
        //    var pass = txtPassword.Text;

        //    if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        //    {
        //        ShowError("Enter Username and Password.");
        //        return;
        //    }

        //    if (SelectedLocation == null || string.IsNullOrWhiteSpace(SelectedLocation.Code))
        //    {
        //        ShowError("Select a Location.");
        //        return;
        //    }

        //    // 1) Validate user/password (hash verify)
        //    var (result, session, msg) = await _authService.LoginAsync(user, pass);
        //    if (result != AuthResult.LoginGranted || session == null)
        //    {
        //        ShowError(msg);
        //        txtPassword.Clear();
        //        txtPassword.Focus();
        //        return;
        //    }


        //    // 2) Validate user has permission to login selected location
        //    bool hasLoc = await _locationService.UserHasLocationAsync(user, SelectedLocation.Code);
        //    if (!hasLoc)
        //    {
        //        ShowError("You do not have permission to login to this location.");
        //        return;
        //    }

        //    // 3) Set session + continue
        //    session.LocationCode = SelectedLocation.Code;
        //    session.LocationDesc = SelectedLocation.Desc;

        //    AppSession.Current = session;

        //    this.DialogResult = DialogResult.OK;
        //    this.Close();
        //}
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

            // Disable button to prevent double-click while awaiting
            btnLogin.Enabled = false;
            try
            {
                // 1) Validate user/password (includes lock logic)
                var (result, session, msg) = await _authService.LoginAsync(user, pass);

                if (result != AuthResult.LoginGranted || session == null)
                {
                    // Special handling for locked accounts
                    if (result == AuthResult.AccountLocked)
                    {
                        ShowError(msg);              // e.g. "Account locked after 4 failed attempts..."
                        txtPassword.Clear();
                        txtUserName.Focus();
                        return;
                    }

                    // Normal invalid cases
                    ShowError(msg);
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                // 2) Validate user has permission to login selected location
                bool hasLoc = await _locationService.UserHasLocationAsync(user, SelectedLocation.Code);
                if (!hasLoc)
                {
                    ShowError("You do not have permission to login to this location.");
                    return;
                }

                // 3) Set session + continue
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
            }
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnTogglePwd_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            int pos = txtPassword.SelectionStart;

            _pwdVisible = !_pwdVisible;
            txtPassword.UseSystemPasswordChar = !_pwdVisible;

            txtPassword.Focus();
            txtPassword.SelectionStart = pos;
        }
    }
}
