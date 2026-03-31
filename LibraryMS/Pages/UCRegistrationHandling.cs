using System.Text.RegularExpressions;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Helper;
using LibraryMS.Win.Interfaces;
using static LibraryMS.BLL.Models.UserUpdateModels;

namespace LibraryMS.Win.Pages
{
    public partial class UCRegistrationHandling : UserControl, IPageActions
    {
        private readonly RegistrationService _service;

        private bool _editMode = true;
        private bool _isLoadedExistingUser = false;
        private string? _loadedUserCode = null;

        public UCRegistrationHandling(RegistrationService service)
        {
            InitializeComponent();

            _service = service;

            Dock = DockStyle.Fill;
            AutoScaleMode = AutoScaleMode.Dpi;

            ConfigureUi();
            BuildResponsiveLayout();
            ApplyResponsiveColumns();

            Resize += (_, __) => ApplyResponsiveColumns();

            Load += async (_, __) => await OnRefreshAsync();
            rdoSubscribed.CheckedChanged += (_, __) => ApplySubscriptionUi();
            rdoNoSubscription.CheckedChanged += (_, __) => ApplySubscriptionUi();
            cmbSubscription.SelectedIndexChanged += (_, __) => UpdateExpireDate();
            cmbGroup.SelectedIndexChanged += (_, __) => ApplyGroupRulesUi();
            cmbLocation.SelectedIndexChanged += (_, __) => UpdateAssignedLocationsList();
            txtCode.KeyDown += TxtCode_KeyDown;
        }
        private async void TxtCode_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F2)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            var picked = FrmLookup.Pick(
                this,
                "User Lookup",
                _service.LookupUsersAsync);

            if (picked == null || string.IsNullOrWhiteSpace(picked.Code))
                return;

