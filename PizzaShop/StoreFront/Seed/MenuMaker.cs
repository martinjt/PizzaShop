using StoreFrontCommon;

namespace StoreFront.Seed;

/// <summary>
/// Add some toppings to the database. If you use these toppings, then you should get relational integrity in the database.
/// </summary>
public static class MenuMaker
{
        public static void CreateToppings(PizzaShopDb db)
        {
                var toppings = new Topping[]
                {
                        new()
                        {
                                ToppingId= 1,
                                Name = "Cheese",
                                Price = 0.50m,
                        },
                        new()
                        {
                                ToppingId= 2,
                                Name = "Pepperoni",
                                Price = 1.0m,
                        },
                        new()
                        {
                                ToppingId= 3,
                                Name = "Sausage",
                                Price = 1.50m,
                        },
                        new()
                        {
                                ToppingId= 4,
                                Name = "Bacon",
                                Price = 1.0m,
                        },
                        new()
                        {
                                ToppingId= 5,
                                Name = "Ham",
                                Price = 1.5m
                        },
                        new()
                        {
                                ToppingId= 6,
                                Name = "Salami",
                                Price = 1.0m,
                        },
                        new()
                        {
                                ToppingId= 7,
                                Name = "Chicken",
                                Price = 1.0m,
                        },
                        new()
                        {
                                ToppingId= 8,
                                Name = "Meatballs",
                                Price = 2.0m,
                        },
                        new()
                        {
                                ToppingId= 9,
                                Name = "Anchovies",
                                Price = 0.50m,
                        },
                        new()
                        {
                                ToppingId= 10,
                                Name = "Olives",
                                Price = 1.0m,
                        },
                        new()
                        {
                                ToppingId= 11,
                                Name = "Onions",
                                Price = 0.50m,
                        },
                        new()
                        {
                                ToppingId= 12,
                                Name = "Peppers",
                                Price = 0.50m,
                        },
                        new()
                        {
                                ToppingId= 13,
                                Name = "Mushrooms",
                                Price = 1.0m,
                        },
                        new()
                        {
                                ToppingId= 14,
                                Name = "Tomatoes",
                                Price = 0.0m,
                        },
                        new()
                        {
                                ToppingId= 15,
                                Name = "Pineapple",
                                Price = 3.5m,
                        },
                        new()
                        {
                                ToppingId= 16,
                                Name = "Spinach",
                                Price = 0.5m,
                        },
                        new()
                        {
                                ToppingId= 17,
                                Name = "Broccoli",
                                Price = 0.5m,
                        },
                        new()
                        {
                                ToppingId= 18,
                                Name = "Roasted Red Peppers",
                                Price = 0.5m,
                        },
                        new()
                        {
                                ToppingId= 19,
                                Name = "Jalapeños",
                                Price = 1.0m,
                        },
                        new()
                        {
                                ToppingId= 20,
                                Name = "Garlic",
                                Price = 1.5m,
                        },
                        new()
                        {
                                ToppingId= 21,
                                Name = "Basil",
                                Price = 1.0m,
                        },
                        new()
                        {
                                ToppingId= 22,
                                Name = "Oregano",
                                Price = 1.0m,
                        },
                };
                
                db.Toppings.AddRange(toppings);
                db.SaveChanges();
        }
}