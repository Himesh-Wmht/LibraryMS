using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class GroupMenuRepository
    {
        private readonly SqlDb _db;
        public GroupMenuRepository(SqlDb db) => _db = db;

        // Seed defaults (optional): create missing ALL rows with status=0
        public async Task EnsureDefaultsAsync()
        {
            const string sqlSeedMissing = @"
                        INSERT INTO dbo.U_MENUGROUPS (GP_ID, GP_MENUID, GP_STATUS, GP_PERMISSION, GP_LOCS, GP_DATE)
                        SELECT g.UG_CODE, m.M_CODE, 0, NULL, 'ALL', SYSDATETIME()
                        FROM dbo.M_TBLUSERGROUPS g
                        CROSS JOIN dbo.M_TBLMENUS m
                        WHERE ISNULL(g.UG_ACTIVE,0) = 1
                          AND NOT EXISTS (
                                SELECT 1
                                FROM dbo.U_MENUGROUPS x
                                WHERE x.GP_ID = g.UG_CODE
                                  AND x.GP_MENUID = m.M_CODE
                                  AND ISNULL(NULLIF(x.GP_LOCS,''),'ALL') = 'ALL'
                          );";

            const string sqlForceUserPrivileges = @"
                                ;WITH Required (GP_ID, GP_MENUID) AS (
                                    SELECT v.GP_ID, v.GP_MENUID
                                    FROM (VALUES ('ADMIN','M00008'),('SADM','M00008')) v(GP_ID, GP_MENUID)
                                )
                                MERGE dbo.U_MENUGROUPS AS tgt
                                USING Required AS src
                                ON  tgt.GP_ID = src.GP_ID
                                AND tgt.GP_MENUID = src.GP_MENUID
                                AND ISNULL(NULLIF(tgt.GP_LOCS,''),'ALL') = 'ALL'
                                WHEN MATCHED THEN
                                    UPDATE SET tgt.GP_STATUS = 1,
                                               tgt.GP_DATE   = SYSDATETIME()
                                WHEN NOT MATCHED THEN
                                    INSERT (GP_ID, GP_MENUID, GP_STATUS, GP_PERMISSION, GP_LOCS, GP_DATE)
                                    VALUES (src.GP_ID, src.GP_MENUID, 1, NULL, 'ALL', SYSDATETIME());";

            try
            {
                await using var con = _db.CreateConnection();
                await con.OpenAsync();

                await using var tx = (SqlTransaction)await con.BeginTransactionAsync();

                // 1) seed missing defaults
                await using (var cmd = new SqlCommand(sqlSeedMissing, (SqlConnection)con, tx))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2) force grant required menus
                await using (var cmd = new SqlCommand(sqlForceUserPrivileges, (SqlConnection)con, tx))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("GroupMenuRepository.EnsureDefaultsAsync", ex);
            }
        }


        public async Task<List<GroupMenuRowDto>> GetMenusForGroupAsync(string groupCode, string locCode)
        {
            const string sql = @"
                        SELECT
                            m.M_CODE,
                            m.M_DESC,
                            NULLIF(m.M_PARENT,'') AS M_PARENT,
                            ISNULL(m.M_CHILD,0) AS M_CHILD,

                            CAST(
                                CASE WHEN EXISTS (
                                    SELECT 1
                                    FROM dbo.U_MENUGROUPS g
                                    WHERE g.GP_ID = @G
                                      AND g.GP_MENUID = m.M_CODE
                                      AND ISNULL(g.GP_STATUS,0) = 1
                                      AND (
                                            ISNULL(NULLIF(g.GP_LOCS,''),'ALL') = 'ALL'
                                            OR ',' + REPLACE(ISNULL(g.GP_LOCS,''),' ','') + ',' LIKE '%,' + @L + ',%'
                                            OR ISNULL(NULLIF(g.GP_LOCS,''),'ALL') = @L
                                          )
                                ) THEN 1 ELSE 0 END
                            AS bit) AS Assigned,

                            CASE 
                                WHEN EXISTS (
                                    SELECT 1 FROM dbo.U_MENUGROUPS g
                                    WHERE g.GP_ID = @G AND g.GP_MENUID = m.M_CODE
                                      AND ISNULL(g.GP_STATUS,0)=1
                                      AND ISNULL(NULLIF(g.GP_LOCS,''),'ALL')='ALL'
                                ) THEN 'ALL'
                                WHEN EXISTS (
                                    SELECT 1 FROM dbo.U_MENUGROUPS g
                                    WHERE g.GP_ID = @G AND g.GP_MENUID = m.M_CODE
                                      AND ISNULL(g.GP_STATUS,0)=1
                                      AND (
                                           ISNULL(NULLIF(g.GP_LOCS,''),'ALL')=@L
                                           OR ',' + REPLACE(ISNULL(g.GP_LOCS,''),' ','') + ',' LIKE '%,' + @L + ',%'
                                      )
                                ) THEN @L
                                ELSE NULL
                            END AS RuleLoc
                        FROM dbo.M_TBLMENUS m
                        ORDER BY ISNULL(NULLIF(m.M_PARENT,''),''), m.M_CODE;";

            try
            {
                var list = new List<GroupMenuRowDto>();

                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@G", SqlDbType.NVarChar, 50).Value = groupCode;
                cmd.Parameters.Add("@L", SqlDbType.NVarChar, 20).Value = locCode;

                await con.OpenAsync();
                await using var r = await cmd.ExecuteReaderAsync();

                while (await r.ReadAsync())
                {
                    list.Add(new GroupMenuRowDto(
                        MenuCode: r.GetString(0),
                        MenuDesc: r.GetString(1),
                        ParentCode: r.IsDBNull(2) ? null : r.GetString(2),
                        ChildOrder: r.IsDBNull(3) ? 0 : Convert.ToInt32(r.GetValue(3)),
                        Assigned: r.GetBoolean(4),
                        RuleLoc: r.IsDBNull(5) ? null : r.GetString(5)  // ✅ correct index = 5
                    ));
                }

                return list;
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("GroupMenuRepository.GetMenusForGroupAsync", ex);
            }
        }

        public async Task SaveGroupMenusAsync(string groupCode, string locCode, List<GroupMenuUpdateDto> updates)
        {
            const string sqlUpsert = @"
                                IF EXISTS (
                                    SELECT 1 FROM dbo.U_MENUGROUPS
                                    WHERE GP_ID=@G AND GP_MENUID=@M AND ISNULL(NULLIF(GP_LOCS,''),'ALL')=@L
                                )
                                BEGIN
                                    UPDATE dbo.U_MENUGROUPS
                                    SET GP_STATUS=@S,
                                        GP_DATE=SYSDATETIME()
                                    WHERE GP_ID=@G AND GP_MENUID=@M AND ISNULL(NULLIF(GP_LOCS,''),'ALL')=@L;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO dbo.U_MENUGROUPS (GP_ID, GP_MENUID, GP_STATUS, GP_PERMISSION, GP_LOCS, GP_DATE)
                                    VALUES (@G, @M, @S, NULL, @L, SYSDATETIME());
                                END;";

            try
            {
                await using var con = _db.CreateConnection();
                await con.OpenAsync();
                await using var tx = (SqlTransaction)await con.BeginTransactionAsync();

                foreach (var u in updates)
                {
                    var saveLoc = string.IsNullOrWhiteSpace(u.Locs) ? locCode : u.Locs;

                    await using var cmd = new SqlCommand(sqlUpsert, (SqlConnection)con, tx);
                    cmd.Parameters.Add("@G", SqlDbType.NVarChar, 50).Value = groupCode;
                    cmd.Parameters.Add("@M", SqlDbType.NVarChar, 100).Value = u.MenuCode;
                    cmd.Parameters.Add("@L", SqlDbType.NVarChar, 20).Value = saveLoc!;
                    cmd.Parameters.Add("@S", SqlDbType.Bit).Value = u.Assigned;

                    await cmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("GroupMenuRepository.SaveGroupMenusAsync", ex);
            }
        }
    }
}
