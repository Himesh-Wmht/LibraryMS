using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;

namespace LibraryMS.DAL.Repositories
{
    public record MenuRow(string Code, string Desc, string? Parent, string? Child);
    public class MenuRepository
    {
        private readonly SqlDb _db;
        public MenuRepository(SqlDb db) => _db = db;
        public async Task<List<MenuRow>> GetMenusByGroupAndLocationAsync(string groupCode, string locCode)
        {
            const string sql = @"
                               ;WITH Allowed AS (
                                    SELECT m.M_CODE, m.M_DESC, m.M_PARENT, m.M_CHILD
                                    FROM U_MENUGROUPS g
                                    INNER JOIN M_TBLMENUS m ON m.M_CODE = g.GP_MENUID
                                    WHERE g.GP_ID = @GroupCode
                                        AND g.GP_STATUS = 1
                                        AND g.GP_LOCS = @LocCode
                                ),
                                MenuWithParents AS (
                                    SELECT M_CODE, M_DESC, M_PARENT,M_CHILD
                                    FROM Allowed

                                    UNION ALL

                                    SELECT p.M_CODE, p.M_DESC, p.M_PARENT, p.M_CHILD
                                    FROM M_TBLMENUS p
                                    INNER JOIN MenuWithParents c ON c.M_PARENT = p.M_CODE
                                )
                                SELECT DISTINCT M_CODE, M_DESC, M_PARENT, M_CHILD
                                FROM MenuWithParents
                                ORDER BY M_PARENT, M_CODE
                                OPTION (MAXRECURSION 100);";

            var list = new List<MenuRow>();

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@GroupCode", SqlDbType.VarChar, 20).Value = groupCode;
            cmd.Parameters.Add("@LocCode", SqlDbType.VarChar, 20).Value = locCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                var code = r.GetString(0);
                var desc = r.GetString(1);
                var parent = r.IsDBNull(2) ? null : r.GetString(2);
                var child = r.IsDBNull(3) ? null : r.GetString(3);
                list.Add(new MenuRow(code, desc, parent, child));
            }

            return list;
        }
    }
}
