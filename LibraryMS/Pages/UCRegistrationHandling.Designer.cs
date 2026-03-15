namespace LibraryMS.Win.Pages
{
    partial class UCRegistrationHandling
    {
        private System.ComponentModel.IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            panelRoot = new Panel();
            gbUserRegistration = new GroupBox();
            tlpMain = new TableLayoutPanel();
            lblUserGroupHint = new Label();
            panelLeft = new Panel();
            panelRight = new Panel();

            lblcolumnname = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            label10 = new Label();

            txtCode = new TextBox();
            txtName = new TextBox();
            txtMobile = new TextBox();
            txtNic = new TextBox();
            txtEmail = new TextBox();
            txtAddress = new TextBox();
            dtpDob = new DateTimePicker();
            cmbGender = new ComboBox();
            cmbSubscription = new ComboBox();
            cmbLocation = new ComboBox();

            label21 = new Label();
            label11 = new Label();
            label12 = new Label();
            label13 = new Label();
            label14 = new Label();
            label15 = new Label();
            label16 = new Label();
            label17 = new Label();
            label18 = new Label();
            label19 = new Label();
            label20 = new Label();

            cmbGroup = new ComboBox();
            txtPassword = new TextBox();
            txtConfirmPassword = new TextBox();
            rdoActive = new RadioButton();
            rdoInactive = new RadioButton();
            rdoMember = new RadioButton();
            rdoNonMember = new RadioButton();
            rdoSubscribed = new RadioButton();
            rdoNoSubscription = new RadioButton();
            dtpValidFrom = new TextBox();
            dtpExpiresOn = new TextBox();
            nudMaxBorrow = new NumericUpDown();
            listAssignelocation = new ListBox();

            panelRoot.SuspendLayout();
            gbUserRegistration.SuspendLayout();
            tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxBorrow).BeginInit();
            SuspendLayout();

            // panelRoot
            panelRoot.BackColor = Color.PapayaWhip;
            panelRoot.Controls.Add(gbUserRegistration);
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.Location = new Point(0, 0);
            panelRoot.Name = "panelRoot";
            panelRoot.Size = new Size(900, 600);
            panelRoot.TabIndex = 0;

            // gbUserRegistration
            gbUserRegistration.Controls.Add(tlpMain);
            gbUserRegistration.Dock = DockStyle.Fill;
            gbUserRegistration.Location = new Point(0, 0);
            gbUserRegistration.Name = "gbUserRegistration";
            gbUserRegistration.Size = new Size(900, 600);
            gbUserRegistration.TabIndex = 0;
            gbUserRegistration.TabStop = false;
            gbUserRegistration.Text = "User Registration";

            // tlpMain
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

            // lblUserGroupHint
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

            // panelLeft
            panelLeft.Dock = DockStyle.Fill;
            panelLeft.Location = new Point(3, 43);
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new Padding(12);
            panelLeft.Size = new Size(441, 532);
            panelLeft.TabIndex = 1;

            // panelRight
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(450, 43);
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(12);
            panelRight.Size = new Size(441, 532);
            panelRight.TabIndex = 2;

            // Left labels
            ConfigureSectionLabel(lblcolumnname, "Basic Details");
            ConfigureLabel(label1, "User Code");
            ConfigureLabel(label2, "Full Name*");
            ConfigureLabel(label3, "Mobile*");
            ConfigureLabel(label4, "NIC");
            ConfigureLabel(label5, "Email");
            ConfigureLabel(label6, "Address");
            ConfigureLabel(label7, "DOB");
            ConfigureLabel(label8, "Gender");
            ConfigureLabel(label9, "Subscription Type");
            ConfigureLabel(label10, "Location");

            // Left controls
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(250, 23);

            txtName.Name = "txtName";
            txtName.Size = new Size(250, 23);

            txtMobile.Name = "txtMobile";
            txtMobile.Size = new Size(250, 23);

            txtNic.Name = "txtNic";
            txtNic.Size = new Size(250, 23);

            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(250, 23);

            txtAddress.Name = "txtAddress";
            txtAddress.Multiline = true;
            txtAddress.Size = new Size(250, 72);

            dtpDob.Name = "dtpDob";
            dtpDob.Size = new Size(250, 23);

            cmbGender.Name = "cmbGender";
            cmbGender.Size = new Size(250, 23);

            cmbSubscription.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSubscription.Name = "cmbSubscription";
            cmbSubscription.Size = new Size(250, 23);

            cmbLocation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLocation.Name = "cmbLocation";
            cmbLocation.Size = new Size(250, 23);

            // Right labels
            ConfigureSectionLabel(label21, "Account Details");
            ConfigureLabel(label11, "User Group");
            ConfigureLabel(label12, "Password");
            ConfigureLabel(label13, "Confirm Password");
            ConfigureLabel(label14, "User Status");
            ConfigureLabel(label15, "Membership");
            ConfigureLabel(label16, "Subscription");
            ConfigureLabel(label17, "Valid From");
            ConfigureLabel(label18, "Expires On");
            ConfigureLabel(label19, "Max Borrow");
            ConfigureLabel(label20, "Assigned Location");

            // Right controls
            cmbGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGroup.Name = "cmbGroup";
            cmbGroup.Size = new Size(250, 23);

            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(250, 23);
            txtPassword.UseSystemPasswordChar = true;

            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(250, 23);
            txtConfirmPassword.UseSystemPasswordChar = true;

            rdoActive.AutoSize = true;
            rdoActive.Name = "rdoActive";
            rdoActive.Text = "Active";
            rdoActive.UseVisualStyleBackColor = true;

            rdoInactive.AutoSize = true;
            rdoInactive.Name = "rdoInactive";
            rdoInactive.Text = "Inactive";
            rdoInactive.UseVisualStyleBackColor = true;

            rdoMember.AutoSize = true;
            rdoMember.Name = "rdoMember";
            rdoMember.Text = "Member";
            rdoMember.UseVisualStyleBackColor = true;

            rdoNonMember.AutoSize = true;
            rdoNonMember.Name = "rdoNonMember";
            rdoNonMember.Text = "Non-Member";
            rdoNonMember.UseVisualStyleBackColor = true;

            rdoSubscribed.AutoSize = true;
            rdoSubscribed.Name = "rdoSubscribed";
            rdoSubscribed.Text = "Subscribed";
            rdoSubscribed.UseVisualStyleBackColor = true;

            rdoNoSubscription.AutoSize = true;
            rdoNoSubscription.Name = "rdoNoSubscription";
            rdoNoSubscription.Text = "No Subscription";
            rdoNoSubscription.UseVisualStyleBackColor = true;

            dtpValidFrom.Enabled = false;
            dtpValidFrom.Name = "dtpValidFrom";
            dtpValidFrom.Size = new Size(250, 23);

            dtpExpiresOn.Enabled = false;
            dtpExpiresOn.Name = "dtpExpiresOn";
            dtpExpiresOn.Size = new Size(250, 23);

            nudMaxBorrow.Name = "nudMaxBorrow";
            nudMaxBorrow.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudMaxBorrow.Size = new Size(250, 23);
            nudMaxBorrow.Value = new decimal(new int[] { 3, 0, 0, 0 });

            listAssignelocation.Name = "listAssignelocation";
            listAssignelocation.Size = new Size(250, 140);

            // add controls to placeholder panels
            panelLeft.Controls.AddRange(new Control[]
            {
                lblcolumnname,
                label1, txtCode,
                label2, txtName,
                label3, txtMobile,
                label4, txtNic,
                label5, txtEmail,
                label6, txtAddress,
                label7, dtpDob,
                label8, cmbGender,
                label9, cmbSubscription,
                label10, cmbLocation
            });

            panelRight.Controls.AddRange(new Control[]
            {
                label21,
                label11, cmbGroup,
                label12, txtPassword,
                label13, txtConfirmPassword,
                label14, rdoActive, rdoInactive,
                label15, rdoMember, rdoNonMember,
                label16, rdoSubscribed, rdoNoSubscription,
                label17, dtpValidFrom,
                label18, dtpExpiresOn,
                label19, nudMaxBorrow,
                label20, listAssignelocation
            });

            // UserControl
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelRoot);
            Name = "UCRegistrationHandling";
            Size = new Size(900, 600);

            panelRoot.ResumeLayout(false);
            gbUserRegistration.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudMaxBorrow).EndInit();
            ResumeLayout(false);
        }

        private static void ConfigureLabel(Label label, string text)
        {
            label.AutoSize = true;
            label.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label.ForeColor = Color.Teal;
            label.Text = text;
        }

        private static void ConfigureSectionLabel(Label label, string text)
        {
            label.AutoSize = true;
            label.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label.ForeColor = Color.CadetBlue;
            label.Text = text;
        }

        #endregion

        private Panel panelRoot;
        private GroupBox gbUserRegistration;
        private TableLayoutPanel tlpMain;
        private Label lblUserGroupHint;
        private Panel panelLeft;
        private Panel panelRight;

        private Label lblcolumnname;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label label10;

        private TextBox txtCode;
        private TextBox txtName;
        private TextBox txtMobile;
        private TextBox txtNic;
        private TextBox txtEmail;
        private TextBox txtAddress;
        private DateTimePicker dtpDob;
        private ComboBox cmbGender;
        private ComboBox cmbSubscription;
        private ComboBox cmbLocation;

        private Label label21;
        private Label label11;
        private Label label12;
        private Label label13;
        private Label label14;
        private Label label15;
        private Label label16;
        private Label label17;
        private Label label18;
        private Label label19;
        private Label label20;

        private ComboBox cmbGroup;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private RadioButton rdoActive;
        private RadioButton rdoInactive;
        private RadioButton rdoMember;
        private RadioButton rdoNonMember;
        private RadioButton rdoSubscribed;
        private RadioButton rdoNoSubscription;
        private TextBox dtpValidFrom;
        private TextBox dtpExpiresOn;
        private NumericUpDown nudMaxBorrow;
        private ListBox listAssignelocation;
    }
}