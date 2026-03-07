using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win
{
    public partial class FrmLookup : Form
    {
        private readonly Func<string?, Task<List<LookupItemDto>>> _loader;

        private readonly TextBox txtSearch = new() { Width = 260 };
        private readonly Button btnSearch = new();
        private readonly Button btnSelect = new();
        private readonly Button btnCancel = new();
        private readonly DataGridView dgv = new();
        private readonly Label lblHint = new();

        public LookupItemDto? SelectedItem { get; private set; }

        public FrmLookup(string title, Func<string?, Task<List<LookupItemDto>>> loader)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            Text = title;

            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(800, 520);
            MinimizeBox = false;
            MaximizeBox = false;
            KeyPreview = true;

            BuildUi();
            WireEvents();
        }

        // ✅ Static helper you wanted
        public static LookupItemDto? Pick(
            IWin32Window owner,
            string title,
            Func<string?, Task<List<LookupItemDto>>> loader)
        {
            using var f = new FrmLookup(title, loader);
            return f.ShowDialog(owner) == DialogResult.OK ? f.SelectedItem : null;
        }

        private void BuildUi()
        {
            BackColor = Color.PapayaWhip;

            var panelRoot = new Panel { Dock = DockStyle.Fill, BackColor = Color.PapayaWhip };
            Controls.Add(panelRoot);

            var gb = new GroupBox { Dock = DockStyle.Fill, Padding = new Padding(10), Text = Text };
            panelRoot.Controls.Add(gb);

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            gb.Controls.Add(tlp);

            // Header
            var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(6, 10, 6, 0) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlp.Controls.Add(header, 0, 0);

            var lblTitle = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.Teal,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = Text
            };

            var flp = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };

            flp.Controls.Add(new Label { Text = "Search:", AutoSize = true, Padding = new Padding(0, 7, 0, 0) });
            flp.Controls.Add(txtSearch);

            SetupBtn(btnSearch, "Search", Color.SteelBlue);
            SetupBtn(btnSelect, "Select", Color.SeaGreen);
            SetupBtn(btnCancel, "Cancel", Color.DimGray);

            flp.Controls.Add(btnSearch);
            flp.Controls.Add(btnSelect);
            flp.Controls.Add(btnCancel);

            header.Controls.Add(lblTitle, 0, 0);
            header.Controls.Add(flp, 1, 0);

            // Grid
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AutoGenerateColumns = true;

            lblHint.Dock = DockStyle.Bottom;
            lblHint.Height = 22;
            lblHint.Text = "Tip: Double-click a row or press Select. (Esc = Cancel)";
            lblHint.Padding = new Padding(6, 2, 0, 0);

            var body = new Panel { Dock = DockStyle.Fill };
            body.Controls.Add(dgv);
            body.Controls.Add(lblHint);

            tlp.Controls.Add(body, 0, 1);
        }

        private void WireEvents()
        {
            Shown += async (_, __) => await ReloadAsync();

            btnSearch.Click += async (_, __) => await ReloadAsync();
            btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };
            btnSelect.Click += (_, __) => SelectCurrent();
            dgv.CellDoubleClick += (_, __) => SelectCurrent();

            txtSearch.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    await ReloadAsync();
                }
            };

            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            };
        }

        private async Task ReloadAsync()
        {
            try
            {
                UseWaitCursor = true;
                btnSearch.Enabled = btnSelect.Enabled = false;

                var list = await _loader(txtSearch.Text);
                dgv.DataSource = list;

                if (dgv.Columns["Code"] != null) dgv.Columns["Code"].HeaderText = "Code";
                if (dgv.Columns["Name"] != null) dgv.Columns["Name"].HeaderText = "Name";
                if (dgv.Columns["Extra"] != null) dgv.Columns["Extra"].HeaderText = "Extra";
            }
            finally
            {
                UseWaitCursor = false;
                btnSearch.Enabled = btnSelect.Enabled = true;
            }
        }

        private void SelectCurrent()
        {
            SelectedItem = dgv.CurrentRow?.DataBoundItem as LookupItemDto;
            if (SelectedItem == null) return;

            DialogResult = DialogResult.OK;
            Close();
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
            b.Margin = new Padding(8, 0, 0, 0);
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.UseVisualStyleBackColor = false;
        }
    }
}