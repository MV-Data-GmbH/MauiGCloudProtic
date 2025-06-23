using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace GCloud.Repository
{
    public interface IProcedureRepository
    {
        IEnumerable ExecuteProcedure<T>(string procedureName, IList<SqlParameter> parameters) where T : class, new();
    }
}