using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Helper;
using LibraryMS.Win.Interfaces;

namespace LibraryMS.Win.Pages;

public partial class UCRegistrationHandling : UserControl, IPageActions
{
    private readonly RegistrationService _service;
    // optional edit toggle
    private bool _editMode = true;
    public UCRegistrationHandling(RegistrationService service)
    {
        InitializeComponent();
        _service = service;

        Dock = DockStyle.Fill;

        // Fix radio grouping 
        FixRadioGroups_DesignerBased();
        // Defaults
        txtPassword.UseSystemPasswordChar = true;
        txtConfirmPassword.UseSystemPasswordChar = true;

        cmbGroup.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbSubscription.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLocation.DropDownStyle = ComboBoxStyle.DropDownList;

        // Correct radio texts
        rdoMember.Text = "Member";
        rdoNonMember.Text = "Non-Member";
        rdoSubscribed.Text = "Subscribed";
        rdoNoSubscription.Text = "No Subscription";

        dtpValidFrom.Enabled = false;
        dtpExpiresOn.Enabled = false;

        // Events
        Load += async (_, __) => await OnRefreshAsync();
        rdoSubscribed.CheckedChanged += (_, __) => ApplySubscriptionUi();
        rdoNoSubscription.CheckedChanged += (_, __) => ApplySubscriptionUi();
        cmbSubscription.SelectedIndexChanged += (_, __) => UpdateExpireDate();

        cmbGroup.SelectedIndexChanged += (_, __) => ApplyGroupRulesUi();
        cmbLocation.SelectedIndexChanged += (_, __) => UpdateAssignedLocationsList();
    }
    private void FixRadioGroups()
    {
        WrapAsGroup(rdoActive, rdoInactive);
        WrapAsGroup(rdoMember, rdoNonMember);
        WrapAsGroup(rdoSubscribed, rdoNoSubscription);
    }
    private void FixRadioGroups_DesignerBased()
    {
        // Create 3 small panels inside panelRight (each panel becomes one radio group)
        var pnlUserStatus = new Panel
        {
            Name = "pnlUserStatus",
            BackColor = Color.Transparent,
            Location = new Point(23, 205),   // around your user status radios
            Size = new Size(110, 45)
        };

        var pnlMembership = new Panel
        {
            Name = "pnlMembership",
            BackColor = Color.Transparent,
            Location = new Point(141, 205),  // around membership radios
            Size = new Size(120, 45)
        };

        var pnlSubscription = new Panel
        {
            Name = "pnlSubscription",
            BackColor = Color.Transparent,
            Location = new Point(253, 205),  // around subscription radios
            Size = new Size(170, 45)
        };

        // Add panels to panelRight
        panelRight.Controls.Add(pnlUserStatus);
        panelRight.Controls.Add(pnlMembership);
        panelRight.Controls.Add(pnlSubscription);

        pnlUserStatus.BringToFront();
        pnlMembership.BringToFront();
        pnlSubscription.BringToFront();

        // Remove radios from panelRight first
        panelRight.Controls.Remove(rdoActive);
        panelRight.Controls.Remove(rdoInactive);
        panelRight.Controls.Remove(rdoMember);
        panelRight.Controls.Remove(rdoNonMember);
        panelRight.Controls.Remove(rdoSubscribed);
        panelRight.Controls.Remove(rdoNoSubscription);

        // Add each pair into its own panel
        pnlUserStatus.Controls.Add(rdoActive);
        pnlUserStatus.Controls.Add(rdoInactive);

        pnlMembership.Controls.Add(rdoMember);
        pnlMembership.Controls.Add(rdoNonMember);

        pnlSubscription.Controls.Add(rdoSubscribed);
        pnlSubscription.Controls.Add(rdoNoSubscription);

        // Set relative positions inside each panel
        rdoActive.Location = new Point(6, 4);
        rdoInactive.Location = new Point(6, 23);

        rdoMember.Location = new Point(6, 4);
        rdoNonMember.Location = new Point(6, 23);

        rdoSubscribed.Location = new Point(6, 4);
        rdoNoSubscription.Location = new Point(6, 23);

        // Optional: better tab behavior
        rdoActive.TabStop = true; rdoInactive.TabStop = false;
        rdoMember.TabStop = true; rdoNonMember.TabStop = false;
        rdoSubscribed.TabStop = true; rdoNoSubscription.TabStop = false;
    }

