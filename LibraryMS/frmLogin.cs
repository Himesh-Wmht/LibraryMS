namespace LibraryMS
{
    public partial class frmLogin : Form
    {
        private bool _pwdVisible = false;

        public frmLogin()
        {
            InitializeComponent();

            this.Opacity = 1.0;
            this.TransparencyKey = Color.Empty;
            // Font size you want
            txtPassword.Font = new Font("Segoe UI", 14f);
            txtUserName.Font = new Font("Segoe UI", 14f);

            // Stop WinForms from auto-snapping the height to the font
            txtPassword.AutoSize = false;
            txtUserName.AutoSize = false;

            // Now you can keep a nice tall box
            txtPassword.Height = 30;
            txtUserName.Height = 30;

            // Password as *
            txtPassword.UseSystemPasswordChar = true; // or: textBoxPassword.PasswordChar = '*';

            pictureBox2.BackColor = txtPassword.BackColor;   // usually white (SystemColors.Window)
            pictureBox2.BringToFront();
            pictureBox2.Cursor = Cursors.Hand;

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnTogglePwd_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            int pos = txtPassword.SelectionStart;

            _pwdVisible = !_pwdVisible;
            txtPassword.UseSystemPasswordChar = !_pwdVisible;

            // Optional: change icon if you have resources
            // pictureBox2.Image = _pwdVisible ? Properties.Resources.unlock : Properties.Resources.lock;

            txtPassword.Focus();
            txtPassword.SelectionStart = pos;
        }
    }
}