            await LoadExistingUserAsync(picked.Code, true);
        }

        private async Task LoadExistingUserAsync(string userCode, bool enableEdit = true)
        {
            var u = await _service.GetUserForEditAsync(userCode);
            if (u == null)
            {
                MessageBox.Show("User not found.");
                return;
            }

            txtCode.Text = u.Code;
            txtName.Text = u.Name;
            txtMobile.Text = u.Mobile;
            txtEmail.Text = u.Email ?? "";
            txtNic.Text = u.Nic ?? "";
            txtAddress.Text = u.Address ?? "";
            dtpDob.Value = u.Dob ?? DateTime.Now.Date;
            dtpValidFrom.Text = (u.RegisteredDate ?? DateTime.Now.Date).ToString("yyyy-MM-dd");
            dtpExpiresOn.Text = (u.ExpiredDate ?? DateTime.Now.Date).ToString("yyyy-MM-dd");

            cmbGroup.SelectedValue = u.GroupCode;

            rdoActive.Checked = u.Active;
            rdoInactive.Checked = !u.Active;
            rdoMember.Checked = u.MemberStatus;
            rdoNonMember.Checked = !u.MemberStatus;

            rdoSubscribed.Checked = u.SubscriptionStatus;
            rdoNoSubscription.Checked = !u.SubscriptionStatus;

            if (!string.IsNullOrWhiteSpace(u.SubscriptionId))
                cmbSubscription.SelectedValue = u.SubscriptionId;
            else if (cmbSubscription.Items.Count > 0)
                cmbSubscription.SelectedIndex = 0;

            if (!string.IsNullOrWhiteSpace(u.Gender) && cmbGender.Items.Contains(u.Gender))
                cmbGender.SelectedItem = u.Gender;
            else if (cmbGender.Items.Count > 0)
                cmbGender.SelectedIndex = 0;

            nudMaxBorrow.Value = Math.Max(
                nudMaxBorrow.Minimum,
                Math.Min(nudMaxBorrow.Maximum, u.MaxBorrow));

            if (u.AllLocations)
                SelectLocationCombo("ALL");
            else if (!string.IsNullOrWhiteSpace(u.LocationCode))
                SelectLocationCombo(u.LocationCode);

            UpdateAssignedLocationsList();
            //ApplyGroupRulesUi();
            //ApplySubscriptionUi();
           // UpdateExpireDate();

            txtPassword.Clear();
            txtConfirmPassword.Clear();

            _isLoadedExistingUser = true;
            _loadedUserCode = u.Code;

            _editMode = enableEdit;
            SetInputsEnabled(enableEdit);
            txtCode.ReadOnly = true;

            MessageBox.Show(
                enableEdit
                    ? "User loaded successfully. You can now edit and save."
                    : "User loaded. Click EDIT to modify, then SAVE to update.",
                "Loaded",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        private void ConfigureUi()
        {
            txtPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true;

            cmbGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSubscription.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLocation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGender.DropDownStyle = ComboBoxStyle.DropDownList;

            rdoMember.Text = "Member";
            rdoNonMember.Text = "Non-Member";
            rdoSubscribed.Text = "Subscribed";
            rdoNoSubscription.Text = "No Subscription";

            dtpValidFrom.Enabled = false;
            dtpExpiresOn.Enabled = false;

            listAssignelocation.SelectionMode = SelectionMode.One;
        }

        private void BuildResponsiveLayout()
        {
            SuspendLayout();

            panelRoot.AutoScroll = false;

            panelLeft.AutoScroll = true;
            panelRight.AutoScroll = true;

            panelLeft.Padding = new Padding(12);
            panelRight.Padding = new Padding(12);

            panelLeft.Controls.Clear();
            panelRight.Controls.Clear();

            panelLeft.Controls.Add(BuildLeftResponsivePanel());
            panelRight.Controls.Add(BuildRightResponsivePanel());

            ResumeLayout(true);
        }

        private void ApplyResponsiveColumns()
        {
            if (tlpMain.IsDisposed) return;

            bool singleColumn = Width < 980;

            tlpMain.SuspendLayout();
            tlpMain.Controls.Clear();
            tlpMain.ColumnStyles.Clear();
            tlpMain.RowStyles.Clear();

            if (singleColumn)
            {
                tlpMain.ColumnCount = 1;
                tlpMain.RowCount = 3;

                tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
                tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

                tlpMain.Controls.Add(lblUserGroupHint, 0, 0);
                tlpMain.Controls.Add(panelLeft, 0, 1);
                tlpMain.Controls.Add(panelRight, 0, 2);

                tlpMain.SetColumnSpan(lblUserGroupHint, 1);
            }
            else
            {
                tlpMain.ColumnCount = 2;
                tlpMain.RowCount = 2;

                tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
                tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                tlpMain.Controls.Add(lblUserGroupHint, 0, 0);
                tlpMain.Controls.Add(panelLeft, 0, 1);
                tlpMain.Controls.Add(panelRight, 1, 1);

                tlpMain.SetColumnSpan(lblUserGroupHint, 2);
            }

            tlpMain.ResumeLayout(true);
        }

        private Control BuildLeftResponsivePanel()
        {
            var layout = CreateStackLayout();

            AddRow(layout, lblcolumnname);
            AddRow(layout, BuildField(label1, txtCode));
            AddRow(layout, BuildField(label2, txtName));
            AddRow(layout, BuildField(label3, txtMobile));
            AddRow(layout, BuildField(label4, txtNic));
            AddRow(layout, BuildField(label5, txtEmail));
            AddRow(layout, BuildField(label6, txtAddress, 72));
            AddRow(layout, BuildTwoFieldRow(label7, dtpDob, label8, cmbGender));
            AddRow(layout, BuildField(label9, cmbSubscription));
            AddRow(layout, BuildField(label10, cmbLocation));

            return layout;
        }

        private Control BuildRightResponsivePanel()
        {
            var layout = CreateStackLayout();

            AddRow(layout, label21);
            AddRow(layout, BuildField(label11, cmbGroup));
            AddRow(layout, BuildField(label12, txtPassword));
            AddRow(layout, BuildField(label13, txtConfirmPassword));
            AddRow(layout, BuildRadioGroups());
            AddRow(layout, BuildTwoFieldRow(label17, dtpValidFrom, label18, dtpExpiresOn));
            AddRow(layout, BuildField(label19, nudMaxBorrow));
            AddRow(layout, BuildField(label20, listAssignelocation, 140));

            return layout;
        }

        private TableLayoutPanel CreateStackLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 0,
                GrowStyle = TableLayoutPanelGrowStyle.AddRows,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            return layout;
        }

        private void AddRow(TableLayoutPanel parent, Control control)
        {
            parent.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            parent.Controls.Add(control, 0, parent.RowCount);
            parent.RowCount++;
        }

        private Control BuildField(Control label, Control input, int inputHeight = 30)
        {
            var field = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(0)
            };

            field.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            field.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            field.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            label.Dock = DockStyle.Top;
            label.Margin = new Padding(0, 0, 0, 4);

            input.Dock = DockStyle.Top;
            input.Margin = new Padding(0);

            if (input is TextBox tb && tb.Multiline)
                tb.Height = inputHeight;
            else if (input is ListBox lb)
                lb.Height = inputHeight;

            field.Controls.Add(label, 0, 0);
            field.Controls.Add(input, 0, 1);

            return field;
        }

        private Control BuildTwoFieldRow(Control label1, Control input1, Control label2, Control input2)
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(0)
            };

            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            row.Controls.Add(BuildField(label1, input1), 0, 0);
            row.Controls.Add(BuildField(label2, input2), 1, 0);

            return row;
        }

        private Control BuildRadioGroups()
        {
            var wrap = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(0)
            };

            wrap.Controls.Add(BuildRadioGroup("User Status", rdoActive, rdoInactive));
            wrap.Controls.Add(BuildRadioGroup("Membership", rdoMember, rdoNonMember));
            wrap.Controls.Add(BuildRadioGroup("Subscription", rdoSubscribed, rdoNoSubscription));

            return wrap;
        }

        private Control BuildRadioGroup(string title, RadioButton rb1, RadioButton rb2)
        {
            var group = new GroupBox
            {
                Text = title,
                Width = 132,
                Height = 86,
                Margin = new Padding(0, 0, 10, 10)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(8, 4, 8, 4)
            };

            rb1.AutoSize = true;
            rb2.AutoSize = true;

            flow.Controls.Add(rb1);
            flow.Controls.Add(rb2);
            group.Controls.Add(flow);

            return group;
        }

        public void OnEdit()
        {
            _editMode = !_editMode;
            SetInputsEnabled(_editMode);
        }

        private void SetInputsEnabled(bool enabled)
        {
            txtCode.ReadOnly = _isLoadedExistingUser || !enabled;
            txtName.ReadOnly = !enabled;
            txtMobile.ReadOnly = !enabled;
            txtNic.ReadOnly = !enabled;
            txtEmail.ReadOnly = !enabled;
            txtAddress.ReadOnly = !enabled;
            txtPassword.ReadOnly = !enabled;
            txtConfirmPassword.ReadOnly = !enabled;

            cmbGroup.Enabled = enabled;
            cmbGender.Enabled = enabled;
            cmbLocation.Enabled = enabled;

            rdoActive.Enabled = enabled;
            rdoInactive.Enabled = enabled;
            rdoMember.Enabled = enabled;
            rdoNonMember.Enabled = enabled;
            rdoSubscribed.Enabled = enabled;
            rdoNoSubscription.Enabled = enabled;

            cmbSubscription.Enabled = enabled && rdoSubscribed.Checked;
            nudMaxBorrow.Enabled = enabled;
            dtpDob.Enabled = enabled;
        }

        public async Task OnRefreshAsync()
        {
            await LoadLookupsAsync();
            ApplyDefaultsFromConfig();
            ClearForm();
            ApplyGroupRulesUi();
            ApplySubscriptionUi();
            UpdateExpireDate();
            UpdateAssignedLocationsList();
        }

        public async Task OnSaveAsync()
        {
            if (!ValidateForm(out var msg))
            {
                MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var groupCode = cmbGroup.SelectedValue?.ToString() ?? "";
            int maxBorrow = (int)nudMaxBorrow.Value;

            if (_isLoadedExistingUser && !string.IsNullOrWhiteSpace(_loadedUserCode))
            {
                if (!ValidateUpdateForm(out var msg2))
                {
                    MessageBox.Show(msg2, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var upd = BuildUpdateRequest(groupCode, maxBorrow);
                var (ok2, message2) = await _service.UpdateUserAsync(upd);

                MessageBox.Show(message2, ok2 ? "Success" : "Error",
                    MessageBoxButtons.OK,
                    ok2 ? MessageBoxIcon.Information : MessageBoxIcon.Error);

                if (ok2) await OnRefreshAsync();
                return;
            }

            if (!ConfirmPoliciesBeforeSave(groupCode, maxBorrow))
            {
                MessageBox.Show("You must agree to continue.", "Policy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var req = BuildRequest(groupCode, maxBorrow);

            var grp = cmbGroup.SelectedItem as UserGroupItem;
            if (grp != null)
            {
                req.MembershipFee = grp.MembershipFee;

                if (grp.MembershipFee > 0)
                {
                    var ask = MessageBox.Show(
                        "Please make payment now?",
                        "Membership Payment",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (ask == DialogResult.Yes)
                    {
                        using var pay = new frmMembershipPayment(grp.Name, grp.MembershipFee);
                        if (pay.ShowDialog() != DialogResult.OK)
                            return;

                        req.PaidAmt = pay.PaidAmount;
                        req.PaymentMethod = pay.PaymentMethod;
                        req.ReferenceNo = pay.ReferenceNo;
                    }
                    else
                    {
                        req.PaidAmt = 0m;
                    }
                }
            }

            var (ok, message) = await _service.RegisterAsync(req, AppSession.Current?.UserCode);

            MessageBox.Show(message, ok ? "Success" : "Error",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (ok) await OnRefreshAsync();
        }

        public Task OnProcessAsync() => Task.CompletedTask;

        private async Task LoadExistingUserAsync(string userCode)
        {
            var u = await _service.GetUserForEditAsync(userCode);
            if (u == null)
            {
                MessageBox.Show("User not found.");
                return;
            }

            txtCode.Text = u.Code;
            txtName.Text = u.Name;
            txtMobile.Text = u.Mobile;
            txtEmail.Text = u.Email ?? "";
            txtNic.Text = u.Nic ?? "";
            txtAddress.Text = u.Address ?? "";
            dtpDob.Value = u.Dob ?? DateTime.Now.Date;

            cmbGroup.SelectedValue = u.GroupCode;

            rdoActive.Checked = u.Active;
            rdoInactive.Checked = !u.Active;

            rdoMember.Checked = u.MemberStatus;
            rdoNonMember.Checked = !u.MemberStatus;

            rdoSubscribed.Checked = u.SubscriptionStatus;
            rdoNoSubscription.Checked = !u.SubscriptionStatus;
           
            if (!string.IsNullOrWhiteSpace(u.SubscriptionId))
                cmbSubscription.SelectedValue = u.SubscriptionId;

            if (!string.IsNullOrWhiteSpace(u.Gender) && cmbGender.Items.Contains(u.Gender))
                cmbGender.SelectedItem = u.Gender;

            nudMaxBorrow.Value = Math.Max(nudMaxBorrow.Minimum, Math.Min(nudMaxBorrow.Maximum, u.MaxBorrow));

            if (u.AllLocations)
                SelectLocationCombo("ALL");
            else if (!string.IsNullOrWhiteSpace(u.LocationCode))
                SelectLocationCombo(u.LocationCode);

            UpdateAssignedLocationsList();
            ApplyGroupRulesUi();
            ApplySubscriptionUi();
            UpdateExpireDate();

            txtPassword.Clear();
            txtConfirmPassword.Clear();

            _isLoadedExistingUser = true;
            _loadedUserCode = u.Code;

            _editMode = false;
            SetInputsEnabled(false);
            txtCode.ReadOnly = true;

            MessageBox.Show(
                "User loaded. Click EDIT to modify, then SAVE to update.",
                "Loaded",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void SelectLocationCombo(string code)
        {
            for (int i = 0; i < cmbLocation.Items.Count; i++)
            {
                if (cmbLocation.Items[i] is ComboItem ci &&
                    ci.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
                {
                    cmbLocation.SelectedIndex = i;
                    return;
                }
            }

            if (cmbLocation.Items.Count > 0)
                cmbLocation.SelectedIndex = 0;
        }

        private async Task LoadLookupsAsync()
        {
            var groups = await _service.GetGroupsAsync();
            cmbGroup.DisplayMember = "Name";
            cmbGroup.ValueMember = "Code";
            cmbGroup.DataSource = groups;

            var subs = await _service.GetSubscriptionsAsync();
            cmbSubscription.DisplayMember = "Desc";
            cmbSubscription.ValueMember = "Id";
            cmbSubscription.DataSource = subs;

            var locs = await _service.GetLocationsAsync();
            cmbLocation.Items.Clear();
            cmbLocation.Items.Add(new ComboItem("ALL", "-- ALL LOCATIONS --"));
            foreach (var l in locs)
                cmbLocation.Items.Add(new ComboItem(l.Code, l.Desc));
            cmbLocation.SelectedIndex = 0;

            cmbGender.Items.Clear();
            cmbGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
            if (cmbGender.Items.Count > 0)
                cmbGender.SelectedIndex = 0;
        }

        private void ApplyDefaultsFromConfig()
        {
            var settings = AppConfig.GetSection<RegistrationUiSettings>("RegistrationUi");

            var v = settings.Defaults.MaxBorrow;
            if (v < nudMaxBorrow.Minimum) v = (int)nudMaxBorrow.Minimum;
            if (v > nudMaxBorrow.Maximum) v = (int)nudMaxBorrow.Maximum;

            nudMaxBorrow.Value = v;
        }

        private void ApplyGroupRulesUi()
        {
            var groupCode = cmbGroup.SelectedValue?.ToString() ?? "";

            if (groupCode.Equals("USER", StringComparison.OrdinalIgnoreCase))
            {
                lblUserGroupHint.Visible = true;
                lblUserGroupHint.Text =
                    "Group = USER → sent for approval; account stays INACTIVE until approved. " +
                    "Default menus: Make Payment, Make Reservation.";

                rdoInactive.Checked = true;
                rdoActive.Enabled = false;
            }
            else
            {
                lblUserGroupHint.Visible = false;
                rdoActive.Enabled = true;
            }
        }

        private void ApplySubscriptionUi()
        {
            cmbSubscription.Enabled = rdoSubscribed.Checked;
            UpdateExpireDate();
        }

        private void UpdateExpireDate()
        {
            var now = DateTime.Now;
            dtpValidFrom.Text = now.ToString("yyyy-MM-dd");

            if (!rdoSubscribed.Checked || cmbSubscription.SelectedItem is not SubscriptionItem sub)
            {
                dtpExpiresOn.Text = now.ToString("yyyy-MM-dd");
                return;
            }

            dtpExpiresOn.Text = now.AddDays(sub.Days).ToString("yyyy-MM-dd");
        }

        private void UpdateAssignedLocationsList()
        {
            listAssignelocation.Items.Clear();

            if (cmbLocation.SelectedItem is not ComboItem sel)
                return;

            if (sel.Code == "ALL")
            {
                foreach (var item in cmbLocation.Items)
                {
                    if (item is ComboItem ci && ci.Code != "ALL")
                        listAssignelocation.Items.Add(ci.Text);
                }
            }
            else
            {
                listAssignelocation.Items.Add(sel.Text);
            }
        }

        private bool ConfirmPoliciesBeforeSave(string groupCode, int maxBorrow)
        {
            var settings = AppConfig.GetSection<RegistrationUiSettings>("RegistrationUi");
            var popup = settings.PolicyPopup;

            var lines = new List<string>();
            lines.AddRange(popup.CommonLines);

            if (popup.GroupLines != null && popup.GroupLines.TryGetValue(groupCode, out var gLines))
                lines.AddRange(gLines);

            foreach (var t in popup.TemplateLines)
                lines.Add(t.Replace("{MaxBorrow}", maxBorrow.ToString()));

            var body = "• " + string.Join(Environment.NewLine + "• ", lines);

            using var dlg = new frmPolicyConsent(popup.Title, body);
            return dlg.ShowDialog() == DialogResult.OK && dlg.Agreed;
        }

        private bool ValidateForm(out string message)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text)) { message = "User Code is required."; return false; }
            if (string.IsNullOrWhiteSpace(txtName.Text)) { message = "Full Name is required."; return false; }
            if (string.IsNullOrWhiteSpace(txtMobile.Text)) { message = "Mobile is required."; return false; }

            var normalized = NormalizeMobile(txtMobile.Text);
            if (!IsValidSriLankaMobile94(normalized))
            {
                message = "Mobile must be a valid Sri Lanka number. Example: 0771234567 or 94771234567";
                return false;
            }

            txtMobile.Text = normalized;

            if (!IsValidEmail(txtEmail.Text))
            {
                message = "Invalid email address.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text)) { message = "Password is required."; return false; }
            if (txtPassword.Text != txtConfirmPassword.Text) { message = "Password and Confirm Password do not match."; return false; }

            if (cmbGroup.SelectedItem is null) { message = "Select User Group."; return false; }
            if (cmbLocation.SelectedItem is null) { message = "Select Location."; return false; }

            if (rdoSubscribed.Checked && cmbSubscription.SelectedItem is null)
            {
                message = "Select Subscription Type.";
                return false;
            }

            message = "";
            return true;
        }

        private bool ValidateUpdateForm(out string message)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text)) { message = "User Code is required."; return false; }
            if (string.IsNullOrWhiteSpace(txtName.Text)) { message = "Full Name is required."; return false; }
            if (string.IsNullOrWhiteSpace(txtMobile.Text)) { message = "Mobile is required."; return false; }
            if (cmbGroup.SelectedItem is null) { message = "Select User Group."; return false; }
            if (cmbLocation.SelectedItem is null) { message = "Select Location."; return false; }

            var normalized = NormalizeMobile(txtMobile.Text);
            if (!IsValidSriLankaMobile94(normalized))
            {
                message = "Mobile must be valid. Example: 0771234567 or 94771234567";
                return false;
            }

            txtMobile.Text = normalized;

            if (!IsValidEmail(txtEmail.Text))
            {
                message = "Invalid email address.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtPassword.Text) || !string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
            {
                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    message = "Password and Confirm Password do not match.";
                    return false;
                }
            }

            message = "";
            return true;
        }

        private UserRegistrationRequest BuildRequest(string groupCode, int maxBorrow)
        {
            var loc = (ComboItem)cmbLocation.SelectedItem!;
            bool allLocs = loc.Code == "ALL";

            SubscriptionItem? sub = null;
            if (rdoSubscribed.Checked && cmbSubscription.SelectedItem is SubscriptionItem s)
                sub = s;

            return new UserRegistrationRequest
            {
                Code = txtCode.Text.Trim(),
                Name = txtName.Text.Trim(),
                Mobile = txtMobile.Text.Trim(),
                GroupCode = groupCode,

                Active = rdoActive.Checked,
                MemberStatus = rdoMember.Checked,
                SubscriptionStatus = rdoSubscribed.Checked,

                Password = txtPassword.Text,
                Email = txtEmail.Text.Trim(),
                Nic = txtNic.Text.Trim(),
                Address = txtAddress.Text.Trim(),

                Dob = dtpDob.Value.Date,
                Gender = cmbGender.SelectedItem?.ToString(),

                SubscriptionId = sub?.Id,
                SubscriptionDays = sub?.Days,

                MaxBorrow = maxBorrow,

                AllLocations = allLocs,
                LocationCode = allLocs ? null : loc.Code
            };
        }

        private UserUpdateRequest BuildUpdateRequest(string groupCode, int maxBorrow)
        {
            var loc = (ComboItem)cmbLocation.SelectedItem!;
            bool allLocs = loc.Code == "ALL";

            SubscriptionItem? sub = null;
            if (rdoSubscribed.Checked && cmbSubscription.SelectedItem is SubscriptionItem s)
                sub = s;

            return new UserUpdateRequest
            {
                Code = txtCode.Text.Trim(),
                Name = txtName.Text.Trim(),
                Mobile = txtMobile.Text.Trim(),
                GroupCode = groupCode,

                Active = rdoActive.Checked,
                MemberStatus = rdoMember.Checked,
                SubscriptionStatus = rdoSubscribed.Checked,

                SubscriptionId = sub?.Id,
                SubscriptionDays = sub?.Days,

                Email = txtEmail.Text.Trim(),
                Nic = txtNic.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Dob = dtpDob.Value.Date,
                Gender = cmbGender.SelectedItem?.ToString(),

                MaxBorrow = maxBorrow,

                AllLocations = allLocs,
                LocationCode = allLocs ? null : loc.Code,

                NewPassword = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text
            };
        }

        private void ClearForm()
        {
            _isLoadedExistingUser = false;
            _loadedUserCode = null;
            _editMode = true;

            txtCode.Clear();
            txtName.Clear();
            txtMobile.Clear();
            txtNic.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();

            listAssignelocation.Items.Clear();

            rdoActive.Checked = true;
            rdoNonMember.Checked = true;
            rdoNoSubscription.Checked = true;

            if (cmbGroup.Items.Count > 0) cmbGroup.SelectedIndex = 0;
            if (cmbSubscription.Items.Count > 0) cmbSubscription.SelectedIndex = 0;
            if (cmbLocation.Items.Count > 0) cmbLocation.SelectedIndex = 0;

            dtpDob.Value = DateTime.Now.Date;
            SetInputsEnabled(true);
            txtCode.ReadOnly = false;
        }

        private static string NormalizeMobile(string? raw)
        {
            var s = (raw ?? "").Trim();
            s = Regex.Replace(s, @"\D", "");

            if (s.StartsWith("0") && s.Length == 10)
                s = "94" + s[1..];
            else if (s.StartsWith("94") && s.Length == 11)
                return s;
            else if (s.Length == 9)
                s = "94" + s;
            else if (s.Length == 10 && !s.StartsWith("0"))
                s = "94" + s[1..];

            return s;
        }

        private static bool IsValidSriLankaMobile94(string mobile94)
        {
            return Regex.IsMatch(mobile94, @"^94\d{9}$");
        }

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;

            email = email.Trim();
            return Regex.IsMatch(
                email,
                @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$",
                RegexOptions.IgnoreCase);
        }

        private sealed class ComboItem
        {
            public string Code { get; }
            public string Text { get; }

            public ComboItem(string code, string text)
            {
                Code = code;
                Text = text;
            }

            public override string ToString() => Text;
        }
    }
}