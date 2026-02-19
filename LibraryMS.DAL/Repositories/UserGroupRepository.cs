using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public record GroupRow(string UG_CODE, string UG_NAME, int UG_ACTIVE, string UG_PERMISSION);
    public sealed class UserGroupRepository
    {
        private readonly SqlDb _db;
        public UserGroupRepository(SqlDb db) => _db = db;

        public async Task<List<UserGroupDto>> GetActiveGroupsAsync()
        {
            const string sql = @"
                                SELECT UG_CODE, UG_NAME,ISNULL(UG_MEMBERSHIPAMT,0) AS MembershipFee
                                FROM M_TBLUSERGROUPS
                                WHERE UG_ACTIVE = 1
                                ORDER BY UG_NAME;";

            var list = new List<UserGroupDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new UserGroupDto(
                    Code: r.GetString(0),
                    Name: r.GetString(1),
                    MembershipFee: r.IsDBNull(2) ? 0m : r.GetDecimal(2)
                ));
            }
            return list;
        }
    }
}