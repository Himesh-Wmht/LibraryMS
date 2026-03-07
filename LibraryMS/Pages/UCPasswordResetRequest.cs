using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;

namespace LibraryMS.Win.Pages
{
    public partial class UCPasswordResetRequest : UserControl
    {
        private readonly PasswordResetService _service;
         
        public UCPasswordResetRequest(PasswordResetService service)
        {
            InitializeComponent();

            _service = service ?? throw new ArgumentNullException(nameof(service));
            Dock = DockStyle.Fill;

            // ✅ permanent UI fix
            EnsureUi();

            btnSubmit.Click += async (_, __) => await SubmitAsync();
            btnClear.Click += (_, __) => ClearForm();
            btnRefresh.Click += (_, __) => ClearForm();

            Load += (_, __) => EnsureUi(); // re-apply just in case
        }

        // ---------------- UI HARDENING ----------------

        private void EnsureUi()
        {
            EnsureButtons();
            EnsureFormLabels();
        }

        private void EnsureButtons()
        {
            btnRefresh.Text = "Refresh";
            btnClear.Text = "Clear";
            btnSubmit.Text = "Submit";

            StyleBtn(btnRefresh, Color.SteelBlue);
            StyleBtn(btnClear, Color.DimGray);
            StyleBtn(btnSubmit, Color.SeaGreen);
        }

        private static void StyleBtn(Button b, Color back)
        {
            b.Width = 100;
            b.Height = 30;
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.UseVisualStyleBackColor = false;
        }

        private void EnsureFormLabels()
        {
            // If labels already exist at col 0 row 0 => ok
            var c00 = tblForm.GetControlFromPosition(0, 0);
            if (c00 is Label) return;

            tblForm.SuspendLayout();
            tblForm.Controls.Clear();

            AddRow(0, "User Code", txtUserCode);

            txtNewPassword.UseSystemPasswordChar = true;
            AddRow(1, "New Password", txtNewPassword);

            txtConfirmPassword.UseSystemPasswordChar = true;
            AddRow(2, "Confirm Password", txtConfirmPassword);

            tblForm.ResumeLayout();
        }

        private void AddRow(int row, string labelText, Control input)
        {
            var lbl = new Label
            {
                Text = labelText,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Black,
                Padding = new Padding(0, 8, 0, 0),
                Dock = DockStyle.Fill
            };

            input.Dock = DockStyle.Fill;

            tblForm.Controls.Add(lbl, 0, row);
            tblForm.Controls.Add(input, 1, row);
        }

        // ---------------- ACTIONS ----------------

        private async Task SubmitAsync()
        {
            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing. Please login again.");
                return;
            }

            var user = txtUserCode.Text.Trim();
            var pwd1 = txtNewPassword.Text;
            var pwd2 = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(user))
            {
                MessageBox.Show("User Code is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(pwd1) || pwd1.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (pwd1 != pwd2)
            {
                MessageBox.Show("Password and Confirm Password do not match.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // ✅ request table stores plaintext (your requirement)
                await _service.CreateRequestAsync(user, pwd1, AppSession.Current.UserCode);

                MessageBox.Show("Password reset request submitted.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            txtUserCode.Clear();
            txtNewPassword.Clear();
            txtConfirmPassword.Clear();
            txtUserCode.Focus();
        }
    }
}