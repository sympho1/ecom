using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestApi.DbSeed;
using RestApi.Models;
using System;
using Microsoft.Extensions.Logging;

namespace RestApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ProductsController: ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ProductDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
            
            if (_context.Products.Any()) return;

            ProductSeed.InitData(context);
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IQueryable<Product>> GetProducts([FromQuery] string department, [FromQuery] ProductRequest request)
        {
            var result = _context.Products as IQueryable<Product>;

            if (!string.IsNullOrEmpty(department))
            {
                result = result.Where(p => p
                .Department.StartsWith(department, StringComparison.InvariantCultureIgnoreCase));
            }

            Response.Headers["x-total-count"] = result.Count().ToString();

            return Ok(
                result.OrderBy(p => p.ProductNumber)
                .Skip(request.Offset)
                .Take(request.Limit)
            );
        }
    }

    public class ProductRequest
    {
        [FromQuery(Name="limit")]
        public int Limit { get; set;} = 15;

        [FromQuery(Name="offset")]
        public int Offset { get; set; }
    }
}