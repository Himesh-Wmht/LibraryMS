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
    public sealed class SubscriptionRepository
    {
        private readonly SqlDb _db;
        public SubscriptionRepository(SqlDb db) => _db = db;

        public async Task<List<SubscriptionDto>> GetActiveSubscriptionsAsync()
        {
            const string sql = @"
                SELECT SUB_ID, SUB_DESC, SUB_DAYS
                FROM M_SUBSCRIPTION
                WHERE SUB_STATUS = 1
                ORDER BY SUB_DESC;";

            var list = new List<SubscriptionDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
                list.Add(new SubscriptionDto(r.GetString(0), r.GetString(1), r.GetInt32(2)));

            return list;
        }
    }
}