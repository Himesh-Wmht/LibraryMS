using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.DAL.Core
{

    public sealed class SqlExecutor
    {
        private readonly SqlDb _db;

        public SqlExecutor(SqlDb db) => _db = db;

        public async Task<List<T>> QueryAsync<T>(
            string sql,
            Func<SqlDataReader, T> map,
            Action<SqlCommand>? param = null,
            CommandType commandType = CommandType.Text)
        {
            try
            {
                var result = new List<T>();

                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con) { CommandType = commandType };
                param?.Invoke(cmd);

                await con.OpenAsync();
                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    result.Add(map(r));

                return result;
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("QueryAsync", ex);
            }
        }

        public async Task<T?> QuerySingleAsync<T>(
            string sql,
            Func<SqlDataReader, T> map,
            Action<SqlCommand>? param = null,
            CommandType commandType = CommandType.Text)
            where T : class
        {
            try
            {
                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con) { CommandType = commandType };
                param?.Invoke(cmd);

                await con.OpenAsync();
                await using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                    return map(r);

                return null;
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("QuerySingleAsync", ex);
            }
        }

        public async Task<int> ExecuteAsync(
            string sql,
            Action<SqlCommand>? param = null,
            CommandType commandType = CommandType.Text)
        {
            try
            {
                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con) { CommandType = commandType };
                param?.Invoke(cmd);

                await con.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("ExecuteAsync", ex);
            }
        }
    }
}
