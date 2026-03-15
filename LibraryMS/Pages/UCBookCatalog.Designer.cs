#nullable disable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryMS.Win.Pages
{
    partial class UCBookCatalog
    {
        private IContainer components = null;

        private Panel panelRoot;
        private GroupBox gbCatalog;
        private TableLayoutPanel tlpRoot;

        private TableLayoutPanel tlpHeader;
        private Label lblTitle;
        private FlowLayoutPanel flpFilters;

        private Label lblSearch;
        private TextBox txtSearch;
        private Label lblCat;
        private ComboBox cmbCategoryFilter;
        private CheckBox chkActiveOnly;
        private Button btnSearch;
        private Button btnNew;
        private Button btnUploadExcel;

        private TableLayoutPanel tlpBody;
        private DataGridView dgvBooks;

        private GroupBox grpForm;
        private TableLayoutPanel tblForm;

        private TextBox txtCode;
        private TextBox txtTitle;
        private TextBox txtAuthor;
        private TextBox txtPublisher;
        private TextBox txtIsbn;
        private ComboBox cmbCategoryForm;
        private NumericUpDown numPrice;
        private CheckBox chkActive;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelRoot = new Panel();
            gbCatalog = new GroupBox();
            tlpRoot = new TableLayoutPanel();
            tlpHeader = new TableLayoutPanel();
            lblTitle = new Label();
            flpFilters = new FlowLayoutPanel();
            lblSearch = new Label();
            txtSearch = new TextBox();
            lblCat = new Label();
            cmbCategoryFilter = new ComboBox();
            chkActiveOnly = new CheckBox();
            btnSearch = new Button();
            btnNew = new Button();
            btnUploadExcel = new Button();
            tlpBody = new TableLayoutPanel();
            dgvBooks = new DataGridView();
            grpForm = new GroupBox();
            tblForm = new TableLayoutPanel();
            txtCode = new TextBox();
            txtTitle = new TextBox();
            txtAuthor = new TextBox();
            txtPublisher = new TextBox();
            txtIsbn = new TextBox();
            cmbCategoryForm = new ComboBox();
            numPrice = new NumericUpDown();
            chkActive = new CheckBox();
            panelRoot.SuspendLayout();
            gbCatalog.SuspendLayout();
            tlpRoot.SuspendLayout();
            tlpHeader.SuspendLayout();
            flpFilters.SuspendLayout();
            tlpBody.SuspendLayout();
            ((ISupportInitialize)dgvBooks).BeginInit();
            grpForm.SuspendLayout();
            tblForm.SuspendLayout();
            ((ISupportInitialize)numPrice).BeginInit();
            SuspendLayout();
            // 
            // panelRoot
            // 
            panelRoot.BackColor = Color.PapayaWhip;
            panelRoot.Controls.Add(gbCatalog);
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.Location = new Point(0, 0);
            panelRoot.Name = "panelRoot";
            panelRoot.Size = new Size(980, 600);
            panelRoot.TabIndex = 0;
            // 
            // gbCatalog
            // 
            gbCatalog.Controls.Add(tlpRoot);
            gbCatalog.Dock = DockStyle.Fill;
            gbCatalog.Location = new Point(0, 0);
            gbCatalog.Name = "gbCatalog";
            gbCatalog.Padding = new Padding(10);
            gbCatalog.Size = new Size(980, 600);
            gbCatalog.TabIndex = 0;
            gbCatalog.TabStop = false;
            gbCatalog.Text = "Book Catalog";
            // 
            // tlpRoot
            // 
            tlpRoot.ColumnCount = 1;
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpHeader, 0, 0);
            tlpRoot.Controls.Add(tlpBody, 0, 1);
            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.Location = new Point(10, 26);
            tlpRoot.Name = "tlpRoot";
            tlpRoot.RowCount = 2;
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRoot.Size = new Size(960, 564);
            tlpRoot.TabIndex = 0;
            // 
            // tlpHeader
            // 
            tlpHeader.ColumnCount = 2;
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle());
            tlpHeader.Controls.Add(lblTitle, 0, 0);
            tlpHeader.Controls.Add(flpFilters, 1, 0);
            tlpHeader.Dock = DockStyle.Fill;
            tlpHeader.Location = new Point(3, 3);
            tlpHeader.Name = "tlpHeader";
            tlpHeader.Padding = new Padding(6, 10, 6, 0);
            tlpHeader.RowCount = 1;
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpHeader.Size = new Size(954, 50);
            tlpHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.Location = new Point(9, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(14, 40);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Book Catalog";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // flpFilters
            // 
            flpFilters.AutoSize = true;
            flpFilters.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpFilters.Controls.Add(lblSearch);
            flpFilters.Controls.Add(txtSearch);
            flpFilters.Controls.Add(lblCat);
            flpFilters.Controls.Add(cmbCategoryFilter);
            flpFilters.Controls.Add(chkActiveOnly);
            flpFilters.Controls.Add(btnSearch);
            flpFilters.Controls.Add(btnNew);
            flpFilters.Controls.Add(btnUploadExcel);
            flpFilters.Dock = DockStyle.Fill;
            flpFilters.Location = new Point(26, 10);
            flpFilters.Margin = new Padding(0);
            flpFilters.Name = "flpFilters";
            flpFilters.Size = new Size(922, 40);
            flpFilters.TabIndex = 1;
            flpFilters.WrapContents = false;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(3, 0);
            lblSearch.Name = "lblSearch";
            lblSearch.Padding = new Padding(0, 7, 0, 0);
            lblSearch.Size = new Size(45, 22);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(57, 3);
            txtSearch.Margin = new Padding(6, 3, 0, 0);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(200, 23);
            txtSearch.TabIndex = 1;
            // 
            // lblCat
            // 
            lblCat.AutoSize = true;
            lblCat.Location = new Point(260, 0);
            lblCat.Name = "lblCat";
            lblCat.Padding = new Padding(12, 7, 0, 0);
            lblCat.Size = new Size(70, 22);
            lblCat.TabIndex = 2;
            lblCat.Text = "Category:";
            // 
            // cmbCategoryFilter
            // 
            cmbCategoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategoryFilter.Location = new Point(339, 3);
            cmbCategoryFilter.Margin = new Padding(6, 3, 0, 0);
            cmbCategoryFilter.Name = "cmbCategoryFilter";
            cmbCategoryFilter.Size = new Size(160, 23);
            cmbCategoryFilter.TabIndex = 3;
            // 
            // chkActiveOnly
            // 
            chkActiveOnly.AutoSize = true;
            chkActiveOnly.Location = new Point(505, 3);
            chkActiveOnly.Margin = new Padding(6, 3, 0, 0);
            chkActiveOnly.Name = "chkActiveOnly";
            chkActiveOnly.Padding = new Padding(12, 5, 0, 0);
            chkActiveOnly.Size = new Size(99, 24);
            chkActiveOnly.TabIndex = 4;
            chkActiveOnly.Text = "Active Only";
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(607, 3);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(90, 30);
            btnSearch.TabIndex = 5;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(703, 3);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(90, 30);
            btnNew.TabIndex = 6;
            // 
            // btnUploadExcel
            // 
            btnUploadExcel.Location = new Point(799, 3);
            btnUploadExcel.Name = "btnUploadExcel";
            btnUploadExcel.Size = new Size(120, 30);
            btnUploadExcel.TabIndex = 7;
            // 
            // tlpBody
            // 
            tlpBody.ColumnCount = 2;
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            tlpBody.Controls.Add(dgvBooks, 0, 0);
            tlpBody.Controls.Add(grpForm, 1, 0);
            tlpBody.Dock = DockStyle.Fill;
            tlpBody.Location = new Point(3, 59);
            tlpBody.Name = "tlpBody";
            tlpBody.RowCount = 1;
            tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpBody.Size = new Size(954, 502);
            tlpBody.TabIndex = 1;
            // 
            // dgvBooks
            // 
            dgvBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBooks.BackgroundColor = Color.White;
            dgvBooks.Dock = DockStyle.Fill;
            dgvBooks.Location = new Point(3, 3);
            dgvBooks.MultiSelect = false;
            dgvBooks.Name = "dgvBooks";
            dgvBooks.RowHeadersVisible = false;
            dgvBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBooks.Size = new Size(642, 496);
            dgvBooks.TabIndex = 0;
            // 
            // grpForm
            // 
            grpForm.Controls.Add(tblForm);
            grpForm.Dock = DockStyle.Fill;
            grpForm.Location = new Point(651, 3);
            grpForm.Name = "grpForm";
            grpForm.Padding = new Padding(10);
            grpForm.Size = new Size(300, 496);
            grpForm.TabIndex = 1;
            grpForm.TabStop = false;
            grpForm.Text = "Book Details";
            // 
            // tblForm
            // 
            tblForm.ColumnCount = 2;
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tblForm.Controls.Add(txtCode, 1, 0);
            tblForm.Controls.Add(txtTitle, 1, 1);
            tblForm.Controls.Add(txtAuthor, 1, 2);
            tblForm.Controls.Add(txtPublisher, 1, 3);
            tblForm.Controls.Add(txtIsbn, 1, 4);
            tblForm.Controls.Add(cmbCategoryForm, 1, 5);
            tblForm.Controls.Add(numPrice, 1, 6);
            tblForm.Controls.Add(chkActive, 1, 7);
            tblForm.Dock = DockStyle.Fill;
            tblForm.Location = new Point(10, 26);
            tblForm.Name = "tblForm";
            tblForm.RowCount = 8;
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblForm.Size = new Size(280, 460);
            tblForm.TabIndex = 0;
            // 
            // txtCode
            // 
            txtCode.Dock = DockStyle.Fill;
            txtCode.Location = new Point(101, 3);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(176, 23);
            txtCode.TabIndex = 0;
            // 
            // txtTitle
            // 
            txtTitle.Dock = DockStyle.Fill;
            txtTitle.Location = new Point(101, 33);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(176, 23);
            txtTitle.TabIndex = 1;
            // 
            // txtAuthor
            // 
            txtAuthor.Dock = DockStyle.Fill;
            txtAuthor.Location = new Point(101, 63);
            txtAuthor.Name = "txtAuthor";
            txtAuthor.Size = new Size(176, 23);
            txtAuthor.TabIndex = 2;
            // 
            // txtPublisher
            // 
            txtPublisher.Dock = DockStyle.Fill;
            txtPublisher.Location = new Point(101, 93);
            txtPublisher.Name = "txtPublisher";
            txtPublisher.Size = new Size(176, 23);
            txtPublisher.TabIndex = 3;
            // 
            // txtIsbn
            // 
            txtIsbn.Dock = DockStyle.Fill;
            txtIsbn.Location = new Point(101, 123);
            txtIsbn.Name = "txtIsbn";
            txtIsbn.Size = new Size(176, 23);
            txtIsbn.TabIndex = 4;
            // 
            // cmbCategoryForm
            // 
            cmbCategoryForm.Dock = DockStyle.Fill;
            cmbCategoryForm.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategoryForm.Location = new Point(101, 153);
            cmbCategoryForm.Name = "cmbCategoryForm";
            cmbCategoryForm.Size = new Size(176, 23);
            cmbCategoryForm.TabIndex = 5;
            // 
            // numPrice
            // 
            numPrice.DecimalPlaces = 2;
            numPrice.Dock = DockStyle.Left;
            numPrice.Location = new Point(101, 183);
            numPrice.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
            numPrice.Name = "numPrice";
            numPrice.Size = new Size(140, 23);
            numPrice.TabIndex = 6;
            // 
            // chkActive
            // 
            chkActive.AutoSize = true;
            chkActive.Location = new Point(101, 215);
            chkActive.Margin = new Padding(3, 5, 3, 3);
            chkActive.Name = "chkActive";
            chkActive.Size = new Size(59, 19);
            chkActive.TabIndex = 8;
            chkActive.Text = "Active";
            // 
            // UCBookCatalog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelRoot);
            Name = "UCBookCatalog";
            Size = new Size(980, 600);
            panelRoot.ResumeLayout(false);
            gbCatalog.ResumeLayout(false);
            tlpRoot.ResumeLayout(false);
            tlpHeader.ResumeLayout(false);
            tlpHeader.PerformLayout();
            flpFilters.ResumeLayout(false);
            flpFilters.PerformLayout();
            tlpBody.ResumeLayout(false);
            ((ISupportInitialize)dgvBooks).EndInit();
            grpForm.ResumeLayout(false);
            tblForm.ResumeLayout(false);
            tblForm.PerformLayout();
            ((ISupportInitialize)numPrice).EndInit();
            ResumeLayout(false);
        }

        private static Label MakeFormLabel(string text)
        {
            return new Label
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Black,
                Padding = new Padding(0, 6, 0, 0)
            };
        }

        private static void SetupBtn(Button b, string text, Color back)
        {
            b.Text = text;
            b.Width = 100;
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