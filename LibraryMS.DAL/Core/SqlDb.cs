using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.DAL.Core
{
    public sealed class SqlDb
    {
        private readonly DbConfig _config;

        public SqlDb(DbConfig config) => _config = config;

        public SqlConnection CreateConnection() => new SqlConnection(_config.ConnectionString);
    }
}
