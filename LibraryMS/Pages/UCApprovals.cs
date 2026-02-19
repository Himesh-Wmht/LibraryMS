using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win.Pages
{
    public partial class UCApprovals : UserControl
    {
        private readonly ApprovalService _service;
        private bool _loadAll;

        public UCApprovals(ApprovalService service)
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
            lblTitle.Text = _loadAll ? "All Registration Approvals" : "Pending Registration Approvals";

            var list = _loadAll
                ? await _service.GetAllAsync()
                : await _service.GetPendingAsync();

            dgvPending.DataSource = list;
        }

        private ApprovalRowDto? Selected =>
            dgvPending.CurrentRow?.DataBoundItem as ApprovalRowDto;

        private async Task ApproveSelectedAsync()
        {
            if (Selected == null) return;

            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing. Please login again.");
                return;
            }

            var ok = MessageBox.Show(
                $"Approve this user?\n\n{Selected.UserCode} - {Selected.Name}",
                "Approve",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            var (success, message) = await _service.ApproveAsync(Selected.ApId, AppSession.Current.UserCode);

            MessageBox.Show(message, success ? "Success" : "Error",
                MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (success) await LoadGridAsync();
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
                $"Reject this user?\n\n{Selected.UserCode} - {Selected.Name}",
                "Reject",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (ok != DialogResult.Yes) return;

            var (success, message) = await _service.RejectAsync(Selected.ApId, AppSession.Current.UserCode);

            MessageBox.Show(message, success ? "Success" : "Error",
                MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (success) await LoadGridAsync();
        }
    }
}
