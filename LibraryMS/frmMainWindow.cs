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
        private readonly BookReservationService _reservationsService;
        private readonly UserLookupService _userLookupService;
        private readonly BookTransferService _booktransfer;
        private readonly LocationLookupService _locationLookupService;
        private readonly BookBorrowService _bookBorrowService;
        private readonly BookReturnService _bookReturnService;
        private readonly FineCollectionService _fineCollectionService;
        private readonly ReportService _reportService;
        private readonly SubscriptionRenewalApprovalService _subscriptionRenewalService;

        // Top actions (we keep references for enabling/disabling if needed)
        private IconButton btnRefresh = null!;
        private IconButton btnEdit = null!;
        private IconButton btnSave = null!;
        private IconButton btnProcess = null!;
        private IconButton btnLogout = null!;

        public frmMainWindow(MenuService menuService, RegistrationService registrationService, ApprovalService approvalService, GroupMenuService groupMenuService, UserGroupRepository groupRepo, BookCatalogService bookCatalogService,BookInventoryService bookInventoryService, BookCategoryService bookCategoryService, PasswordResetService passwordResetService, UserLockService userLockService, BookReservationService reservationsService, UserLookupService userLookupService, BookTransferService bookTransfer, LocationLookupService locationLookupService, BookBorrowService bookBorrowService,
BookReturnService bookReturnService, FineCollectionService fineCollectionService, ReportService reportService, SubscriptionRenewalApprovalService subscriptionRenewalService)
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

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
            _reservationsService = reservationsService ?? throw new ArgumentNullException(nameof(reservationsService));
            _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
            _booktransfer = bookTransfer ?? throw new ArgumentNullException(nameof(bookTransfer));
            _locationLookupService = locationLookupService ?? throw new ArgumentNullException(nameof(locationLookupService));
            _bookBorrowService = bookBorrowService ?? throw new ArgumentNullException(nameof(bookBorrowService));
            _bookReturnService = bookReturnService ?? throw new ArgumentNullException(nameof(bookReturnService));
            _fineCollectionService = fineCollectionService ?? throw new ArgumentNullException(nameof(fineCollectionService));
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _subscriptionRenewalService = subscriptionRenewalService ?? throw new ArgumentNullException(nameof(subscriptionRenewalService));

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
            ConfigureSideMenu();

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
            _locationLookupService = locationLookupService;
        }
        private void ConfigureSideMenu()
        {
            tvMenus.BorderStyle = BorderStyle.None;
            tvMenus.HideSelection = false;
            tvMenus.FullRowSelect = true;

            // remove old tree lines
            tvMenus.ShowLines = false;
            tvMenus.ShowPlusMinus = false;
            tvMenus.ShowRootLines = false;

            // spacing
            tvMenus.ItemHeight = 34;
            tvMenus.Indent = 20;

            // colors
            tvMenus.BackColor = Color.FromArgb(206, 177, 140);
            tvMenus.ForeColor = Color.FromArgb(45, 32, 20);
            tvMenus.Font = new Font("Segoe UI", 10f, FontStyle.Regular);

            // custom draw
            tvMenus.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            tvMenus.DrawNode += TvMenus_DrawNode;
            tvMenus.NodeMouseClick += TvMenus_NodeMouseClick;
        }
        private void TvMenus_DrawNode(object? sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null) return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            bool isSelected = tvMenus.SelectedNode == e.Node;
            bool isParent = e.Node.Nodes.Count > 0;

            Rectangle fullRow = new Rectangle(
                6,
                e.Bounds.Top + 2,
                tvMenus.ClientSize.Width - 12,
                tvMenus.ItemHeight - 4
            );

            using (var bg = new SolidBrush(tvMenus.BackColor))
                g.FillRectangle(bg, fullRow);

            if (isParent)
            {
                // Parent header
                int arrowX = 12 + (e.Node.Level * tvMenus.Indent);
                int arrowY = fullRow.Top + (fullRow.Height / 2) - 4;

                DrawChevron(g, arrowX, arrowY, e.Node.IsExpanded);

                Rectangle textRect = new Rectangle(
                    arrowX + 16,
                    fullRow.Top,
                    fullRow.Width - arrowX - 20,
                    fullRow.Height
                );

                TextRenderer.DrawText(
                    g,
                    e.Node.Text.ToUpper(),
                    new Font("Segoe UI", 10f, FontStyle.Bold),
                    textRect,
                    Color.FromArgb(28, 48, 60),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );
            }
            else
            {
                int textX = 22 + (e.Node.Level * tvMenus.Indent);

                Rectangle childRect = new Rectangle(
                    textX - 6,
                    fullRow.Top,
                    tvMenus.ClientSize.Width - textX - 10,
                    fullRow.Height
                );

                if (isSelected)
                {
                    using var fill = new SolidBrush(Color.FromArgb(241, 221, 196));
                    using var pen = new Pen(Color.FromArgb(190, 155, 120));
                    FillRoundedRectangle(g, fill, childRect, 8);
                    DrawRoundedRectangle(g, pen, childRect, 8);

                    // left highlight bar
                    using var barBrush = new SolidBrush(Color.FromArgb(33, 57, 70));
                    g.FillRectangle(barBrush, childRect.X, childRect.Y, 4, childRect.Height);
                }

                Rectangle textRect = new Rectangle(
                    textX + 6,
                    fullRow.Top,
                    tvMenus.ClientSize.Width - textX - 20,
                    fullRow.Height
                );

                TextRenderer.DrawText(
                    g,
                    e.Node.Text,
                    new Font("Segoe UI", 9.75f, FontStyle.Regular),
                    textRect,
                    Color.FromArgb(40, 30, 20),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );
            }
        }
        private void TvMenus_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            tvMenus.SelectedNode = e.Node;

            if (e.Node.Nodes.Count > 0)
            {
                if (e.Node.IsExpanded)
                    e.Node.Collapse();
                else
                    e.Node.Expand();
            }
        }
        private void DrawChevron(Graphics g, int x, int y, bool expanded)
        {
            Point[] points;

            if (expanded)
            {
                points = new[]
                {
            new Point(x, y),
            new Point(x + 8, y),
            new Point(x + 4, y + 6)
        };
            }
            else
            {
                points = new[]
                {
            new Point(x, y),
            new Point(x, y + 8),
            new Point(x + 6, y + 4)
        };
            }

            using var brush = new SolidBrush(Color.FromArgb(70, 55, 40));
            g.FillPolygon(brush, points);
        }
        private void FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using var path = RoundedRect(rect, radius);
            g.FillPath(brush, path);
        }

        private void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using var path = RoundedRect(rect, radius);
            g.DrawPath(pen, path);
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
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
            if (menu.Code == "M00014")
            {
                ShowPage(new UCBookReservationRequest(_reservationsService, _userLookupService));
                return;
            }
            if (menu.Code == "M00013")
            {
                ShowPage(new UCReservationApprovals(_reservationsService));
                return;
            }
            if (menu.Code == "M00021")
            {
                ShowPage(new UCBookTransferRequest(_booktransfer, _locationLookupService));
                return;
            }
            if (menu.Code == "M00022")
            {
                ShowPage(new UCBookTransferApprovals(_booktransfer));
                return;
            }
            if (menu.Code == "M00023")
            {
                ShowPage(new UCBookBorrow(_bookBorrowService, _reservationsService, _userLookupService));
                return;
            }
            if (menu.Code == "M00024")
            {
                ShowPage(new UCBookReturn(_bookBorrowService, _bookReturnService));
                return;
            }
            if (menu.Code == "M00025")
            {
                ShowPage(new UCFineCollection(_fineCollectionService));
                return;
            }
            if (menu.Code == "M00041")
            {
                ShowPage(new UCReportGrid(
                    "Borrowing / Issued Report",
                    (from, to, top) => _reportService.GetBorrowingIssuedAsync(AppSession.Current!.LocationCode, from, to),
                    useDates: true,
                    useTop: false
                ));
                return;
            }
            if (menu.Code == "M00042")
            {
                ShowPage(new UCReportGrid(
                    "Overdue Items Report",
                    (from, to, top) => _reportService.GetOverdueItemsAsync(AppSession.Current!.LocationCode),
                    useDates: false,
                    useTop: false
                ));
                return;
            }
            if (menu.Code == "M00042")
            {
                ShowPage(new UCReportGrid(
                    "Overdue Items Report",
                    (from, to, top) => _reportService.GetOverdueItemsAsync(AppSession.Current!.LocationCode),
                    useDates: false,
                    useTop: false
                ));
                return;
            }
            if (menu.Code == "M00043")
            {
                ShowPage(new UCReportGrid(
                    "Book Availability Report",
                    (from, to, top) => _reportService.GetBookAvailabilityAsync(AppSession.Current!.LocationCode),
                    useDates: false,
                    useTop: false
                ));
                return;
            }
            if (menu.Code == "M00044")
            {
                ShowPage(new UCReportGrid(
                    "Member Activity Report",
                    (from, to, top) => _reportService.GetMemberActivityAsync(AppSession.Current!.LocationCode, from, to),
                    useDates: true,
                    useTop: false
                ));
                return;
            }
            if (menu.Code == "M00045")
            {
                ShowPage(new UCReportGrid(
                    "Fine Summary Report",
                    (from, to, top) => _reportService.GetFineSummaryAsync(AppSession.Current!.LocationCode, from, to),
                    useDates: true,
                    useTop: false
                ));
                return;
            }
            if (menu.Code == "M00046")
            {
                ShowPage(new UCReportGrid(
                    "Most Borrowed Books Report",
                    (from, to, top) => _reportService.GetMostBorrowedBooksAsync(AppSession.Current!.LocationCode, from, to, top),
                    useDates: true,
                    useTop: true
                ));
                return;
            }
            if (menu.Code == "M00030") // SUBSCRIPTION RENEWAL APPROVALS
            {
                ShowPage(new UCSubscriptionRenewalApprovals(_subscriptionRenewalService));
                return;
            }
            if (menu.Code == "M00047")
            {
                ShowPage(new UCReportGrid(
                    "Book Borrow History",
                    (from, to, top) => _reportService.GetBookBorrowHistoryAsync(AppSession.Current!.LocationCode, from, to),
                    useDates: true,
                    useTop: false
                ));
                return;
            }

            if (menu.Code == "M00048")
            {
                ShowPage(new UCReportGrid(
                    "Book Return History",
                    (from, to, top) => _reportService.GetBookReturnHistoryAsync(AppSession.Current!.LocationCode, from, to),
                    useDates: true,
                    useTop: false
                ));
                return;
            }

            if (menu.Code == "M00049")
            {
                ShowPage(new UCReportGrid(
                    "User Details Report",
                    (from, to, top) => _reportService.GetUserDetailsAsync(),
                    useDates: false,
                    useTop: false
                ));
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
