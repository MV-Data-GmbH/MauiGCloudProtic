using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCloud.Models.Domain;

namespace GCloud.Service
{
    public interface ITurnoverJournalService : IAbstractService<TurnoverJournal>
    {
        void Add(Guid storeId, string userId, decimal increaseTurnover);
    }
}
