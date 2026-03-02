#nullable disable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryMS.Win.Pages
{
    partial class UCUserUnlockApprovals
    {
        private IContainer components = null;

        private Panel panelRoot;
        private GroupBox gb;
        private TableLayoutPanel tlpRoot;

        private TableLayoutPanel tlpHeader;
        private Label lblTitle;
        private FlowLayoutPanel flpActions;

        private Button btnRefresh;
        private Button btnUnlock;

        private DataGridView dgvPending;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();

            panelRoot = new Panel();
            gb = new GroupBox();
            tlpRoot = new TableLayoutPanel();

            tlpHeader = new TableLayoutPanel();
            lblTitle = new Label();
            flpActions = new FlowLayoutPanel();

            btnRefresh = new Button();
            btnUnlock = new Button();

            dgvPending = new DataGridView();

            panelRoot.SuspendLayout();
            gb.SuspendLayout();
            tlpRoot.SuspendLayout();
            tlpHeader.SuspendLayout();
            flpActions.SuspendLayout();
            ((ISupportInitialize)dgvPending).BeginInit();
            SuspendLayout();

            panelRoot.BackColor = Color.PapayaWhip;
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.Controls.Add(gb);

            gb.Dock = DockStyle.Fill;
            gb.Padding = new Padding(10);
            gb.Text = "User Unlock";
            gb.Controls.Add(tlpRoot);

            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.ColumnCount = 1;
            tlpRoot.RowCount = 2;
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpHeader, 0, 0);
            tlpRoot.Controls.Add(dgvPending, 0, 1);

            tlpHeader.Dock = DockStyle.Fill;
            tlpHeader.ColumnCount = 2;
            tlpHeader.RowCount = 1;
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpHeader.Padding = new Padding(6, 10, 6, 0);

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Text = "Pending User Unlock Requests";

            flpActions.AutoSize = true;
            flpActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpActions.FlowDirection = FlowDirection.LeftToRight;
            flpActions.WrapContents = false;
            flpActions.Dock = DockStyle.Fill;
            flpActions.Margin = new Padding(0);

            SetupBtn(btnRefresh, "Refresh", Color.SteelBlue);
            SetupBtn(btnUnlock, "Unlock", Color.SeaGreen);

            flpActions.Controls.Add(btnRefresh);
            flpActions.Controls.Add(btnUnlock);

            tlpHeader.Controls.Add(lblTitle, 0, 0);
            tlpHeader.Controls.Add(flpActions, 1, 0);

            dgvPending.Dock = DockStyle.Fill;
            dgvPending.BackgroundColor = Color.White;
            dgvPending.BorderStyle = BorderStyle.FixedSingle;
            dgvPending.RowHeadersVisible = false;
            dgvPending.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPending.Name = "dgvPending";

            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelRoot);
            Name = "UCUserUnlockApprovals";
            Size = new Size(900, 600);

            panelRoot.ResumeLayout(false);
            gb.ResumeLayout(false);
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
            b.UseVisualStyleBackColor = false;
        }
    }
}