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

        private readonly Label lblTitle = new();

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

        // responsive layout fields
        private readonly GroupBox gbMain = new();
        private readonly TableLayoutPanel tlpRoot = new();
        private readonly TableLayoutPanel tlpHeader = new();
        private readonly TableLayoutPanel tlpBody = new();

        private readonly GroupBox grpLines = new();
        private readonly GroupBox grpForm = new();
        private readonly TableLayoutPanel tblForm = new();

        public UCBookTransferRequest(BookTransferService svc, LocationLookupService locs)
        {
            InitializeComponent();
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _locs = locs ?? throw new ArgumentNullException(nameof(locs));

            Dock = DockStyle.Fill;

            BuildUi();
            WireEvents();
            ApplyResponsiveLayout();

            Resize += (_, __) => ApplyResponsiveLayout();
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

            RefreshHeaderText();

            await Task.CompletedTask;
        }

        public void OnEdit() { }

        public async Task OnSaveAsync()
        {
            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(FromLoc))
            {
                MessageBox.Show("From Location missing in session.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtToLoc.Text))
            {
                MessageBox.Show("Select To Location (F2).");
                return;
            }

            if (txtToLoc.Text.Trim().Equals(FromLoc, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("To Location cannot be same as From Location.");
                return;
            }

            if (_lines.Count == 0)
            {
                MessageBox.Show("Add at least 1 book line.");
                return;
            }

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
                MessageBox.Show(
                    $"Transfer request submitted.\nDocNo: {docNo}",
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
                await Task.CompletedTask;
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
            if (dgvLines.CurrentRow?.DataBoundItem is not TransferLineDto line)
                return;

            _lines.Remove(line);
        }

        private void BuildUi()
        {
            SuspendLayout();

            BackColor = Color.PapayaWhip;
            Controls.Clear();

            txtRemark.Multiline = true;
            txtRemark.Height = 72;
            txtRemark.ScrollBars = ScrollBars.Vertical;

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            gbMain.Dock = DockStyle.Fill;
            gbMain.Padding = new Padding(10);
            gbMain.Text = "Location Transfer Request";
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

            tlpHeader.Dock = DockStyle.Top;
            tlpHeader.AutoSize = true;
            tlpHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpHeader.Margin = new Padding(0);
            tlpHeader.Padding = new Padding(8, 10, 8, 6);
            tlpRoot.Controls.Add(tlpHeader, 0, 0);

            tlpBody.Dock = DockStyle.Fill;
            tlpBody.Margin = new Padding(0);
            tlpBody.Padding = new Padding(0);
            tlpRoot.Controls.Add(tlpBody, 0, 1);

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
            dgvLines.BorderStyle = BorderStyle.FixedSingle;
            dgvLines.DataSource = _lines;

            grpLines.Dock = DockStyle.Fill;
            grpLines.Text = "Transfer Lines";
            grpLines.Padding = new Padding(6);
            grpLines.Margin = new Padding(3);
            grpLines.Controls.Add(dgvLines);

            grpForm.Dock = DockStyle.Fill;
            grpForm.Text = "Transfer Details";
            grpForm.Padding = new Padding(6);
            grpForm.Margin = new Padding(3);

            tblForm.Dock = DockStyle.Fill;
            tblForm.ColumnCount = 2;
            tblForm.Padding = new Padding(10);
            tblForm.Margin = new Padding(0);
            tblForm.AutoScroll = true;
            tblForm.ColumnStyles.Clear();
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            tblForm.RowStyles.Clear();
            for (int i = 0; i < 6; i++)
                tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            tblForm.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // actions
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F)); // remark
            tblForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // filler

            AddRow(tblForm, 0, "From", new Label
            {
                Text = FromLocDesc ?? "",
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 0)
            });

            AddRow(tblForm, 1, "To (F2)", txtToLoc);
            AddRow(tblForm, 2, "To Desc", lblToLocDesc);
            AddRow(tblForm, 3, "Book (F2)", txtBookCode);
            AddRow(tblForm, 4, "Title", txtBookTitle);
            AddRow(tblForm, 5, "Qty", numQty);

            SetupBtn(btnAdd, "Add Line", Color.SeaGreen);
            SetupBtn(btnRemove, "Remove", Color.IndianRed);
            SetupBtn(btnClear, "Clear", Color.DimGray);

            btnAdd.AutoSize = true;
            btnAdd.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAdd.Margin = new Padding(0, 0, 8, 8);

            btnRemove.AutoSize = true;
            btnRemove.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRemove.Margin = new Padding(0, 0, 8, 8);

            btnClear.AutoSize = true;
            btnClear.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnClear.Margin = new Padding(0, 0, 0, 8);

            var flpActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            flpActions.Controls.Add(btnAdd);
            flpActions.Controls.Add(btnRemove);
            flpActions.Controls.Add(btnClear);

            tblForm.Controls.Add(new Label
            {
                Text = "Actions",
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 0),
                Dock = DockStyle.Fill
            }, 0, 6);

            tblForm.Controls.Add(flpActions, 1, 6);

            AddRow(tblForm, 7, "Remark", txtRemark);

            grpForm.Controls.Add(tblForm);

            RefreshHeaderText();
            ApplyResponsiveLayout();

            ResumeLayout(true);
        }

        private void RefreshHeaderText()
        {
            var fromText = string.IsNullOrWhiteSpace(FromLocDesc)
                ? "Location Transfer Request"
                : $"Location Transfer Request - From: {FromLocDesc}";

            lblTitle.Text = fromText;

            if (tblForm.GetControlFromPosition(1, 0) is Label fromLabel)
                fromLabel.Text = FromLocDesc ?? "";
        }

        private void ApplyResponsiveLayout()
        {
            if (IsDisposed)
                return;

            bool narrow = Width < 1050;

            SuspendLayout();
            tlpHeader.SuspendLayout();
            tlpBody.SuspendLayout();

            tlpHeader.Controls.Clear();
            tlpHeader.ColumnStyles.Clear();
            tlpHeader.RowStyles.Clear();

            tlpHeader.ColumnCount = 1;
            tlpHeader.RowCount = 1;
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpHeader.Controls.Add(lblTitle, 0, 0);

            tlpBody.Controls.Clear();
            tlpBody.ColumnStyles.Clear();
            tlpBody.RowStyles.Clear();

            if (narrow)
            {
                tlpBody.ColumnCount = 1;
                tlpBody.RowCount = 2;

                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
                tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

                grpForm.MinimumSize = Size.Empty;

                tlpBody.Controls.Add(grpLines, 0, 0);
                tlpBody.Controls.Add(grpForm, 0, 1);
            }
            else
            {
                tlpBody.ColumnCount = 2;
                tlpBody.RowCount = 1;

                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));
                tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                grpForm.MinimumSize = new Size(360, 0);

                tlpBody.Controls.Add(grpLines, 0, 0);
                tlpBody.Controls.Add(grpForm, 1, 0);
            }

            tlpBody.ResumeLayout(true);
            tlpHeader.ResumeLayout(true);
            ResumeLayout(true);
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
            input.Margin = new Padding(3);

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
            b.Margin = new Padding(0);
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.TextAlign = ContentAlignment.MiddleCenter;
            b.UseVisualStyleBackColor = false;
        }
    }
}