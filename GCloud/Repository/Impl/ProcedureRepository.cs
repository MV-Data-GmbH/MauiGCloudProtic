using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Autofac.Core;
using GCloud.Models.Domain;

namespace GCloud.Repository.Impl
{
    public class ProcedureRepository : IProcedureRepository
    {
        private GCloudContext _context;

        public ProcedureRepository(DbContext context)
        {
            _context = (GCloudContext) context;

        }

        public IEnumerable ExecuteProcedure<T>(string procedureName, IList<SqlParameter> parameters) where T : class, new()
        {
            var parameterString = string.Join(",", parameters.Select(x => $"{x.ParameterName}={x.ParameterName}"));
            var result = new List<T>();
            try
            {
                result = _context.Database.SqlQuery<T>($"EXEC {procedureName} {parameterString}", parameters.ToArray()).ToList();
            }
            catch (Exception)
            {
                using (var context = new GCloudContext())
                {
                    result = context.Database.SqlQuery<T>($"EXEC {procedureName} {parameterString}", parameters.ToArray()).ToList();
                }
            }

            return result;
        }
    }
}