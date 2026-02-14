using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.DAL.Core
{
    public sealed class DbConfig
    {
        public DbConfig(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));

            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
    }
}
