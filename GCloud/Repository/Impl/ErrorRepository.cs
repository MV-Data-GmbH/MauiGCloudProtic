using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Repository.Impl
{
    public class ErrorRepository : AbstractRepository<Error>, IErrorRepository
    {
        public ErrorRepository(DbContext context) : base(context)
        {
        }
    }
}