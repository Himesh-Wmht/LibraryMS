#nullable disable
using System.ComponentModel;
using System.Drawing;

namespace LibraryMS.Win.Pages
{
    partial class UCBookCategory
    {
        private IContainer components = null;

        private System.Windows.Forms.Panel panelRoot;
        private System.Windows.Forms.GroupBox gbCats;
        private System.Windows.Forms.TableLayoutPanel tlpRoot;

        private System.Windows.Forms.TableLayoutPanel tlpHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel flpActions;

        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.CheckBox chkActiveOnly;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnNew;

        private System.Windows.Forms.DataGridView dgvCats;

        private System.Windows.Forms.GroupBox grpForm;
        private System.Windows.Forms.TableLayoutPanel tblForm;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.CheckBox chkActive;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();

            panelRoot = new System.Windows.Forms.Panel();
            gbCats = new System.Windows.Forms.GroupBox();
            tlpRoot = new System.Windows.Forms.TableLayoutPanel();

            tlpHeader = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            flpActions = new System.Windows.Forms.FlowLayoutPanel();

            txtSearch = new System.Windows.Forms.TextBox();
            chkActiveOnly = new System.Windows.Forms.CheckBox();
            btnSearch = new System.Windows.Forms.Button();
            btnNew = new System.Windows.Forms.Button();

            dgvCats = new System.Windows.Forms.DataGridView();

            grpForm = new System.Windows.Forms.GroupBox();
            tblForm = new System.Windows.Forms.TableLayoutPanel();
            txtCode = new System.Windows.Forms.TextBox();
            txtName = new System.Windows.Forms.TextBox();
            chkActive = new System.Windows.Forms.CheckBox();

            var tlpBody = new System.Windows.Forms.TableLayoutPanel();

            panelRoot.SuspendLayout();
            gbCats.SuspendLayout();
            tlpRoot.SuspendLayout();
            tlpHeader.SuspendLayout();
            flpActions.SuspendLayout();
            ((ISupportInitialize)dgvCats).BeginInit();
            grpForm.SuspendLayout();
            SuspendLayout();

            // panelRoot
            panelRoot.BackColor = Color.PapayaWhip;
            panelRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            panelRoot.Controls.Add(gbCats);
            panelRoot.Name = "panelRoot";
            panelRoot.Size = new Size(900, 600);

            // gbCats
            gbCats.Dock = System.Windows.Forms.DockStyle.Fill;
            gbCats.Padding = new System.Windows.Forms.Padding(10);
            gbCats.Text = "Book Categories";
            gbCats.Controls.Add(tlpRoot);

            // tlpRoot
            tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpRoot.ColumnCount = 1;
            tlpRoot.RowCount = 2;
            tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpHeader, 0, 0);
            tlpRoot.Controls.Add(tlpBody, 0, 1);

            // tlpHeader
            tlpHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpHeader.ColumnCount = 2;
            tlpHeader.RowCount = 1;
            tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpHeader.Padding = new System.Windows.Forms.Padding(6, 10, 6, 0);

            // lblTitle
            lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Text = "Book Categories";

            // flpActions
            flpActions.AutoSize = true;
            flpActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flpActions.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            flpActions.WrapContents = false;
            flpActions.Dock = System.Windows.Forms.DockStyle.Fill;
            flpActions.Margin = new System.Windows.Forms.Padding(0);

            var lblSearch = new System.Windows.Forms.Label
            {
                Text = "Search:",
                AutoSize = true,
                Padding = new System.Windows.Forms.Padding(0, 7, 0, 0)
            };

            txtSearch.Name = "txtSearch";
            txtSearch.Width = 220;
            txtSearch.Margin = new System.Windows.Forms.Padding(6, 3, 0, 0);

            chkActiveOnly.Name = "chkActiveOnly";
            chkActiveOnly.Text = "Active Only";
            chkActiveOnly.AutoSize = true;
            chkActiveOnly.Padding = new System.Windows.Forms.Padding(12, 5, 0, 0);
            chkActiveOnly.Margin = new System.Windows.Forms.Padding(6, 3, 0, 0);

            SetupBtn(btnSearch, "Search", Color.SteelBlue);
            SetupBtn(btnNew, "New", Color.DimGray);

            flpActions.Controls.Add(lblSearch);
            flpActions.Controls.Add(txtSearch);
            flpActions.Controls.Add(chkActiveOnly);
            flpActions.Controls.Add(btnSearch);
            flpActions.Controls.Add(btnNew);

            tlpHeader.Controls.Add(lblTitle, 0, 0);
            tlpHeader.Controls.Add(flpActions, 1, 0);

            // tlpBody
            tlpBody.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpBody.ColumnCount = 2;
            tlpBody.RowCount = 1;
            tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
            tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
            tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            // dgvCats
            dgvCats.Name = "dgvCats";
            dgvCats.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvCats.BackgroundColor = Color.White;
            dgvCats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            dgvCats.RowHeadersVisible = false;
            dgvCats.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;

            // grpForm
            grpForm.Dock = System.Windows.Forms.DockStyle.Fill;
            grpForm.Padding = new System.Windows.Forms.Padding(10);
            grpForm.Text = "Category Details";
            grpForm.Controls.Add(tblForm);

            // tblForm
            tblForm.Dock = System.Windows.Forms.DockStyle.Fill;
            tblForm.ColumnCount = 2;
            tblForm.RowCount = 4;
            tblForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            tblForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            tblForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            var lblCode = MakeFormLabel("Code");
            var lblName = MakeFormLabel("Name");

            txtCode.Name = "txtCode";
            txtCode.Dock = System.Windows.Forms.DockStyle.Fill;

            txtName.Name = "txtName";
            txtName.Dock = System.Windows.Forms.DockStyle.Fill;

            chkActive.Name = "chkActive";
            chkActive.Text = "Active";
            chkActive.AutoSize = true;
            chkActive.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);

            tblForm.Controls.Add(lblCode, 0, 0);
            tblForm.Controls.Add(txtCode, 1, 0);

            tblForm.Controls.Add(lblName, 0, 1);
            tblForm.Controls.Add(txtName, 1, 1);

            tblForm.Controls.Add(new System.Windows.Forms.Label() { Text = "", AutoSize = true }, 0, 2);
            tblForm.Controls.Add(chkActive, 1, 2);

            tlpBody.Controls.Add(dgvCats, 0, 0);
            tlpBody.Controls.Add(grpForm, 1, 0);

            // UCBookCategory
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panelRoot);
            Name = "UCBookCategory";
            Size = new Size(900, 600);

            panelRoot.ResumeLayout(false);
            gbCats.ResumeLayout(false);
            tlpRoot.ResumeLayout(false);
            tlpHeader.ResumeLayout(false);
            tlpHeader.PerformLayout();
            flpActions.ResumeLayout(false);
            flpActions.PerformLayout();
            ((ISupportInitialize)dgvCats).EndInit();
            grpForm.ResumeLayout(false);
            ResumeLayout(false);
        }

        private static System.Windows.Forms.Label MakeFormLabel(string text)
        {
            return new System.Windows.Forms.Label
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Black,
                Padding = new System.Windows.Forms.Padding(0, 6, 0, 0)
            };
        }

        private static void SetupBtn(System.Windows.Forms.Button b, string text, Color back)
        {
            b.Text = text;
            b.Width = 100;
            b.Height = 30;
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }
}
