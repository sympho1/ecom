using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestApi.DbSeed;
using RestApi.Models;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.JsonPatch;

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

            if (request.Limit >= 100)
            {
                _logger.LogInformation("requete superieure à plus de 100 produits");
            }

            Response.Headers["x-total-count"] = result.Count().ToString();

            return Ok(
                result.OrderBy(p => p.ProductNumber)
                .Skip(request.Offset)
                .Take(request.Limit)
            );
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> PostProduct([FromBody] Product product)
        {
            try
            {
                _context.Products.Add(product);
                _context.SaveChanges();

                return new CreatedResult($"/products/{product.ProductNumber.ToLower()}", product);
            }

            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossible d'ajouter un produit.");

                return ValidationProblem(ex.Message);
            }
        }

        [HttpGet]
        [Route("{productNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Product> GetProductByNumber([FromRoute] string productNumber)
        {
            var productDb = _context.Products
                        .FirstOrDefault(
                            p => p.ProductNumber.Equals(productNumber, 
                            StringComparison.InvariantCultureIgnoreCase));
            
            if (productDb == null) return NotFound();

            return Ok(productDb);
        }


        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> PutProduct([FromBody] Product product)
        {
            try
            {
                var productDb = _context.Products
                .FirstOrDefault(
                    p => p.ProductNumber.Equals(product.ProductNumber, StringComparison.InvariantCultureIgnoreCase)
                );


                if (productDb == null) return NotFound();

                productDb.Name = product.Name;
                productDb.Price = product.Price;
                productDb.Department = product.Department;

                _context.SaveChanges();

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "impossible de mettre à jour le produit.");
                return ValidationProblem(ex.Message);
            }
        }

        [HttpPatch]
        [Route("{ProductNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> PatchProduct([FromRoute] string ProductNumber, [FromBody] JsonPatchDocument<Product> patch)
        {
            try
            {
                var productDb = _context.Products
                .FirstOrDefault(
                    p => p.ProductNumber.Equals(ProductNumber, StringComparison.InvariantCultureIgnoreCase)
                );

                if (productDb == null) return NotFound();

                patch.ApplyTo(productDb, ModelState);

                if (!ModelState.IsValid || !TryValidateModel(productDb)) return ValidationProblem(ModelState);

                _context.SaveChanges();

                return Ok(productDb);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "impossible de patccher le produit");
                return ValidationProblem(ex.Message);
            }
        }

        [HttpDelete]
        [Route("{productNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Product> DeleteProduct([FromRoute] string productNumber)
        {
            var productDb = _context.Products
            .FirstOrDefault(
                p => p.ProductNumber.Equals(productNumber, StringComparison.InvariantCultureIgnoreCase)
            );

            if (productDb == null) return NotFound();

            _context.Products.Remove(productDb);
            _context.SaveChanges();

            return NoContent();
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