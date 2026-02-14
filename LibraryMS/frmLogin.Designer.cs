namespace LibraryMS
{
    partial class frmLogin
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            pictureBox1 = new PictureBox();
            btnLogin = new Button();
            panel1 = new Panel();
            linkLabel1 = new LinkLabel();
            cmbLoginlocs = new ComboBox();
            pictureBox2 = new PictureBox();
            txtPassword = new TextBox();
            txtUserName = new TextBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImageLayout = ImageLayout.None;
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(-15, -2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(945, 532);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.DarkSeaGreen;
            btnLogin.BackgroundImageLayout = ImageLayout.Zoom;
            btnLogin.Font = new Font("Tahoma", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogin.Location = new Point(305, 331);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(303, 38);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "LOGIN";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += button1_Click;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(linkLabel1);
            panel1.Controls.Add(cmbLoginlocs);
            panel1.Controls.Add(pictureBox2);
            panel1.Controls.Add(txtPassword);
            panel1.Controls.Add(txtUserName);
            panel1.Controls.Add(btnLogin);
            panel1.Controls.Add(pictureBox1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(931, 526);
            panel1.TabIndex = 6;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.BackColor = Color.PapayaWhip;
            linkLabel1.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            linkLabel1.ForeColor = Color.SeaGreen;
            linkLabel1.LinkColor = Color.Green;
            linkLabel1.Location = new Point(531, 381);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(92, 13);
            linkLabel1.TabIndex = 10;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Fogot Password ?\r\n";
            // 
            // cmbLoginlocs
            // 
            cmbLoginlocs.BackColor = Color.PapayaWhip;
            cmbLoginlocs.FormattingEnabled = true;
            cmbLoginlocs.Items.AddRange(new object[] { "Location 01", "Location 02" });
            cmbLoginlocs.Location = new Point(306, 377);
            cmbLoginlocs.Name = "cmbLoginlocs";
            cmbLoginlocs.Size = new Size(209, 23);
            cmbLoginlocs.TabIndex = 9;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox2.Image = Win.Properties.Resources.Lock;
            pictureBox2.Location = new Point(610, 270);
            pictureBox2.Margin = new Padding(6);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Padding = new Padding(6);
            pictureBox2.Size = new Size(32, 36);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 8;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.PapayaWhip;
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPassword.Location = new Point(315, 273);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(287, 26);
            txtPassword.TabIndex = 7;
            // 
            // txtUserName
            // 
            txtUserName.BackColor = Color.PapayaWhip;
            txtUserName.BorderStyle = BorderStyle.None;
            txtUserName.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtUserName.Location = new Point(315, 211);
            txtUserName.Name = "txtUserName";
            txtUserName.Size = new Size(287, 26);
            txtUserName.TabIndex = 6;
            txtUserName.TextChanged += txtUserName_TextChanged;
            // 
            // frmLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(931, 526);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "frmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmLogin";
            TransparencyKey = Color.White;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private RichTextBox richTextBox1;
        private RichTextBox richTextBox2;
        private Button btnLogin;
        private Panel panel1;
        private TextBox txtUserName;
        private TextBox txtPassword;
        private PictureBox pictureBox2;
        private Panel panelContent;
        private ComboBox cmbLoginlocs;
        private LinkLabel linkLabel1;
    }
}
