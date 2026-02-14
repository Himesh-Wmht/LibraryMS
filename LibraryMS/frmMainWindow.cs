using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome.Sharp;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.Win.Interfaces;

namespace LibraryMS.Win
{
    public partial class frmMainWindow : Form
    {
        private readonly MenuService _menuService;

        // Right side host panel (inside splitContainer1.Panel2)
       // private readonly Panel panelPageHost;

        // Top actions (we keep references for enabling/disabling if needed)
        private IconButton btnRefresh = null!;
        private IconButton btnEdit = null!;
        private IconButton btnSave = null!;
        private IconButton btnProcess = null!;
        private IconButton btnLogout = null!;

        public frmMainWindow(MenuService menuService)
        {
            InitializeComponent();

            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));

            // ---- SplitContainer standard layout ----
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Orientation = Orientation.Vertical;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Panel1MinSize = 200;
            splitContainer1.SplitterDistance = 250;

            // Left menu
            tvMenus.Dock = DockStyle.Fill;
            tvMenus.AfterSelect += TvMenus_AfterSelect;

            // Right host panel (Panel2)
            panelPageHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.PapayaWhip,
                Name = "panelPageHost"
            };
            splitContainer1.Panel2.Controls.Clear();
            splitContainer1.Panel2.Controls.Add(panelPageHost);

            // Form settings
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;

            // Top bar
            BuildTopBar();

            // Single Load handler (clean)
            Load += async (_, __) =>
            {
                ApplySessionToTopBar();
                await LoadMenusAsync();
            };
        }

        // ---------------- TOP BAR ----------------

        private void BuildTopBar()
        {
            // top panel theme
            panelTop.BackColor = Color.FromArgb(33, 57, 70);   // dark
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            lblTitle.Text = "Library Management System";

            // user label styling
            lblTopUser.ForeColor = Color.White;
            lblTopUser.AutoSize = true;
            lblTopUser.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            lblTopUser.Padding = new Padding(8, 10, 8, 0);

            // location combo styling
            cmbTopLocation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTopLocation.Width = 170;
            cmbTopLocation.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Clear and add right-side controls
            flpTopRight.Controls.Clear();
            flpTopRight.Controls.Add(cmbTopLocation);
            flpTopRight.Controls.Add(lblTopUser);

            // Add icons with text under each icon
            flpTopRight.Controls.Add(CreateTopIconWithText(IconChar.RotateRight, "Refresh", out btnRefresh));
            flpTopRight.Controls.Add(CreateTopIconWithText(IconChar.PenToSquare, "Edit", out btnEdit));
            flpTopRight.Controls.Add(CreateTopIconWithText(IconChar.FloppyDisk, "Save", out btnSave));
            flpTopRight.Controls.Add(CreateTopIconWithText(IconChar.Play, "Process", out btnProcess));
            flpTopRight.Controls.Add(CreateTopIconWithText(IconChar.RightFromBracket, "Logout", out btnLogout));

            // wire actions
            btnRefresh.Click += async (_, __) => await RefreshCurrentAsync();
            btnEdit.Click += (_, __) => InvokePageAction(p => p.OnEdit());
            btnSave.Click += async (_, __) => await InvokePageActionAsync(p => p.OnSaveAsync());
            btnProcess.Click += async (_, __) => await InvokePageActionAsync(p => p.OnProcessAsync());
            btnLogout.Click += (_, __) => LogoutToLogin();
        }

        // Panel contains IconButton (top) + Label (bottom)
        private Panel CreateTopIconWithText(IconChar icon, string text, out IconButton iconButton)
        {
            iconButton = new IconButton
            {
                IconChar = icon,
                IconColor = Color.White,
                IconSize = 18,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Size = new Size(44, 24),
                Dock = DockStyle.Top,
                Text = ""
            };
            iconButton.FlatAppearance.BorderSize = 0;

            var lbl = new Label
            {
                Text = text,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular)
            };

            var host = new Panel
            {
                Width = 60,
                Height = 44,
                Margin = new Padding(6, 0, 0, 0),
                BackColor = Color.Transparent
            };

            host.Controls.Add(lbl);
            host.Controls.Add(iconButton);

            // hover effect
            void enter(object? s, EventArgs e) => host.BackColor = Color.FromArgb(45, 75, 92);
            void leave(object? s, EventArgs e) => host.BackColor = Color.Transparent;

            host.MouseEnter += enter; host.MouseLeave += leave;
            iconButton.MouseEnter += enter; iconButton.MouseLeave += leave;
            lbl.MouseEnter += enter; lbl.MouseLeave += leave;

            // tooltip
            var tip = new ToolTip();
            tip.SetToolTip(iconButton, text);

            return host;
        }

        private void ApplySessionToTopBar()
        {
            var s = AppSession.Current;
            if (s == null) return;

            lblTopUser.Text = string.IsNullOrWhiteSpace(s.UserName) ? s.UserCode : s.UserName;

            cmbTopLocation.Items.Clear();
            cmbTopLocation.Items.Add(s.LocationDesc);
            cmbTopLocation.SelectedIndex = 0;

            // For now location is fixed after login
            cmbTopLocation.Enabled = false;
        }

        // ---------------- MENUS ----------------

        private async Task LoadMenusAsync()
        {
            var session = AppSession.Current;
            if (session == null)
            {
                MessageBox.Show("Session not found. Please login again.");
                DialogResult = DialogResult.Retry; // go back to login loop
                Close();
                return;
            }

            var menus = await _menuService.GetAllowedMenusAsync(session.GroupCode, session.LocationCode);
            BuildMenuTree(menus);

            // Show welcome page
            ShowPage(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Text = $"Welcome {session.UserCode} - {session.LocationDesc}"
            });
        }

        private void BuildMenuTree(List<MenuNode> menus)
        {
            tvMenus.Nodes.Clear();

            var byParent = menus
                .GroupBy(m => m.Parent ?? "")
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            if (!byParent.TryGetValue("", out var roots))
                roots = new();

            foreach (var root in roots)
            {
                // store whole MenuNode (better than only Code)
                var node = new TreeNode(root.Desc) { Tag = root };
                AddChildren(node, root.Code, byParent);
                tvMenus.Nodes.Add(node);
            }

            tvMenus.ExpandAll();
        }

        private void AddChildren(TreeNode parentNode, string parentCode,
            Dictionary<string, List<MenuNode>> byParent)
        {
            if (!byParent.TryGetValue(parentCode, out var children)) return;

            foreach (var child in children)
            {
                var node = new TreeNode(child.Desc) { Tag = child };
                AddChildren(node, child.Code, byParent);
                parentNode.Nodes.Add(node);
            }
        }

        private void TvMenus_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count > 0) return; // ignore parent nodes

            if (e.Node.Tag is not MenuNode menu) return;

            OpenMenu(menu);
        }

        private void OpenMenu(MenuNode menu)
        {
            // For now: placeholder page
            ShowPage(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Text = $"Selected Menu: {menu.Code} - {menu.Desc}"
            });
        }

        private void ShowPage(Control page)
        {
            panelPageHost.SuspendLayout();
            panelPageHost.Controls.Clear();

            page.Dock = DockStyle.Fill;
            panelPageHost.Controls.Add(page);

            panelPageHost.ResumeLayout();
        }

        // ---------------- TOOLBAR ACTIONS ----------------

        private Control? CurrentPage =>
            panelPageHost.Controls.Count > 0 ? panelPageHost.Controls[0] : null;

        private void InvokePageAction(Action<IPageActions> action)
        {
            if (CurrentPage is IPageActions page)
                action(page);
        }

        private async Task InvokePageActionAsync(Func<IPageActions, Task> action)
        {
            if (CurrentPage is IPageActions page)
                await action(page);
        }

        private async Task RefreshCurrentAsync()
        {
            await LoadMenusAsync();
        }

        // ---------------- LOGOUT ----------------

        private void LogoutToLogin()
        {
            var ans = MessageBox.Show(
                "Do you want to logout?",
                "Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (ans != DialogResult.Yes) return;

            AppSession.Current = null;

            // Go back to login (Program.cs loop must handle DialogResult.Retry)
            DialogResult = DialogResult.Retry;
            Close();
        }
    }
}
