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
    public sealed class UCBookReservationRequest : UserControl, IPageActions
    {
        private readonly BookReservationService _res;
        private readonly UserLookupService _users;

        private readonly Label lblTitle = new();
        private readonly TextBox txtSearchBook = new() { Width = 220 };
        private readonly Button btnSearch = new();
        private readonly Button btnClear = new();
        private readonly Button btnCancel  = new();

        private readonly TextBox txtUserCode = new();
        private readonly Label lblUserName = new() { AutoSize = true };

        private readonly Label lblLoc = new() { AutoSize = true };

        private readonly DataGridView dgvBooks = new();
        private readonly DataGridView dgvMine = new();

        private readonly TextBox txtBookCode = new() { ReadOnly = true };
        private readonly TextBox txtBookTitle = new() { ReadOnly = true };
        private readonly NumericUpDown numQty = new() { Minimum = 1, Maximum = 1000, Value = 1 };
        private readonly NumericUpDown numHoldDays = new() { Minimum = 1, Maximum = 14, Value = 3 };
        private readonly TextBox txtRemark = new();

        private bool _loading;

        public UCBookReservationRequest(BookReservationService res, UserLookupService users)
        {
            _res = res ?? throw new ArgumentNullException(nameof(res));
            _users = users ?? throw new ArgumentNullException(nameof(users));
            Dock = DockStyle.Fill;

            BuildUi();
            WireEvents();

            Load += async (_, __) => await OnRefreshAsync();
        }

        private string? LocCode => AppSession.Current?.LocationCode;
        private string? LocDesc => AppSession.Current?.LocationDesc;

        public async Task OnRefreshAsync()
        {
            if (_loading) return;
            _loading = true;
            try
            {
                if (AppSession.Current == null)
                {
                    MessageBox.Show("Session missing. Please login again.");
                    return;
                }

                lblTitle.Text = $"Book Reservation - {LocDesc}";
                lblLoc.Text = LocDesc ?? "";

                // default to current user
                txtUserCode.Text = AppSession.Current.UserCode;
                lblUserName.Text = AppSession.Current.UserName ?? "";

                await LoadBooksAsync();
                await LoadMyAsync();
                ClearForm();
            }
            finally { _loading = false; }
        }

        public void OnEdit() { }

        public async Task OnSaveAsync()
        {
            if (AppSession.Current == null) { MessageBox.Show("Session missing."); return; }
            if (string.IsNullOrWhiteSpace(LocCode)) { MessageBox.Show("Location missing."); return; }
            if (string.IsNullOrWhiteSpace(txtUserCode.Text)) { MessageBox.Show("Select a User (F2)."); return; }
            if (string.IsNullOrWhiteSpace(txtBookCode.Text)) { MessageBox.Show("Select a Book."); return; }

            try
            {
                var dto = new ReservationRequestDto(
                    UserCode: txtUserCode.Text.Trim(),
                    LocCode: LocCode!,
                    BookCode: txtBookCode.Text.Trim(),
                    Qty: (int)numQty.Value,
                    HoldDays: (int)numHoldDays.Value,
                    Remark: string.IsNullOrWhiteSpace(txtRemark.Text) ? null : txtRemark.Text.Trim()
                );

                await _res.CreateRequestAsync(dto);

                MessageBox.Show("Reservation request submitted for approval.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadMyAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // PROCESS = Cancel selected reservation
        // ✅ TOP toolbar PROCESS should NOT cancel anymore (Cancel is now a page button)
        public Task OnProcessAsync()
        {
            MessageBox.Show("Use Save button to Save or Cancel button to cancel a reservation.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return Task.CompletedTask;
        }

        private async Task LoadBooksAsync()
        {
            if (string.IsNullOrWhiteSpace(LocCode)) return;
            dgvBooks.DataSource = await _res.SearchAvailableAsync(LocCode!, txtSearchBook.Text);
        }

        private async Task LoadMyAsync()
        {
            if (string.IsNullOrWhiteSpace(LocCode)) return;
            dgvMine.DataSource = await _res.GetMyAsync(txtUserCode.Text.Trim(), LocCode!);
        }

        private async Task CancelSelectedAsync()
        {
            if (AppSession.Current == null) { MessageBox.Show("Session missing."); return; }

            var sel = dgvMine.CurrentRow?.DataBoundItem as ResMyRowDto;
            if (sel == null)
            {
                MessageBox.Show("Select a reservation from 'My Reservations' to cancel.");
                return;
            }

            if (sel.Status != "P" && sel.Status != "A")
            {
                MessageBox.Show("Only Pending/Approved reservations can be cancelled.");
                return;
            }

            var ok = MessageBox.Show(
                $"Cancel this reservation?\n\n{sel.BookCode} - {sel.Title}",
                "Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (ok != DialogResult.Yes) return;

            // ✅ Use your existing cancel method name
            var rows = await _res.CancelAsync(sel.ResId, txtUserCode.Text.Trim());
            if (rows == 0)
            {
                MessageBox.Show("Cancel failed (already processed).");
                return;
            }

            await LoadMyAsync();
            await LoadBooksAsync();
        }
      
        private void WireEvents()
        {
            btnSearch.Click += async (_, __) => await LoadBooksAsync();
            btnClear.Click += (_, __) => ClearForm();

            btnCancel.Click += async (_, __) => await CancelSelectedAsync();
            dgvBooks.SelectionChanged += (_, __) => BindSelectedBook();

            // F2 User lookup
            txtUserCode.KeyDown += async (_, e) =>
            {
                if (e.KeyCode != Keys.F2) return;
                e.Handled = true;
                e.SuppressKeyPress = true;

                var pick = LibraryMS.Win.FrmLookup.Pick(this, "Select User", q => _users.LookupUsersAsync(q));
                if (pick == null) return;

                txtUserCode.Text = pick.Code;
                lblUserName.Text = pick.Name;
                await LoadMyAsync();
            };
        }

        private void BindSelectedBook()
        {
            var b = dgvBooks.CurrentRow?.DataBoundItem as BookAvailRowDto;
            if (b == null) return;

            txtBookCode.Text = b.BookCode;
            txtBookTitle.Text = b.Title;

            if (b.Available > 0 && numQty.Value > b.Available)
                numQty.Value = b.Available;
        }

        private void ClearForm()
        {
            txtBookCode.Clear();
            txtBookTitle.Clear();
            numQty.Value = 1;
            numHoldDays.Value = 3;
            txtRemark.Clear();
        }

        private void BuildUi()
        {
            BackColor = Color.PapayaWhip;

            var gb = new GroupBox { Dock = DockStyle.Fill, Padding = new Padding(10), Text = "Reservation Request" };
            Controls.Add(gb);

            var tlpRoot = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            gb.Controls.Add(tlpRoot);

            // Header
            var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(6, 10, 6, 0) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Text = "Book Reservation";

            var flp = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Dock = DockStyle.Fill, Margin = new Padding(0) };
            flp.Controls.Add(new Label { Text = "Book Search:", AutoSize = true, Padding = new Padding(0, 7, 0, 0) });
            flp.Controls.Add(txtSearchBook);

            SetupBtn(btnSearch, "Search", Color.SteelBlue);
            SetupBtn(btnClear, "Clear", Color.DimGray);
            SetupBtn(btnCancel, "Cancel", Color.DarkOrange);
            
            flp.Controls.Add(btnSearch);
            flp.Controls.Add(btnClear);
            flp.Controls.Add(btnCancel);

            header.Controls.Add(lblTitle, 0, 0);
            header.Controls.Add(flp, 1, 0);
            tlpRoot.Controls.Add(header, 0, 0);

            // Body
            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpRoot.Controls.Add(body, 0, 1);

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

            SetupGrid(dgvBooks);
            SetupGrid(dgvMine);

            left.Controls.Add(new GroupBox { Dock = DockStyle.Fill, Text = "Available Books (Approved reservations reduce availability)", Controls = { dgvBooks } }, 0, 0);
            left.Controls.Add(new GroupBox { Dock = DockStyle.Fill, Text = "My Reservations (Select and press Button to SAVE or CANCEL )", Controls = { dgvMine } }, 0, 1);
            left.Controls.Add(new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "My Reservations (Select and click Cancel / press Delete)",
                Controls = { dgvMine }
            }, 0, 1);

            var right = new GroupBox { Dock = DockStyle.Fill, Text = "Request Details (Press SAVE to submit)" };
            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            for (int i = 0; i < 9; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            AddRow(form, 0, "Location", lblLoc);
            AddRow(form, 1, "User (F2)", txtUserCode);
            AddRow(form, 2, "User Name", lblUserName);
            AddRow(form, 3, "Book Code", txtBookCode);
            AddRow(form, 4, "Title", txtBookTitle);
            AddRow(form, 5, "Qty", numQty);
            AddRow(form, 6, "Hold Days", numHoldDays);
            AddRow(form, 7, "Remark", txtRemark);

            right.Controls.Add(form);

            body.Controls.Add(left, 0, 0);
            body.Controls.Add(right, 1, 0);
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

        private static void AddRow(TableLayoutPanel t, int row, string label, Control input)
        {
            var lbl = new Label { Text = label, AutoSize = true, Padding = new Padding(0, 6, 0, 0), Dock = DockStyle.Fill };
            input.Dock = DockStyle.Fill;
            t.Controls.Add(lbl, 0, row);
            t.Controls.Add(input, 1, row);
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