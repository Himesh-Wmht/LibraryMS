#nullable disable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryMS.Win.Pages
{
    partial class UCBookCategory
    {
        private IContainer components = null;

        private Panel panelRoot;
        private GroupBox gbCats;
        private TableLayoutPanel tlpRoot;

        private TableLayoutPanel tlpHeader;
        private Label lblTitle;
        private FlowLayoutPanel flpActions;

        private Label lblSearch;
        private TextBox txtSearch;
        private CheckBox chkActiveOnly;
        private Button btnSearch;
        private Button btnNew;

        private TableLayoutPanel tlpBody;
        private DataGridView dgvCats;

        private GroupBox grpForm;
        private TableLayoutPanel tblForm;
        private TextBox txtCode;
        private TextBox txtName;
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
            gbCats = new GroupBox();
            tlpRoot = new TableLayoutPanel();
            tlpHeader = new TableLayoutPanel();
            lblTitle = new Label();
            flpActions = new FlowLayoutPanel();
            lblSearch = new Label();
            txtSearch = new TextBox();
            chkActiveOnly = new CheckBox();
            btnSearch = new Button();
            btnNew = new Button();
            tlpBody = new TableLayoutPanel();
            dgvCats = new DataGridView();
            grpForm = new GroupBox();
            tblForm = new TableLayoutPanel();
            txtCode = new TextBox();
            txtName = new TextBox();
            chkActive = new CheckBox();
            panelRoot.SuspendLayout();
            gbCats.SuspendLayout();
            tlpRoot.SuspendLayout();
            tlpHeader.SuspendLayout();
            flpActions.SuspendLayout();
            tlpBody.SuspendLayout();
            ((ISupportInitialize)dgvCats).BeginInit();
            grpForm.SuspendLayout();
            tblForm.SuspendLayout();
            SuspendLayout();
            // 
            // panelRoot
            // 
            panelRoot.BackColor = Color.PapayaWhip;
            panelRoot.Controls.Add(gbCats);
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.Location = new Point(0, 0);
            panelRoot.Name = "panelRoot";
            panelRoot.Size = new Size(900, 600);
            panelRoot.TabIndex = 0;
            // 
            // gbCats
            // 
            gbCats.Controls.Add(tlpRoot);
            gbCats.Dock = DockStyle.Fill;
            gbCats.Location = new Point(0, 0);
            gbCats.Name = "gbCats";
            gbCats.Padding = new Padding(10);
            gbCats.Size = new Size(900, 600);
            gbCats.TabIndex = 0;
            gbCats.TabStop = false;
            gbCats.Text = "Book Categories";
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
            tlpRoot.RowStyles.Add(new RowStyle());
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRoot.Size = new Size(880, 564);
            tlpRoot.TabIndex = 0;
            // 
            // tlpHeader
            // 
            tlpHeader.AutoSize = true;
            tlpHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpHeader.ColumnCount = 2;
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle());
            tlpHeader.Controls.Add(lblTitle, 0, 0);
            tlpHeader.Controls.Add(flpActions, 1, 0);
            tlpHeader.Dock = DockStyle.Top;
            tlpHeader.Location = new Point(3, 3);
            tlpHeader.Name = "tlpHeader";
            tlpHeader.Padding = new Padding(6, 10, 6, 6);
            tlpHeader.RowCount = 1;
            tlpHeader.RowStyles.Add(new RowStyle());
            tlpHeader.Size = new Size(874, 121);
            tlpHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.Location = new Point(9, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(628, 105);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Book Categories";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // flpActions
            // 
            flpActions.AutoSize = true;
            flpActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpActions.Controls.Add(lblSearch);
            flpActions.Controls.Add(txtSearch);
            flpActions.Controls.Add(chkActiveOnly);
            flpActions.Controls.Add(btnSearch);
            flpActions.Controls.Add(btnNew);
            flpActions.Dock = DockStyle.Fill;
            flpActions.Location = new Point(640, 10);
            flpActions.Margin = new Padding(0);
            flpActions.Name = "flpActions";
            flpActions.Size = new Size(228, 105);
            flpActions.TabIndex = 1;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(0, 0);
            lblSearch.Margin = new Padding(0, 0, 6, 0);
            lblSearch.Name = "lblSearch";
            lblSearch.Padding = new Padding(0, 7, 0, 0);
            lblSearch.Size = new Size(45, 22);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(0, 22);
            txtSearch.Margin = new Padding(0, 0, 8, 0);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(220, 23);
            txtSearch.TabIndex = 1;
            // 
            // chkActiveOnly
            // 
            chkActiveOnly.AutoSize = true;
            chkActiveOnly.Location = new Point(0, 47);
            chkActiveOnly.Margin = new Padding(0, 2, 8, 0);
            chkActiveOnly.Name = "chkActiveOnly";
            chkActiveOnly.Padding = new Padding(8, 4, 0, 0);
            chkActiveOnly.Size = new Size(95, 23);
            chkActiveOnly.TabIndex = 2;
            chkActiveOnly.Text = "Active Only";
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(103, 45);
            btnSearch.Margin = new Padding(0, 0, 8, 0);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(90, 30);
            btnSearch.TabIndex = 3;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(0, 75);
            btnNew.Margin = new Padding(0);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(90, 30);
            btnNew.TabIndex = 4;
            btnNew.Text = "New";
            btnNew.UseVisualStyleBackColor = true;
            // 
            // tlpBody
            // 
            tlpBody.ColumnCount = 2;
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            tlpBody.Controls.Add(dgvCats, 0, 0);
            tlpBody.Controls.Add(grpForm, 1, 0);
            tlpBody.Dock = DockStyle.Fill;
            tlpBody.Location = new Point(3, 130);
            tlpBody.Name = "tlpBody";
            tlpBody.RowCount = 1;
            tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpBody.Size = new Size(874, 431);
            tlpBody.TabIndex = 1;
            // 
            // dgvCats
            // 
            dgvCats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCats.BackgroundColor = Color.White;
            dgvCats.Dock = DockStyle.Fill;
            dgvCats.Location = new Point(3, 3);
            dgvCats.Name = "dgvCats";
            dgvCats.RowHeadersVisible = false;
            dgvCats.Size = new Size(588, 425);
            dgvCats.TabIndex = 0;
            // 
            // grpForm
            // 
            grpForm.Controls.Add(tblForm);
            grpForm.Dock = DockStyle.Fill;
            grpForm.Location = new Point(597, 3);
            grpForm.Name = "grpForm";
            grpForm.Padding = new Padding(10);
            grpForm.Size = new Size(274, 425);
            grpForm.TabIndex = 1;
            grpForm.TabStop = false;
            grpForm.Text = "Category Details";
            // 
            // tblForm
            // 
            tblForm.AutoScroll = true;
            tblForm.ColumnCount = 2;
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tblForm.Controls.Add(txtCode, 1, 0);
            tblForm.Controls.Add(txtName, 1, 1);
            tblForm.Controls.Add(chkActive, 1, 2);
            tblForm.Dock = DockStyle.Fill;
            tblForm.Location = new Point(10, 26);
            tblForm.Name = "tblForm";
            tblForm.RowCount = 4;
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblForm.Size = new Size(254, 389);
            tblForm.TabIndex = 0;
            // 
            // txtCode
            // 
            txtCode.Dock = DockStyle.Fill;
            txtCode.Location = new Point(91, 3);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(160, 23);
            txtCode.TabIndex = 0;
            // 
            // txtName
            // 
            txtName.Dock = DockStyle.Fill;
            txtName.Location = new Point(91, 37);
            txtName.Name = "txtName";
            txtName.Size = new Size(160, 23);
            txtName.TabIndex = 1;
            // 
            // chkActive
            // 
            chkActive.AutoSize = true;
            chkActive.Location = new Point(91, 73);
            chkActive.Margin = new Padding(3, 5, 3, 3);
            chkActive.Name = "chkActive";
            chkActive.Size = new Size(59, 19);
            chkActive.TabIndex = 3;
            chkActive.Text = "Active";
            // 
            // UCBookCategory
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelRoot);
            Name = "UCBookCategory";
            Size = new Size(900, 600);
            panelRoot.ResumeLayout(false);
            gbCats.ResumeLayout(false);
            tlpRoot.ResumeLayout(false);
            tlpRoot.PerformLayout();
            tlpHeader.ResumeLayout(false);
            tlpHeader.PerformLayout();
            flpActions.ResumeLayout(false);
            flpActions.PerformLayout();
            tlpBody.ResumeLayout(false);
            ((ISupportInitialize)dgvCats).EndInit();
            grpForm.ResumeLayout(false);
            tblForm.ResumeLayout(false);
            tblForm.PerformLayout();
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
            b.MinimumSize = new Size(95, 30);
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