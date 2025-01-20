namespace StoreFront.Seed;

/// <summary>
/// We add some orders to the database, that have already been delivered. This is mainly to act as examples for testing.
/// </summary>
public static class OrderMaker
{
    public static async Task DeliveredOrders(PizzaShopDb db)
    {
        var orders = new Order[]
        {
            new()
            {
                Id = 1,
                Status = OrderStatus.Delivered,
                CreatedTime = DateTimeOffset.UtcNow,
                DeliveryAddress = new Address
                {
                    Id = 1,
                    Number = "123",
                    City = "Exeter",
                    PostalCode = "EX2 5LG",
                },
                Pizzas =
                [
                    new()
                    {
                        Id = 1,
                        Size = PizzaSize.Medium,
                        Toppings = new List<PizzaTopping>()
                        {
                            new()
                            {
                                //cheese
                                ToppingId = 1,
                            },
                            new()
                            {
                                //pepperoni
                                ToppingId = 2,
                            },
                            new()
                            {
                                //salami
                                ToppingId = 6,
                            },
                        }
                    },

                    new()
                    {
                        Id = 1,
                        Size = PizzaSize.Medium,
                        Toppings = new List<PizzaTopping>()
                        {
                            new()
                            {
                                //cheese
                                ToppingId = 1,
                            },
                            new()
                            {
                                //meatballs
                                ToppingId = 8,
                            },
                            new()
                            {
                                //peppers
                                ToppingId = 12,
                            },
                        }
                    }
                ]
            },
            new()
            {
                Id = 2,
                Status = OrderStatus.Delivered,
                CreatedTime = DateTimeOffset.UtcNow,
                DeliveryAddress = new Address
                {
                    Id = 1,
                    Number = "456",
                    City = "Altrincham",
                    PostalCode = "WA15 9AH",
                },
                Pizzas =
                [
                    new()
                    {
                        Id = 1,
                        Size = PizzaSize.Large,
                        Toppings = new List<PizzaTopping>()
                        {
                            new()
                            {
                                //cheese
                                ToppingId = 1,
                            },
                            new()
                            {
                                //ham
                                ToppingId = 5,
                            },
                            new()
                            {
                                //pineapple - you monsters!!
                                ToppingId = 15,
                            },
                        }
                    }
                ]
            },
            new()
            {
                Id = 3,
                Status = OrderStatus.Delivered,
                CreatedTime = DateTimeOffset.UtcNow,
                DeliveryAddress = new Address
                {
                    Id = 1,
                    Number = "789",
                    City = "Huddersfield",
                    PostalCode = "HD2 2QH",
                },
                Pizzas =
                [
                    new()
                    {
                        Id = 1,
                        Size = PizzaSize.Large,
                        Toppings = new List<PizzaTopping>()
                        {
                            new()
                            {
                                //cheese
                                ToppingId = 1,
                            },
                            new()
                            {
                                //broccoli
                                ToppingId = 16,
                            },
                            new()
                            {
                                //spinach
                                ToppingId = 17,
                            },
                        }
                    }
                ]
            },
        };
        
        await db.Orders.AddRangeAsync(orders);
        await db.SaveChangesAsync();
    }
}