namespace LibraryMS.Win
{
    partial class frmMainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelTop = new Panel();
            flpTopRight = new FlowLayoutPanel();
            lblTopUser = new Label();
            cmbTopLocation = new ComboBox();
            btnRefresh = new FontAwesome.Sharp.IconButton();
            btnEdit = new FontAwesome.Sharp.IconButton();
            btnSave = new FontAwesome.Sharp.IconButton();
            btnProcess = new FontAwesome.Sharp.IconButton();
            btnLogout = new FontAwesome.Sharp.IconButton();
            lblTitle = new Label();
            panelContent = new Panel();
            splitContainer1 = new SplitContainer();
            tvMenus = new TreeView();
            panelPageHost = new Panel();
            panelTop.SuspendLayout();
            flpTopRight.SuspendLayout();
            panelContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.LightSlateGray;
            panelTop.BorderStyle = BorderStyle.Fixed3D;
            panelTop.Controls.Add(flpTopRight);
            panelTop.Controls.Add(lblTitle);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1261, 60);
            panelTop.TabIndex = 0;
            // 
            // flpTopRight
            // 
            flpTopRight.AutoSize = true;
            flpTopRight.Controls.Add(lblTopUser);
            flpTopRight.Controls.Add(cmbTopLocation);
            flpTopRight.Controls.Add(btnRefresh);
            flpTopRight.Controls.Add(btnEdit);
            flpTopRight.Controls.Add(btnSave);
            flpTopRight.Controls.Add(btnProcess);
            flpTopRight.Controls.Add(btnLogout);
            flpTopRight.Dock = DockStyle.Right;
            flpTopRight.Location = new Point(610, 0);
            flpTopRight.Name = "flpTopRight";
            flpTopRight.Padding = new Padding(10);
            flpTopRight.Size = new Size(647, 56);
            flpTopRight.TabIndex = 1;
            flpTopRight.WrapContents = false;
            // 
            // lblTopUser
            // 
            lblTopUser.AutoSize = true;
            lblTopUser.Font = new Font("Tahoma", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTopUser.Location = new Point(13, 10);
            lblTopUser.Name = "lblTopUser";
            lblTopUser.Size = new Size(89, 18);
            lblTopUser.TabIndex = 1;
            lblTopUser.Text = "lblTopUser";
            // 
            // cmbTopLocation
            // 
            cmbTopLocation.FormattingEnabled = true;
            cmbTopLocation.Location = new Point(108, 13);
            cmbTopLocation.Name = "cmbTopLocation";
            cmbTopLocation.Size = new Size(121, 23);
            cmbTopLocation.TabIndex = 0;
            // 
            // btnRefresh
            // 
            btnRefresh.IconChar = FontAwesome.Sharp.IconChar.None;
            btnRefresh.IconColor = Color.Black;
            btnRefresh.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnRefresh.Location = new Point(235, 13);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "btnRefresh ";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.IconChar = FontAwesome.Sharp.IconChar.None;
            btnEdit.IconColor = Color.Black;
            btnEdit.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnEdit.Location = new Point(316, 13);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(75, 23);
            btnEdit.TabIndex = 3;
            btnEdit.Text = "btnEdit";
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.IconChar = FontAwesome.Sharp.IconChar.None;
            btnSave.IconColor = Color.Black;
            btnSave.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnSave.Location = new Point(397, 13);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 4;
            btnSave.Text = "btnSave";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnProcess
            // 
            btnProcess.IconChar = FontAwesome.Sharp.IconChar.None;
            btnProcess.IconColor = Color.Black;
            btnProcess.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnProcess.Location = new Point(478, 13);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new Size(75, 23);
            btnProcess.TabIndex = 5;
            btnProcess.Text = "btnProcess";
            btnProcess.UseVisualStyleBackColor = true;
            // 
            // btnLogout
            // 
            btnLogout.IconChar = FontAwesome.Sharp.IconChar.None;
            btnLogout.IconColor = Color.Black;
            btnLogout.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnLogout.Location = new Point(559, 13);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(75, 23);
            btnLogout.TabIndex = 6;
            btnLogout.Text = "btnLogout";
            btnLogout.UseVisualStyleBackColor = true;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Tahoma", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(59, 13);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(68, 23);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "label1";
            // 
            // panelContent
            // 
            panelContent.BackColor = Color.PapayaWhip;
            panelContent.Controls.Add(splitContainer1);
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(0, 60);
            panelContent.Name = "panelContent";
            panelContent.Size = new Size(1261, 574);
            panelContent.TabIndex = 2;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tvMenus);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(panelPageHost);
            splitContainer1.Size = new Size(1261, 574);
            splitContainer1.SplitterDistance = 250;
            splitContainer1.TabIndex = 0;
            // 
            // tvMenus
            // 
            tvMenus.BackColor = Color.Tan;
            tvMenus.Dock = DockStyle.Fill;
            tvMenus.LineColor = Color.PeachPuff;
            tvMenus.Location = new Point(0, 0);
            tvMenus.Name = "tvMenus";
            tvMenus.Size = new Size(250, 574);
            tvMenus.TabIndex = 0;
            // 
            // panelPageHost
            // 
            panelPageHost.BackColor = Color.PapayaWhip;
            panelPageHost.Dock = DockStyle.Fill;
            panelPageHost.Location = new Point(0, 0);
            panelPageHost.Name = "panelPageHost";
            panelPageHost.Size = new Size(1007, 574);
            panelPageHost.TabIndex = 0;
            // 
            // frmMainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.PapayaWhip;
            ClientSize = new Size(1261, 634);
            Controls.Add(panelContent);
            Controls.Add(panelTop);
            Name = "frmMainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmMainWindow";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            flpTopRight.ResumeLayout(false);
            flpTopRight.PerformLayout();
            panelContent.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panelTop;
        private Panel panelContent;
        private SplitContainer splitContainer1;
        private TreeView tvMenus;
        private Panel panelPageHost;
        private Label lblTitle;
        private FlowLayoutPanel flpTopRight;
        private ComboBox cmbTopLocation;
        private Label lblTopUser;
    }
}