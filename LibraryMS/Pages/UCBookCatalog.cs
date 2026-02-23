using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Interfaces;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win.Pages
{
    public partial class UCBookCatalog : UserControl, IPageActions
    {
        private readonly BookCatalogService _service;
        private bool _loading;
        private bool _isEdit;

        public UCBookCatalog(BookCatalogService service)
        {
            InitializeComponent();
            _service = service ?? throw new ArgumentNullException(nameof(service));

            Dock = DockStyle.Fill;

            // ✅ ensures labels + button captions exist always
            EnsureUi();

            dgvBooks.AutoGenerateColumns = true;
            dgvBooks.ReadOnly = true;
            dgvBooks.AllowUserToAddRows = false;
            dgvBooks.AllowUserToDeleteRows = false;
            dgvBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBooks.MultiSelect = false;

            dgvBooks.SelectionChanged += (_, __) => BindSelected();

            btnSearch.Click += async (_, __) => await LoadGridAsync();
            btnNew.Click += (_, __) => ClearForm();

            // Ensure load is wired even if designer missed it
            Load += UCBookCatalog_Load;
        }

        public async Task OnRefreshAsync()
        {
            EnsureUi(); // ✅ safe to re-run
            await LoadLookupsAsync();
            await LoadGridAsync();
            ClearForm();
        }

        public async Task OnSaveAsync()
        {
            EnsureUi(); // ✅ safe to re-run

            if (!ValidateForm(out var msg))
            {
                MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new BookUpsertDto(
                Code: txtCode.Text.Trim(),
                Title: txtTitle.Text.Trim(),
                Author: NullIfEmpty(txtAuthor.Text),
                Publisher: NullIfEmpty(txtPublisher.Text),
                Isbn: NullIfEmpty(txtIsbn.Text),
                CategoryCode: cmbCategoryForm.SelectedValue?.ToString(),
                Price: numPrice.Value,
                Active: chkActive.Checked
            );

            await _service.SaveAsync(dto);
            MessageBox.Show("Saved successfully.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await LoadGridAsync();
            _isEdit = true;
            txtCode.ReadOnly = true;
        }

        public void OnEdit() { /* selecting grid auto loads */ }
        public Task OnProcessAsync() => Task.CompletedTask;

        private async void UCBookCatalog_Load(object? sender, EventArgs e)
        {
            if (_loading) return;
            _loading = true;
            try { await OnRefreshAsync(); }
            finally { _loading = false; }
        }

        private async Task LoadLookupsAsync()
        {
            var cats = await _service.GetCategoriesAsync();

            // Filter combo: add "ALL"
            var filterList = cats.ToList();
            filterList.Insert(0, new BookCategoryDto(Code: "", Name: "ALL", Active: true));

            cmbCategoryFilter.DisplayMember = "Name";
            cmbCategoryFilter.ValueMember = "Code";
            cmbCategoryFilter.DataSource = filterList;

            cmbCategoryForm.DisplayMember = "Name";
            cmbCategoryForm.ValueMember = "Code";
            cmbCategoryForm.DataSource = cats.ToList();

            chkActiveOnly.Checked = true;
        }

        private async Task LoadGridAsync()
        {
            var cat = cmbCategoryFilter.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(cat)) cat = null;

            var list = await _service.SearchAsync(txtSearch.Text, cat, chkActiveOnly.Checked);
            dgvBooks.DataSource = list;
        }

        private BookRowDto? Selected => dgvBooks.CurrentRow?.DataBoundItem as BookRowDto;

        private void BindSelected()
        {
            if (Selected == null) return;

            _isEdit = true;
            txtCode.ReadOnly = true;

            txtCode.Text = Selected.Code;
            txtTitle.Text = Selected.Title;
            txtAuthor.Text = Selected.Author ?? "";
            txtPublisher.Text = Selected.Publisher ?? "";
            txtIsbn.Text = Selected.Isbn ?? "";
            chkActive.Checked = Selected.Active;
            numPrice.Value = Selected.Price;

            if (!string.IsNullOrWhiteSpace(Selected.CategoryCode))
                cmbCategoryForm.SelectedValue = Selected.CategoryCode;
        }

        private void ClearForm()
        {
            _isEdit = false;
            txtCode.ReadOnly = false;

            txtCode.Clear();
            txtTitle.Clear();
            txtAuthor.Clear();
            txtPublisher.Clear();
            txtIsbn.Clear();
            numPrice.Value = 0;
            chkActive.Checked = true;

            if (cmbCategoryForm.Items.Count > 0) cmbCategoryForm.SelectedIndex = 0;

            txtCode.Focus();
        }

        private bool ValidateForm(out string msg)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text)) { msg = "Book Code is required."; return false; }
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) { msg = "Title is required."; return false; }
            msg = "";
            return true;
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        // -------------------- PERMANENT UI FIXES --------------------

        private void EnsureUi()
        {
            EnsureHeaderButtons();
            EnsureFormLabels();
        }

        private void EnsureHeaderButtons()
        {
            // Designer sometimes wipes Text -> enforce always
            btnSearch.Text = "Search";
            btnNew.Text = "New";

            StyleBtn(btnSearch, Color.SteelBlue);
            StyleBtn(btnNew, Color.DimGray);
        }

        private static void StyleBtn(Button b, Color back)
        {
            b.Width = 100;
            b.Height = 30;
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.UseVisualStyleBackColor = false;
        }

        private void EnsureFormLabels()
        {
            // If labels already exist in col 0, do nothing
            var col0row0 = tblForm.GetControlFromPosition(0, 0);
            if (col0row0 is Label) return;

            tblForm.SuspendLayout();

            // Rebuild table content: labels in col 0, controls in col 1
            tblForm.Controls.Clear();

            AddRow(0, "Code", txtCode);
            AddRow(1, "Title", txtTitle);
            AddRow(2, "Author", txtAuthor);
            AddRow(3, "Publisher", txtPublisher);
            AddRow(4, "ISBN", txtIsbn);
            AddRow(5, "Category", cmbCategoryForm);
            AddRow(6, "Price", numPrice);

            // Active row
            tblForm.Controls.Add(new Label { Text = "", AutoSize = true }, 0, 7);

            chkActive.Text = "Active";
            chkActive.AutoSize = true;
            chkActive.Margin = new Padding(3, 5, 3, 3);

            tblForm.Controls.Add(chkActive, 1, 7);

            tblForm.ResumeLayout();
        }

        private void AddRow(int row, string labelText, Control input)
        {
            var lbl = new Label
            {
                Text = labelText,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Black,
                Padding = new Padding(0, 6, 0, 0),
                Dock = DockStyle.Fill
            };

            input.Dock = DockStyle.Fill;

            tblForm.Controls.Add(lbl, 0, row);
            tblForm.Controls.Add(input, 1, row);
        }
    }
}
