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
        private readonly Label lblMemberName = new() { AutoSize = true };

        private readonly TextBox txtSearch = new() { Width = 220 };
        private readonly Button btnSearch = new();
        private readonly Button btnClearSearch = new();

        private readonly TextBox txtMemberCode = new();
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

        // responsive layout fields
        private readonly GroupBox gbMain = new();
        private readonly TableLayoutPanel tlpRoot = new();
        private readonly TableLayoutPanel tlpHeader = new();
        private readonly FlowLayoutPanel flpHeaderActions = new();
        private readonly TableLayoutPanel tlpBody = new();

        private readonly TableLayoutPanel tlpLeft = new();
        private readonly GroupBox grpBooks = new();
        private readonly GroupBox grpReservations = new();
        private readonly GroupBox grpLines = new();
        private readonly GroupBox grpRight = new();

        private readonly TableLayoutPanel tblForm = new();
        private readonly FlowLayoutPanel flpLineActions = new();

        public UCBookBorrow(BookBorrowService borrow, BookReservationService res, UserLookupService users)
        {
            _borrow = borrow ?? throw new ArgumentNullException(nameof(borrow));
            _res = res ?? throw new ArgumentNullException(nameof(res));
            _users = users ?? throw new ArgumentNullException(nameof(users));

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
            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(LocCode))
            {
                MessageBox.Show("Location missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMemberCode.Text))
            {
                MessageBox.Show("Select Member (F2).");
                return;
            }

            if (_lines.Count == 0)
            {
                MessageBox.Show("Add at least one line.");
                return;
            }

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

                MessageBox.Show(
                    $"Borrow saved.\nDocNo: {docNo}",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

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
            if (string.IsNullOrWhiteSpace(LocCode))
                return;

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
            if (dgvBooks.CurrentRow?.DataBoundItem is not BookAvailRowDto row)
                return;

            txtBookCode.Text = row.BookCode;
            txtBookTitle.Text = row.Title;
            txtReservationId.Clear();

            if (row.Available > 0 && numQty.Value > row.Available)
                numQty.Value = row.Available;
        }

        private void BindSelectedReservation()
        {
            if (dgvReservations.CurrentRow?.DataBoundItem is not ResMyRowDto row)
                return;

            txtBookCode.Text = row.BookCode;
            txtBookTitle.Text = row.Title;
            txtReservationId.Text = row.ResId.ToString();
            numQty.Value = row.Qty > 0 ? row.Qty : 1;
        }

        private void AddSelectedReservationToGrid()
        {
            if (dgvReservations.CurrentRow?.DataBoundItem is not ResMyRowDto row)
                return;

            BindSelectedReservation();

            var existing = _lines.FirstOrDefault(x => x.ReservationId == row.ResId);
            if (existing != null)
                return;

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
            if (dgvLines.CurrentRow?.DataBoundItem is not BorrowLineDto row)
                return;

            _lines.Remove(row);
        }

        private void ClearEntry()
        {
            txtBookCode.Clear();
            txtBookTitle.Clear();
            txtReservationId.Clear();
            txtRemark.Clear();
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

            txtSearch.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    await LoadBooksAsync();
                }
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

            dgvLines.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    RemoveSelectedLine();
                }
            };
        }

        private void BuildUi()
        {
            SuspendLayout();

            BackColor = Color.PapayaWhip;
            Controls.Clear();

            txtRemark.Multiline = true;
            txtRemark.Height = 72;
            txtRemark.ScrollBars = ScrollBars.Vertical;

            gbMain.Dock = DockStyle.Fill;
            gbMain.Padding = new Padding(10);
            gbMain.Text = "Book Borrow";
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

            // Header
            tlpHeader.Dock = DockStyle.Top;
            tlpHeader.AutoSize = true;
            tlpHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpHeader.Margin = new Padding(0);
            tlpHeader.Padding = new Padding(6, 10, 6, 6);

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            flpHeaderActions.AutoSize = true;
            flpHeaderActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpHeaderActions.FlowDirection = FlowDirection.LeftToRight;
            flpHeaderActions.WrapContents = true;
            flpHeaderActions.Dock = DockStyle.Fill;
            flpHeaderActions.Margin = new Padding(0);
            flpHeaderActions.Padding = new Padding(0);

            var lblSearchCaption = new Label
            {
                Text = "Book Search:",
                AutoSize = true,
                Padding = new Padding(0, 7, 0, 0),
                Margin = new Padding(0, 0, 8, 0)
            };

            txtSearch.Width = 220;
            txtSearch.Margin = new Padding(0, 0, 8, 0);

            SetupBtn(btnSearch, "Search", Color.SteelBlue);
            SetupBtn(btnClearSearch, "Clear", Color.DimGray);

            btnSearch.Width = 90;
            btnClearSearch.Width = 90;
            btnSearch.Margin = new Padding(0, 0, 8, 0);
            btnClearSearch.Margin = new Padding(0);

            flpHeaderActions.Controls.Add(lblSearchCaption);
            flpHeaderActions.Controls.Add(txtSearch);
            flpHeaderActions.Controls.Add(btnSearch);
            flpHeaderActions.Controls.Add(btnClearSearch);

            tlpRoot.Controls.Add(tlpHeader, 0, 0);

            // Body
            tlpBody.Dock = DockStyle.Fill;
            tlpBody.Margin = new Padding(0);
            tlpBody.Padding = new Padding(0);
            tlpRoot.Controls.Add(tlpBody, 0, 1);

            // Left side
            tlpLeft.Dock = DockStyle.Fill;
            tlpLeft.ColumnCount = 1;
            tlpLeft.RowCount = 3;
            tlpLeft.Margin = new Padding(0, 0, 10, 0);
            tlpLeft.Padding = new Padding(0);
            tlpLeft.RowStyles.Clear();
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));

            SetupGrid(dgvBooks);
            SetupGrid(dgvReservations);
            SetupGrid(dgvLines);
            dgvLines.DataSource = _lines;

            grpBooks.Dock = DockStyle.Fill;
            grpBooks.Text = "Available Books";
            grpBooks.Padding = new Padding(6);
            grpBooks.Margin = new Padding(3);
            grpBooks.Controls.Add(dgvBooks);

            grpReservations.Dock = DockStyle.Fill;
            grpReservations.Text = "Active Reservations of Selected Member (Double-click to Add)";
            grpReservations.Padding = new Padding(6);
            grpReservations.Margin = new Padding(3);
            grpReservations.Controls.Add(dgvReservations);

            grpLines.Dock = DockStyle.Fill;
            grpLines.Text = "Borrow Lines";
            grpLines.Padding = new Padding(6);
            grpLines.Margin = new Padding(3);
            grpLines.Controls.Add(dgvLines);

            tlpLeft.Controls.Add(grpBooks, 0, 0);
            tlpLeft.Controls.Add(grpReservations, 0, 1);
            tlpLeft.Controls.Add(grpLines, 0, 2);

            // Right side
            grpRight.Dock = DockStyle.Fill;
            grpRight.Text = "Borrow Details (Press SAVE to submit)";
            grpRight.Padding = new Padding(6);
            grpRight.Margin = new Padding(3);

            tblForm.Dock = DockStyle.Fill;
            tblForm.ColumnCount = 2;
            tblForm.RowCount = 12;
            tblForm.Padding = new Padding(10);
            tblForm.Margin = new Padding(0);
            tblForm.AutoScroll = true;
            tblForm.ColumnStyles.Clear();
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            tblForm.RowStyles.Clear();
            for (int i = 0; i < 8; i++)
                tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));  // Remark
            tblForm.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Actions label
            tblForm.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Actions buttons
            tblForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Filler

            txtMemberCode.Margin = new Padding(0, 2, 0, 2);
            txtBookCode.Margin = new Padding(0, 2, 0, 2);
            txtBookTitle.Margin = new Padding(0, 2, 0, 2);
            txtReservationId.Margin = new Padding(0, 2, 0, 2);
            txtRemark.Margin = new Padding(0, 2, 0, 2);
            numQty.Margin = new Padding(0, 2, 0, 2);
            dtDueDate.Margin = new Padding(0, 2, 0, 2);

            AddRow(tblForm, 0, "Location", lblLoc);
            AddRow(tblForm, 1, "Member (F2)", txtMemberCode);
            AddRow(tblForm, 2, "Member Name", lblMemberName);
            AddRow(tblForm, 3, "Book Code", txtBookCode);
            AddRow(tblForm, 4, "Title", txtBookTitle);
            AddRow(tblForm, 5, "Qty", numQty);
            AddRow(tblForm, 6, "Due Date", dtDueDate);
            AddRow(tblForm, 7, "Reservation", txtReservationId);
            AddRow(tblForm, 8, "Remark", txtRemark);

            SetupBtn(btnAdd, "Add Line", Color.SeaGreen);
            SetupBtn(btnRemove, "Remove", Color.IndianRed);
            SetupBtn(btnLineClear, "Clear", Color.DimGray);

            btnAdd.AutoSize = true;
            btnAdd.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAdd.Margin = new Padding(0, 0, 8, 8);

            btnRemove.AutoSize = true;
            btnRemove.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRemove.Margin = new Padding(0, 0, 8, 8);

            btnLineClear.AutoSize = true;
            btnLineClear.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnLineClear.Margin = new Padding(0, 0, 0, 8);

            flpLineActions.Dock = DockStyle.Fill;
            flpLineActions.FlowDirection = FlowDirection.LeftToRight;
            flpLineActions.WrapContents = true;
            flpLineActions.AutoSize = true;
            flpLineActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpLineActions.Margin = new Padding(0);
            flpLineActions.Padding = new Padding(0);

            flpLineActions.Controls.Add(btnAdd);
            flpLineActions.Controls.Add(btnRemove);
            flpLineActions.Controls.Add(btnLineClear);

            var lblActions = new Label
            {
                Text = "Actions",
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 0),
                Dock = DockStyle.Fill
            };

            tblForm.Controls.Add(lblActions, 0, 9);
            tblForm.Controls.Add(flpLineActions, 1, 10);

            grpRight.Controls.Add(tblForm);

            ApplyResponsiveLayout();

            ResumeLayout(true);
        }

        private void ApplyResponsiveLayout()
        {
            if (IsDisposed)
                return;

            // keep side-by-side layout longer
            bool narrow = Width < 980;

            SuspendLayout();
            tlpHeader.SuspendLayout();
            tlpBody.SuspendLayout();

            // Header layout
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
                flpHeaderActions.Dock = DockStyle.Top;
                flpHeaderActions.WrapContents = true;

                tlpHeader.Controls.Add(lblTitle, 0, 0);
                tlpHeader.Controls.Add(flpHeaderActions, 0, 1);
            }
            else
            {
                tlpHeader.ColumnCount = 2;
                tlpHeader.RowCount = 1;

                tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                tlpHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                lblTitle.Margin = new Padding(0);
                flpHeaderActions.Dock = DockStyle.Fill;
                flpHeaderActions.WrapContents = false;

                tlpHeader.Controls.Add(lblTitle, 0, 0);
                tlpHeader.Controls.Add(flpHeaderActions, 1, 0);
            }

            // Body layout
            tlpBody.Controls.Clear();
            tlpBody.ColumnStyles.Clear();
            tlpBody.RowStyles.Clear();

            if (narrow)
            {
                tlpBody.ColumnCount = 1;
                tlpBody.RowCount = 2;

                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
                tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

                grpRight.MinimumSize = Size.Empty;
                tlpLeft.Margin = new Padding(0);

                tlpBody.Controls.Add(tlpLeft, 0, 0);
                tlpBody.Controls.Add(grpRight, 0, 1);
            }
            else
            {
                tlpBody.ColumnCount = 2;
                tlpBody.RowCount = 1;

                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380F));
                tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                grpRight.MinimumSize = new Size(380, 0);
                tlpLeft.Margin = new Padding(0, 0, 10, 0);

                tlpBody.Controls.Add(tlpLeft, 0, 0);
                tlpBody.Controls.Add(grpRight, 1, 0);
            }

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
            dgv.AllowUserToResizeRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoGenerateColumns = true;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.RowTemplate.Height = 28;
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }

        private static void AddRow(TableLayoutPanel t, int row, string label, Control input)
        {
            var lbl = new Label
            {
                Text = label,
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 0),
                Dock = DockStyle.Fill
            };

            input.Dock = DockStyle.Fill;
            t.Controls.Add(lbl, 0, row);
            t.Controls.Add(input, 1, row);
        }

        private static void SetupBtn(Button b, string text, Color back)
        {
            b.Text = text;
            b.Height = 36;
            b.MinimumSize = new Size(95, 36);
            b.Padding = new Padding(12, 0, 12, 0);
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