using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryMS.Win.Pages
{
    partial class UCGroupMenus
    {
        private IContainer components = null;

        private Panel panelRoot;
        private GroupBox gb;
        private TableLayoutPanel tlp;

        private Panel panelTop;
        private Label lblGroup;
        private ComboBox cmbGroup;
        private CheckBox chkSelectAll;

        private FlowLayoutPanel flpButtons;
        private Button btnRefresh;
        private Button btnSave;

        private DataGridView dgvMenus;
        private DataGridViewCheckBoxColumn colAssigned;
        private DataGridViewTextBoxColumn colCode;
        private DataGridViewTextBoxColumn colDesc;
        private DataGridViewTextBoxColumn colParent;

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
            tlp = new TableLayoutPanel();
            panelTop = new Panel();
            lblGroup = new Label();
            cmbGroup = new ComboBox();
            chkSelectAll = new CheckBox();

            flpButtons = new FlowLayoutPanel();
            btnRefresh = new Button();
            btnSave = new Button();

            dgvMenus = new DataGridView();
            colAssigned = new DataGridViewCheckBoxColumn();
            colCode = new DataGridViewTextBoxColumn();
            colDesc = new DataGridViewTextBoxColumn();
            colParent = new DataGridViewTextBoxColumn();

            panelRoot.SuspendLayout();
            gb.SuspendLayout();
            tlp.SuspendLayout();
            panelTop.SuspendLayout();
            flpButtons.SuspendLayout();
            ((ISupportInitialize)dgvMenus).BeginInit();
            SuspendLayout();

            // panelRoot
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.BackColor = Color.PapayaWhip;
            panelRoot.Controls.Add(gb);

            // gb
            gb.Dock = DockStyle.Fill;
            gb.Padding = new Padding(10);
            gb.Text = "Group Menus";
            gb.Controls.Add(tlp);

            // tlp
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 1;
            tlp.RowCount = 2;
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlp.Controls.Add(panelTop, 0, 0);
            tlp.Controls.Add(dgvMenus, 0, 1);

            // panelTop
            panelTop.Dock = DockStyle.Fill;

            lblGroup.Text = "User Group:";
            lblGroup.AutoSize = true;
            lblGroup.Location = new Point(10, 20);
            lblGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblGroup.ForeColor = Color.Teal;

            cmbGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGroup.Location = new Point(90, 16);
            cmbGroup.Width = 220;

            chkSelectAll.Text = "Select All";
            chkSelectAll.AutoSize = true;
            chkSelectAll.Location = new Point(330, 18);

            flpButtons.Dock = DockStyle.Right;
            flpButtons.FlowDirection = FlowDirection.LeftToRight;
            flpButtons.WrapContents = false;
            flpButtons.AutoSize = true;
            flpButtons.Location = new Point(600, 10);

            SetupBtn(btnRefresh, "Refresh", Color.SteelBlue);
            SetupBtn(btnSave, "Save", Color.SeaGreen);

            flpButtons.Controls.Add(btnRefresh);
            flpButtons.Controls.Add(btnSave);

            panelTop.Controls.Add(lblGroup);
            panelTop.Controls.Add(cmbGroup);
            panelTop.Controls.Add(chkSelectAll);
            panelTop.Controls.Add(flpButtons);

            // dgvMenus
            dgvMenus.Dock = DockStyle.Fill;
            dgvMenus.BackgroundColor = Color.White;
            dgvMenus.BorderStyle = BorderStyle.FixedSingle;
            dgvMenus.RowHeadersVisible = false;
            dgvMenus.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvMenus.Columns.AddRange(new DataGridViewColumn[]
            {
                colAssigned, colCode, colDesc, colParent
            });

            colAssigned.HeaderText = "Assigned";
            colAssigned.DataPropertyName = "Assigned";
            colAssigned.Width = 90;

            colCode.HeaderText = "Code";
            colCode.DataPropertyName = "MenuCode";
            colCode.ReadOnly = true;

            colDesc.HeaderText = "Menu";
            colDesc.DataPropertyName = "MenuDesc";
            colDesc.ReadOnly = true;

            colParent.HeaderText = "Parent";
            colParent.DataPropertyName = "ParentCode";
            colParent.ReadOnly = true;

            // UCGroupMenus
            Controls.Add(panelRoot);
            Name = "UCGroupMenus";
            Size = new Size(900, 600);

            panelRoot.ResumeLayout(false);
            gb.ResumeLayout(false);
            tlp.ResumeLayout(false);
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            flpButtons.ResumeLayout(false);
            ((ISupportInitialize)dgvMenus).EndInit();
            ResumeLayout(false);
        }

        private static void SetupBtn(Button b, string text, Color back)
        {
            b.Text = text;
            b.Width = 90;
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
