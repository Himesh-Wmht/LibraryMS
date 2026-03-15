using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Services;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win.Pages
{
    public partial class UCSubscriptionRenewalApprovals : UserControl
    {
        private readonly SubscriptionRenewalApprovalService _service;

        public UCSubscriptionRenewalApprovals(SubscriptionRenewalApprovalService service)
        {
            InitializeComponent();

            _service = service ?? throw new ArgumentNullException(nameof(service));
            Dock = DockStyle.Fill;

            dgvPending.AllowUserToAddRows = false;
            dgvPending.AllowUserToDeleteRows = false;
            dgvPending.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPending.MultiSelect = false;
            dgvPending.AutoGenerateColumns = true;
            dgvPending.EditMode = DataGridViewEditMode.EditOnEnter;

            Load += async (_, __) => await LoadGridAsync();
            btnRefresh.Click += async (_, __) => await LoadGridAsync();
            btnApprove.Click += async (_, __) => await ApproveSelectedAsync();
            btnReject.Click += async (_, __) => await RejectSelectedAsync();
        }

        private SubscriptionRenewalApprovalRowDto? Selected =>
            dgvPending.CurrentRow?.DataBoundItem as SubscriptionRenewalApprovalRowDto;

        private string? CurrentRemark
        {
            get
            {
                if (dgvPending.CurrentRow == null)
                    return null;

                var value = dgvPending.CurrentRow.Cells["Remark"]?.Value?.ToString()?.Trim();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        private async Task LoadGridAsync()
        {
            lblTitle.Text = "Pending Subscription Renewal Approvals";
            dgvPending.DataSource = await _service.GetPendingAsync();

            foreach (DataGridViewColumn col in dgvPending.Columns)
                col.ReadOnly = true;

            if (dgvPending.Columns["IsProcessed"] != null)
                dgvPending.Columns["IsProcessed"].Visible = false;

            if (dgvPending.Columns["IsRejected"] != null)
                dgvPending.Columns["IsRejected"].Visible = false;

            if (dgvPending.Columns["Remark"] != null)
            {
                dgvPending.Columns["Remark"].ReadOnly = false;
                dgvPending.Columns["Remark"].HeaderText = "Remark";
                dgvPending.Columns["Remark"].FillWeight = 180;
            }

            if (dgvPending.Columns["SubId"] != null)
                dgvPending.Columns["SubId"].HeaderText = "Renewal ID";

            if (dgvPending.Columns["UserCode"] != null)
                dgvPending.Columns["UserCode"].HeaderText = "User Code";

            if (dgvPending.Columns["RenewalDate"] != null)
                dgvPending.Columns["RenewalDate"].HeaderText = "Renewal Date";

            if (dgvPending.Columns["ExpiryDate"] != null)
                dgvPending.Columns["ExpiryDate"].HeaderText = "Expiry Date";

            if (dgvPending.Columns["RenewalStatus"] != null)
                dgvPending.Columns["RenewalStatus"].HeaderText = "Status";

            if (dgvPending.Columns["PaidAmt"] != null)
                dgvPending.Columns["PaidAmt"].HeaderText = "Paid Amount";

            if (dgvPending.Columns["DueAmt"] != null)
                dgvPending.Columns["DueAmt"].HeaderText = "Due Amount";

            if (dgvPending.Columns["PayAmt"] != null)
                dgvPending.Columns["PayAmt"].HeaderText = "Pay Amount";

            if (dgvPending.Columns["MDate"] != null)
                dgvPending.Columns["MDate"].HeaderText = "Modified Date";
        }

        private async Task ApproveSelectedAsync()
        {
            if (Selected == null) return;

            dgvPending.EndEdit();
            var cm = BindingContext[dgvPending.DataSource] as CurrencyManager;
            cm?.EndCurrentEdit();

            var ok = MessageBox.Show(
                $"Approve this subscription renewal?\n\nUser: {Selected.UserCode}\nRenewal ID: {Selected.SubId}",
                "Approve",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            var (success, message) = await _service.ApproveAsync(
                Selected.SubId,
                CurrentRemark,
                null
            );

            MessageBox.Show(
                message,
                success ? "Success" : "Error",
                MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (success)
                await LoadGridAsync();
        }

        private async Task RejectSelectedAsync()
        {
            if (Selected == null) return;

            dgvPending.EndEdit();
            var cm = BindingContext[dgvPending.DataSource] as CurrencyManager;
            cm?.EndCurrentEdit();

            var ok = MessageBox.Show(
                $"Reject this subscription renewal?\n\nUser: {Selected.UserCode}\nRenewal ID: {Selected.SubId}",
                "Reject",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (ok != DialogResult.Yes) return;

            var (success, message) = await _service.RejectAsync(
                Selected.SubId,
                CurrentRemark
            );

            MessageBox.Show(
                message,
                success ? "Success" : "Error",
                MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (success)
                await LoadGridAsync();
        }
    }
}