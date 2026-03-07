using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Interfaces;
using static LibraryMS.DAL.Repositories.Dtos;
using LibraryMS.Win.Helper;

namespace LibraryMS.Win.Pages
{
    public partial class UCBookInventory : UserControl, IPageActions
    {
        private readonly BookInventoryService _service;
        private bool _loading;

        public UCBookInventory(BookInventoryService service)
        {
            InitializeComponent();
            _service = service ?? throw new ArgumentNullException(nameof(service));

            Dock = DockStyle.Fill;

            // grid setup
            dgvInv.AutoGenerateColumns = true;
            dgvInv.ReadOnly = true;
            dgvInv.AllowUserToAddRows = false;
            dgvInv.AllowUserToDeleteRows = false;
            dgvInv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInv.MultiSelect = false;

            dgvInv.SelectionChanged += (_, __) => BindSelected();

            btnSearch.Click += async (_, __) => await LoadGridAsync();
            btnNew.Click += (_, __) => ClearForm();

            txtBookCode.Leave += async (_, __) => await FillTitleAsync();

            Load += UCBookInventory_Load;
            txtBookCode.KeyDown += TxtBookCode_KeyDown;
        }
        private void TxtBookCode_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F2) return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            PickBook();   // open search window
        }
        private async void UCBookInventory_Load(object? sender, EventArgs e)
        {
            if (_loading) return;
            _loading = true;
            try { await OnRefreshAsync(); }
            finally { _loading = false; }
        }
        private void PickBook()
        {
            // Opens search window and returns selected row
            var pick = FrmLookup.Pick(
                owner: this,
                title: "Select Book",
                loader: q => _service.LookupBooksAsync(q)   // you added this in service
            );

            if (pick == null) return;

            txtBookCode.Text = pick.Code;
            txtBookTitle.Text = pick.Name; // book title
            numQty.Focus();
        }
        private string? CurrentLocCode => AppSession.Current?.LocationCode;
        private string? CurrentLocDesc => AppSession.Current?.LocationDesc;

        public async Task OnRefreshAsync()
        {
            if (AppSession.Current == null)
            {
                lblTitle.Text = "Book Inventory";
                lblLoc.Text = "(no session)";
                dgvInv.DataSource = null;
                return;
            }

            lblTitle.Text = $"Book Inventory - {CurrentLocDesc}";
            lblLoc.Text = CurrentLocDesc ?? "";

            chkActiveOnly.Checked = true;

            await LoadGridAsync();
            ClearForm();
        }

        public void OnEdit() { }
        public Task OnProcessAsync() => Task.CompletedTask;

        public async Task OnSaveAsync()
        {
            if (AppSession.Current == null)
            {
                MessageBox.Show("Session missing. Please login again.");
                return;
            }

            if (!ValidateForm(out var msg))
            {
                MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var code = txtBookCode.Text.Trim();

            // verify book exists + fill title
            var title = await _service.GetBookTitleAsync(code);
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Invalid Book Code. Book not found in Catalog.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBookCode.Focus();
                return;
            }
            txtBookTitle.Text = title;

            var dto = new InvUpsertDto(
                BookCode: code,
                LocCode: CurrentLocCode!,
                Qty: (int)numQty.Value,
                Reorder: (int)numReorder.Value,
                Active: chkActive.Checked
            );

            await _service.SaveAsync(dto);

            MessageBox.Show("Saved successfully.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await LoadGridAsync();
            txtBookCode.ReadOnly = true;
        }

        private async Task LoadGridAsync()
        {
            var loc = CurrentLocCode;
            if (string.IsNullOrWhiteSpace(loc))
            {
                dgvInv.DataSource = null;
                return;
            }

            var list = await _service.SearchAsync(loc, txtSearch.Text, chkActiveOnly.Checked);
            dgvInv.DataSource = list;
        }

        private InvRowDto? Selected => dgvInv.CurrentRow?.DataBoundItem as InvRowDto;

        private void BindSelected()
        {
            if (Selected == null) return;

            txtBookCode.ReadOnly = true;

            txtBookCode.Text = Selected.BookCode;
            txtBookTitle.Text = Selected.Title;
            numQty.Value = Selected.Qty;
            numReorder.Value = Selected.Reorder;
            chkActive.Checked = Selected.Active;
        }

        private void ClearForm()
        {
            txtBookCode.ReadOnly = false;

            txtBookCode.Clear();
            txtBookTitle.Clear();
            numQty.Value = 0;
            numReorder.Value = 0;
            chkActive.Checked = true;

            txtBookCode.Focus();
        }

        private bool ValidateForm(out string msg)
        {
            if (AppSession.Current == null) { msg = "Session missing. Please login again."; return false; }
            if (string.IsNullOrWhiteSpace(CurrentLocCode)) { msg = "Location missing in session."; return false; }
            if (string.IsNullOrWhiteSpace(txtBookCode.Text)) { msg = "Book Code is required."; return false; }
            msg = "";
            return true;
        }

        private async Task FillTitleAsync()
        {
            var code = txtBookCode.Text.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                txtBookTitle.Clear();
                return;
            }

            try
            {
                var title = await _service.GetBookTitleAsync(code);
                txtBookTitle.Text = title ?? "";
            }
            catch
            {
                txtBookTitle.Clear();
            }
        }
    }
}