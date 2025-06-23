using GCloudPhone;

public class Cart
{
    private static readonly Lazy<Cart> lazy = new Lazy<Cart>(() => new Cart());

    public static Cart Instance { get { return lazy.Value; } }

    public List<OrderItemViewModel> Items { get; private set; }

    private Cart()
    {
        Items = new List<OrderItemViewModel>();
    }

    public event Action ItemCountChanged;

    private void NotifyItemCountChanged()
    {
        ItemCountChanged?.Invoke();
    }

    public List<OrderItemViewModel> GetItems()
    {
        return Items;
    }

    public int AddItem(OrderItemViewModel item)
    {
        int nextId = 0;
        var existingItem = Items.FirstOrDefault(i => i.ProductID == item.ProductID && i.Idc == item.Idc);
        if (existingItem != null)
        {
            nextId = existingItem.Idc;
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            nextId = Items.Any() ? Items.Max(i => i.Idc) + 1 : 1;
            item.Idc = nextId;

            Items.Add(item);
        }
        NotifyItemCountChanged();
        
        return nextId;
    }


    public void RemoveItem(OrderItemViewModel item)
    {
        var itemToRemove = Items.FirstOrDefault(i =>
            i.ProductID == item.ProductID &&
            i.Reference == item.Reference);

        if (itemToRemove != null)
        {
            Items.Remove(itemToRemove);
            NotifyItemCountChanged();
        }
        else
        {
            // Log a message if the item was not found
            Console.WriteLine("Item not found in cart. Could not remove.");
        }
    }
    public void ClearCart()
    {
        Items.Clear();
        NotifyItemCountChanged();
    }

    public void UpdateItemQuantity(int productId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.ProductID == productId);
        if (item != null)
        {
            item.Quantity = quantity;
            NotifyItemCountChanged();
        }
    }
}
