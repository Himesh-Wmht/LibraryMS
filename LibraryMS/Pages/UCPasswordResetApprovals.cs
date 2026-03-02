using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win.Pages
{
    public partial class UCPasswordResetApprovals : UserControl
    {
        private readonly PasswordResetService _service;
        private bool _loadAll;

        public UCPasswordResetApprovals(PasswordResetService service)
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

            btnLoadAll.Click += async (_, __) =>
            {
                _loadAll = !_loadAll;
                btnLoadAll.Text = _loadAll ? "Load Pending" : "Load All";
                await LoadGridAsync();
            };

            btnApprove.Click += async (_, __) => await ApproveSelectedAsync();
            btnReject.Click += async (_, __) => await RejectSelectedAsync();
        }

        private async Task LoadGridAsync()
        {
            lblTitle.Text = _loadAll ? "All Password Reset Requests" : "Pending Password Reset Requests";

            var list = _loadAll
                ? await _service.GetAllAsync()
                : await _service.GetPendingAsync();

            dgvPending.DataSource = list;
        }

        private PwdResetRowDto? Selected => dgvPending.CurrentRow?.DataBoundItem as PwdResetRowDto;

        private async Task ApproveSelectedAsync()
        {
            if (Selected == null) return;

            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing. Please login again.");
                return;
            }

            var ok = MessageBox.Show(
                $"Approve password reset?\n\n{Selected.UserCode} - {Selected.Name}",
                "Approve",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            try
            {
                // ✅ service hashes & updates M_TBLUSERS.U_PASSWORD
                await _service.ApproveAsync(Selected.PrId, AppSession.Current.UserCode);

                MessageBox.Show("Approved and password updated.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadGridAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RejectSelectedAsync()
        {
            if (Selected == null) return;

            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing. Please login again.");
                return;
            }

            var ok = MessageBox.Show(
                $"Reject password reset?\n\n{Selected.UserCode} - {Selected.Name}",
                "Reject",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (ok != DialogResult.Yes) return;

            try
            {
                await _service.RejectAsync(Selected.PrId, AppSession.Current.UserCode, remark: null);

                MessageBox.Show("Rejected.", "Done",
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