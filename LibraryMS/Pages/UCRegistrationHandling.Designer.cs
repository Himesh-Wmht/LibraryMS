namespace LibraryMS.Win.Pages
{
    partial class UCRegistrationHandling
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelRoot = new Panel();
            gbUserRegistration = new GroupBox();
            tlpMain = new TableLayoutPanel();
            lblUserGroupHint = new Label();
            panelLeft = new Panel();
            dtpDob = new DateTimePicker();
            label10 = new Label();
            label9 = new Label();
            label8 = new Label();
            label7 = new Label();
            txtAddress = new TextBox();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            lblcolumnname = new Label();
            cmbLocation = new ComboBox();
            cmbSubscription = new ComboBox();
            cmbGender = new ComboBox();
            txtEmail = new TextBox();
            txtNic = new TextBox();
            txtMobile = new TextBox();
            txtName = new TextBox();
            txtCode = new TextBox();
            panelRight = new Panel();
            nudMaxBorrow = new NumericUpDown();
            label21 = new Label();
            label20 = new Label();
            listAssignelocation = new ListBox();
            label19 = new Label();
            label18 = new Label();
            dtpExpiresOn = new TextBox();
            label17 = new Label();
            dtpValidFrom = new TextBox();
            label16 = new Label();
            rdoNoSubscription = new RadioButton();
            rdoSubscribed = new RadioButton();
            label15 = new Label();
            rdoNonMember = new RadioButton();
            rdoMember = new RadioButton();
            label14 = new Label();
            rdoInactive = new RadioButton();
            rdoActive = new RadioButton();
            label13 = new Label();
            label12 = new Label();
            label11 = new Label();
            txtConfirmPassword = new TextBox();
            cmbGroup = new ComboBox();
            txtPassword = new TextBox();
            panelRoot.SuspendLayout();
            gbUserRegistration.SuspendLayout();
            tlpMain.SuspendLayout();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxBorrow).BeginInit();
            SuspendLayout();
            // 
            // panelRoot
            // 
            panelRoot.BackColor = Color.PapayaWhip;
            panelRoot.Controls.Add(gbUserRegistration);
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.Location = new Point(0, 0);
            panelRoot.Name = "panelRoot";
            panelRoot.Size = new Size(900, 600);
            panelRoot.TabIndex = 0;
            // 
            // gbUserRegistration
            // 
            gbUserRegistration.Controls.Add(tlpMain);
            gbUserRegistration.Dock = DockStyle.Fill;
            gbUserRegistration.Location = new Point(0, 0);
            gbUserRegistration.Name = "gbUserRegistration";
            gbUserRegistration.Size = new Size(900, 600);
            gbUserRegistration.TabIndex = 0;
            gbUserRegistration.TabStop = false;
            gbUserRegistration.Text = "User Registration";
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpMain.Controls.Add(lblUserGroupHint, 0, 0);
            tlpMain.Controls.Add(panelLeft, 0, 1);
            tlpMain.Controls.Add(panelRight, 1, 1);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(3, 19);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 2;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Size = new Size(894, 578);
            tlpMain.TabIndex = 0;
            // 
            // lblUserGroupHint
            // 
            lblUserGroupHint.BackColor = Color.LightGoldenrodYellow;
            lblUserGroupHint.BorderStyle = BorderStyle.FixedSingle;
            tlpMain.SetColumnSpan(lblUserGroupHint, 2);
            lblUserGroupHint.Dock = DockStyle.Fill;
            lblUserGroupHint.Location = new Point(3, 0);
            lblUserGroupHint.Name = "lblUserGroupHint";
            lblUserGroupHint.Size = new Size(888, 40);
            lblUserGroupHint.TabIndex = 0;
            lblUserGroupHint.Text = "Group = USER → pending approval ...";
            lblUserGroupHint.TextAlign = ContentAlignment.MiddleLeft;
            lblUserGroupHint.Visible = false;
            // 
            // panelLeft
            // 
            panelLeft.BackColor = Color.Transparent;
            panelLeft.Controls.Add(dtpDob);
            panelLeft.Controls.Add(label10);
            panelLeft.Controls.Add(label9);
            panelLeft.Controls.Add(label8);
            panelLeft.Controls.Add(label7);
            panelLeft.Controls.Add(txtAddress);
            panelLeft.Controls.Add(label6);
            panelLeft.Controls.Add(label5);
            panelLeft.Controls.Add(label4);
            panelLeft.Controls.Add(label3);
            panelLeft.Controls.Add(label2);
            panelLeft.Controls.Add(label1);
            panelLeft.Controls.Add(lblcolumnname);
            panelLeft.Controls.Add(cmbLocation);
            panelLeft.Controls.Add(cmbSubscription);
            panelLeft.Controls.Add(cmbGender);
            panelLeft.Controls.Add(txtEmail);
            panelLeft.Controls.Add(txtNic);
            panelLeft.Controls.Add(txtMobile);
            panelLeft.Controls.Add(txtName);
            panelLeft.Controls.Add(txtCode);
            panelLeft.Dock = DockStyle.Fill;
            panelLeft.Location = new Point(3, 43);
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new Padding(10);
            panelLeft.Size = new Size(441, 532);
            panelLeft.TabIndex = 1;
            // 
            // dtpDob
            // 
            dtpDob.Location = new Point(13, 398);
            dtpDob.Name = "dtpDob";
            dtpDob.Size = new Size(236, 23);
            dtpDob.TabIndex = 21;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.ForeColor = Color.Teal;
            label10.Location = new Point(13, 480);
            label10.Name = "label10";
            label10.Size = new Size(53, 15);
            label10.TabIndex = 20;
            label10.Text = "Location";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.ForeColor = Color.Teal;
            label9.Location = new Point(13, 428);
            label9.Name = "label9";
            label9.Size = new Size(102, 15);
            label9.TabIndex = 19;
            label9.Text = "Subscription Type";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label8.ForeColor = Color.Teal;
            label8.Location = new Point(255, 380);
            label8.Name = "label8";
            label8.Size = new Size(45, 15);
            label8.TabIndex = 18;
            label8.Text = "Gender";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.ForeColor = Color.Teal;
            label7.Location = new Point(13, 383);
            label7.Name = "label7";
            label7.Size = new Size(32, 15);
            label7.TabIndex = 17;
            label7.Text = "DOB";
            // 
            // txtAddress
            // 
            txtAddress.Location = new Point(13, 323);
            txtAddress.Multiline = true;
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(415, 50);
            txtAddress.TabIndex = 16;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label6.ForeColor = Color.Teal;
            label6.Location = new Point(13, 302);
            label6.Name = "label6";
            label6.Size = new Size(49, 15);
            label6.TabIndex = 15;
            label6.Text = "Address";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.Teal;
            label5.Location = new Point(13, 249);
            label5.Name = "label5";
            label5.Size = new Size(36, 15);
            label5.TabIndex = 14;
            label5.Text = "Email";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.Teal;
            label4.Location = new Point(13, 196);
            label4.Name = "label4";
            label4.Size = new Size(27, 15);
            label4.TabIndex = 13;
            label4.Text = "NIC";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.Teal;
            label3.Location = new Point(13, 141);
            label3.Name = "label3";
            label3.Size = new Size(49, 15);
            label3.TabIndex = 12;
            label3.Text = "Mobile*";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.Teal;
            label2.Location = new Point(13, 93);
            label2.Name = "label2";
            label2.Size = new Size(66, 15);
            label2.TabIndex = 11;
            label2.Text = "Full Name*";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Teal;
            label1.Location = new Point(12, 44);
            label1.Name = "label1";
            label1.Size = new Size(60, 15);
            label1.TabIndex = 10;
            label1.Text = "User Code";
            // 
            // lblcolumnname
            // 
            lblcolumnname.AutoSize = true;
            lblcolumnname.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblcolumnname.ForeColor = Color.CadetBlue;
            lblcolumnname.Location = new Point(13, 11);
            lblcolumnname.Name = "lblcolumnname";
            lblcolumnname.Size = new Size(73, 15);
            lblcolumnname.TabIndex = 9;
            lblcolumnname.Text = "Basic Details";
            // 
            // cmbLocation
            // 
            cmbLocation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLocation.FormattingEnabled = true;
            cmbLocation.Location = new Point(13, 497);
            cmbLocation.Name = "cmbLocation";
            cmbLocation.Size = new Size(415, 23);
            cmbLocation.TabIndex = 8;
            // 
            // cmbSubscription
            // 
            cmbSubscription.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSubscription.FormattingEnabled = true;
            cmbSubscription.Location = new Point(13, 447);
            cmbSubscription.Name = "cmbSubscription";
            cmbSubscription.Size = new Size(415, 23);
            cmbSubscription.TabIndex = 7;
            // 
            // cmbGender
            // 
            cmbGender.FormattingEnabled = true;
            cmbGender.Location = new Point(255, 398);
            cmbGender.Name = "cmbGender";
            cmbGender.Size = new Size(173, 23);
            cmbGender.TabIndex = 6;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(13, 266);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(415, 23);
            txtEmail.TabIndex = 4;
            // 
            // txtNic
            // 
            txtNic.Location = new Point(13, 213);
            txtNic.Name = "txtNic";
            txtNic.Size = new Size(415, 23);
            txtNic.TabIndex = 3;
            // 
            // txtMobile
            // 
            txtMobile.Location = new Point(13, 158);
            txtMobile.Name = "txtMobile";
            txtMobile.Size = new Size(415, 23);
            txtMobile.TabIndex = 2;
            // 
            // txtName
            // 
            txtName.Location = new Point(13, 109);
            txtName.Name = "txtName";
            txtName.Size = new Size(415, 23);
            txtName.TabIndex = 1;
            // 
            // txtCode
            // 
            txtCode.Location = new Point(13, 62);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(415, 23);
            txtCode.TabIndex = 0;
            // 
            // panelRight
            // 
            panelRight.BackColor = Color.Transparent;
            panelRight.Controls.Add(nudMaxBorrow);
            panelRight.Controls.Add(label21);
            panelRight.Controls.Add(label20);
            panelRight.Controls.Add(listAssignelocation);
            panelRight.Controls.Add(label19);
            panelRight.Controls.Add(label18);
            panelRight.Controls.Add(dtpExpiresOn);
            panelRight.Controls.Add(label17);
            panelRight.Controls.Add(dtpValidFrom);
            panelRight.Controls.Add(label16);
            panelRight.Controls.Add(rdoNoSubscription);
            panelRight.Controls.Add(rdoSubscribed);
            panelRight.Controls.Add(label15);
            panelRight.Controls.Add(rdoNonMember);
            panelRight.Controls.Add(rdoMember);
            panelRight.Controls.Add(label14);
            panelRight.Controls.Add(rdoInactive);
            panelRight.Controls.Add(rdoActive);
            panelRight.Controls.Add(label13);
            panelRight.Controls.Add(label12);
            panelRight.Controls.Add(label11);
            panelRight.Controls.Add(txtConfirmPassword);
            panelRight.Controls.Add(cmbGroup);
            panelRight.Controls.Add(txtPassword);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(450, 43);
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(10);
            panelRight.Size = new Size(441, 532);
            panelRight.TabIndex = 2;
            // 
            // nudMaxBorrow
            // 
            nudMaxBorrow.Location = new Point(13, 340);
            nudMaxBorrow.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudMaxBorrow.Name = "nudMaxBorrow";
            nudMaxBorrow.Size = new Size(415, 23);
            nudMaxBorrow.TabIndex = 41;
            nudMaxBorrow.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label21.ForeColor = Color.CadetBlue;
            label21.Location = new Point(12, 11);
            label21.Name = "label21";
            label21.Size = new Size(91, 15);
            label21.TabIndex = 21;
            label21.Text = "Account Details";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label20.ForeColor = Color.Teal;
            label20.Location = new Point(12, 382);
            label20.Name = "label20";
            label20.Size = new Size(104, 15);
            label20.TabIndex = 40;
            label20.Text = "Assigned Location";
            // 
            // listAssignelocation
            // 
            listAssignelocation.FormattingEnabled = true;
            listAssignelocation.ItemHeight = 15;
            listAssignelocation.Location = new Point(13, 401);
            listAssignelocation.Name = "listAssignelocation";
            listAssignelocation.Size = new Size(415, 124);
            listAssignelocation.TabIndex = 39;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label19.ForeColor = Color.Teal;
            label19.Location = new Point(13, 313);
            label19.Name = "label19";
            label19.Size = new Size(71, 15);
            label19.TabIndex = 38;
            label19.Text = "Max Borrow";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label18.ForeColor = Color.Teal;
            label18.Location = new Point(225, 248);
            label18.Name = "label18";
            label18.Size = new Size(63, 15);
            label18.TabIndex = 36;
            label18.Text = "Expires On";
            // 
            // dtpExpiresOn
            // 
            dtpExpiresOn.Enabled = false;
            dtpExpiresOn.Location = new Point(225, 266);
            dtpExpiresOn.Name = "dtpExpiresOn";
            dtpExpiresOn.Size = new Size(203, 23);
            dtpExpiresOn.TabIndex = 35;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label17.ForeColor = Color.Teal;
            label17.Location = new Point(13, 250);
            label17.Name = "label17";
            label17.Size = new Size(64, 15);
            label17.TabIndex = 34;
            label17.Text = "Valid From";
            // 
            // dtpValidFrom
            // 
            dtpValidFrom.Enabled = false;
            dtpValidFrom.Location = new Point(13, 266);
            dtpValidFrom.Name = "dtpValidFrom";
            dtpValidFrom.Size = new Size(192, 23);
            dtpValidFrom.TabIndex = 33;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label16.ForeColor = Color.Teal;
            label16.Location = new Point(243, 193);
            label16.Name = "label16";
            label16.Size = new Size(74, 15);
            label16.TabIndex = 32;
            label16.Text = "Subscription";
            // 
            // rdoNoSubscription
            // 
            rdoNoSubscription.AutoSize = true;
            rdoNoSubscription.Location = new Point(259, 228);
            rdoNoSubscription.Name = "rdoNoSubscription";
            rdoNoSubscription.Size = new Size(102, 19);
            rdoNoSubscription.TabIndex = 31;
            rdoNoSubscription.Text = "No Subscribed";
            rdoNoSubscription.UseVisualStyleBackColor = true;
            // 
            // rdoSubscribed
            // 
            rdoSubscribed.AutoSize = true;
            rdoSubscribed.Location = new Point(259, 209);
            rdoSubscribed.Name = "rdoSubscribed";
            rdoSubscribed.Size = new Size(83, 19);
            rdoSubscribed.TabIndex = 30;
            rdoSubscribed.TabStop = true;
            rdoSubscribed.Text = "Subscribed";
            rdoSubscribed.UseVisualStyleBackColor = true;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label15.ForeColor = Color.Teal;
            label15.Location = new Point(128, 194);
            label15.Name = "label15";
            label15.Size = new Size(77, 15);
            label15.TabIndex = 29;
            label15.Text = "Membership ";
            // 
            // rdoNonMember
            // 
            rdoNonMember.AutoSize = true;
            rdoNonMember.Location = new Point(147, 228);
            rdoNonMember.Name = "rdoNonMember";
            rdoNonMember.Size = new Size(98, 19);
            rdoNonMember.TabIndex = 28;
            rdoNonMember.Text = "Non-Member";
            rdoNonMember.UseVisualStyleBackColor = true;
            // 
            // rdoMember
            // 
            rdoMember.AutoSize = true;
            rdoMember.Location = new Point(147, 208);
            rdoMember.Name = "rdoMember";
            rdoMember.Size = new Size(70, 19);
            rdoMember.TabIndex = 27;
            rdoMember.TabStop = true;
            rdoMember.Text = "Member";
            rdoMember.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label14.ForeColor = Color.Teal;
            label14.Location = new Point(13, 194);
            label14.Name = "label14";
            label14.Size = new Size(66, 15);
            label14.TabIndex = 26;
            label14.Text = "User Status";
            // 
            // rdoInactive
            // 
            rdoInactive.AutoSize = true;
            rdoInactive.Location = new Point(29, 228);
            rdoInactive.Name = "rdoInactive";
            rdoInactive.Size = new Size(66, 19);
            rdoInactive.TabIndex = 25;
            rdoInactive.Text = "Inactive";
            rdoInactive.UseVisualStyleBackColor = true;
            // 
            // rdoActive
            // 
            rdoActive.AutoSize = true;
            rdoActive.Location = new Point(29, 209);
            rdoActive.Name = "rdoActive";
            rdoActive.Size = new Size(58, 19);
            rdoActive.TabIndex = 24;
            rdoActive.TabStop = true;
            rdoActive.Text = "Active";
            rdoActive.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label13.ForeColor = Color.Teal;
            label13.Location = new Point(13, 140);
            label13.Name = "label13";
            label13.Size = new Size(103, 15);
            label13.TabIndex = 23;
            label13.Text = "Confirm Password";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label12.ForeColor = Color.Teal;
            label12.Location = new Point(13, 91);
            label12.Name = "label12";
            label12.Size = new Size(57, 15);
            label12.TabIndex = 22;
            label12.Text = "Password";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label11.ForeColor = Color.Teal;
            label11.Location = new Point(13, 44);
            label11.Name = "label11";
            label11.Size = new Size(66, 15);
            label11.TabIndex = 21;
            label11.Text = "User Group";
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.Location = new Point(13, 158);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.PasswordChar = '*';
            txtConfirmPassword.Size = new Size(415, 23);
            txtConfirmPassword.TabIndex = 10;
            txtConfirmPassword.UseSystemPasswordChar = true;
            // 
            // cmbGroup
            // 
            cmbGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGroup.FormattingEnabled = true;
            cmbGroup.Location = new Point(13, 62);
            cmbGroup.Name = "cmbGroup";
            cmbGroup.Size = new Size(415, 23);
            cmbGroup.TabIndex = 9;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(13, 109);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(415, 23);
            txtPassword.TabIndex = 9;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // UCRegistrationHandling
            // 
            Controls.Add(panelRoot);
            Name = "UCRegistrationHandling";
            Size = new Size(900, 600);
            panelRoot.ResumeLayout(false);
            gbUserRegistration.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxBorrow).EndInit();
            ResumeLayout(false);
        }


        #endregion

        private Panel panelRoot;
        private GroupBox gbUserRegistration;
        private TableLayoutPanel tlpMain;
        private Label lblUserGroupHint;
        private Panel panelLeft;
        private TextBox txtConfirmPassword;
        private Panel panelRight;
        private TextBox txtEmail;
        private TextBox txtNic;
        private TextBox txtMobile;
        private TextBox txtName;
        private TextBox txtCode;
        private ComboBox cmbLocation;
        private ComboBox cmbSubscription;
        private ComboBox cmbGender;
        private ComboBox cmbGroup;
        private TextBox txtPassword;
        private Label lblcolumnname;
        private Label label8;
        private Label label7;
        private TextBox txtAddress;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private Label label10;
        private Label label9;
        private Label label14;
        private RadioButton rdoInactive;
        private RadioButton rdoActive;
        private Label label13;
        private Label label12;
        private Label label11;
        private Label label17;
        private TextBox dtpValidFrom;
        private Label label16;
        private RadioButton rdoNoSubscription;
        private RadioButton rdoSubscribed;
        private Label label15;
        private RadioButton rdoNonMember;
        private RadioButton rdoMember;
        private Label label19;
        private Label label18;
        private TextBox dtpExpiresOn;
        private ListBox listAssignelocation;
        private Label label20;
        private Label label21;
        private DateTimePicker dtpDob;
        private NumericUpDown nudMaxBorrow;
    }
}
