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
    public sealed partial class UCBookReturn : UserControl, IPageActions
    {
        private readonly BookBorrowService _borrow;
        private readonly BookReturnService _returns;

        private readonly Label lblTitle = new();
        private readonly TextBox txtSearch = new() { Width = 220 };
        private readonly Button btnSearch = new();
        private readonly Button btnClearSearch = new();

        private readonly TextBox txtBorrowDoc = new() { ReadOnly = true };
        private readonly TextBox txtBookCode = new() { ReadOnly = true };
        private readonly TextBox txtTitle = new() { ReadOnly = true };
        private readonly NumericUpDown numOutstanding = new() { ReadOnly = true, Minimum = 0, Maximum = 1000 };
        private readonly NumericUpDown numReturnQty = new() { Minimum = 1, Maximum = 1000, Value = 1 };
        private readonly DateTimePicker dtDueDate = new() { Format = DateTimePickerFormat.Short, Enabled = false };
        private readonly ComboBox cmbCondition = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox txtRemark = new();
        private readonly Label lblFinePreview = new() { AutoSize = true, ForeColor = Color.Maroon };

        private readonly Button btnAdd = new();
        private readonly Button btnRemove = new();
        private readonly Button btnLineClear = new();

        private readonly DataGridView dgvBorrows = new();
        private readonly DataGridView dgvDetails = new();
        private readonly DataGridView dgvLines = new();

        private readonly BindingList<ReturnLineDto> _lines = new();

        private string? _selectedMemberCode;

        public UCBookReturn(BookBorrowService borrow, BookReturnService returns)
        {
            _borrow = borrow ?? throw new ArgumentNullException(nameof(borrow));
            _returns = returns ?? throw new ArgumentNullException(nameof(returns));

            Dock = DockStyle.Fill;
            BuildUi();
            WireEvents();

            Load += async (_, __) => await OnRefreshAsync();
        }

        private string? LocCode => AppSession.Current?.LocationCode;
        private string? LocDesc => AppSession.Current?.LocationDesc;

        public async Task OnRefreshAsync()
        {
            lblTitle.Text = $"Book Return - {LocDesc}";
            _lines.Clear();
            ClearEntry();
            await LoadBorrowsAsync();
            dgvDetails.DataSource = null;
        }

        public void OnEdit() { }

        public async Task OnSaveAsync()
        {
            if (AppSession.Current == null) { MessageBox.Show("Session missing."); return; }
            if (string.IsNullOrWhiteSpace(LocCode)) { MessageBox.Show("Location missing."); return; }
            if (string.IsNullOrWhiteSpace(txtBorrowDoc.Text)) { MessageBox.Show("Select Borrow Document."); return; }
            if (string.IsNullOrWhiteSpace(_selectedMemberCode)) { MessageBox.Show("Member missing."); return; }
            if (_lines.Count == 0) { MessageBox.Show("Add at least one return line."); return; }

            try
            {
                var dto = new ReturnCreateDto(
                    BorrowDocNo: txtBorrowDoc.Text.Trim(),
                    MemberCode: _selectedMemberCode!,
                    LocCode: LocCode!,
                    ReturnedBy: AppSession.Current.UserCode,
                    Remark: string.IsNullOrWhiteSpace(txtRemark.Text) ? null : txtRemark.Text.Trim(),
                    Lines: _lines.ToList()
                );

                var result = await _returns.CreateAsync(dto);

                var msg = $"Return saved.\nReturn Doc: {result.ReturnDocNo}";
                if (!string.IsNullOrWhiteSpace(result.FineDocNo) && result.FineTotal > 0)
                    msg += $"\nFine Doc: {result.FineDocNo}\nFine Total: {result.FineTotal:N2}";

                MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private async Task LoadBorrowsAsync()
        {
            if (string.IsNullOrWhiteSpace(LocCode))
            {
                dgvBorrows.DataSource = null;
                return;
            }

            dgvBorrows.DataSource = await _borrow.SearchOpenAsync(LocCode!, txtSearch.Text);
        }

        private async Task LoadDetailsAsync()
        {
            if (dgvBorrows.CurrentRow?.DataBoundItem is not BorrowOpenRowDto row)
            {
                dgvDetails.DataSource = null;
                return;
            }

            txtBorrowDoc.Text = row.DocNo;
            _selectedMemberCode = row.MemberCode;
            dgvDetails.DataSource = await _borrow.GetOpenDetailsAsync(row.DocNo);
        }

        private void BindSelectedDetail()
        {
            if (dgvDetails.CurrentRow?.DataBoundItem is not BorrowOpenDetailRowDto row) return;

            txtBookCode.Text = row.BookCode;
            txtTitle.Text = row.Title;
            numOutstanding.Value = row.OutstandingQty;
            numReturnQty.Maximum = row.OutstandingQty > 0 ? row.OutstandingQty : 1;
            numReturnQty.Value = row.OutstandingQty > 0 ? 1 : numReturnQty.Minimum;
            dtDueDate.Value = row.DueDate;
            lblFinePreview.Text = "";
        }

        private void AddLine()
        {
            if (dgvDetails.CurrentRow?.DataBoundItem is not BorrowOpenDetailRowDto row)
            {
                MessageBox.Show("Select a borrow detail line.");
                return;
            }

            var qty = (int)numReturnQty.Value;
            var condition = cmbCondition.SelectedItem?.ToString() ?? "N";

            var existing = _lines.FirstOrDefault(x => x.BorrowLineNo == row.LineNo);
            if (existing != null)
            {
                MessageBox.Show("This borrow line is already added.");
                return;
            }

            _lines.Add(new ReturnLineDto(
                BorrowLineNo: row.LineNo,
                BookCode: row.BookCode,
                Qty: qty,
                Condition: condition,
                Remark: string.IsNullOrWhiteSpace(txtRemark.Text) ? null : txtRemark.Text.Trim()
            ));
        }

        private void RemoveSelectedLine()
        {
            if (dgvLines.CurrentRow?.DataBoundItem is not ReturnLineDto row) return;
            _lines.Remove(row);
        }

        private void ClearEntry()
        {
            txtBorrowDoc.Clear();
            txtBookCode.Clear();
            txtTitle.Clear();
            txtRemark.Clear();

            numOutstanding.Value = 0;
            numReturnQty.Value = 1;
            numReturnQty.Maximum = 1000;

            _selectedMemberCode = null;
            lblFinePreview.Text = "";

            if (cmbCondition.Items.Count > 0)
                cmbCondition.SelectedIndex = 0;
        }

        private void WireEvents()
        {
            btnSearch.Click += async (_, __) => await LoadBorrowsAsync();

            btnClearSearch.Click += async (_, __) =>
            {
                txtSearch.Clear();
                await LoadBorrowsAsync();
            };

            dgvBorrows.SelectionChanged += async (_, __) => await LoadDetailsAsync();
            dgvDetails.SelectionChanged += (_, __) => BindSelectedDetail();

            btnAdd.Click += (_, __) => AddLine();
            btnRemove.Click += (_, __) => RemoveSelectedLine();
            btnLineClear.Click += (_, __) => ClearEntry();
        }


        private void BuildUi()
        {
            BackColor = Color.PapayaWhip;

            cmbCondition.Items.AddRange(new object[] { "N", "O", "D", "L" });
            cmbCondition.SelectedIndex = 0;

            var gb = new GroupBox
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Text = "Book Return"
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
                Text = "Borrow Search:",
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

            body.ColumnStyles.Clear();
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
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 32F));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));

            SetupGrid(dgvBorrows);
            SetupGrid(dgvDetails);
            SetupGrid(dgvLines);

            dgvLines.DataSource = _lines;

            var gbBorrows = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Open Borrow Documents",
                Padding = new Padding(6)
            };
            gbBorrows.Controls.Add(dgvBorrows);

            var gbDetails = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Borrow Detail Lines",
                Padding = new Padding(6)
            };
            gbDetails.Controls.Add(dgvDetails);

            var gbLines = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Return Lines",
                Padding = new Padding(6)
            };
            gbLines.Controls.Add(dgvLines);

            left.Controls.Add(gbBorrows, 0, 0);
            left.Controls.Add(gbDetails, 0, 1);
            left.Controls.Add(gbLines, 0, 2);

            var right = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Return Details (Press SAVE to process)",
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

            form.ColumnStyles.Clear();
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            form.RowStyles.Clear();
            for (int i = 0; i < 9; i++)
                form.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));   // Actions label
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));   // Actions buttons
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // filler

            txtBorrowDoc.Margin = new Padding(0, 2, 0, 2);
            txtBookCode.Margin = new Padding(0, 2, 0, 2);
            txtTitle.Margin = new Padding(0, 2, 0, 2);
            txtRemark.Margin = new Padding(0, 2, 0, 2);
            numOutstanding.Margin = new Padding(0, 2, 0, 2);
            numReturnQty.Margin = new Padding(0, 2, 0, 2);
            dtDueDate.Margin = new Padding(0, 2, 0, 2);
            cmbCondition.Margin = new Padding(0, 2, 0, 2);

            AddRow(form, 0, "Borrow Doc", txtBorrowDoc);
            AddRow(form, 1, "Book Code", txtBookCode);
            AddRow(form, 2, "Title", txtTitle);
            AddRow(form, 3, "Outstanding", numOutstanding);
            AddRow(form, 4, "Return Qty", numReturnQty);
            AddRow(form, 5, "Due Date", dtDueDate);
            AddRow(form, 6, "Condition", cmbCondition);
            AddRow(form, 7, "Remark", txtRemark);
            AddRow(form, 8, "Fine Preview", lblFinePreview);

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

        //private static void SetupBtn(Button b, string text, Color back)
        //{
        //    b.Text = text;
        //    b.Height = 34;
        //    b.MinimumSize = new Size(95, 34); // prevents text cutoff
        //    b.BackColor = back;
        //    b.ForeColor = Color.White;
        //    b.FlatStyle = FlatStyle.Flat;
        //    b.FlatAppearance.BorderSize = 0;
        //    b.Margin = new Padding(0);
        //    b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        //    b.TextAlign = ContentAlignment.MiddleCenter;
        //    b.UseVisualStyleBackColor = false;
        //}
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
            b.TextAlign = ContentAlignment.MiddleCenter;
            b.UseVisualStyleBackColor = false;
        }
    }
}