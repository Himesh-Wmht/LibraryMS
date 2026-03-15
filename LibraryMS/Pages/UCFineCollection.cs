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
            ScrollBars = ScrollBars.Vertical,
            WordWrap = true
        };

        private readonly TextBox txtCalculation = new()
        {
            ReadOnly = true,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = true
        };

        private readonly Button btnHelp = new();
        private readonly Button btnClearForm = new();

        // responsive layout fields
        private readonly GroupBox gbMain = new();
        private readonly TableLayoutPanel tlpRoot = new();
        private readonly TableLayoutPanel tlpHeader = new();
        private readonly FlowLayoutPanel flpHeaderActions = new();
        private readonly TableLayoutPanel tlpBody = new();

        private readonly GroupBox grpPendingFines = new();
        private readonly GroupBox grpFineEntry = new();

        private readonly TableLayoutPanel tlpForm = new();
        private readonly FlowLayoutPanel flpBottomButtons = new();

        public UCFineCollection(FineCollectionService fines)
        {
            _fines = fines ?? throw new ArgumentNullException(nameof(fines));

            Dock = DockStyle.Fill;
            BuildUi();
            WireEvents();
            ApplyResponsiveLayout();

            Resize += (_, __) => ApplyResponsiveLayout();
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
            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFineDoc.Text))
            {
                MessageBox.Show("Select a fine.");
                return;
            }

            if (numPayAmount.Value <= 0)
            {
                MessageBox.Show("Enter pay amount.");
                return;
            }

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

                MessageBox.Show(
                    "Fine payment saved.",
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

        public async Task OnProcessAsync()
        {
            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFineDoc.Text))
            {
                MessageBox.Show("Select a fine.");
                return;
            }

            if (numPayAmount.Value <= 0)
            {
                MessageBox.Show("Enter refund amount in Amount field.");
                return;
            }

            var ok = MessageBox.Show(
                "Refund this fine amount?",
                "Refund",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ok != DialogResult.Yes)
                return;

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

                MessageBox.Show(
                    "Refund saved.",
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

        private async Task LoadFinesAsync()
        {
            dgvFines.DataSource = await _fines.SearchOpenAsync(txtSearch.Text);
        }

        private async Task BindSelectedFineAsync()
        {
            if (dgvFines.CurrentRow?.DataBoundItem is not MemberFineRowDto row)
                return;

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

            MessageBox.Show(
                helpText,
                "Fine Collection Help",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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

            txtSearch.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    await LoadFinesAsync();
                }
            };

            dgvFines.SelectionChanged += async (_, __) => await BindSelectedFineAsync();

            btnHelp.Click += (_, __) => ShowHelp();
            btnClearForm.Click += (_, __) => ClearEntry();
        }

        private void BuildUi()
        {
            SuspendLayout();

            BackColor = Color.PapayaWhip;
            Controls.Clear();

            cmbPayMode.Items.Clear();
            cmbPayMode.Items.AddRange(new object[] { "CASH", "CARD", "ONLINE" });
            cmbPayMode.SelectedIndex = 0;

            txtRemark.Multiline = true;
            txtRemark.Height = 60;
            txtRemark.ScrollBars = ScrollBars.Vertical;

            gbMain.Dock = DockStyle.Fill;
            gbMain.Padding = new Padding(10);
            gbMain.Text = "Fine Collection";
            Controls.Add(gbMain);

            // root
            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.Margin = new Padding(0);
            tlpRoot.Padding = new Padding(0);
            tlpRoot.ColumnCount = 1;
            tlpRoot.RowCount = 2;
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
            tlpHeader.Padding = new Padding(8, 10, 8, 6);

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            flpHeaderActions.AutoSize = true;
            flpHeaderActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpHeaderActions.FlowDirection = FlowDirection.LeftToRight;
            flpHeaderActions.WrapContents = true;
            flpHeaderActions.Margin = new Padding(0);
            flpHeaderActions.Padding = new Padding(0);

            flpHeaderActions.Controls.Add(new Label
            {
                Text = "Fine Search:",
                AutoSize = true,
                Padding = new Padding(0, 7, 0, 0),
                Margin = new Padding(0, 0, 8, 0)
            });

            txtSearch.Width = 220;
            txtSearch.Margin = new Padding(0, 0, 8, 0);

            SetupBtn(btnSearch, "Search", Color.SteelBlue);
            SetupBtn(btnClearSearch, "Clear", Color.DimGray);

            btnSearch.Margin = new Padding(0, 0, 8, 0);
            btnClearSearch.Margin = new Padding(0);

            flpHeaderActions.Controls.Add(txtSearch);
            flpHeaderActions.Controls.Add(btnSearch);
            flpHeaderActions.Controls.Add(btnClearSearch);

            tlpRoot.Controls.Add(tlpHeader, 0, 0);

            // body
            tlpBody.Dock = DockStyle.Fill;
            tlpBody.Margin = new Padding(0);
            tlpBody.Padding = new Padding(0);
            tlpRoot.Controls.Add(tlpBody, 0, 1);

            SetupGrid(dgvFines);

            grpPendingFines.Dock = DockStyle.Fill;
            grpPendingFines.Text = "Pending Fines";
            grpPendingFines.Padding = new Padding(6);
            grpPendingFines.Margin = new Padding(3);
            grpPendingFines.Controls.Add(dgvFines);

            grpFineEntry.Dock = DockStyle.Fill;
            grpFineEntry.Text = "Fine Payment / Refund";
            grpFineEntry.Padding = new Padding(6);
            grpFineEntry.Margin = new Padding(3);

            tlpForm.Dock = DockStyle.Fill;
            tlpForm.ColumnCount = 2;
            tlpForm.Padding = new Padding(10);
            tlpForm.Margin = new Padding(0);
            tlpForm.AutoScroll = true;
            tlpForm.ColumnStyles.Clear();
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            tlpForm.RowStyles.Clear();
            for (int i = 0; i < 9; i++)
                tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));   // Remark
            tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));   // Purpose
            tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));  // Calculation
            tlpForm.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // Buttons
            tlpForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // filler

            AddRow(tlpForm, 0, "Fine Doc", txtFineDoc);
            AddRow(tlpForm, 1, "Member", txtMemberCode);
            AddRow(tlpForm, 2, "Ref Doc", txtRefDoc);
            AddRow(tlpForm, 3, "Total", txtTotal);
            AddRow(tlpForm, 4, "Paid", txtPaid);
            AddRow(tlpForm, 5, "Balance", txtBalance);
            AddRow(tlpForm, 6, "Amount", numPayAmount);
            AddRow(tlpForm, 7, "Pay Mode", cmbPayMode);
            AddRow(tlpForm, 8, "Ref No", txtRefNo);
            AddRow(tlpForm, 9, "Remark", txtRemark);
            AddRow(tlpForm, 10, "Purpose", txtPurpose);
            AddRow(tlpForm, 11, "Calculation", txtCalculation);

            SetupBtn(btnHelp, "Help", Color.SlateGray);
            SetupBtn(btnClearForm, "Clear", Color.DimGray);

            btnHelp.AutoSize = true;
            btnHelp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnHelp.Margin = new Padding(0, 0, 8, 8);

            btnClearForm.AutoSize = true;
            btnClearForm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnClearForm.Margin = new Padding(0, 0, 0, 8);

            flpBottomButtons.Dock = DockStyle.Fill;
            flpBottomButtons.FlowDirection = FlowDirection.LeftToRight;
            flpBottomButtons.WrapContents = true;
            flpBottomButtons.AutoSize = true;
            flpBottomButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpBottomButtons.Margin = new Padding(0);
            flpBottomButtons.Padding = new Padding(0);

            flpBottomButtons.Controls.Add(btnHelp);
            flpBottomButtons.Controls.Add(btnClearForm);

            tlpForm.Controls.Add(new Label
            {
                Text = "Help / Clear",
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 0),
                Dock = DockStyle.Fill
            }, 0, 12);

            tlpForm.Controls.Add(flpBottomButtons, 1, 12);

            grpFineEntry.Controls.Add(tlpForm);

            ApplyResponsiveLayout();

            ResumeLayout(true);
        }

        private void ApplyResponsiveLayout()
        {
            if (IsDisposed)
                return;

            bool narrow = Width < 1100;

            SuspendLayout();
            tlpHeader.SuspendLayout();
            tlpBody.SuspendLayout();

            // header
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

            // body
            tlpBody.Controls.Clear();
            tlpBody.ColumnStyles.Clear();
            tlpBody.RowStyles.Clear();

            if (narrow)
            {
                tlpBody.ColumnCount = 1;
                tlpBody.RowCount = 2;

                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
                tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));

                grpFineEntry.MinimumSize = Size.Empty;

                tlpBody.Controls.Add(grpPendingFines, 0, 0);
                tlpBody.Controls.Add(grpFineEntry, 0, 1);
            }
            else
            {
                tlpBody.ColumnCount = 2;
                tlpBody.RowCount = 1;

                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
                tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                grpFineEntry.MinimumSize = new Size(360, 0);

                tlpBody.Controls.Add(grpPendingFines, 0, 0);
                tlpBody.Controls.Add(grpFineEntry, 1, 0);
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