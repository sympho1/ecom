using Microsoft.AspNetCore.Mvc;
using RestApi.DbSeed;
using RestApi.Models;

namespace RestApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ProductsController: ControllerBase
    {
        private readonly ProductDbContext _context;

        public ProductsController(ProductDbContext context)
        {
            _context = context;
            // if (_context.Products.Any()) return;

            ProductSeed.InitData(context);
        }
    }
}