using GCloud.Models.Domain;
using System.Data.Entity;

namespace GCloud.Repository.Impl
{
    public class BillRepository : AbstractRepository<Bill>, IBillRepository
    {
        public BillRepository(DbContext context) : base(context)
        {
        }
    }
}