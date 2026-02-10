using System;
using System.Collections.Generic;
using System.Linq;

namespace DelegatesDemo
{
    class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
    }

    class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public decimal OrderAmount { get; set; }
    }

    class Assignment
    {
        public void Run()
        {
            List<Customer> customers = new()
            {
                new Customer { CustomerId = 1, CustomerName = "Alice" },
                new Customer { CustomerId = 2, CustomerName = "Bob" },
                new Customer { CustomerId = 3, CustomerName = "Charlie" }
            };

            List<Order> orders = new()
            {
                new Order { OrderId = 101, CustomerId = 1, OrderAmount = 200 },
                new Order { OrderId = 102, CustomerId = 1, OrderAmount = 150 },
                new Order { OrderId = 103, CustomerId = 2, OrderAmount = 300 },
                new Order { OrderId = 104, CustomerId = 2, OrderAmount = 100 }
            };

            var result =
                from c in customers
                join o in orders
                    on c.CustomerId equals o.CustomerId
                group o by new { c.CustomerId, c.CustomerName } into g
                select new
                {
                    CustomerName = g.Key.CustomerName,
                    OrderCount = g.Count(),
                    TotalOrderValue = g.Sum(x => x.OrderAmount),
                    Orders = g.ToList()
                };

            foreach (var item in result)
            {
                Console.WriteLine($"Customer: {item.CustomerName}");
                Console.WriteLine($"Orders Count: {item.OrderCount}");
                Console.WriteLine($"Total Order Value: {item.TotalOrderValue}");

                foreach (var order in item.Orders)
                {
                    Console.WriteLine($"OrderId: {order.OrderId}, Amount: {order.OrderAmount}");
                }

                Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            }
        }
    }
}
