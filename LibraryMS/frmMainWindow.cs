using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome.Sharp;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.DAL.Repositories;
using LibraryMS.Win.Interfaces;
using LibraryMS.Win.Pages;

namespace LibraryMS.Win
{
    public partial class frmMainWindow : Form
    {
        private readonly MenuService _menuService;
        private readonly RegistrationService _registrationService;
        private readonly ApprovalService _approvalService;
        private readonly GroupMenuService _groupMenuService;
        private readonly UserGroupRepository _groupRepo;
        private readonly BookCatalogService _bookCatalogService;
        private readonly BookInventoryService _bookInventoryService;
        private readonly BookCategoryService _bookCategoryService;
        private readonly PasswordResetService _passwordResetService;
        private readonly UserLockService _userlock;

        // Top actions (we keep references for enabling/disabling if needed)
        private IconButton btnRefresh = null!;
        private IconButton btnEdit = null!;
        private IconButton btnSave = null!;
        private IconButton btnProcess = null!;
        private IconButton btnLogout = null!;

        public frmMainWindow(MenuService menuService, RegistrationService registrationService, ApprovalService approvalService, GroupMenuService groupMenuService, UserGroupRepository groupRepo, BookCatalogService bookCatalogService,BookInventoryService bookInventoryService, BookCategoryService bookCategoryService, PasswordResetService passwordResetService, UserLockService userLockService)
        {
            InitializeComponent();

            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _registrationService = registrationService;
            _approvalService = approvalService ?? throw new ArgumentNullException(nameof(approvalService));
            _groupMenuService = groupMenuService;
            _groupRepo = groupRepo;
            _bookCatalogService = bookCatalogService ?? throw new ArgumentNullException(nameof(bookCatalogService));
            _bookInventoryService = bookInventoryService ?? throw new ArgumentNullException(nameof(bookInventoryService));
            _bookCategoryService = bookCategoryService ?? throw new ArgumentNullException(nameof(bookCategoryService));
            _passwordResetService = passwordResetService ?? throw new ArgumentNullException(nameof(passwordResetService));
            _userlock = userLockService ?? throw new ArgumentNullException(nameof(userLockService));


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
                await _groupMenuService.EnsureAsync();
                await LoadMenusAsync();
            };
            _groupRepo = groupRepo;
            _bookCategoryService = bookCategoryService;
            _passwordResetService = passwordResetService;
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
            tvMenus.BeginUpdate();
            tvMenus.Nodes.Clear();

            // Build dictionary: parentCode -> children
            var byParent = menus
                .GroupBy(m => m.Parent ?? "")
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.ChildOrder).ThenBy(x => x.Code).ToList(),
                    StringComparer.OrdinalIgnoreCase
                );

            // Roots = parent is null/empty
            byParent.TryGetValue("", out var roots);
            roots ??= new List<MenuNode>();

            foreach (var root in roots.OrderBy(r => r.ChildOrder).ThenBy(r => r.Code))
            {
                var rootNode = new TreeNode(root.Desc) { Tag = root };   // store full object
                AddChildren(rootNode, root.Code, byParent);
                tvMenus.Nodes.Add(rootNode);
            }

            tvMenus.ExpandAll();
            tvMenus.EndUpdate();
        }

        private void AddChildren(TreeNode parentNode,string parentCode, Dictionary<string, List<MenuNode>> byParent)
        {
            if (!byParent.TryGetValue(parentCode, out var children)) return;

            foreach (var child in children)
            {
                var childNode = new TreeNode(child.Desc) { Tag = child };
                AddChildren(childNode, child.Code, byParent);
                parentNode.Nodes.Add(childNode);
            }
        }
        private void TvMenus_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            // If it has children, it is a MAIN menu -> don't open a page
            if (e.Node.Nodes.Count > 0) return;

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
            if (menu.Code == "M00012") // REGISTRATION HANDLING
            {
                ShowPage(new LibraryMS.Win.Pages.UCRegistrationHandling(_registrationService));
                return;
            }
            if (menu.Code == "M00003") // whatever code in DB
            {
                ShowPage(new UCApprovals(_approvalService));
                return;
            }
            if (menu.Code == "M00008") // whatever code in DB
            {
                ShowPage(new UCGroupMenus(_groupMenuService, _groupRepo));
                return;
            }
            if (menu.Code == "M00005")
            {
                ShowPage(new UCBookCatalog(_bookCatalogService));
                return;
            }
            if (menu.Code == "M00006")
            {
                ShowPage(new UCBookInventory(_bookInventoryService));
                return;
            }
            if (menu.Code == "M00016") // Book Categories
            {
                ShowPage(new UCBookCategory(_bookCategoryService));
                return;
            }
            if (menu.Code == "M00019") // PASSWORD RESET REQUEST
            {
                ShowPage(new UCPasswordResetRequest(_passwordResetService));
                return;
            }

            if (menu.Code == "M00017") // PASSWORD RESET APPROVALS
            {
                ShowPage(new UCPasswordResetApprovals(_passwordResetService));
                return;
            }
            if (menu.Code == "M00018")
            {
                ShowPage(new UCUserUnlockApprovals(_userlock));
                return;
            }
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
            // ✅ If a page is open, refresh that page only
            if (CurrentPage is IPageActions page)
            {
                await page.OnRefreshAsync();
                return;
            }

            // fallback if nothing loaded
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
