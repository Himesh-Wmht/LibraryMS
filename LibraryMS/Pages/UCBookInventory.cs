using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Interfaces;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win.Pages
{
    public sealed partial class UCBookInventory : UserControl, IPageActions
    {
        private readonly BookInventoryService _service;

        private readonly DataGridView dgv = new();
        private readonly TextBox txtSearch = new();
        private readonly CheckBox chkActiveOnly = new() { Text = "Active Only", Checked = true };
        private readonly Button btnSearch = new() { Text = "Search" };
        private readonly Button btnNew = new() { Text = "New" };

        private readonly TextBox txtBookCode = new();
        private readonly NumericUpDown numQty = new() { Minimum = 0, Maximum = 100000 };
        private readonly NumericUpDown numReorder = new() { Minimum = 0, Maximum = 100000 };
        private readonly CheckBox chkActive = new() { Text = "Active", Checked = true };
        private readonly Label lblLoc = new() { AutoSize = true };

        private bool _loading;
        private bool _isEdit;

        public UCBookInventory(BookInventoryService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Dock = DockStyle.Fill;

            BuildUi();
            WireEvents();

            Load += async (_, __) => await OnRefreshAsync();
        }

        private void BuildUi()
        {
            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            txtSearch.Width = 240;

            top.Controls.Add(new Label { Text = "Search:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
            top.Controls.Add(txtSearch);
            top.Controls.Add(chkActiveOnly);
            top.Controls.Add(btnSearch);
            top.Controls.Add(btnNew);

            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoGenerateColumns = true;

            var form = new TableLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 330,
                Padding = new Padding(10),
                ColumnCount = 2,
                RowCount = 6
            };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            form.Controls.Add(new Label { Text = "Location:", AutoSize = true }, 0, 0);
            form.Controls.Add(lblLoc, 1, 0);

            form.Controls.Add(new Label { Text = "Book Code:", AutoSize = true }, 0, 1);
            form.Controls.Add(txtBookCode, 1, 1);

            form.Controls.Add(new Label { Text = "Qty:", AutoSize = true }, 0, 2);
            form.Controls.Add(numQty, 1, 2);

            form.Controls.Add(new Label { Text = "Reorder:", AutoSize = true }, 0, 3);
            form.Controls.Add(numReorder, 1, 3);

            form.Controls.Add(new Label { Text = "", AutoSize = true }, 0, 4);
            form.Controls.Add(chkActive, 1, 4);

            Controls.Add(dgv);
            Controls.Add(form);
            Controls.Add(top);
        }

        private void WireEvents()
        {
            btnSearch.Click += async (_, __) => await LoadGridAsync();
            btnNew.Click += (_, __) => ClearForm();
            dgv.SelectionChanged += (_, __) => BindSelected();
        }

        private string? CurrentLocCode => AppSession.Current?.LocationCode;

        public async Task OnRefreshAsync()
        {
            if (_loading) return;
            _loading = true;
            try
            {
                lblLoc.Text = AppSession.Current?.LocationDesc ?? "(no session)";
                await LoadGridAsync();
                ClearForm();
            }
            finally { _loading = false; }
        }

        public void OnEdit()
        {
            // selection already binds to form
        }

        public async Task OnSaveAsync()
        {
            if (!ValidateForm(out var msg))
            {
                MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var loc = CurrentLocCode!;
            var dto = new InvUpsertDto(
                BookCode: txtBookCode.Text.Trim(),
                LocCode: loc,
                Qty: (int)numQty.Value,
                Reorder: (int)numReorder.Value,
                Active: chkActive.Checked
            );

            await _service.SaveAsync(dto);
            MessageBox.Show("Saved successfully.");

            await LoadGridAsync();
            _isEdit = true;
            txtBookCode.ReadOnly = true;
        }

        public Task OnProcessAsync() => Task.CompletedTask;

        private async Task LoadGridAsync()
        {
            var loc = CurrentLocCode;
            if (string.IsNullOrWhiteSpace(loc))
            {
                dgv.DataSource = null;
                return;
            }

            var list = await _service.SearchAsync(loc, txtSearch.Text, chkActiveOnly.Checked);
            dgv.DataSource = list;
        }

        private InvRowDto? Selected => dgv.CurrentRow?.DataBoundItem as InvRowDto;

        private void BindSelected()
        {
            if (Selected == null) return;

            _isEdit = true;
            txtBookCode.ReadOnly = true;

            txtBookCode.Text = Selected.BookCode;
            numQty.Value = Selected.Qty;
            numReorder.Value = Selected.Reorder;
            chkActive.Checked = Selected.Active;
        }

        private void ClearForm()
        {
            _isEdit = false;
            txtBookCode.ReadOnly = false;

            txtBookCode.Clear();
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
    }
}
