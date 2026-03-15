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
            btnUploadExcel.Click += async (_, __) => await UploadExcelAsync();

            Load += UCBookCatalog_Load;
        }

        public async Task OnRefreshAsync()
        {
            EnsureUi();
            await LoadLookupsAsync();
            await LoadGridAsync();
            ClearForm();
        }

        public async Task OnSaveAsync()
        {
            EnsureUi();

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

        public void OnEdit() { }
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

        private async Task UploadExcelAsync()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select Book Catalog Excel",
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            Cursor = Cursors.WaitCursor;
            try
            {
                var result = await _service.ImportExcelAsync(ofd.FileName);

                if (result.Ok)
                {
                    MessageBox.Show(
                        $"Excel upload completed.\n\n" +
                        $"Catalog upserted: {result.CatalogUpsertCount}\n" +
                        $"Inventory upserted: {result.InventoryUpsertCount}",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadGridAsync();
                    return;
                }

                ShowImportErrors(result.Errors);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ShowImportErrors(System.Collections.Generic.IEnumerable<string> errors)
        {
            var text = string.Join(Environment.NewLine, errors);

            using var frm = new Form
            {
                Text = "Excel Upload Errors",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(900, 550),
                MinimizeBox = false,
                MaximizeBox = false
            };

            var txt = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                Text = text
            };

            frm.Controls.Add(txt);
            frm.ShowDialog(this);
        }

        private bool ValidateForm(out string msg)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text)) { msg = "Book Code is required."; return false; }
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) { msg = "Title is required."; return false; }
            msg = "";
            return true;
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private void EnsureUi()
        {
            EnsureHeaderButtons();
            EnsureFormLabels();
        }

        private void EnsureHeaderButtons()
        {
            btnSearch.Text = "Search";
            btnNew.Text = "New";
            btnUploadExcel.Text = "Upload Excel";

            StyleBtn(btnSearch, Color.SteelBlue);
            StyleBtn(btnNew, Color.DimGray);
            StyleBtn(btnUploadExcel, Color.DarkOrange);
        }

        private static void StyleBtn(Button b, Color back)
        {
            b.Width = 110;
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
            var col0row0 = tblForm.GetControlFromPosition(0, 0);
            if (col0row0 is Label) return;

            tblForm.SuspendLayout();
            tblForm.Controls.Clear();

            AddRow(0, "Code", txtCode);
            AddRow(1, "Title", txtTitle);
            AddRow(2, "Author", txtAuthor);
            AddRow(3, "Publisher", txtPublisher);
            AddRow(4, "ISBN", txtIsbn);
            AddRow(5, "Category", cmbCategoryForm);
            AddRow(6, "Price", numPrice);

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