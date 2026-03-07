using System.Text.RegularExpressions;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Helper;
using LibraryMS.Win.Interfaces;
using static LibraryMS.BLL.Models.UserUpdateModels;

namespace LibraryMS.Win.Pages;

public partial class UCRegistrationHandling : UserControl, IPageActions
{
    private readonly RegistrationService _service;
    // optional edit toggle
    private bool _editMode = true;
    private Panel pnlSearch = null!;
    private TextBox txtSearchUser = null!;
    private Button btnSearchUser = null!;
    private ListBox lstSearchResults = null!;

    private bool _isLoadedExistingUser = false;
    private string? _loadedUserCode = null;
    public UCRegistrationHandling(RegistrationService service)
    {
        InitializeComponent();
        BuildSearchBar();
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
    private void BuildSearchBar()
    {
        pnlSearch = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(10),
            BackColor = Color.FromArgb(255, 246, 220) // light
        };

        var lbl = new Label
        {
            Text = "Search User (Code / Name)",
            AutoSize = true,
            ForeColor = Color.Teal,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            Location = new Point(10, 8)
        };

        txtSearchUser = new TextBox
        {
            Location = new Point(10, 28),
            Width = 260
        };

        btnSearchUser = new Button
        {
            Text = "Search",
            Location = new Point(280, 26),
            Width = 90,
            Height = 26
        };

        lstSearchResults = new ListBox
        {
            Location = new Point(10, 58),
            Width = 415,
            Height = 120,
            Visible = false
        };

        btnSearchUser.Click += async (_, __) => await DoSearchAsync();
        txtSearchUser.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await DoSearchAsync();
            }
        };

        lstSearchResults.DoubleClick += async (_, __) => await SelectSearchResultAsync();

        pnlSearch.Controls.Add(lbl);
        pnlSearch.Controls.Add(txtSearchUser);
        pnlSearch.Controls.Add(btnSearchUser);
        pnlSearch.Controls.Add(lstSearchResults);

        // panelRoot already contains gbUserRegistration (Dock=Fill)
        panelRoot.Controls.Add(pnlSearch);
        panelRoot.Controls.SetChildIndex(pnlSearch, 0); // keep it on top
    }

    private void ShowSearchResults(bool show)
    {
        lstSearchResults.Visible = show;
        pnlSearch.Height = show ? 190 : 60;
    }

    private async Task DoSearchAsync()
    {
        var q = txtSearchUser.Text.Trim();
        if (string.IsNullOrWhiteSpace(q))
        {
            ShowSearchResults(false);
            return;
        }

        var results = await _service.SearchUsersAsync(q);

        lstSearchResults.DataSource = null;
        lstSearchResults.DataSource = results; // UserSearchItem.ToString shows "CODE - NAME"
        ShowSearchResults(results.Count > 0);
    }

    private async Task SelectSearchResultAsync()
    {
        if (lstSearchResults.SelectedItem is not UserSearchItem item)
            return;

        ShowSearchResults(false);
        txtSearchUser.Clear();

        await LoadExistingUserAsync(item.Code);
    }

    private async Task LoadExistingUserAsync(string userCode)
    {
        var u = await _service.GetUserForEditAsync(userCode);
        if (u == null)
        {
            MessageBox.Show("User not found.");
            return;
        }

        // fill UI
        txtCode.Text = u.Code;
        txtName.Text = u.Name;
        txtMobile.Text = u.Mobile;
        txtEmail.Text = u.Email ?? "";
        txtNic.Text = u.Nic ?? "";
        txtAddress.Text = u.Address ?? "";
        dtpDob.Value = (u.Dob ?? DateTime.Now.Date);

        // group
        cmbGroup.SelectedValue = u.GroupCode;

        // status
        rdoActive.Checked = u.Active;
        rdoInactive.Checked = !u.Active;

        // membership
        rdoMember.Checked = u.MemberStatus;
        rdoNonMember.Checked = !u.MemberStatus;

        // subscription
        rdoSubscribed.Checked = u.SubscriptionStatus;
        rdoNoSubscription.Checked = !u.SubscriptionStatus;

        if (!string.IsNullOrWhiteSpace(u.SubscriptionId))
            cmbSubscription.SelectedValue = u.SubscriptionId;

        // gender (if present)
        if (!string.IsNullOrWhiteSpace(u.Gender) && cmbGender.Items.Contains(u.Gender))
            cmbGender.SelectedItem = u.Gender;

        nudMaxBorrow.Value = Math.Max(nudMaxBorrow.Minimum, Math.Min(nudMaxBorrow.Maximum, u.MaxBorrow));

        // location selection (your UI supports ALL or one)
        if (u.AllLocations)
            SelectLocationCombo("ALL");
        else if (!string.IsNullOrWhiteSpace(u.LocationCode))
            SelectLocationCombo(u.LocationCode!);

        UpdateAssignedLocationsList();
        ApplyGroupRulesUi();
        ApplySubscriptionUi();
        UpdateExpireDate();

        // clear password fields (optional update)
        txtPassword.Clear();
        txtConfirmPassword.Clear();

        // mark as existing and lock UI
        _isLoadedExistingUser = true;
        _loadedUserCode = u.Code;

        _editMode = false;
        SetInputsEnabled(false);
        txtCode.ReadOnly = true;

        MessageBox.Show("User loaded. Click EDIT to modify, then SAVE to update.", "Loaded",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SelectLocationCombo(string code)
    {
        for (int i = 0; i < cmbLocation.Items.Count; i++)
        {
            if (cmbLocation.Items[i] is ComboItem ci && ci.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                cmbLocation.SelectedIndex = i;
                return;
            }
        }
        // fallback
        if (cmbLocation.Items.Count > 0) cmbLocation.SelectedIndex = 0;
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
        //txtCode.ReadOnly = !enabled;
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
    private bool ValidateUpdateForm(out string message)
    {
        if (string.IsNullOrWhiteSpace(txtCode.Text)) { message = "User Code is required."; return false; }
        if (string.IsNullOrWhiteSpace(txtName.Text)) { message = "Full Name is required."; return false; }
        if (string.IsNullOrWhiteSpace(txtMobile.Text)) { message = "Mobile is required."; return false; }
        if (cmbGroup.SelectedItem is null) { message = "Select User Group."; return false; }
        if (cmbLocation.SelectedItem is null) { message = "Select Location."; return false; }

        // ✅ MOBILE normalize + validate
        var normalized = NormalizeMobile(txtMobile.Text);
        if (!IsValidSriLankaMobile94(normalized))
        {
            message = "Mobile must be valid. Example: 0771234567 or 94771234567";
            return false;
        }
        txtMobile.Text = normalized;

        // ✅ EMAIL validate (optional)
        if (!IsValidEmail(txtEmail.Text))
        {
            message = "Invalid email address.";
            return false;
        }

        // password optional in update, but if provided must match confirm
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

    private UserUpdateRequest BuildUpdateRequest(string groupCode, int maxBorrow)
    {
        var loc = (ComboItem)cmbLocation.SelectedItem!;
        bool allLocs = loc.Code == "ALL";

        SubscriptionItem? sub = null;
        if (rdoSubscribed.Checked && cmbSubscription.SelectedItem is SubscriptionItem s)
            sub = s;

        return new UserUpdateRequest 
        {
            Code = txtCode.Text.Trim(),                 // key
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

            if (ok2) await OnRefreshAsync(); // clears + resets to insert mode
            return;
        }
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

        var (ok, message) = await _service.RegisterAsync(req, AppSession.Current?.UserCode);

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


        // ✅ MOBILE normalize + validate
        var normalized = NormalizeMobile(txtMobile.Text);
        if (!IsValidSriLankaMobile94(normalized))
        {
            message = "Mobile must be a valid Sri Lanka number. Example: 0771234567 or 94771234567";
            return false;
        }

        // set normalized value back to UI (94xxxxxxxxx)
        txtMobile.Text = normalized;

        // ✅ EMAIL validate (optional field)
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
        listAssignelocation.ClearSelected();

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

    public Task OnProcessAsync()
    {
        throw new NotImplementedException();
    }
    private static string NormalizeMobile(string? raw)
{
    var s = (raw ?? "").Trim();

    // keep digits only
    s = Regex.Replace(s, @"\D", "");

    // cases:
    // 0XXXXXXXXX  -> 94XXXXXXXXX
    // 94XXXXXXXXX -> 94XXXXXXXXX
    // XXXXXXXXX   -> 94XXXXXXXXX  (if 9 digits and starts without 0)
    // XXXXXXXXXX  -> 94XXXXXXXXX  (if 10 digits without 0)
    if (s.StartsWith("0") && s.Length == 10)
        s = "94" + s.Substring(1);
    else if (s.StartsWith("94") && s.Length == 11)
        return s;
    else if (s.Length == 9) // some users type 9 digits without leading 0
        s = "94" + s;
    else if (s.Length == 10 && !s.StartsWith("0")) // typed 10 digits without 0
        s = "94" + s.Substring(1);

    return s;
}

private static bool IsValidSriLankaMobile94(string mobile94)
{
    // Must be 94 + 9 digits => total 11 digits
    // e.g. 94771234567
    return Regex.IsMatch(mobile94, @"^94\d{9}$");
}

private static bool IsValidEmail(string? email)
{
    if (string.IsNullOrWhiteSpace(email)) return true; // optional
    email = email.Trim();

    // Simple, safe regex (good enough for app validation)
    return Regex.IsMatch(email,
        @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$",
        RegexOptions.IgnoreCase);
}
    private sealed class ComboItem
    {
        public string Code { get; }
        public string Text { get; }
        public ComboItem(string code, string text) { Code = code; Text = text; }
        public override string ToString() => Text;
    }
}
