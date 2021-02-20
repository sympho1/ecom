using System;
using RestApi.Extensions;
using RestApi.Models;

namespace RestApi.DbSeed
{
    public static class ProductSeed
    {
        public static void InitData(ProductDbContext ctx)
        {
            var rnd = new Random();

            var adjectives = new [] {"Small", "Ergonomic", "Rustic", "Smart", "Sleek"};
            var materials = new [] { "Steel", "Wooden", "Concrete", "Plastic", "Granite", "Rubber" };
            var names = new [] { "Chair", "Car", "Computer", "Pants", "Shoes" };
            var departments = new [] { "Books", "Movies", "Music", "Games", "Electronics" };

            ctx.Products.AddRange( 
                700.Times(
                    x => {
                        var adjective = adjectives[rnd.Next(0, 5)];
                        var material = materials[rnd.Next(0, 5)];
                        var name = names[rnd.Next(0, 5)];
                        var department = departments[rnd.Next(0, 5)];
                        var productId = $"{x, -3:000}";

                        return new Product{
                            ProductNumber = $"{department}-{name}-{productId}",
                            Name = $"{adjective} {material} {name}",
                            Price = (double) rnd.Next(1000, 9000) / 100,
                            Department = department                        
                        };
                    }
                )
            );

            ctx.SaveChanges();
        }
    }
}