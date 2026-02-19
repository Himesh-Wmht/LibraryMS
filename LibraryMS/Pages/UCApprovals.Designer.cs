using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryMS.Win.Pages
{
    partial class UCApprovals
    {
        private IContainer components = null;

        private Panel panelRoot;
        private GroupBox gbApprovals;
        private TableLayoutPanel tlpRoot;

        private TableLayoutPanel tlpHeader;
        private Label lblTitle;
        private FlowLayoutPanel flpActions;

        private Button btnRefresh;
        private Button btnLoadAll;
        private Button btnApprove;
        private Button btnReject;

        private DataGridView dgvPending;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();

            panelRoot = new Panel();
            gbApprovals = new GroupBox();
            tlpRoot = new TableLayoutPanel();

            tlpHeader = new TableLayoutPanel();
            lblTitle = new Label();
            flpActions = new FlowLayoutPanel();

            btnRefresh = new Button();
            btnLoadAll = new Button();
            btnApprove = new Button();
            btnReject = new Button();

            dgvPending = new DataGridView();

            panelRoot.SuspendLayout();
            gbApprovals.SuspendLayout();
            tlpRoot.SuspendLayout();
            tlpHeader.SuspendLayout();
            flpActions.SuspendLayout();
            ((ISupportInitialize)dgvPending).BeginInit();
            SuspendLayout();

            // panelRoot
            panelRoot.BackColor = Color.PapayaWhip;
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.Controls.Add(gbApprovals);
            panelRoot.Name = "panelRoot";
            panelRoot.Size = new Size(900, 600);

            // gbApprovals
            gbApprovals.Dock = DockStyle.Fill;
            gbApprovals.Padding = new Padding(10);
            gbApprovals.Text = "Approvals";
            gbApprovals.Controls.Add(tlpRoot);
            gbApprovals.Name = "gbApprovals";

            // tlpRoot
            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.ColumnCount = 1;
            tlpRoot.RowCount = 2;
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpHeader, 0, 0);
            tlpRoot.Controls.Add(dgvPending, 0, 1);
            tlpRoot.Name = "tlpRoot";

            // tlpHeader  ✅ FIXED HEADER LAYOUT
            tlpHeader.Dock = DockStyle.Fill;
            tlpHeader.ColumnCount = 2;
            tlpHeader.RowCount = 1;
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // title fills
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // buttons autosize
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpHeader.Padding = new Padding(6, 10, 6, 0);
            tlpHeader.Name = "tlpHeader";

            // lblTitle
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Text = "Pending Registration Approvals";
            lblTitle.Name = "lblTitle";

            // flpActions
            flpActions.AutoSize = true;
            flpActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpActions.FlowDirection = FlowDirection.LeftToRight;
            flpActions.WrapContents = false;
            flpActions.Dock = DockStyle.Fill;
            flpActions.Margin = new Padding(0);
            flpActions.Padding = new Padding(0, 0, 0, 0);
            flpActions.Name = "flpActions";

            SetupBtn(btnRefresh, "Refresh", Color.SteelBlue);
            SetupBtn(btnLoadAll, "Load All", Color.DimGray);
            SetupBtn(btnApprove, "Approve", Color.SeaGreen);
            SetupBtn(btnReject, "Reject", Color.IndianRed);

            flpActions.Controls.Add(btnRefresh);
            flpActions.Controls.Add(btnLoadAll);
            flpActions.Controls.Add(btnApprove);
            flpActions.Controls.Add(btnReject);

            tlpHeader.Controls.Add(lblTitle, 0, 0);
            tlpHeader.Controls.Add(flpActions, 1, 0);

            // dgvPending
            dgvPending.Dock = DockStyle.Fill;
            dgvPending.BackgroundColor = Color.White;
            dgvPending.BorderStyle = BorderStyle.FixedSingle;
            dgvPending.RowHeadersVisible = false;
            dgvPending.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPending.Name = "dgvPending";

            // UCApprovals
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelRoot);
            Name = "UCApprovals";
            Size = new Size(900, 600);

            panelRoot.ResumeLayout(false);
            gbApprovals.ResumeLayout(false);
            tlpRoot.ResumeLayout(false);
            tlpHeader.ResumeLayout(false);
            tlpHeader.PerformLayout();
            flpActions.ResumeLayout(false);
            ((ISupportInitialize)dgvPending).EndInit();
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
        }
    }
}
