using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;

namespace GCloud.Repository.Impl
{
    public class UserRepository : AbstractRepository<User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context)
        {
        }
    }
}