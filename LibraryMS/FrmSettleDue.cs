using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryMS.Win
{
    public partial class FrmSettleDue : Form
    {
        private readonly decimal _maxDue;

        private TextBox txtAmount = null!;
        private ComboBox cmbMethod = null!;
        private TextBox txtRef = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;
        private Label lblDue = null!;

        public decimal PayAmount { get; private set; }
        public string? PaymentMethod => cmbMethod.SelectedItem?.ToString();
        public string? ReferenceNo => string.IsNullOrWhiteSpace(txtRef.Text) ? null : txtRef.Text.Trim();

        public FrmSettleDue(decimal dueAmt)
        {
            _maxDue = dueAmt;

            Text = "Settle Due Amount";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 420;
            Height = 240;

            BuildUi();
        }

        private void BuildUi()
        {
            lblDue = new Label
            {
                Text = $"Due Amount: {_maxDue:N2}",
                Dock = DockStyle.Top,
                Height = 35,
                Padding = new Padding(12, 12, 12, 0),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(12),
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            panel.Controls.Add(new Label { Text = "Pay Amount", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            txtAmount = new TextBox { Dock = DockStyle.Fill, Text = _maxDue.ToString("0.00", CultureInfo.InvariantCulture) };
            panel.Controls.Add(txtAmount, 1, 0);

            panel.Controls.Add(new Label { Text = "Method", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            cmbMethod = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMethod.Items.AddRange(new object[] { "CASH", "CARD", "ONLINE" });
            cmbMethod.SelectedIndex = 0;
            panel.Controls.Add(cmbMethod, 1, 1);

            panel.Controls.Add(new Label { Text = "Reference", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
            txtRef = new TextBox { Dock = DockStyle.Fill };
            panel.Controls.Add(txtRef, 1, 2);

            var flp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btnOk = new Button { Text = "OK", Width = 90, Height = 30 };
            btnCancel = new Button { Text = "Cancel", Width = 90, Height = 30 };
            btnOk.Click += (_, __) => OnOk();
            btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };
            flp.Controls.Add(btnOk);
            flp.Controls.Add(btnCancel);

            panel.Controls.Add(flp, 0, 3);
            panel.SetColumnSpan(flp, 2);

            Controls.Add(panel);
            Controls.Add(lblDue);
        }

        private void OnOk()
        {
            if (!decimal.TryParse(txtAmount.Text.Trim(), out var amt))
            {
                MessageBox.Show("Invalid amount.");
                return;
            }

            if (amt <= 0m)
            {
                MessageBox.Show("Amount must be greater than 0.");
                return;
            }

            if (amt > _maxDue)
            {
                MessageBox.Show("Amount cannot exceed the due amount.");
                return;
            }

            PayAmount = amt;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
