using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryMS.Win
{
    public partial class frmMembershipPayment : Form
    {
        public decimal PaidAmount => nudPaid.Value;
        public string? PaymentMethod => cmbMethod.SelectedItem?.ToString();
        public string? ReferenceNo => txtRef.Text.Trim();

        public frmMembershipPayment(string groupName, decimal membershipFee)
        {
            Text = "Membership Payment";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Size = new Size(420, 270);

            var lbl = new Label
            {
                Text = $"Membership amount for {groupName}: {membershipFee:N2}",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 10, 12, 0)
            };

            nudPaid = new NumericUpDown
            {
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = membershipFee,
                Value = membershipFee,
                Dock = DockStyle.Top,
                Height = 28
            };

            cmbMethod = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 28
            };
            cmbMethod.Items.AddRange(new object[] { "Cash", "Card", "Bank Transfer", "Other" });
            cmbMethod.SelectedIndex = 0;

            txtRef = new TextBox { Dock = DockStyle.Top, Height = 28, PlaceholderText = "Reference No (optional)" };

            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 90 };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90 };

            var pnlBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10)
            };
            pnlBtns.Controls.Add(btnOk);
            pnlBtns.Controls.Add(btnCancel);

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            pnl.Controls.Add(txtRef);
            pnl.Controls.Add(new Label { Text = "Reference No", Dock = DockStyle.Top, Height = 18 });
            pnl.Controls.Add(cmbMethod);
            pnl.Controls.Add(new Label { Text = "Payment Method", Dock = DockStyle.Top, Height = 18 });
            pnl.Controls.Add(nudPaid);
            pnl.Controls.Add(new Label { Text = "Paid Amount", Dock = DockStyle.Top, Height = 18 });
            pnl.Controls.Add(lbl);

            Controls.Add(pnl);
            Controls.Add(pnlBtns);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private NumericUpDown nudPaid = null!;
        private ComboBox cmbMethod = null!;
        private TextBox txtRef = null!;
    }
}
