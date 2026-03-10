using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Interfaces;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win.Pages
{
    public sealed partial class UCBookBorrow : UserControl, IPageActions
    {
        private readonly BookBorrowService _borrow;
        private readonly BookReservationService _res;
        private readonly UserLookupService _users;

        private readonly Label lblTitle = new();
        private readonly Label lblLoc = new() { AutoSize = true };

        private readonly TextBox txtSearch = new() { Width = 220 };
        private readonly Button btnSearch = new();
        private readonly Button btnClearSearch = new();

        private readonly TextBox txtMemberCode = new();
        private readonly Label lblMemberName = new() { AutoSize = true };

        private readonly TextBox txtBookCode = new() { ReadOnly = true };
        private readonly TextBox txtBookTitle = new() { ReadOnly = true };
        private readonly NumericUpDown numQty = new() { Minimum = 1, Maximum = 1000, Value = 1 };
        private readonly DateTimePicker dtDueDate = new() { Format = DateTimePickerFormat.Short };
        private readonly TextBox txtReservationId = new() { ReadOnly = true };
        private readonly TextBox txtRemark = new();

        private readonly Button btnAdd = new();
        private readonly Button btnRemove = new();
        private readonly Button btnLineClear = new();

        private readonly DataGridView dgvBooks = new();
        private readonly DataGridView dgvReservations = new();
        private readonly DataGridView dgvLines = new();

        private readonly BindingList<BorrowLineDto> _lines = new();

        public UCBookBorrow(BookBorrowService borrow, BookReservationService res, UserLookupService users)
        {
            _borrow = borrow ?? throw new ArgumentNullException(nameof(borrow));
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
            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing.");
                return;
            }

            lblTitle.Text = $"Book Borrow - {LocDesc}";
            lblLoc.Text = LocDesc ?? "";
            txtMemberCode.Text = AppSession.Current.UserCode;
            lblMemberName.Text = AppSession.Current.UserName ?? "";
            dtDueDate.Value = DateTime.Today.AddDays(14);

            _lines.Clear();
            ClearEntry();

            await LoadBooksAsync();
            await LoadReservationsAsync();
        }

        public void OnEdit() { }

        public async Task OnSaveAsync()
        {
            if (AppSession.Current == null) { MessageBox.Show("Session missing."); return; }
            if (string.IsNullOrWhiteSpace(LocCode)) { MessageBox.Show("Location missing."); return; }
            if (string.IsNullOrWhiteSpace(txtMemberCode.Text)) { MessageBox.Show("Select Member (F2)."); return; }
            if (_lines.Count == 0) { MessageBox.Show("Add at least one line."); return; }

            try
            {
                var dto = new BorrowCreateDto(
                    MemberCode: txtMemberCode.Text.Trim(),
                    LocCode: LocCode!,
                    BorrowedBy: AppSession.Current.UserCode,
                    Remark: string.IsNullOrWhiteSpace(txtRemark.Text) ? null : txtRemark.Text.Trim(),
                    Lines: _lines.ToList()
                );

                var docNo = await _borrow.CreateAsync(dto);

                MessageBox.Show($"Borrow saved.\nDocNo: {docNo}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await OnRefreshAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public Task OnProcessAsync()
        {
            RemoveSelectedLine();
            return Task.CompletedTask;
        }

        private async Task LoadBooksAsync()
        {
            if (string.IsNullOrWhiteSpace(LocCode)) return;
            dgvBooks.DataSource = await _res.SearchAvailableAsync(LocCode!, txtSearch.Text);
        }

        private async Task LoadReservationsAsync()
        {
            if (string.IsNullOrWhiteSpace(LocCode) || string.IsNullOrWhiteSpace(txtMemberCode.Text))
            {
                dgvReservations.DataSource = null;
                return;
            }

            dgvReservations.DataSource = await _res.GetActiveByUserAsync(txtMemberCode.Text.Trim(), LocCode!);
        }

        private void BindSelectedBook()
        {
            if (dgvBooks.CurrentRow?.DataBoundItem is not BookAvailRowDto row) return;

            txtBookCode.Text = row.BookCode;
            txtBookTitle.Text = row.Title;
            txtReservationId.Clear();

            if (row.Available > 0 && numQty.Value > row.Available)
                numQty.Value = row.Available;
        }
        private void BindSelectedReservation()
        {
            if (dgvReservations.CurrentRow?.DataBoundItem is not ResMyRowDto row) return;

            txtBookCode.Text = row.BookCode;
            txtBookTitle.Text = row.Title;
            txtReservationId.Text = row.ResId.ToString();
            numQty.Value = row.Qty > 0 ? row.Qty : 1;
        }

        private void AddSelectedReservationToGrid()
        {
            if (dgvReservations.CurrentRow?.DataBoundItem is not ResMyRowDto row) return;

            BindSelectedReservation();

            var existing = _lines.FirstOrDefault(x => x.ReservationId == row.ResId);
            if (existing != null) return;

            _lines.Add(new BorrowLineDto(
                BookCode: row.BookCode,
                Title: row.Title,
                Qty: row.Qty,
                DueDate: dtDueDate.Value.Date,
                ReservationId: row.ResId
            ));
        }


        private void AddLine()
        {
            if (string.IsNullOrWhiteSpace(txtBookCode.Text))
            {
                MessageBox.Show("Select Book.");
                return;
            }

            var code = txtBookCode.Text.Trim();
            var title = txtBookTitle.Text.Trim();
            var qty = (int)numQty.Value;
            var dueDate = dtDueDate.Value.Date;
            int? reservationId = int.TryParse(txtReservationId.Text, out var rid) ? rid : null;

            var existing = _lines.FirstOrDefault(x =>
                x.BookCode.Equals(code, StringComparison.OrdinalIgnoreCase) &&
                x.ReservationId == reservationId);

            if (existing != null)
            {
                var idx = _lines.IndexOf(existing);
                _lines[idx] = existing with { Qty = existing.Qty + qty, DueDate = dueDate };
            }
            else
            {
                _lines.Add(new BorrowLineDto(code, title, qty, dueDate, reservationId));
            }

            ClearEntry();
        }

        private void RemoveSelectedLine()
        {
            if (dgvLines.CurrentRow?.DataBoundItem is not BorrowLineDto row) return;
            _lines.Remove(row);
        }

        private void ClearEntry()
        {
            txtBookCode.Clear();
            txtBookTitle.Clear();
            txtReservationId.Clear();
            numQty.Value = 1;
            dtDueDate.Value = DateTime.Today.AddDays(14);
        }

        private void WireEvents()
        {
            btnSearch.Click += async (_, __) => await LoadBooksAsync();
            btnClearSearch.Click += async (_, __) =>
            {
                txtSearch.Clear();
                await LoadBooksAsync();
            };

            btnAdd.Click += (_, __) => AddLine();
            btnRemove.Click += (_, __) => RemoveSelectedLine();
            btnLineClear.Click += (_, __) => ClearEntry();

            dgvBooks.SelectionChanged += (_, __) => BindSelectedBook();
            dgvReservations.SelectionChanged += (_, __) => BindSelectedReservation();
            dgvReservations.CellDoubleClick += (_, __) => AddSelectedReservationToGrid();
            txtMemberCode.KeyDown += async (_, e) =>
            {
                if (e.KeyCode != Keys.F2) return;
                e.Handled = true;
                e.SuppressKeyPress = true;

                var pick = LibraryMS.Win.FrmLookup.Pick(this, "Select Member", q => _users.LookupUsersAsync(q));
                if (pick == null) return;

                txtMemberCode.Text = pick.Code;
                lblMemberName.Text = pick.Name;
                _lines.Clear();
                ClearEntry();
                await LoadReservationsAsync();
            };
        }

        private void BuildUi()
        {
            BackColor = Color.PapayaWhip;

            var gb = new GroupBox
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Text = "Book Borrow"
            };
            Controls.Add(gb);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            gb.Controls.Add(root);

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(6, 10, 6, 0)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            var flp = new FlowLayoutPanel
            {
                AutoSize = true,
                WrapContents = false,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            var lblSearch = new Label
            {
                Text = "Book Search:",
                AutoSize = true,
                Padding = new Padding(0, 7, 0, 0),
                Margin = new Padding(0, 6, 8, 0)
            };

            txtSearch.Width = 220;
            txtSearch.Margin = new Padding(0, 3, 10, 0);

            SetupBtn(btnSearch, "Search", Color.SteelBlue);
            SetupBtn(btnClearSearch, "Clear", Color.DimGray);

            btnSearch.Width = 74;
            btnClearSearch.Width = 74;

            btnSearch.Margin = new Padding(0, 0, 8, 0);
            btnClearSearch.Margin = new Padding(0, 0, 0, 0);

            flp.Controls.Add(lblSearch);
            flp.Controls.Add(txtSearch);
            flp.Controls.Add(btnSearch);
            flp.Controls.Add(btnClearSearch);

            header.Controls.Add(lblTitle, 0, 0);
            header.Controls.Add(flp, 1, 0);
            root.Controls.Add(header, 0, 0);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));

            root.Controls.Add(body, 0, 1);

            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0, 0, 10, 0),
                Padding = new Padding(0)
            };
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

            SetupGrid(dgvBooks);
            SetupGrid(dgvReservations);
            SetupGrid(dgvLines);
            dgvLines.DataSource = _lines;

            var gbBooks = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Available Books",
                Padding = new Padding(6)
            };
            gbBooks.Controls.Add(dgvBooks);

            var gbReservations = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Active Reservations of Selected Member (Click to Add)",
                Padding = new Padding(6)
            };
            gbReservations.Controls.Add(dgvReservations);

            var gbLines = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Borrow Lines",
                Padding = new Padding(6)
            };
            gbLines.Controls.Add(dgvLines);

            left.Controls.Add(gbBooks, 0, 0);
            left.Controls.Add(gbReservations, 0, 1);
            left.Controls.Add(gbLines, 0, 2);

            var right = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Borrow Details (Press SAVE to submit)",
                Padding = new Padding(6)
            };

            var form = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(10),
                Margin = new Padding(0)
            };

            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            form.RowStyles.Clear();
            for (int i = 0; i < 9; i++)
                form.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));   // Actions label
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));   // Actions buttons
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // filler

            txtMemberCode.Margin = new Padding(0, 2, 0, 2);
            txtBookCode.Margin = new Padding(0, 2, 0, 2);
            txtBookTitle.Margin = new Padding(0, 2, 0, 2);
            txtReservationId.Margin = new Padding(0, 2, 0, 2);
            txtRemark.Margin = new Padding(0, 2, 0, 2);
            numQty.Margin = new Padding(0, 2, 0, 2);
            dtDueDate.Margin = new Padding(0, 2, 0, 2);

            AddRow(form, 0, "Location", lblLoc);
            AddRow(form, 1, "Member (F2)", txtMemberCode);
            AddRow(form, 2, "Member Name", lblMemberName);
            AddRow(form, 3, "Book Code", txtBookCode);
            AddRow(form, 4, "Title", txtBookTitle);
            AddRow(form, 5, "Qty", numQty);
            AddRow(form, 6, "Due Date", dtDueDate);
            AddRow(form, 7, "Reservation", txtReservationId);
            AddRow(form, 8, "Remark", txtRemark);

            SetupBtn(btnAdd, "Add Line", Color.SeaGreen);
            SetupBtn(btnRemove, "Remove", Color.IndianRed);
            SetupBtn(btnLineClear, "Clear", Color.DimGray);

            btnAdd.Dock = DockStyle.Fill;
            btnRemove.Dock = DockStyle.Fill;
            btnLineClear.Dock = DockStyle.Fill;

            btnAdd.Margin = new Padding(0, 0, 6, 0);
            btnRemove.Margin = new Padding(0, 0, 6, 0);
            btnLineClear.Margin = new Padding(0);

            var lblActions = new Label
            {
                Text = "Actions",
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 0),
                Dock = DockStyle.Fill
            };

            var actions = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            actions.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            actions.Controls.Add(btnAdd, 0, 0);
            actions.Controls.Add(btnRemove, 1, 0);
            actions.Controls.Add(btnLineClear, 2, 0);

            form.Controls.Add(lblActions, 0, 9);
            form.SetColumnSpan(lblActions, 2);

            form.Controls.Add(actions, 0, 10);
            form.SetColumnSpan(actions, 2);

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
            b.Height = 34;
            b.MinimumSize = new Size(0, 34);
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Margin = new Padding(4, 0, 4, 0);
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.UseVisualStyleBackColor = false;
        }
    }
}