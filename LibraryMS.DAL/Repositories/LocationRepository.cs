using System.Data;
using System.Data.SqlClient;
using LibraryMS.DAL.Core;

namespace LibraryMS.DAL.Repositories
{
    public class LocationRepository
    {
        private readonly SqlDb _db;
        public LocationRepository(SqlDb db) => _db = db;

        public async Task<List<(string Code, string Desc)>> GetActiveLocationsForUserAsync(string userCode)
        {
            const string sql = @"
                                SELECT l.L_CODE, l.L_DESC
                                FROM M_TBLUSERLOCATION ul
                                INNER JOIN M_LOCATION l ON l.L_CODE = ul.UL_USERLOC
                                WHERE ul.UL_USERCODE = @UserCode
                                  AND ul.UL_ACTIVE = 1
                                  AND l.L_ACTIVE = 1
                                ORDER BY l.L_DESC;";
            try
            {
                var list = new List<(string Code, string Desc)>();

                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@UserCode", SqlDbType.VarChar, 20).Value = userCode;

                await con.OpenAsync();
                await using var r = await cmd.ExecuteReaderAsync();

                while (await r.ReadAsync())
                    list.Add((r.GetString(0), r.GetString(1)));

                return list;
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("GetActiveLocationsForUserAsync", ex);
            }
        }
        public async Task<bool> UserHasLocationAsync(string userCode, string locCode)
        {
            const string sql = @"
                                SELECT 1
                                FROM M_TBLUSERLOCATION ul
                                INNER JOIN M_LOCATION l ON l.L_CODE = ul.UL_USERLOC
                                WHERE ul.UL_USERCODE = @UserCode
                                  AND ul.UL_USERLOC = @LocCode
                                  AND ul.UL_ACTIVE = 1
                                  AND l.L_ACTIVE = 1;";
            try
            {
                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@UserCode", SqlDbType.VarChar, 20).Value = userCode;
                cmd.Parameters.Add("@LocCode", SqlDbType.VarChar, 20).Value = locCode;

                await con.OpenAsync();
                var x = await cmd.ExecuteScalarAsync();
                return x != null;
            } catch (Exception ex) 
            { 
                throw DbExceptionHelper.Wrap("UserHasLocationAsync", ex); 
            }
          
        }
    }
}

