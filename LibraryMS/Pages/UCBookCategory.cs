using System;
using System.Drawing;
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

            EnsureUi();
            ApplyResponsiveLayout();

            dgvCats.AutoGenerateColumns = true;
            dgvCats.ReadOnly = true;
            dgvCats.AllowUserToAddRows = false;
            dgvCats.AllowUserToDeleteRows = false;
            dgvCats.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCats.MultiSelect = false;

            dgvCats.SelectionChanged += (_, __) => BindSelected();

            btnSearch.Click += async (_, __) => await LoadGridAsync();
            btnNew.Click += (_, __) => ClearForm();

            txtSearch.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    await LoadGridAsync();
                }
            };

            Resize += (_, __) => ApplyResponsiveLayout();
            Load += UCBookCategory_Load;
        }

        public async Task OnRefreshAsync()
        {
            EnsureUi();
            ApplyResponsiveLayout();
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
            try
            {
                await OnRefreshAsync();
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task LoadGridAsync()
        {
            var list = await _service.SearchAsync(txtSearch.Text, chkActiveOnly.Checked);
            dgvCats.DataSource = list;
        }

        private BookCategoryRowDto? Selected =>
            dgvCats.CurrentRow?.DataBoundItem as BookCategoryRowDto;

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
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                msg = "Category Code is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                msg = "Category Name is required.";
                return false;
            }

            msg = "";
            return true;
        }

        // ---------- UI / Responsive ----------

        private void EnsureUi()
        {
            EnsureHeaderButtons();
            EnsureFormLabels();
            StyleGrid();
        }

        private void EnsureHeaderButtons()
        {
            lblTitle.Text = "Book Categories";
            lblSearch.Text = "Search:";
            btnSearch.Text = "Search";
            btnNew.Text = "New";

            StyleBtn(btnSearch, Color.SteelBlue);
            StyleBtn(btnNew, Color.DimGray);
        }

        private static void StyleBtn(Button b, Color back)
        {
            b.Width = 100;
            b.Height = 30;
            b.MinimumSize = new Size(95, 30);
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.UseVisualStyleBackColor = false;
        }

        private void StyleGrid()
        {
            dgvCats.BackgroundColor = Color.White;
            dgvCats.BorderStyle = BorderStyle.FixedSingle;
            dgvCats.RowHeadersVisible = false;
            dgvCats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCats.Dock = DockStyle.Fill;
        }

        private void EnsureFormLabels()
        {
            var col0row0 = tblForm.GetControlFromPosition(0, 0);
            if (col0row0 is Label)
                return;

            tblForm.SuspendLayout();
            tblForm.Controls.Clear();

            AddRow(0, "Code", txtCode);
            AddRow(1, "Name", txtName);

            tblForm.Controls.Add(new Label
            {
                Text = "",
                AutoSize = true
            }, 0, 2);

            chkActive.Text = "Active";
            chkActive.AutoSize = true;
            chkActive.Margin = new Padding(3, 5, 3, 3);
            tblForm.Controls.Add(chkActive, 1, 2);

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

        private void ApplyResponsiveLayout()
        {
            if (IsDisposed)
                return;

            bool narrow = Width < 980;

            SuspendLayout();
            tlpHeader.SuspendLayout();
            tlpBody.SuspendLayout();

            // Header
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
                flpActions.Dock = DockStyle.Top;
                flpActions.WrapContents = true;

                tlpHeader.Controls.Add(lblTitle, 0, 0);
                tlpHeader.Controls.Add(flpActions, 0, 1);
            }
            else
            {
                tlpHeader.ColumnCount = 2;
                tlpHeader.RowCount = 1;

                tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                tlpHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                lblTitle.Margin = new Padding(0);
                flpActions.Dock = DockStyle.Fill;
                flpActions.WrapContents = false;

                tlpHeader.Controls.Add(lblTitle, 0, 0);
                tlpHeader.Controls.Add(flpActions, 1, 0);
            }

            // Body
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

                tlpBody.Controls.Add(dgvCats, 0, 0);
                tlpBody.Controls.Add(grpForm, 0, 1);
            }
            else
            {
                tlpBody.ColumnCount = 2;
                tlpBody.RowCount = 1;

                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
                tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
                tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                grpForm.MinimumSize = new Size(260, 0);

                tlpBody.Controls.Add(dgvCats, 0, 0);
                tlpBody.Controls.Add(grpForm, 1, 0);
            }

            tlpBody.ResumeLayout(true);
            tlpHeader.ResumeLayout(true);
            ResumeLayout(true);
        }
    }
}