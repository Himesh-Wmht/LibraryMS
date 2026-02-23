using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Interfaces;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win.Pages
{
    public partial class UCBookCategory : UserControl, IPageActions
    {
        private readonly BookCategoryService _service;
        private bool _loading;

        public UCBookCategory(BookCategoryService service)
        {
            InitializeComponent();
            _service = service ?? throw new ArgumentNullException(nameof(service));

            Dock = DockStyle.Fill;

            dgvCats.AutoGenerateColumns = true;
            dgvCats.ReadOnly = true;
            dgvCats.AllowUserToAddRows = false;
            dgvCats.AllowUserToDeleteRows = false;
            dgvCats.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCats.MultiSelect = false;

            dgvCats.SelectionChanged += (_, __) => BindSelected();

            btnSearch.Click += async (_, __) => await LoadGridAsync();
            btnNew.Click += (_, __) => ClearForm();

            Load += UCBookCategory_Load;
        }

        public async Task OnRefreshAsync()
        {
            await LoadGridAsync();
            ClearForm();
        }

        public async Task OnSaveAsync()
        {
            if (!ValidateForm(out var msg))
            {
                MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new BookCategoryUpsertDto(
                Code: txtCode.Text.Trim(),
                Name: txtName.Text.Trim(),
                Active: chkActive.Checked
            );

            await _service.SaveAsync(dto);
            MessageBox.Show("Saved successfully.");

            await LoadGridAsync();
            txtCode.ReadOnly = true;
        }

        public void OnEdit() { }
        public Task OnProcessAsync() => Task.CompletedTask;

        private async void UCBookCategory_Load(object? sender, EventArgs e)
        {
            if (_loading) return;
            _loading = true;
            try { await OnRefreshAsync(); }
            finally { _loading = false; }
        }

        private async Task LoadGridAsync()
        {
            var list = await _service.SearchAsync(txtSearch.Text, chkActiveOnly.Checked);
            dgvCats.DataSource = list;
        }

        private BookCategoryRowDto? Selected => dgvCats.CurrentRow?.DataBoundItem as BookCategoryRowDto;

        private void BindSelected()
        {
            if (Selected == null) return;

            txtCode.ReadOnly = true;
            txtCode.Text = Selected.Code;
            txtName.Text = Selected.Name;
            chkActive.Checked = Selected.Active;
        }

        private void ClearForm()
        {
            txtCode.ReadOnly = false;
            txtCode.Clear();
            txtName.Clear();
            chkActive.Checked = true;
            txtCode.Focus();
        }

        private bool ValidateForm(out string msg)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text)) { msg = "Category Code is required."; return false; }
            if (string.IsNullOrWhiteSpace(txtName.Text)) { msg = "Category Name is required."; return false; }
            msg = "";
            return true;
        }
    }
}
