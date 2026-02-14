using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.DAL.Core
{
    public static class DbExceptionHelper
    {
        public static Exception Wrap(string operation, Exception ex)
        {
            // Don't leak raw connection string or sensitive details
            if (ex is SqlException sqlEx)
                return new Exception($"Database error during {operation}. (SQL #{sqlEx.Number})", ex);

            return new Exception($"Unexpected error during {operation}.", ex);
        }
    }
}
