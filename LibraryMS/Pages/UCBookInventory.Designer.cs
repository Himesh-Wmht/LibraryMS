#nullable disable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryMS.Win.Pages
{
    partial class UCBookInventory
    {
        private IContainer components = null;

        private Panel panelRoot;
        private GroupBox gbInv;
        private TableLayoutPanel tlpRoot;

        private TableLayoutPanel tlpHeader;
        private Label lblTitle;
        private FlowLayoutPanel flpFilters;

        private Label lblSearch;
        private TextBox txtSearch;
        private CheckBox chkActiveOnly;
        private Button btnSearch;
        private Button btnNew;

        private TableLayoutPanel tlpBody;
        private DataGridView dgvInv;

        private GroupBox grpForm;
        private TableLayoutPanel tblForm;

        private Label lblLocLabel;
        private Label lblLoc;

        private TextBox txtBookCode;
        private TextBox txtBookTitle;
        private NumericUpDown numQty;
        private NumericUpDown numReorder;
        private CheckBox chkActive;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();

            panelRoot = new Panel();
            gbInv = new GroupBox();
            tlpRoot = new TableLayoutPanel();

            tlpHeader = new TableLayoutPanel();
            lblTitle = new Label();
            flpFilters = new FlowLayoutPanel();

            lblSearch = new Label();
            txtSearch = new TextBox();
            chkActiveOnly = new CheckBox();
            btnSearch = new Button();
            btnNew = new Button();

            tlpBody = new TableLayoutPanel();
            dgvInv = new DataGridView();

            grpForm = new GroupBox();
            tblForm = new TableLayoutPanel();

            lblLocLabel = new Label();
            lblLoc = new Label();

            txtBookCode = new TextBox();
            txtBookTitle = new TextBox();
            numQty = new NumericUpDown();
            numReorder = new NumericUpDown();
            chkActive = new CheckBox();

            panelRoot.SuspendLayout();
            gbInv.SuspendLayout();
            tlpRoot.SuspendLayout();
            tlpHeader.SuspendLayout();
            flpFilters.SuspendLayout();
            tlpBody.SuspendLayout();
            ((ISupportInitialize)dgvInv).BeginInit();
            grpForm.SuspendLayout();
            tblForm.SuspendLayout();
            ((ISupportInitialize)numQty).BeginInit();
            ((ISupportInitialize)numReorder).BeginInit();
            SuspendLayout();

            // panelRoot
            panelRoot.BackColor = Color.PapayaWhip;
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.Controls.Add(gbInv);

            // gbInv
            gbInv.Dock = DockStyle.Fill;
            gbInv.Padding = new Padding(10);
            gbInv.Text = "Book Inventory";
            gbInv.Controls.Add(tlpRoot);

            // tlpRoot
            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.ColumnCount = 1;
            tlpRoot.RowCount = 2;
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpHeader, 0, 0);
            tlpRoot.Controls.Add(tlpBody, 0, 1);

            // tlpHeader
            tlpHeader.Dock = DockStyle.Fill;
            tlpHeader.ColumnCount = 2;
            tlpHeader.RowCount = 1;
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpHeader.Padding = new Padding(6, 10, 6, 0);

            // lblTitle
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Text = "Book Inventory";

            // flpFilters
            flpFilters.AutoSize = true;
            flpFilters.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpFilters.FlowDirection = FlowDirection.LeftToRight;
            flpFilters.WrapContents = false;
            flpFilters.Dock = DockStyle.Fill;
            flpFilters.Margin = new Padding(0);

            lblSearch.AutoSize = true;
            lblSearch.Text = "Search:";
            lblSearch.Padding = new Padding(0, 7, 0, 0);

            txtSearch.Width = 220;
            txtSearch.Margin = new Padding(6, 3, 0, 0);

            chkActiveOnly.Text = "Active Only";
            chkActiveOnly.AutoSize = true;
            chkActiveOnly.Padding = new Padding(12, 5, 0, 0);
            chkActiveOnly.Margin = new Padding(6, 3, 0, 0);

            SetupBtn(btnSearch, "Search", Color.SteelBlue);
            SetupBtn(btnNew, "New", Color.DimGray);

            flpFilters.Controls.Add(lblSearch);
            flpFilters.Controls.Add(txtSearch);
            flpFilters.Controls.Add(chkActiveOnly);
            flpFilters.Controls.Add(btnSearch);
            flpFilters.Controls.Add(btnNew);

            tlpHeader.Controls.Add(lblTitle, 0, 0);
            tlpHeader.Controls.Add(flpFilters, 1, 0);

            // tlpBody
            tlpBody.Dock = DockStyle.Fill;
            tlpBody.ColumnCount = 2;
            tlpBody.RowCount = 1;
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // dgvInv
            dgvInv.Dock = DockStyle.Fill;
            dgvInv.BackgroundColor = Color.White;
            dgvInv.BorderStyle = BorderStyle.FixedSingle;
            dgvInv.RowHeadersVisible = false;
            dgvInv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInv.Name = "dgvInv";

            // grpForm
            grpForm.Dock = DockStyle.Fill;
            grpForm.Padding = new Padding(10);
            grpForm.Text = "Inventory Details";
            grpForm.Controls.Add(tblForm);

            // tblForm
            tblForm.Dock = DockStyle.Fill;
            tblForm.ColumnCount = 2;
            tblForm.RowCount = 6;
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // loc
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // book code
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // title
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // qty
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // reorder
            tblForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // active

            lblLocLabel.Text = "Location";
            lblLocLabel.AutoSize = true;
            lblLocLabel.Padding = new Padding(0, 6, 0, 0);

            lblLoc.AutoSize = true;
            lblLoc.Padding = new Padding(0, 6, 0, 0);

            txtBookTitle.ReadOnly = true;
            numQty.Maximum = 1000000;
            numReorder.Maximum = 1000000;

            tblForm.Controls.Add(lblLocLabel, 0, 0);
            tblForm.Controls.Add(lblLoc, 1, 0);

            tblForm.Controls.Add(new Label { Text = "Book Code", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 1);
            tblForm.Controls.Add(txtBookCode, 1, 1);

            tblForm.Controls.Add(new Label { Text = "Title", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 2);
            tblForm.Controls.Add(txtBookTitle, 1, 2);

            tblForm.Controls.Add(new Label { Text = "Qty", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 3);
            tblForm.Controls.Add(numQty, 1, 3);

            tblForm.Controls.Add(new Label { Text = "Reorder", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 4);
            tblForm.Controls.Add(numReorder, 1, 4);

            chkActive.Text = "Active";
            chkActive.AutoSize = true;

            tblForm.Controls.Add(new Label { Text = "", AutoSize = true }, 0, 5);
            tblForm.Controls.Add(chkActive, 1, 5);

            tlpBody.Controls.Add(dgvInv, 0, 0);
            tlpBody.Controls.Add(grpForm, 1, 0);

            // UCBookInventory
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelRoot);
            Name = "UCBookInventory";
            Size = new Size(900, 600);

            panelRoot.ResumeLayout(false);
            gbInv.ResumeLayout(false);
            tlpRoot.ResumeLayout(false);
            tlpHeader.ResumeLayout(false);
            tlpHeader.PerformLayout();
            flpFilters.ResumeLayout(false);
            flpFilters.PerformLayout();
            tlpBody.ResumeLayout(false);
            ((ISupportInitialize)dgvInv).EndInit();
            grpForm.ResumeLayout(false);
            tblForm.ResumeLayout(false);
            tblForm.PerformLayout();
            ((ISupportInitialize)numQty).EndInit();
            ((ISupportInitialize)numReorder).EndInit();
            ResumeLayout(false);
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