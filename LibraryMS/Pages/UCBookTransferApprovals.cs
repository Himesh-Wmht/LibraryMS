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

        public UCBookTransferApprovals(BookTransferService svc)
        {
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            Dock = DockStyle.Fill;

            BuildUi();
            WireEvents();

            Load += async (_, __) => await OnRefreshAsync();
        }

        private string? LocCode => AppSession.Current?.LocationCode;
        private string? LocDesc => AppSession.Current?.LocationDesc;

        public async Task OnRefreshAsync()
        {
            if (AppSession.Current == null) { MessageBox.Show("Session missing."); return; }

            lblTitle.Text = _loadAll ? $"All Transfers - {LocDesc}" : $"Pending Transfers - {LocDesc}";
            await LoadHeadersAsync();
            dgvD.DataSource = null;
        }

        public void OnEdit() { }

        // Top SAVE -> Approve
        public Task OnSaveAsync() => ApproveSelectedAsync();

        // Top PROCESS -> Reject
        public Task OnProcessAsync() => RejectSelectedAsync();

        private TransferHeaderRowDto? Selected => dgvH.CurrentRow?.DataBoundItem as TransferHeaderRowDto;

        private async Task LoadHeadersAsync()
        {
            if (string.IsNullOrWhiteSpace(LocCode)) { dgvH.DataSource = null; return; }
            dgvH.DataSource = await _svc.GetPendingAsync(LocCode!, _loadAll);
        }

        private async Task LoadDetailsAsync()
        {
            if (Selected == null) { dgvD.DataSource = null; return; }
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

            var ok = MessageBox.Show($"Approve transfer?\n\nDoc: {Selected.DocNo}\nFrom: {Selected.FromLoc}\nTo: {Selected.ToLoc}",
                "Approve", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            try
            {
                await _svc.ApproveAsync(Selected.DocNo, AppSession.Current.UserCode);
                MessageBox.Show("Approved. Inventory added to destination.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

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

            var ok = MessageBox.Show($"Reject transfer?\n\nDoc: {Selected.DocNo}", "Reject",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (ok != DialogResult.Yes) return;

            await _svc.RejectAsync(Selected.DocNo, AppSession.Current.UserCode, "Rejected by receiver");
            MessageBox.Show("Rejected.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
            BackColor = Color.PapayaWhip;

            var gb = new GroupBox { Dock = DockStyle.Fill, Padding = new Padding(10), Text = "Transfer Approval Handling" };
            Controls.Add(gb);

            var tlpRoot = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            gb.Controls.Add(tlpRoot);

            var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(6, 10, 6, 0) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Text = "Pending Transfers";

            var flp = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Dock = DockStyle.Fill, Margin = new Padding(0) };
            SetupBtn(btnRefresh, "Refresh", Color.SteelBlue);
            SetupBtn(btnLoadAll, "Load All", Color.DimGray);
            SetupBtn(btnApprove, "Approve", Color.SeaGreen);
            SetupBtn(btnReject, "Reject", Color.IndianRed);

            flp.Controls.Add(btnRefresh);
            flp.Controls.Add(btnLoadAll);
            flp.Controls.Add(btnApprove);
            flp.Controls.Add(btnReject);

            header.Controls.Add(lblTitle, 0, 0);
            header.Controls.Add(flp, 1, 0);

            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));

            SetupGrid(dgvH);
            SetupGrid(dgvD);

            body.Controls.Add(new GroupBox { Dock = DockStyle.Fill, Text = "Transfers", Controls = { dgvH } }, 0, 0);
            body.Controls.Add(new GroupBox { Dock = DockStyle.Fill, Text = "Transfer Details", Controls = { dgvD } }, 0, 1);

            tlpRoot.Controls.Add(header, 0, 0);
            tlpRoot.Controls.Add(body, 0, 1);
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
            b.Height = 30;
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Margin = new Padding(8, 0, 0, 0);
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.UseVisualStyleBackColor = false;
        }
    }
}