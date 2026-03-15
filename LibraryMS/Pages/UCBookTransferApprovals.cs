using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Interfaces;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win.Pages
{
    public sealed partial class UCBookTransferApprovals : UserControl, IPageActions
    {
        private readonly BookTransferService _svc;
        private bool _loadAll;

        private readonly Label lblTitle = new();
        private readonly Button btnRefresh = new();
        private readonly Button btnLoadAll = new();
        private readonly Button btnApprove = new();
        private readonly Button btnReject = new();

        private readonly DataGridView dgvH = new();
        private readonly DataGridView dgvD = new();

        // responsive layout fields
        private readonly GroupBox gbMain = new();
        private readonly TableLayoutPanel tlpRoot = new();
        private readonly TableLayoutPanel tlpHeader = new();
        private readonly FlowLayoutPanel flpActions = new();
        private readonly TableLayoutPanel tlpBody = new();
        private readonly GroupBox grpTransfers = new();
        private readonly GroupBox grpDetails = new();

        public UCBookTransferApprovals(BookTransferService svc)
        {
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            Dock = DockStyle.Fill;

            BuildUi();
            WireEvents();
            ApplyResponsiveLayout();

            Resize += (_, __) => ApplyResponsiveLayout();
            Load += async (_, __) => await OnRefreshAsync();
        }

        private string? LocCode => AppSession.Current?.LocationCode;
        private string? LocDesc => AppSession.Current?.LocationDesc;

        public async Task OnRefreshAsync()
        {
            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing.");
                return;
            }

            lblTitle.Text = _loadAll ? $"All Transfers - {LocDesc}" : $"Pending Transfers - {LocDesc}";
            await LoadHeadersAsync();
            dgvD.DataSource = null;
        }

        public void OnEdit() { }

        public Task OnSaveAsync() => ApproveSelectedAsync();

        public Task OnProcessAsync() => RejectSelectedAsync();

        private TransferHeaderRowDto? Selected => dgvH.CurrentRow?.DataBoundItem as TransferHeaderRowDto;

        private async Task LoadHeadersAsync()
        {
            if (string.IsNullOrWhiteSpace(LocCode))
            {
                dgvH.DataSource = null;
                return;
            }

            dgvH.DataSource = await _svc.GetPendingAsync(LocCode!, _loadAll);
        }

        private async Task LoadDetailsAsync()
        {
            if (Selected == null)
            {
                dgvD.DataSource = null;
                return;
            }

            dgvD.DataSource = await _svc.GetDetailsAsync(Selected.DocNo);
        }

        private async Task ApproveSelectedAsync()
        {
            if (Selected == null) return;
            if (AppSession.Current == null) return;

            if (Selected.Status != "P")
            {
                MessageBox.Show("Only PENDING transfers can be approved.");
                return;
            }

            var ok = MessageBox.Show(
                $"Approve transfer?\n\nDoc: {Selected.DocNo}\nFrom: {Selected.FromLoc}\nTo: {Selected.ToLoc}",
                "Approve",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            try
            {
                await _svc.ApproveAsync(Selected.DocNo, AppSession.Current.UserCode);

                MessageBox.Show(
                    "Approved. Inventory added to destination.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await LoadHeadersAsync();
                dgvD.DataSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RejectSelectedAsync()
        {
            if (Selected == null) return;
            if (AppSession.Current == null) return;

            if (Selected.Status != "P")
            {
                MessageBox.Show("Only PENDING transfers can be rejected.");
                return;
            }

            var ok = MessageBox.Show(
                $"Reject transfer?\n\nDoc: {Selected.DocNo}",
                "Reject",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (ok != DialogResult.Yes) return;

            await _svc.RejectAsync(Selected.DocNo, AppSession.Current.UserCode, "Rejected by receiver");

            MessageBox.Show(
                "Rejected.",
                "Done",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadHeadersAsync();
            dgvD.DataSource = null;
        }

        private void WireEvents()
        {
            btnRefresh.Click += async (_, __) => await OnRefreshAsync();

            btnLoadAll.Click += async (_, __) =>
            {
                _loadAll = !_loadAll;
                btnLoadAll.Text = _loadAll ? "Load Pending" : "Load All";
                await OnRefreshAsync();
            };

            btnApprove.Click += async (_, __) => await ApproveSelectedAsync();
            btnReject.Click += async (_, __) => await RejectSelectedAsync();

            dgvH.SelectionChanged += async (_, __) => await LoadDetailsAsync();
        }

        private void BuildUi()
        {
            SuspendLayout();

            BackColor = Color.PapayaWhip;
            Controls.Clear();

            gbMain.Dock = DockStyle.Fill;
            gbMain.Padding = new Padding(10);
            gbMain.Text = "Transfer Approval Handling";
            Controls.Add(gbMain);

            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.ColumnCount = 1;
            tlpRoot.RowCount = 2;
            tlpRoot.Margin = new Padding(0);
            tlpRoot.Padding = new Padding(0);
            tlpRoot.ColumnStyles.Clear();
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.RowStyles.Clear();
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            gbMain.Controls.Add(tlpRoot);

            // header
            tlpHeader.Dock = DockStyle.Top;
            tlpHeader.AutoSize = true;
            tlpHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpHeader.Margin = new Padding(0);
            tlpHeader.Padding = new Padding(6, 10, 6, 6);

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Text = "Pending Transfers";

            flpActions.AutoSize = true;
            flpActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpActions.FlowDirection = FlowDirection.LeftToRight;
            flpActions.WrapContents = true;
            flpActions.Dock = DockStyle.Fill;
            flpActions.Margin = new Padding(0);
            flpActions.Padding = new Padding(0);

            SetupBtn(btnRefresh, "Refresh", Color.SteelBlue);
            SetupBtn(btnLoadAll, "Load All", Color.DimGray);
            SetupBtn(btnApprove, "Approve", Color.SeaGreen);
            SetupBtn(btnReject, "Reject", Color.IndianRed);

            flpActions.Controls.Add(btnRefresh);
            flpActions.Controls.Add(btnLoadAll);
            flpActions.Controls.Add(btnApprove);
            flpActions.Controls.Add(btnReject);

            tlpRoot.Controls.Add(tlpHeader, 0, 0);

            // body
            tlpBody.Dock = DockStyle.Fill;
            tlpBody.Margin = new Padding(0);
            tlpBody.Padding = new Padding(0);
            tlpRoot.Controls.Add(tlpBody, 0, 1);

            SetupGrid(dgvH);
            SetupGrid(dgvD);

            grpTransfers.Dock = DockStyle.Fill;
            grpTransfers.Text = "Transfers";
            grpTransfers.Padding = new Padding(6);
            grpTransfers.Margin = new Padding(3);
            grpTransfers.Controls.Add(dgvH);

            grpDetails.Dock = DockStyle.Fill;
            grpDetails.Text = "Transfer Details";
            grpDetails.Padding = new Padding(6);
            grpDetails.Margin = new Padding(3);
            grpDetails.Controls.Add(dgvD);

            ApplyResponsiveLayout();

            ResumeLayout(true);
        }

        private void ApplyResponsiveLayout()
        {
            if (IsDisposed)
                return;

            bool narrow = Width < 950;

            SuspendLayout();
            tlpHeader.SuspendLayout();
            tlpBody.SuspendLayout();

            // header layout
            tlpHeader.Controls.Clear();
            tlpHeader.ColumnStyles.Clear();
            tlpHeader.RowStyles.Clear();

            if (narrow)
            {
                tlpHeader.ColumnCount = 1;
                tlpHeader.RowCount = 2;

                tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                lblTitle.Margin = new Padding(0, 0, 0, 6);
                flpActions.Dock = DockStyle.Top;
                flpActions.WrapContents = true;

                tlpHeader.Controls.Add(lblTitle, 0, 0);
                tlpHeader.Controls.Add(flpActions, 0, 1);
            }
            else
            {
                tlpHeader.ColumnCount = 2;
                tlpHeader.RowCount = 1;

                tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                tlpHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                lblTitle.Margin = new Padding(0);
                flpActions.Dock = DockStyle.Fill;
                flpActions.WrapContents = false;

                tlpHeader.Controls.Add(lblTitle, 0, 0);
                tlpHeader.Controls.Add(flpActions, 1, 0);
            }

            // body layout
            tlpBody.Controls.Clear();
            tlpBody.ColumnStyles.Clear();
            tlpBody.RowStyles.Clear();

            tlpBody.ColumnCount = 1;
            tlpBody.RowCount = 2;
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));

            tlpBody.Controls.Add(grpTransfers, 0, 0);
            tlpBody.Controls.Add(grpDetails, 0, 1);

            tlpBody.ResumeLayout(true);
            tlpHeader.ResumeLayout(true);
            ResumeLayout(true);
        }

        private static void SetupGrid(DataGridView dgv)
        {
            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoGenerateColumns = true;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private static void SetupBtn(Button b, string text, Color back)
        {
            b.Text = text;
            b.Width = 100;
            b.Height = 36;
            b.MinimumSize = new Size(95, 36);
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Margin = new Padding(8, 0, 0, 0);
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.TextAlign = ContentAlignment.MiddleCenter;
            b.UseVisualStyleBackColor = false;
        }
    }
}