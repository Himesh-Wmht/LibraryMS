namespace LibraryMS
{
    partial class frmLogin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private PictureBox pictureBox1;
        private TableLayoutPanel tableRoot;
        private Panel panelCard;
        private TableLayoutPanel tblLogin;
        private Label lblTitle;
        private TextBox txtUserName;
        private Panel pnlPassword;
        private TextBox txtPassword;
        private PictureBox pictureBox2;
        private ComboBox cmbLoginlocs;
        private Button btnLogin;
        private LinkLabel linkLabel1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            pictureBox1 = new PictureBox();
            tableRoot = new TableLayoutPanel();
            panelCard = new Panel();
            tblLogin = new TableLayoutPanel();
            lblTitle = new Label();
            txtUserName = new TextBox();
            pnlPassword = new Panel();
            txtPassword = new TextBox();
            pictureBox2 = new PictureBox();
            cmbLoginlocs = new ComboBox();
            btnLogin = new Button();
            linkLabel1 = new LinkLabel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            pictureBox1.SuspendLayout();
            tableRoot.SuspendLayout();
            panelCard.SuspendLayout();
            tblLogin.SuspendLayout();
            pnlPassword.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Controls.Add(tableRoot);
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1000, 620);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // tableRoot
            // 
            tableRoot.BackColor = Color.Transparent;
            tableRoot.ColumnCount = 3;
            tableRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 430F));
            tableRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableRoot.Controls.Add(panelCard, 1, 1);
            tableRoot.Dock = DockStyle.Fill;
            tableRoot.Location = new Point(0, 0);
            tableRoot.Name = "tableRoot";
            tableRoot.RowCount = 3;
            tableRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 380F));
            tableRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableRoot.Size = new Size(1000, 620);
            tableRoot.TabIndex = 1;
            // 
            // panelCard
            // 
            panelCard.BackColor = Color.PapayaWhip;
            panelCard.BorderStyle = BorderStyle.FixedSingle;
            panelCard.Controls.Add(tblLogin);
            panelCard.Dock = DockStyle.Fill;
            panelCard.Location = new Point(285, 120);
            panelCard.Margin = new Padding(0);
            panelCard.Name = "panelCard";
            panelCard.Size = new Size(430, 380);
            panelCard.TabIndex = 0;
            // 
            // tblLogin
            // 
            tblLogin.ColumnCount = 1;
            tblLogin.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblLogin.Controls.Add(lblTitle, 0, 0);
            tblLogin.Controls.Add(txtUserName, 0, 1);
            tblLogin.Controls.Add(pnlPassword, 0, 2);
            tblLogin.Controls.Add(cmbLoginlocs, 0, 3);
            tblLogin.Controls.Add(btnLogin, 0, 4);
            tblLogin.Controls.Add(linkLabel1, 0, 5);
            tblLogin.Dock = DockStyle.Fill;
            tblLogin.Location = new Point(0, 0);
            tblLogin.Name = "tblLogin";
            tblLogin.Padding = new Padding(24);
            tblLogin.RowCount = 6;
            tblLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            tblLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            tblLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            tblLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            tblLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            tblLogin.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblLogin.Size = new Size(428, 378);
            tblLogin.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.SeaGreen;
            lblTitle.Location = new Point(27, 24);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(374, 58);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Welcome Back to Your Library";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtUserName
            // 
            txtUserName.BackColor = Color.PapayaWhip;
            txtUserName.BorderStyle = BorderStyle.FixedSingle;
            txtUserName.Dock = DockStyle.Fill;
            txtUserName.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtUserName.Location = new Point(27, 92);
            txtUserName.Margin = new Padding(3, 10, 3, 10);
            txtUserName.Name = "txtUserName";
            txtUserName.PlaceholderText = "Username";
            txtUserName.Size = new Size(374, 29);
            txtUserName.TabIndex = 1;
            txtUserName.TextChanged += txtUserName_TextChanged;
            // 
            // pnlPassword
            // 
            pnlPassword.BackColor = Color.PapayaWhip;
            pnlPassword.BorderStyle = BorderStyle.FixedSingle;
            pnlPassword.Controls.Add(txtPassword);
            pnlPassword.Controls.Add(pictureBox2);
            pnlPassword.Dock = DockStyle.Fill;
            pnlPassword.Location = new Point(27, 150);
            pnlPassword.Margin = new Padding(3, 10, 3, 10);
            pnlPassword.Name = "pnlPassword";
            pnlPassword.Padding = new Padding(10, 6, 6, 6);
            pnlPassword.Size = new Size(374, 38);
            pnlPassword.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.PapayaWhip;
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Dock = DockStyle.Fill;
            txtPassword.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPassword.Location = new Point(10, 6);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Password";
            txtPassword.Size = new Size(330, 22);
            txtPassword.TabIndex = 0;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.PapayaWhip;
            pictureBox2.Cursor = Cursors.Hand;
            pictureBox2.Dock = DockStyle.Right;
            pictureBox2.Image = Win.Properties.Resources.Lock;
            pictureBox2.Location = new Point(340, 6);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Padding = new Padding(2);
            pictureBox2.Size = new Size(26, 24);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // cmbLoginlocs
            // 
            cmbLoginlocs.BackColor = Color.PapayaWhip;
            cmbLoginlocs.Dock = DockStyle.Fill;
            cmbLoginlocs.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cmbLoginlocs.FormattingEnabled = true;
            cmbLoginlocs.Location = new Point(27, 208);
            cmbLoginlocs.Margin = new Padding(3, 10, 3, 10);
            cmbLoginlocs.Name = "cmbLoginlocs";
            cmbLoginlocs.Size = new Size(374, 28);
            cmbLoginlocs.TabIndex = 3;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.DarkSeaGreen;
            btnLogin.Dock = DockStyle.Fill;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Tahoma", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogin.Location = new Point(27, 266);
            btnLogin.Margin = new Padding(3, 10, 3, 10);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(374, 38);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "LOGIN";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += button1_Click;
            // 
            // linkLabel1
            // 
            linkLabel1.Dock = DockStyle.Top;
            linkLabel1.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            linkLabel1.LinkColor = Color.Green;
            linkLabel1.Location = new Point(27, 320);
            linkLabel1.Margin = new Padding(3, 6, 3, 0);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(374, 22);
            linkLabel1.TabIndex = 5;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Forgot Password?";
            linkLabel1.TextAlign = ContentAlignment.MiddleCenter;
            linkLabel1.VisitedLinkColor = Color.Green;
            // 
            // frmLogin
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1000, 620);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(820, 520);
            Name = "frmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmLogin";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            pictureBox1.ResumeLayout(false);
            tableRoot.ResumeLayout(false);
            panelCard.ResumeLayout(false);
            tblLogin.ResumeLayout(false);
            tblLogin.PerformLayout();
            pnlPassword.ResumeLayout(false);
            pnlPassword.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
        }

        #endregion
    }
}