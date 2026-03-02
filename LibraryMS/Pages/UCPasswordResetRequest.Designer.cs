#nullable disable
using System.ComponentModel;
using System.Drawing;

namespace LibraryMS.Win.Pages
{
    partial class UCPasswordResetRequest
    {
        private IContainer components = null;

        private System.Windows.Forms.Panel panelRoot;
        private System.Windows.Forms.GroupBox gbReq;
        private System.Windows.Forms.TableLayoutPanel tlpRoot;

        private System.Windows.Forms.TableLayoutPanel tlpHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel flpActions;

        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSubmit;

        private System.Windows.Forms.GroupBox gbForm;
        private System.Windows.Forms.TableLayoutPanel tblForm;

        private System.Windows.Forms.TextBox txtUserCode;
        private System.Windows.Forms.TextBox txtNewPassword;
        private System.Windows.Forms.TextBox txtConfirmPassword;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelRoot = new Panel();
            gbReq = new GroupBox();
            tlpRoot = new TableLayoutPanel();
            tlpHeader = new TableLayoutPanel();
            lblTitle = new Label();
            flpActions = new FlowLayoutPanel();
            btnRefresh = new Button();
            btnClear = new Button();
            btnSubmit = new Button();
            gbForm = new GroupBox();
            tblForm = new TableLayoutPanel();
            txtUserCode = new TextBox();
            txtNewPassword = new TextBox();
            txtConfirmPassword = new TextBox();
            panelRoot.SuspendLayout();
            gbReq.SuspendLayout();
            tlpRoot.SuspendLayout();
            tlpHeader.SuspendLayout();
            flpActions.SuspendLayout();
            gbForm.SuspendLayout();
            tblForm.SuspendLayout();
            SuspendLayout();
            // 
            // panelRoot
            // 
            panelRoot.BackColor = Color.PapayaWhip;
            panelRoot.Controls.Add(gbReq);
            panelRoot.Dock = DockStyle.Fill;
            panelRoot.Location = new Point(0, 0);
            panelRoot.Name = "panelRoot";
            panelRoot.Size = new Size(900, 600);
            panelRoot.TabIndex = 0;
            // 
            // gbReq
            // 
            gbReq.Controls.Add(tlpRoot);
            gbReq.Dock = DockStyle.Fill;
            gbReq.Location = new Point(0, 0);
            gbReq.Name = "gbReq";
            gbReq.Padding = new Padding(10);
            gbReq.Size = new Size(900, 600);
            gbReq.TabIndex = 0;
            gbReq.TabStop = false;
            gbReq.Text = "Password Reset Request";
            // 
            // tlpRoot
            // 
            tlpRoot.ColumnCount = 1;
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpHeader, 0, 0);
            tlpRoot.Controls.Add(gbForm, 0, 1);
            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.Location = new Point(10, 26);
            tlpRoot.Name = "tlpRoot";
            tlpRoot.RowCount = 2;
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRoot.Size = new Size(880, 564);
            tlpRoot.TabIndex = 0;
            // 
            // tlpHeader
            // 
            tlpHeader.ColumnCount = 2;
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle());
            tlpHeader.Controls.Add(lblTitle, 0, 0);
            tlpHeader.Controls.Add(flpActions, 1, 0);
            tlpHeader.Dock = DockStyle.Fill;
            tlpHeader.Location = new Point(3, 3);
            tlpHeader.Name = "tlpHeader";
            tlpHeader.Padding = new Padding(6, 10, 6, 0);
            tlpHeader.RowCount = 1;
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpHeader.Size = new Size(874, 50);
            tlpHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Teal;
            lblTitle.Location = new Point(9, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(613, 40);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Password Reset Request";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // flpActions
            // 
            flpActions.AutoSize = true;
            flpActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpActions.Controls.Add(btnRefresh);
            flpActions.Controls.Add(btnClear);
            flpActions.Controls.Add(btnSubmit);
            flpActions.Dock = DockStyle.Fill;
            flpActions.Location = new Point(625, 10);
            flpActions.Margin = new Padding(0);
            flpActions.Name = "flpActions";
            flpActions.Size = new Size(243, 40);
            flpActions.TabIndex = 1;
            flpActions.WrapContents = false;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(3, 3);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 0;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(84, 3);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 23);
            btnClear.TabIndex = 1;
            // 
            // btnSubmit
            // 
            btnSubmit.Location = new Point(165, 3);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(75, 23);
            btnSubmit.TabIndex = 2;
            // 
            // gbForm
            // 
            gbForm.Controls.Add(tblForm);
            gbForm.Dock = DockStyle.Top;
            gbForm.Location = new Point(3, 59);
            gbForm.Name = "gbForm";
            gbForm.Padding = new Padding(10);
            gbForm.Size = new Size(874, 220);
            gbForm.TabIndex = 1;
            gbForm.TabStop = false;
            gbForm.Text = "Request Details";
            // 
            // tblForm
            // 
            tblForm.ColumnCount = 2;
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tblForm.Controls.Add(txtUserCode, 1, 0);
            tblForm.Controls.Add(txtNewPassword, 1, 1);
            tblForm.Controls.Add(txtConfirmPassword, 1, 2);
            tblForm.Dock = DockStyle.Fill;
            tblForm.Location = new Point(10, 26);
            tblForm.Name = "tblForm";
            tblForm.RowCount = 4;
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblForm.Size = new Size(854, 184);
            tblForm.TabIndex = 0;
            // 
            // txtUserCode
            // 
            txtUserCode.Dock = DockStyle.Fill;
            txtUserCode.Location = new Point(259, 3);
            txtUserCode.Name = "txtUserCode";
            txtUserCode.Size = new Size(592, 23);
            txtUserCode.TabIndex = 0;
            // 
            // txtNewPassword
            // 
            txtNewPassword.Dock = DockStyle.Fill;
            txtNewPassword.Location = new Point(259, 39);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.Size = new Size(592, 23);
            txtNewPassword.TabIndex = 1;
            txtNewPassword.UseSystemPasswordChar = true;
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.Dock = DockStyle.Fill;
            txtConfirmPassword.Location = new Point(259, 75);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(592, 23);
            txtConfirmPassword.TabIndex = 2;
            txtConfirmPassword.UseSystemPasswordChar = true;
            // 
            // UCPasswordResetRequest
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelRoot);
            Name = "UCPasswordResetRequest";
            Size = new Size(900, 600);
            panelRoot.ResumeLayout(false);
            gbReq.ResumeLayout(false);
            tlpRoot.ResumeLayout(false);
            tlpHeader.ResumeLayout(false);
            tlpHeader.PerformLayout();
            flpActions.ResumeLayout(false);
            gbForm.ResumeLayout(false);
            tblForm.ResumeLayout(false);
            tblForm.PerformLayout();
            ResumeLayout(false);
        }

        private static System.Windows.Forms.Label MakeLabel(string text)
        {
            return new System.Windows.Forms.Label
            {
                Text = text,
                AutoSize = true,
                Padding = new System.Windows.Forms.Padding(0, 8, 0, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Black
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
            b.UseVisualStyleBackColor = false;
        }
    }
}