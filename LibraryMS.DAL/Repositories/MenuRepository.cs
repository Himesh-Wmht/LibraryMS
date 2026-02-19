using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public record MenuRow(string Code, string Desc, string? Parent, int Child);

    public class MenuRepository
    {
        private readonly SqlDb _db;
        public MenuRepository(SqlDb db) => _db = db;

        public async Task<List<MenuRow>> GetMenusByGroupAndLocationAsync(string groupCode, string locCode)
        {
            const string sql = @"
;WITH Allowed AS (
    SELECT m.M_CODE, m.M_DESC, NULLIF(m.M_PARENT,'') AS M_PARENT, ISNULL(m.M_CHILD,0) AS M_CHILD
    FROM dbo.U_MENUGROUPS g
    INNER JOIN dbo.M_TBLMENUS m ON m.M_CODE = g.GP_MENUID
    WHERE g.GP_ID = @GroupCode
      AND ISNULL(g.GP_STATUS,0) = 1
      AND (
            ISNULL(NULLIF(g.GP_LOCS,''),'ALL') = 'ALL'
            OR ',' + REPLACE(ISNULL(g.GP_LOCS,''),' ','') + ',' LIKE '%,' + @LocCode + ',%'
          )
),
MenuWithParents AS (
    SELECT M_CODE, M_DESC, M_PARENT, M_CHILD
    FROM Allowed

    UNION ALL

    SELECT p.M_CODE, p.M_DESC, NULLIF(p.M_PARENT,''), ISNULL(p.M_CHILD,0)
    FROM dbo.M_TBLMENUS p
    INNER JOIN MenuWithParents c ON c.M_PARENT = p.M_CODE
)
SELECT
    M_CODE,
    MAX(M_DESC) AS M_DESC,
    M_PARENT,
    MAX(M_CHILD) AS M_CHILD
FROM MenuWithParents
GROUP BY M_CODE, M_PARENT
ORDER BY ISNULL(M_PARENT,''), M_CODE
OPTION (MAXRECURSION 100);";

            var list = new List<MenuRow>();

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@GroupCode", SqlDbType.NVarChar, 50).Value = groupCode;
            cmd.Parameters.Add("@LocCode", SqlDbType.NVarChar, 20).Value = locCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                list.Add(new MenuRow(
                    Code: r.GetString(0),
                    Desc: r.GetString(1),
                    Parent: r.IsDBNull(2) ? null : r.GetString(2),
                    Child: r.IsDBNull(3) ? 0 : Convert.ToInt32(r.GetValue(3))
                ));
            }

            return list;
        }

        // Used by Group Menu screen (load all menus)
        public async Task<List<MenuRow>> GetAllMenusAsync()
        {
            const string sql = @"
SELECT M_CODE, M_DESC, NULLIF(M_PARENT,''), ISNULL(M_CHILD,0)
FROM dbo.M_TBLMENUS
ORDER BY ISNULL(NULLIF(M_PARENT,''),''), M_CODE;";

            var list = new List<MenuRow>();

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            await con.OpenAsync();

            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new MenuRow(
                    Code: r.GetString(0),
                    Desc: r.GetString(1),
                    Parent: r.IsDBNull(2) ? null : r.GetString(2),
                    Child: r.IsDBNull(3) ? 0 : Convert.ToInt32(r.GetValue(3))
                ));
            }

            return list;
        }
    }
}
