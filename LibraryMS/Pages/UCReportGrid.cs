using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.Win.Helper;
using LibraryMS.Win.Interfaces;

namespace LibraryMS.Win.Pages
{
    public partial class UCReportGrid : UserControl, IPageActions
    {
        private readonly string _title;
        private readonly Func<DateTime?, DateTime?, int, Task<DataTable>> _loader;

        private readonly Label lblTitle = new();
        private readonly DateTimePicker dtFrom = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
        private readonly DateTimePicker dtTo = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
        private readonly NumericUpDown numTop = new() { Minimum = 1, Maximum = 500, Value = 20 };
        private readonly Label lblTop = new() { Text = "Top N", AutoSize = true };

        private readonly Button btnReload = new();
        private readonly Button btnExportExcel = new();
        private readonly Button btnExportPdf = new();

        private readonly DataGridView dgv = new();

        private readonly bool _useDates;
        private readonly bool _useTop;

        private DataTable? _currentData;

        public UCReportGrid(
            string title,
            Func<DateTime?, DateTime?, int, Task<DataTable>> loader,
            bool useDates = false,
            bool useTop = false)
        {
            _title = title ?? throw new ArgumentNullException(nameof(title));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _useDates = useDates;
            _useTop = useTop;

            Dock = DockStyle.Fill;
            BuildUi();
            WireEvents();

            Load += async (_, __) => await OnRefreshAsync();
        }

        // Top bar Refresh should reset filters and load ALL data
        public async Task OnRefreshAsync()
        {
            lblTitle.Text = _title;

            if (_useDates)
            {
                dtFrom.Checked = false;
                dtTo.Checked = false;
            }

            if (_useTop)
                numTop.Value = 20;

            await LoadDataAsync(useCurrentFilters: false);
        }

        public void OnEdit() { }
        public Task OnSaveAsync() => Task.CompletedTask;
        public Task OnProcessAsync() => Task.CompletedTask;

        private async Task LoadDataAsync(bool useCurrentFilters)
        {
            DateTime? from = null;
            DateTime? to = null;
            var topN = 0;

            if (useCurrentFilters)
            {
                if (_useDates)
                {
                    from = dtFrom.Checked ? dtFrom.Value.Date : (DateTime?)null;
                    to = dtTo.Checked ? dtTo.Value.Date : (DateTime?)null;
                }

                if (_useTop)
                    topN = (int)numTop.Value;
            }

            _currentData = await _loader(from, to, topN);
            dgv.DataSource = _currentData;
        }

        private void WireEvents()
        {
            btnReload.Click += async (_, __) => await LoadDataAsync(useCurrentFilters: true);

            btnExportExcel.Click += (_, __) => ExportExcel();
            btnExportPdf.Click += (_, __) => ExportPdf();
        }

        private void ExportExcel()
        {
            if (_currentData == null || _currentData.Rows.Count == 0)
            {
                MessageBox.Show("No data available to export.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"{SanitizeFileName(_title)}_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                ReportExportHelper.ExportToExcel(_currentData, sfd.FileName, "Report");
                MessageBox.Show("Excel export completed.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportPdf()
        {
            if (_currentData == null || _currentData.Rows.Count == 0)
            {
                MessageBox.Show("No data available to export.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF File (*.pdf)|*.pdf",
                FileName = $"{SanitizeFileName(_title)}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                ReportExportHelper.ExportToPdf(_currentData, sfd.FileName, _title);
                MessageBox.Show("PDF export completed.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string SanitizeFileName(string value)
        {
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');

            return value.Replace(" ", "_");
        }

        private void BuildUi()
        {
            BackColor = Color.PapayaWhip;

            var gb = new GroupBox
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Text = "Report Viewer"
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

            var header = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(6, 10, 6, 0)
            };

            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.Margin = new Padding(0, 7, 18, 0);
            header.Controls.Add(lblTitle);

            if (_useDates)
            {
                header.Controls.Add(new Label { Text = "From", AutoSize = true, Padding = new Padding(0, 7, 0, 0) });
                header.Controls.Add(dtFrom);

                header.Controls.Add(new Label { Text = "To", AutoSize = true, Padding = new Padding(10, 7, 0, 0) });
                header.Controls.Add(dtTo);
            }

            if (_useTop)
            {
                lblTop.Margin = new Padding(10, 7, 0, 0);
                header.Controls.Add(lblTop);
                numTop.Width = 70;
                header.Controls.Add(numTop);
            }

            SetupBtn(btnReload, "Reload", Color.SteelBlue);
            SetupBtn(btnExportExcel, "Export Excel", Color.SeaGreen);
            SetupBtn(btnExportPdf, "Export PDF", Color.IndianRed);

            btnExportExcel.Width = 110;
            btnExportPdf.Width = 110;

            header.Controls.Add(btnReload);
            header.Controls.Add(btnExportExcel);
            header.Controls.Add(btnExportPdf);

            root.Controls.Add(header, 0, 0);

            SetupGrid(dgv);
            root.Controls.Add(dgv, 0, 1);
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
            b.Width = 90;
            b.Height = 30;
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Margin = new Padding(10, 0, 0, 0);
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.UseVisualStyleBackColor = false;
        }
    }
}