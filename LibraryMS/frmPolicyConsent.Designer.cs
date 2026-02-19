namespace LibraryMS.Win
{
    partial class frmPolicyConsent
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
            txtBody = new TextBox();
            chkAgree = new CheckBox();
            btnOk = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // txtBody
            // 
            txtBody.Dock = DockStyle.Fill;
            txtBody.Location = new Point(0, 0);
            txtBody.Multiline = true;
            txtBody.Name = "txtBody";
            txtBody.ScrollBars = ScrollBars.Vertical;
            txtBody.Size = new Size(800, 450);
            txtBody.TabIndex = 0;
            // 
            // chkAgree
            // 
            chkAgree.AutoSize = true;
            chkAgree.Location = new Point(423, 419);
            chkAgree.Name = "chkAgree";
            chkAgree.Size = new Size(63, 19);
            chkAgree.TabIndex = 1;
            chkAgree.Text = "I Agree";
            chkAgree.UseVisualStyleBackColor = true;
            chkAgree.CheckedChanged += chkAgree_CheckedChanged;
            // 
            // btnOk
            // 
            btnOk.Location = new Point(507, 415);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 23);
            btnOk.TabIndex = 2;
            btnOk.Text = "Continue";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(630, 415);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmPolicyConsent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.PapayaWhip;
            ClientSize = new Size(800, 450);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(chkAgree);
            Controls.Add(txtBody);
            Name = "frmPolicyConsent";
            Text = "frmPolicyConsent";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtBody;
        private CheckBox chkAgree;
        private Button btnOk;
        private Button btnCancel;
    }
}