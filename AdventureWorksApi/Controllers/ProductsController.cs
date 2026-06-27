using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksApi.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksDbContext _db;

        public ProductsController(
            AdventureWorksDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [EndpointSummary("Get all products")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(
                await _db.Products
                    .Take(50)
                    .ToListAsync());
        }

        [HttpGet("{id}")]
        [EndpointSummary("Get a product by ID")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(x => x.ProductID == id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }
    }
}
