using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.Win.Pages
{
    public partial class UCGroupMenus : UserControl
    {
        private readonly GroupMenuService _service;
        private readonly UserGroupRepository _groups;

        private bool _loading; // avoid events firing while loading

        public UCGroupMenus(GroupMenuService service, UserGroupRepository groups)
        {
            InitializeComponent();

            _service = service ?? throw new ArgumentNullException(nameof(service));
            _groups = groups ?? throw new ArgumentNullException(nameof(groups));

            Dock = DockStyle.Fill;

            dgvMenus.AllowUserToAddRows = false;
            dgvMenus.AllowUserToDeleteRows = false;
            dgvMenus.AutoGenerateColumns = false;
            dgvMenus.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMenus.MultiSelect = false;

            // If designer didn’t add columns, build columns here (safe)
            BuildGridColumnsIfMissing();

            Load += async (_, __) => await InitAsync();
            btnRefresh.Click += async (_, __) => await LoadMenusAsync();
            btnSave.Click += async (_, __) => await SaveAsync();

            cmbGroup.SelectedIndexChanged += async (_, __) =>
            {
                if (_loading) return;
                await LoadMenusAsync();
            };

            chkSelectAll.CheckedChanged += (_, __) =>
            {
                if (_loading) return;
                ApplySelectAll();
            };
        }

        // ✅ Use current location from session (since your U_MENUGROUPS supports GP_LOCS)
        private string CurrentLocCode =>
            AppSession.Current?.LocationCode ?? "ALL";

        private string CurrentGroupCode =>
            cmbGroup.SelectedValue?.ToString() ?? "";

        private async Task InitAsync()
        {
            _loading = true;
            try
            {
                // optional seed (creates missing rows as status=0)
                await _service.EnsureAsync();

                // load groups
                var gs = await _groups.GetActiveGroupsAsync();
                cmbGroup.DisplayMember = "Name";
                cmbGroup.ValueMember = "Code";
                cmbGroup.DataSource = gs.Select(x => new { x.Code, x.Name }).ToList();

                if (cmbGroup.Items.Count > 0)
                    cmbGroup.SelectedIndex = 0;
            }
            finally
            {
                _loading = false;
            }

            await LoadMenusAsync();
        }

        private async Task LoadMenusAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentGroupCode)) return;

            _loading = true;
            try
            {
                var loc = CurrentLocCode;

                // ✅ NEW signature requires locCode
                var list = await _service.GetMenusForGroupAsync(CurrentGroupCode, loc);

                // ensure it's a List<> so Count/All etc work safely
                var bound = list?.ToList() ?? new List<GroupMenuRowDto>();

                dgvMenus.DataSource = null;
                dgvMenus.DataSource = bound;

                chkSelectAll.Checked = bound.Count > 0 && bound.All(x => x.Assigned);
            }
            finally
            {
                _loading = false;
            }
        }

        private void ApplySelectAll()
        {
            if (dgvMenus.DataSource is not List<GroupMenuRowDto> list) return;

            var newValue = chkSelectAll.Checked;

            for (int i = 0; i < list.Count; i++)
                list[i] = list[i] with { Assigned = newValue };

            dgvMenus.DataSource = null;
            dgvMenus.DataSource = list;
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentGroupCode)) return;

            if (dgvMenus.DataSource is not List<GroupMenuRowDto> list) return;

            var loc = CurrentLocCode;

            // ✅ Build updates (Locs optional - we keep null and repository will use locCode)
            var updates = list.Select(x =>
                new GroupMenuUpdateDto(
                    MenuCode: x.MenuCode,
                    Assigned: x.Assigned,
                    Locs: null
                )).ToList();

            // ✅ NEW signature requires locCode + updates
            await _service.SaveAsync(CurrentGroupCode, loc, updates);

            MessageBox.Show("Menus updated successfully.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await LoadMenusAsync();
        }

        // ---------------- Grid Columns ----------------

        private void BuildGridColumnsIfMissing()
        {
            if (dgvMenus.Columns.Count > 0) return;

            dgvMenus.Columns.Clear();

            // MenuCode (hidden)
            dgvMenus.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GroupMenuRowDto.MenuCode),
                HeaderText = "Code",
                Visible = false
            });

            // MenuDesc
            dgvMenus.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GroupMenuRowDto.MenuDesc),
                HeaderText = "Menu",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            // Parent
            dgvMenus.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GroupMenuRowDto.ParentCode),
                HeaderText = "Parent",
                Width = 120,
                ReadOnly = true
            });

            // Child Order
            dgvMenus.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GroupMenuRowDto.ChildOrder),
                HeaderText = "Order",
                Width = 60,
                ReadOnly = true
            });

            // RuleLoc (ALL / loc / null)
            dgvMenus.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GroupMenuRowDto.RuleLoc),
                HeaderText = "Loc Rule",
                Width = 80,
                ReadOnly = true
            });

            // Assigned checkbox
            dgvMenus.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(GroupMenuRowDto.Assigned),
                HeaderText = "Allowed",
                Width = 70
            });
        }
    }
}
