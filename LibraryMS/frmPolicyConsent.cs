using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace LibraryMS.Win
{
    public partial class frmPolicyConsent : Form
    {
        public bool Agreed => chkAgree.Checked;
        public frmPolicyConsent(string title, string body)
        {
            InitializeComponent();
            Text = title;

            txtBody.ReadOnly = true;
            txtBody.Text = body;

            btnOk.Enabled = false;
            chkAgree.CheckedChanged += (_, __) => btnOk.Enabled = chkAgree.Checked;

            btnOk.Click += (_, __) => { DialogResult = DialogResult.OK; Close(); };
            btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };

        }

        private void chkAgree_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
