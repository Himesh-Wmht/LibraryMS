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
    public sealed partial class UCBookTransferRequest : UserControl, IPageActions
    {
        private readonly BookTransferService _svc;
        private readonly LocationLookupService _locs;

        private readonly TextBox txtToLoc = new();
        private readonly Label lblToLocDesc = new() { AutoSize = true };

        private readonly TextBox txtBookCode = new();
        private readonly TextBox txtBookTitle = new() { ReadOnly = true };
        private readonly NumericUpDown numQty = new() { Minimum = 1, Maximum = 100000, Value = 1 };

        private readonly Button btnAdd = new();
        private readonly Button btnRemove = new();
        private readonly Button btnClear = new();

        private readonly TextBox txtRemark = new();

        private readonly DataGridView dgvLines = new();

        private readonly BindingList<TransferLineDto> _lines = new();

        public UCBookTransferRequest(BookTransferService svc, LocationLookupService locs)
        {
            InitializeComponent();
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _locs = locs ?? throw new ArgumentNullException(nameof(locs));

            Dock = DockStyle.Fill;
            BuildUi();
            WireEvents();

            Load += async (_, __) => await OnRefreshAsync();
        }

        private string? FromLoc => AppSession.Current?.LocationCode;
        private string? FromLocDesc => AppSession.Current?.LocationDesc;

        public async Task OnRefreshAsync()
        {
            
            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing. Please login again.");
                return;
            }

            _lines.Clear();
            txtToLoc.Clear();
            lblToLocDesc.Text = "";
            txtBookCode.Clear();
            txtBookTitle.Clear();
            numQty.Value = 1;
            numQty.Maximum = 100000;
            txtRemark.Clear();

            await Task.CompletedTask;
        }

        public void OnEdit() { }

        // SAVE = submit transfer
        public async Task OnSaveAsync()
        {
            if (AppSession.Current == null) { MessageBox.Show("Session missing."); return; }
            if (string.IsNullOrWhiteSpace(FromLoc)) { MessageBox.Show("From Location missing in session."); return; }
            if (string.IsNullOrWhiteSpace(txtToLoc.Text)) { MessageBox.Show("Select To Location (F2)."); return; }
            if (txtToLoc.Text.Trim().Equals(FromLoc, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("To Location cannot be same as From Location.");
                return;
            }
            if (_lines.Count == 0) { MessageBox.Show("Add at least 1 book line."); return; }
            foreach (var line in _lines)
            {
                var available = await _svc.GetAvailableQtyAsync(FromLoc!, line.BookCode);
                if (line.Qty > available)
                {
                    MessageBox.Show(
                        $"Book {line.BookCode} has only {available} available copies after reservations.\nPlease adjust the line quantity.",
                        "Insufficient Qty",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }
            var dto = new TransferCreateDto(
                FromLoc: FromLoc!,
                ToLoc: txtToLoc.Text.Trim(),
                ReqBy: AppSession.Current.UserCode,
                Remark: string.IsNullOrWhiteSpace(txtRemark.Text) ? null : txtRemark.Text.Trim(),
                Lines: _lines.ToList()
            );

            try
            {
                var docNo = await _svc.CreateAsync(dto);
                MessageBox.Show($"Transfer request submitted.\nDocNo: {docNo}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await OnRefreshAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // PROCESS = remove selected line (optional)
        public Task OnProcessAsync()
        {
            RemoveSelectedLine();
            return Task.CompletedTask;
        }

        private void WireEvents()
        {
            btnAdd.Click += async (_, __) => await AddLineAsync();
            btnRemove.Click += (_, __) => RemoveSelectedLine();
            btnClear.Click += async (_, __) => await OnRefreshAsync();

            txtToLoc.KeyDown += async (_, e) =>
            {
                if (e.KeyCode != Keys.F2) return;
                e.Handled = true;
                e.SuppressKeyPress = true;

                var pick = LibraryMS.Win.FrmLookup.Pick(this, "Select To Location", q => _locs.LookupAsync(q));
                if (pick == null) return;

                txtToLoc.Text = pick.Code;
                lblToLocDesc.Text = pick.Name;
            };

            txtBookCode.KeyDown += async (_, e) =>
            {
                if (e.KeyCode != Keys.F2) return;
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (string.IsNullOrWhiteSpace(FromLoc))
                {
                    MessageBox.Show("From Location missing in session.");
                    return;
                }

                var pick = LibraryMS.Win.FrmLookup.Pick(this, "Select Book", q => _svc.LookupBooksInLocAsync(FromLoc!, q));
                if (pick == null) return;

                txtBookCode.Text = pick.Code;
                txtBookTitle.Text = pick.Name;

                var available = await _svc.GetAvailableQtyAsync(FromLoc!, pick.Code);
                numQty.Maximum = Math.Max(1, available);
                numQty.Value = available > 0 ? 1 : numQty.Minimum;
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

        private async Task AddLineAsync()
        {
            if (string.IsNullOrWhiteSpace(txtBookCode.Text))
            {
                MessageBox.Show("Select Book (F2).");
                return;
            }

            if (string.IsNullOrWhiteSpace(FromLoc))
            {
                MessageBox.Show("From Location missing in session.");
                return;
            }

            var code = txtBookCode.Text.Trim();
            var title = txtBookTitle.Text.Trim();
            var qty = (int)numQty.Value;

            var available = await _svc.GetAvailableQtyAsync(FromLoc!, code);

            if (available <= 0)
            {
                MessageBox.Show("This book is not physically available in this location.");
                return;
            }

            var existing = _lines.FirstOrDefault(x => x.BookCode.Equals(code, StringComparison.OrdinalIgnoreCase));
            var existingQty = existing?.Qty ?? 0;
            var finalQty = existingQty + qty;

            if (finalQty > available)
            {
                MessageBox.Show(
                    $"Only {available} copies are available after reservations.\nCurrent line qty: {existingQty}\nTrying to add: {qty}",
                    "Insufficient Qty",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (existing != null)
            {
                var idx = _lines.IndexOf(existing);
                _lines[idx] = existing with { Qty = finalQty };
            }
            else
            {
                _lines.Add(new TransferLineDto(code, title, qty));
            }

            txtBookCode.Clear();
            txtBookTitle.Clear();
            numQty.Value = 1;
            numQty.Maximum = 100000;
            txtBookCode.Focus();
        }
        private void RemoveSelectedLine()
        {
            if (dgvLines.CurrentRow?.DataBoundItem is not TransferLineDto line) return;
            _lines.Remove(line);
        }

        private void BuildUi()
        {
            BackColor = Color.PapayaWhip;

            var gb = new GroupBox { Dock = DockStyle.Fill, Padding = new Padding(10), Text = "Location Transfer Request" };
            Controls.Add(gb);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            root.ColumnStyles.Clear();
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390F));
            gb.Controls.Add(root);
            // Left: Lines grid
            dgvLines.Dock = DockStyle.Fill;
            dgvLines.ReadOnly = true;
            dgvLines.AllowUserToAddRows = false;
            dgvLines.AllowUserToDeleteRows = false;
            dgvLines.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLines.MultiSelect = false;
            dgvLines.AutoGenerateColumns = true;
            dgvLines.BackgroundColor = Color.White;
            dgvLines.RowHeadersVisible = false;
            dgvLines.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLines.DataSource = _lines;

            root.Controls.Add(dgvLines, 0, 0);

            // Right: Form
            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
            form.ColumnStyles.Clear();
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 10; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            AddRow(form, 0, "From", new Label { Text = FromLocDesc ?? "", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
            AddRow(form, 1, "To (F2)", txtToLoc);
            AddRow(form, 2, "To Desc", lblToLocDesc);
            AddRow(form, 3, "Book (F2)", txtBookCode);
            AddRow(form, 4, "Title", txtBookTitle);
            AddRow(form, 5, "Qty", numQty);

            SetupBtn(btnAdd, "Add Line", Color.SeaGreen);
            SetupBtn(btnRemove, "Remove", Color.IndianRed);
            SetupBtn(btnClear, "Clear", Color.DimGray);

            btnAdd.Dock = DockStyle.Fill;
            btnRemove.Dock = DockStyle.Fill;
            btnClear.Dock = DockStyle.Fill;

            btnAdd.Margin = new Padding(0, 0, 6, 0);
            btnRemove.Margin = new Padding(0, 0, 6, 0);
            btnClear.Margin = new Padding(0);

            var actions = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));

            actions.Controls.Add(btnAdd, 0, 0);
            actions.Controls.Add(btnRemove, 1, 0);
            actions.Controls.Add(btnClear, 2, 0);

            form.Controls.Add(new Label { Text = "Actions", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 6);
            form.Controls.Add(actions, 1, 6);

            AddRow(form, 7, "Remark", txtRemark);

            root.Controls.Add(form, 1, 0);
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
            b.Height = 30;
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Margin = new Padding(0);
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.UseVisualStyleBackColor = false;
        }
    }
}