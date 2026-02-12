namespace ExceptionHandlingDemo;

class Program
{
    public static void _Main()
    {
        var processor = new OrderProcessor();

        processor.Process("SKU-100", 2);
        processor.Process("", -1);
        processor.Process("SKU-999", 1);
        processor.Process("SKU-200", 3);
    }
}

public class OrderProcessor
{
    private readonly Dictionary<string, decimal> _catalog = new()
    {
        ["SKU-100"] = 29.99m,
        ["SKU-200"] = 49.99m,
        ["SKU-300"] = 99.99m
    };

    private readonly List<Order> _savedOrders = [];

    public void Process(string sku, int quantity)
    {
        try
        {
            var order = CreateOrder(sku, quantity);
            Save(order);
            Console.WriteLine($"Saved order: {order.Sku}, qty {order.Quantity}, total {order.Total}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process order: {ex.Message}");
        }
        finally
        {
            // _savedOrders.Clear();
            Console.WriteLine("Process :: Inside Finally");
        }
    }

    private Order CreateOrder(string sku, int quantity)
    {
        try
        {
            Validate(sku, quantity);
            var price = LookupPrice(sku);
            return new Order(sku, quantity, price * quantity);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating order: {ex.Message}");
            throw;
        }
        finally
        {
            Console.WriteLine("CreateOrder :: Inside Finally");
        }
    }

    private void Validate(string sku, int quantity)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU is required");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
            throw;
        }
        finally
        {
            Console.WriteLine("Validate :: Inside Finally");
        }
    }

    private decimal LookupPrice(string sku)
    {
        try
        {
            if (!_catalog.TryGetValue(sku, out var price))
                throw new KeyNotFoundException($"Unknown SKU: {sku}");

            return price;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            Console.WriteLine("LookupPrice :: Inside Finally");
        }
    }

    private void Save(Order order)
    {
        try
        {
            if (Random.Shared.Next(5) == 0)
                throw new IOException("Connection timeout");

            _savedOrders.Add(order);
        }
        catch (IOException)
        {
            Console.WriteLine("Save failed, continuing...");
        }
        finally
        {
            Console.WriteLine("Save :: Inside Finally");
        }
    }

}
public record Order(string Sku, int Quantity, decimal Total);
