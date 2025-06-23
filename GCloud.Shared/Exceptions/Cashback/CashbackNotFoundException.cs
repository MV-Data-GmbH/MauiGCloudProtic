
using GCloud.Shared.Exceptions.User;

namespace GCloud.Shared.Exceptions.Cashback
{
    public class CashbackNotFoundException : BaseCashbackException
    {
        public CashbackNotFoundException(Guid? cashbackId) : base(ExceptionStatusCode.CashbackNotFound, $"Cashback Eintrack konnte nicht gefunden werden.", cashbackId)
        {
        }
    }
}
