using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Repository.Impl
{
    public class TurnoverJournalRepository : AbstractRepository<TurnoverJournal>, ITurnoverJournalRepository
    {
        public TurnoverJournalRepository(DbContext context) : base(context)
        {
        }
    }
}