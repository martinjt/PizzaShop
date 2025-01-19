namespace StoreFront;

public static class MenuMaker
{
        public static void CreateToppings(PizzaShopDb db)
        {
                var toppings = new Topping[]
                {
                        new()
                        {
                                Id=1,
                                Name = "Cheese",
                                Price = 0.50m,
                        },
                        new()
                        {
                                Id=2,
                                Name = "Pepperoni",
                                Price = 1.0m,
                        },
                        new()
                        {
                                Id=3,
                                Name = "Sausage",
                                Price = 1.50m,
                        },
                        new()
                        {
                                Id=4,
                                Name = "Bacon",
                                Price = 1.0m,
                        },
                        new()
                        {
                                Id=5,
                                Name = "Ham",
                                Price = 1.5m
                        },
                        new()
                        {
                                Id=6,
                                Name = "Salami",
                                Price = 1.0m,
                        },
                        new()
                        {
                                Id=7,
                                Name = "Chicken",
                                Price = 1.0m,
                        },
                        new()
                        {
                                Id=8,
                                Name = "Meatballs",
                                Price = 2.0m,
                        },
                        new()
                        {
                                Id=9,
                                Name = "Anchovies",
                                Price = 0.50m,
                        },
                        new()
                        {
                                Id=10,
                                Name = "Olives",
                                Price = 1.0m,
                        },
                        new()
                        {
                                Id=11,
                                Name = "Onions",
                                Price = 0.50m,
                        },
                        new()
                        {
                                Id=12,
                                Name = "Peppers",
                                Price = 0.50m,
                        },
                        new()
                        {
                                Id=13,
                                Name = "Mushrooms",
                                Price = 1.0m,
                        },
                        new()
                        {
                                Id=14,
                                Name = "Tomatoes",
                                Price = 0.0m,
                        },
                        new()
                        {
                                Id=15,
                                Name = "Pineapple",
                                Price = 3.5m,
                        },
                        new()
                        {
                                Id=16,
                                Name = "Spinach",
                                Price = 0.5m,
                        },
                        new()
                        {
                                Id=17,
                                Name = "Broccoli",
                                Price = 0.5m,
                        },
                        new()
                        {
                                Id=18,
                                Name = "Roasted Red Peppers",
                                Price = 0.5m,
                        },
                        new()
                        {
                                Id=19,
                                Name = "Jalapeños",
                                Price = 1.0m,
                        },
                        new()
                        {
                                Id=20,
                                Name = "Garlic",
                                Price = 1.5m,
                        },
                        new()
                        {
                                Id=21,
                                Name = "Basil",
                                Price = 1.0m,
                        },
                        new()
                        {
                                Id=22,
                                Name = "Oregano",
                                Price = 1.0m,
                        },
                };
                
                db.Toppings.AddRange(toppings);
                db.SaveChanges();
        }
}