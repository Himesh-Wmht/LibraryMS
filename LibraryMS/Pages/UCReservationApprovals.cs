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
    public partial class UCReservationApprovals : UserControl, IPageActions
    {
        private readonly BookReservationService _service;
        private bool _loadAll;

        private readonly Label lblTitle = new();
        private readonly Button btnRefresh = new();
        private readonly Button btnLoadAll = new();
        private readonly Button btnApprove = new();
        private readonly Button btnReject = new();
        private readonly Button btnCancel = new();
        private readonly DataGridView dgv = new();
        public UCReservationApprovals(BookReservationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
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
            lblTitle.Text = _loadAll ? $"All Reservation Requests - {LocDesc}" : $"Pending Reservation Requests - {LocDesc}";
            await LoadGridAsync();
        }

        public void OnEdit() { }

        // Map top SAVE to Approve
        public Task OnSaveAsync() => ApproveSelectedAsync();

        // Map top PROCESS to Reject

        public Task OnProcessAsync() => ProcessSelectedAsync();

        private async Task LoadGridAsync()
        {
            if (string.IsNullOrWhiteSpace(LocCode)) { dgv.DataSource = null; return; }
            dgv.DataSource = await _service.GetPendingAsync(LocCode!, _loadAll);
        }
     
        private ResPendingRowDto? Selected => dgv.CurrentRow?.DataBoundItem as ResPendingRowDto;
      
        private async Task ApproveSelectedAsync()
        {
            if (Selected == null) return;
            if (AppSession.Current == null) return;

            if (Selected.Status != "P")
            {
                MessageBox.Show("Only PENDING requests can be approved.");
                return;
            }

            var ok = MessageBox.Show(
                $"Approve reservation?\n\n{Selected.UserCode} - {Selected.UserName}\n{Selected.BookCode} - {Selected.Title}",
                "Approve",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            try
            {
                await _service.ApproveAsync(Selected.ResId, AppSession.Current.UserCode);
                MessageBox.Show("Approved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadGridAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // ✅ Reject (Admin)
        private async Task RejectSelectedAsync()
        {
            if (Selected == null) return;
            if (AppSession.Current == null) return;

            if (Selected.Status != "P")
            {
                MessageBox.Show("Only PENDING requests can be rejected.");
                return;
            }

            var ok = MessageBox.Show(
                $"Reject reservation?\n\n{Selected.UserCode} - {Selected.UserName}\n{Selected.BookCode} - {Selected.Title}",
                "Reject",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (ok != DialogResult.Yes) return;

            try
            {
                await _service.RejectAsync(Selected.ResId, AppSession.Current.UserCode, remark: "Rejected by admin");
                MessageBox.Show("Rejected.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadGridAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // ✅ Process/Issue (reduce inventory) - only for Approved
        private async Task ProcessSelectedAsync()
        {
            if (Selected == null) return;

            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing. Please login again.");
                return;
            }

            if (Selected.Status != "A")
            {
                MessageBox.Show("Only APPROVED reservations can be processed.");
                return;
            }

            var ok = MessageBox.Show(
                $"Process (Issue) this reservation and reduce inventory?\n\n" +
                $"{Selected.UserCode} - {Selected.UserName}\n" +
                $"{Selected.BookCode} - {Selected.Title}\nQty: {Selected.Qty}",
                "Process Reservation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            try
            {
                await _service.ProcessAsync(Selected.ResId, AppSession.Current.UserCode);
                MessageBox.Show("Processed. Inventory reduced.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadGridAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // ✅ Cancel (Admin)
        private async Task CancelSelectedAsync()
        {
            if (Selected == null) return;

            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing. Please login again.");
                return;
            }

            if (Selected.Status != "P" && Selected.Status != "A")
            {
                MessageBox.Show("Only Pending/Approved requests can be cancelled.");
                return;
            }

            var ok = MessageBox.Show(
                $"Cancel this reservation?\n\n{Selected.UserCode} - {Selected.UserName}\n{Selected.BookCode} - {Selected.Title}",
                "Cancel Reservation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (ok != DialogResult.Yes) return;

            try
            {
                var rows = await _service.CancelByAdminAsync(
                    Selected.ResId,
                    AppSession.Current.UserCode,
                    "Cancelled by admin"
                );

                if (rows == 0)
                {
                    MessageBox.Show("Cancel failed (already processed).");
                    return;
                }

                MessageBox.Show("Cancelled.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadGridAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //private async Task RejectSelectedAsync()
        //{
        //    if (Selected == null) return;
        //    if (AppSession.Current == null) return;

        //    var ok = MessageBox.Show($"Reject reservation?\n\n{Selected.UserCode} - {Selected.UserName}\n{Selected.BookCode} - {Selected.Title}",
        //        "Reject", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        //    if (ok != DialogResult.Yes) return;

        //    await _service.RejectAsync(Selected.ResId, AppSession.Current.UserCode, remark: null);
        //    MessageBox.Show("Rejected.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    await LoadGridAsync();
        //}

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
            btnCancel.Click += async (_, __) => await CancelSelectedAsync();
        }

        private void BuildUi()
        {
            BackColor = Color.PapayaWhip;

            var gb = new GroupBox { Dock = DockStyle.Fill, Padding = new Padding(10), Text = "Reservation Approvals" };
            Controls.Add(gb);

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            gb.Controls.Add(tlp);

            var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(6, 10, 6, 0) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Text = "Pending Reservation Requests";

            var flp = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Dock = DockStyle.Fill, Margin = new Padding(0) };

            SetupBtn(btnRefresh, "Refresh", Color.SteelBlue);
            SetupBtn(btnLoadAll, "Load All", Color.DimGray);
            SetupBtn(btnApprove, "Approve", Color.SeaGreen);
            SetupBtn(btnReject, "Reject", Color.IndianRed);
            SetupBtn(btnCancel, "Cancel", Color.DarkOrange);


            flp.Controls.Add(btnRefresh);
            flp.Controls.Add(btnLoadAll);
            flp.Controls.Add(btnApprove);
            flp.Controls.Add(btnReject);
            flp.Controls.Add(btnCancel);

            header.Controls.Add(lblTitle, 0, 0);
            header.Controls.Add(flp, 1, 0);

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

            tlp.Controls.Add(header, 0, 0);
            tlp.Controls.Add(dgv, 0, 1);
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