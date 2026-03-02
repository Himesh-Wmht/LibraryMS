using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win.Pages
{
    public partial class UCUserUnlockApprovals : UserControl
    {
        private readonly UserLockService _service;

        public UCUserUnlockApprovals(UserLockService service)
        {
            InitializeComponent();

            _service = service ?? throw new ArgumentNullException(nameof(service));
            Dock = DockStyle.Fill;

            dgvPending.ReadOnly = true;
            dgvPending.AllowUserToAddRows = false;
            dgvPending.AllowUserToDeleteRows = false;
            dgvPending.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPending.MultiSelect = false;
            dgvPending.AutoGenerateColumns = true;

            Load += async (_, __) => await LoadGridAsync();

            btnRefresh.Click += async (_, __) => await LoadGridAsync();
            btnUnlock.Click += async (_, __) => await UnlockSelectedAsync();
        }

        private async Task LoadGridAsync()
        {
            lblTitle.Text = "Pending User Unlock Requests";
            dgvPending.DataSource = await _service.GetPendingAsync();
        }

        private UserLockRowDto? Selected =>
            dgvPending.CurrentRow?.DataBoundItem as UserLockRowDto;

        private async Task UnlockSelectedAsync()
        {
            if (Selected == null) return;

            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing. Please login again.");
                return;
            }

            var ok = MessageBox.Show(
                $"Unlock this user?\n\n{Selected.UserCode}",
                "Unlock",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            try
            {
                await _service.UnlockAsync(Selected.UlId, AppSession.Current.UserCode);

                MessageBox.Show("User unlocked successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadGridAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}