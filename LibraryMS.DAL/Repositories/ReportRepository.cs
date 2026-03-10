using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;


namespace LibraryMS.DAL.Repositories
    {
        public sealed class ReportRepository
        {
            private readonly SqlDb _db;
            public ReportRepository(SqlDb db) => _db = db;

            public async Task<DataTable> ExecuteReportAsync(string procedureName, params SqlParameter[] parameters)
            {
                var dt = new DataTable();

                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(procedureName, con)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 0
                };

                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                await con.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);

                return dt;
            }
        }
    }