    private static void WrapAsGroup(RadioButton rb1, RadioButton rb2)
    {
        // already grouped? (avoid double grouping)
        if (rb1.Parent == rb2.Parent && rb1.Parent is Panel && rb1.Parent != null)
            return;

        var parent = rb1.Parent ?? throw new InvalidOperationException("RadioButton has no parent.");
        var p1 = rb1.Location;
        var p2 = rb2.Location;

        int left = Math.Min(p1.X, p2.X);
        int top = Math.Min(p1.Y, p2.Y);
        int right = Math.Max(p1.X + rb1.Width, p2.X + rb2.Width);
        int bottom = Math.Max(p1.Y + rb1.Height, p2.Y + rb2.Height);

        var groupPanel = new Panel
        {
            BackColor = Color.Transparent,
            Location = new Point(left, top),
            Size = new Size(right - left + 5, bottom - top + 5),
            TabStop = false
        };

        // add panel to same parent
        parent.Controls.Add(groupPanel);
        groupPanel.BringToFront();

        // move radios into panel, keep relative positions
        rb1.Parent = groupPanel;
        rb2.Parent = groupPanel;

        rb1.Location = new Point(p1.X - left, p1.Y - top);
        rb2.Location = new Point(p2.X - left, p2.Y - top);

        // good UX: only first radio gets tabstop
        rb1.TabStop = true;
        rb2.TabStop = false;
    }
    // ✅ do NOT throw (toolbar will crash)
    public void OnEdit()
    {
        _editMode = !_editMode;
        SetInputsEnabled(_editMode);
    }
    private void SetInputsEnabled(bool enabled)
    {
        // Basic inputs
        txtCode.ReadOnly = !enabled;
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
    // ========== TOP TOOLBAR ==========
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

        // Policy Agreement popup from appsettings.json
        if (!ConfirmPoliciesBeforeSave(groupCode, maxBorrow))
        {
            MessageBox.Show("You must agree to continue.", "Policy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var req = BuildRequest(groupCode, maxBorrow);
        // membership fee comes from selected group item
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
                        return; // user cancelled payment => stop save

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

        var (ok, message) = await _service.RegisterAsync(req);

        MessageBox.Show(message, ok ? "Success" : "Error",
            MessageBoxButtons.OK,
            ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

        if (ok) await OnRefreshAsync();
    }

    // ========== LOOKUPS ==========
    private async Task LoadLookupsAsync()
    {
        // Groups
        var groups = await _service.GetGroupsAsync();
        cmbGroup.DisplayMember = "Name";
        cmbGroup.ValueMember = "Code";
        cmbGroup.DataSource = groups;

        // Subscriptions
        var subs = await _service.GetSubscriptionsAsync();
        cmbSubscription.DisplayMember = "Desc";
        cmbSubscription.ValueMember = "Id";
        cmbSubscription.DataSource = subs;

        // Locations (combo + ALL)
        var locs = await _service.GetLocationsAsync();
        cmbLocation.Items.Clear();
        cmbLocation.Items.Add(new ComboItem("ALL", "-- ALL LOCATIONS --"));

        foreach (var l in locs)
            cmbLocation.Items.Add(new ComboItem(l.Code, l.Desc));

        cmbLocation.SelectedIndex = 0;

        // Gender
        cmbGender.Items.Clear();
        cmbGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
        if (cmbGender.Items.Count > 0) cmbGender.SelectedIndex = 0;
    }

    // ========== UI RULES ==========
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

            // enforce inactive
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
            // show all from combo list (skip ALL option)
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

    // ========== POLICY POPUP ==========
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

    // ========== VALIDATION + REQUEST ==========
    private bool ValidateForm(out string message)
    {
        if (string.IsNullOrWhiteSpace(txtCode.Text)) { message = "User Code is required."; return false; }
        if (string.IsNullOrWhiteSpace(txtName.Text)) { message = "Full Name is required."; return false; }
        if (string.IsNullOrWhiteSpace(txtMobile.Text)) { message = "Mobile is required."; return false; }

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

    private void ClearForm()
    {
        txtCode.Clear();
        txtName.Clear();
        txtMobile.Clear();
        txtNic.Clear();
        txtEmail.Clear();
        txtAddress.Clear();
        txtPassword.Clear();
        txtConfirmPassword.Clear();
        listAssignelocation.ClearSelected();

        rdoActive.Checked = true;
        rdoNonMember.Checked = true;
        rdoNoSubscription.Checked = true;

        if (cmbGroup.Items.Count > 0) cmbGroup.SelectedIndex = 0;
        if (cmbSubscription.Items.Count > 0) cmbSubscription.SelectedIndex = 0;
        if (cmbLocation.Items.Count > 0) cmbLocation.SelectedIndex = 0;

        dtpDob.Value = DateTime.Now.Date;
    }

    public Task OnProcessAsync()
    {
        throw new NotImplementedException();
    }

    private sealed class ComboItem
    {
        public string Code { get; }
        public string Text { get; }
        public ComboItem(string code, string text) { Code = code; Text = text; }
        public override string ToString() => Text;
    }
}
