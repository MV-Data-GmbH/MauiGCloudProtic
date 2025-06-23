

namespace GCloud.Shared.Exceptions.Cashier
{
    public class CashRegisterNotInStoreException : BaseCashierException
    {
        public CashRegisterNotInStoreException(Guid cashRegiserId) : base(cashRegiserId, ExceptionStatusCode.CashRegisterNotInStore, "Die Registrierkasse wurde dieser Filiale nicht zugewiesen!")
        {
        }
    }
}
