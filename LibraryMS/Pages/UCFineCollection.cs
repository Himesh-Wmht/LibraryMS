using System;
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
    public sealed partial class UCFineCollection : UserControl, IPageActions
    {
        private readonly FineCollectionService _fines;

        private readonly Label lblTitle = new();
        private readonly TextBox txtSearch = new() { Width = 220 };
        private readonly Button btnSearch = new();
        private readonly Button btnClearSearch = new();

        private readonly DataGridView dgvFines = new();

        private readonly TextBox txtFineDoc = new() { ReadOnly = true };
        private readonly TextBox txtMemberCode = new() { ReadOnly = true };
        private readonly TextBox txtRefDoc = new() { ReadOnly = true };
        private readonly TextBox txtTotal = new() { ReadOnly = true };
        private readonly TextBox txtPaid = new() { ReadOnly = true };
        private readonly TextBox txtBalance = new() { ReadOnly = true };

        private readonly NumericUpDown numPayAmount = new() { DecimalPlaces = 2, Maximum = 1000000, Minimum = 0 };
        private readonly ComboBox cmbPayMode = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox txtRefNo = new();
        private readonly TextBox txtRemark = new();

        private readonly TextBox txtPurpose = new()
        {
            ReadOnly = true,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };

        private readonly TextBox txtCalculation = new()
        {
            ReadOnly = true,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };

        private readonly Button btnHelp = new();
        private readonly Button btnClearForm = new();

        public UCFineCollection(FineCollectionService fines)
        {
            _fines = fines ?? throw new ArgumentNullException(nameof(fines));

            Dock = DockStyle.Fill;
            BuildUi();
            WireEvents();

            Load += async (_, __) => await OnRefreshAsync();
        }

        public async Task OnRefreshAsync()
        {
            lblTitle.Text = "Fine Collection";
            await LoadFinesAsync();
            ClearEntry();
        }

        public void OnEdit() { }

        public async Task OnSaveAsync()
        {
            if (AppSession.Current == null) { MessageBox.Show("Session missing."); return; }
            if (string.IsNullOrWhiteSpace(txtFineDoc.Text)) { MessageBox.Show("Select a fine."); return; }
            if (numPayAmount.Value <= 0) { MessageBox.Show("Enter pay amount."); return; }

            try
            {
                var dto = new FinePaymentDto(
                    FineDocNo: txtFineDoc.Text.Trim(),
                    PayDate: DateTime.Now,
                    PayMode: cmbPayMode.SelectedItem?.ToString() ?? "CASH",
                    Amount: numPayAmount.Value,
                    RefNo: string.IsNullOrWhiteSpace(txtRefNo.Text) ? null : txtRefNo.Text.Trim(),
                    ReceivedBy: AppSession.Current.UserCode,
                    Remark: string.IsNullOrWhiteSpace(txtRemark.Text) ? null : txtRemark.Text.Trim()
                );

                await _fines.PayAsync(dto);

                MessageBox.Show("Fine payment saved.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await OnRefreshAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task OnProcessAsync()
        {
            if (AppSession.Current == null) { MessageBox.Show("Session missing."); return; }
            if (string.IsNullOrWhiteSpace(txtFineDoc.Text)) { MessageBox.Show("Select a fine."); return; }
            if (numPayAmount.Value <= 0) { MessageBox.Show("Enter refund amount in Amount field."); return; }

            var ok = MessageBox.Show("Refund this fine amount?", "Refund",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            try
            {
                var dto = new FineRefundDto(
                    FineDocNo: txtFineDoc.Text.Trim(),
                    RefundDate: DateTime.Now,
                    Amount: numPayAmount.Value,
                    Mode: cmbPayMode.SelectedItem?.ToString() ?? "CASH",
                    Reason: "MANUAL",
                    ApprovedBy: AppSession.Current.UserCode,
                    Remark: string.IsNullOrWhiteSpace(txtRemark.Text) ? null : txtRemark.Text.Trim()
                );

                await _fines.RefundAsync(dto);

                MessageBox.Show("Refund saved.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await OnRefreshAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadFinesAsync()
        {
            dgvFines.DataSource = await _fines.SearchOpenAsync(txtSearch.Text);
        }

        private async Task BindSelectedFineAsync()
        {
            if (dgvFines.CurrentRow?.DataBoundItem is not MemberFineRowDto row) return;

            txtFineDoc.Text = row.FineDocNo;
            txtMemberCode.Text = row.MemberCode;
            txtRefDoc.Text = row.RefDocNo;
            txtTotal.Text = row.Total.ToString("N2");
            txtPaid.Text = row.Paid.ToString("N2");
            txtBalance.Text = row.Balance.ToString("N2");
            numPayAmount.Value = row.Balance > 0 ? row.Balance : 0;

            var details = await _fines.GetDetailsAsync(row.FineDocNo);

            if (details.Count == 0)
            {
                txtPurpose.Clear();
                txtCalculation.Clear();
                return;
            }

            txtPurpose.Text = string.Join(
                Environment.NewLine,
                details.Select(d =>
                    $"{FineTypeMeaning(d.FineType)}" +
                    $"{(string.IsNullOrWhiteSpace(d.BookCode) ? "" : $" - Book: {d.BookCode}")}")
            );

            txtCalculation.Text = string.Join(
                Environment.NewLine + Environment.NewLine,
                details.Select(d =>
                    $"Type: {FineTypeMeaning(d.FineType)}{Environment.NewLine}" +
                    $"Book: {(string.IsNullOrWhiteSpace(d.BookCode) ? "-" : d.BookCode)}{Environment.NewLine}" +
                    $"Qty: {d.Qty}{Environment.NewLine}" +
                    $"Rate: {d.Rate:N2}{Environment.NewLine}" +
                    $"Amount: {d.Amount:N2}{Environment.NewLine}" +
                    $"Calculation: {(string.IsNullOrWhiteSpace(d.Remark) ? "-" : d.Remark)}")
            );
        }

        private void ClearEntry()
        {
            txtFineDoc.Clear();
            txtMemberCode.Clear();
            txtRefDoc.Clear();
            txtTotal.Clear();
            txtPaid.Clear();
            txtBalance.Clear();
            txtRefNo.Clear();
            txtRemark.Clear();
            txtPurpose.Clear();
            txtCalculation.Clear();
            numPayAmount.Value = 0;
            if (cmbPayMode.Items.Count > 0)
                cmbPayMode.SelectedIndex = 0;
        }

        private void ShowHelp()
        {
            var helpText =
@"Fine Status Codes
P = Pending
T = Partially Paid
D = Paid
R = Refunded
W = Waived
X = Cancelled

Fine Type Codes
O = Overdue Fine
D = Damaged Fine
L = Lost Fine
X = Other Fine

Return Condition Codes
N = Normal
O = Overdue
D = Damaged
L = Lost";

            MessageBox.Show(helpText, "Fine Collection Help",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string FineTypeMeaning(string code) => (code ?? "").Trim().ToUpperInvariant() switch
        {
            "O" => "Overdue Fine",
            "D" => "Damaged Fine",
            "L" => "Lost Fine",
            "X" => "Other Fine",
            _ => code
        };

        private void WireEvents()
        {
            btnSearch.Click += async (_, __) => await LoadFinesAsync();

            btnClearSearch.Click += async (_, __) =>
            {
                txtSearch.Clear();
                await LoadFinesAsync();
            };

            dgvFines.SelectionChanged += async (_, __) => await BindSelectedFineAsync();

            btnHelp.Click += (_, __) => ShowHelp();
            btnClearForm.Click += (_, __) => ClearEntry();
        }

        private void BuildUi()
        {
            BackColor = Color.PapayaWhip;

            cmbPayMode.Items.AddRange(new object[] { "CASH", "CARD", "ONLINE" });
            cmbPayMode.SelectedIndex = 0;

            var gb = new GroupBox
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Text = "Fine Collection"
            };
            Controls.Add(gb);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            gb.Controls.Add(root);

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
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
                Margin = new Padding(0)
            };

            flp.Controls.Add(new Label
            {
                Text = "Fine Search:",
                AutoSize = true,
                Padding = new Padding(0, 7, 0, 0)
            });

            flp.Controls.Add(txtSearch);

            SetupBtn(btnSearch, "Search", Color.SteelBlue);
            SetupBtn(btnClearSearch, "Clear", Color.DimGray);

            btnSearch.Width = 90;
            btnClearSearch.Width = 90;

            flp.Controls.Add(btnSearch);
            flp.Controls.Add(btnClearSearch);

            header.Controls.Add(lblTitle, 0, 0);
            header.Controls.Add(flp, 1, 0);

            root.Controls.Add(header, 0, 0);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };
            body.ColumnStyles.Clear();
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 430F));
            root.Controls.Add(body, 0, 1);

            SetupGrid(dgvFines);

            body.Controls.Add(new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Pending Fines",
                Controls = { dgvFines }
            }, 0, 0);

            var right = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Fine Payment / Refund"
            };

            var form = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            form.ColumnStyles.Clear();
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            form.RowStyles.Clear();
            for (int i = 0; i < 10; i++)
                form.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 65F));   // Purpose
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 125F));  // Calculation
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));   // Bottom buttons
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // filler

            AddRow(form, 0, "Fine Doc", txtFineDoc);
            AddRow(form, 1, "Member", txtMemberCode);
            AddRow(form, 2, "Ref Doc", txtRefDoc);
            AddRow(form, 3, "Total", txtTotal);
            AddRow(form, 4, "Paid", txtPaid);
            AddRow(form, 5, "Balance", txtBalance);
            AddRow(form, 6, "Amount", numPayAmount);
            AddRow(form, 7, "Pay Mode", cmbPayMode);
            AddRow(form, 8, "Ref No", txtRefNo);
            AddRow(form, 9, "Remark", txtRemark);
            AddRow(form, 10, "Purpose", txtPurpose);
            AddRow(form, 11, "Calculation", txtCalculation);

            SetupBtn(btnHelp, "Help", Color.SlateGray);
            SetupBtn(btnClearForm, "Clear", Color.DimGray);

            btnHelp.Dock = DockStyle.Fill;
            btnClearForm.Dock = DockStyle.Fill;

            btnHelp.Margin = new Padding(0, 0, 6, 0);
            btnClearForm.Margin = new Padding(0);

            var bottomButtons = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 34,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            bottomButtons.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            bottomButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            bottomButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            bottomButtons.Controls.Add(btnHelp, 0, 0);
            bottomButtons.Controls.Add(btnClearForm, 1, 0);

            form.Controls.Add(new Label
            {
                Text = "Help / Clear",
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 0)
            }, 0, 12);

            form.Controls.Add(bottomButtons, 1, 12);

            right.Controls.Add(form);
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

        private static void SetupBtn(Button b, string text, Color back)
        {
            b.Text = text;
            b.Height = 34;
            b.MinimumSize = new Size(95, 34);
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