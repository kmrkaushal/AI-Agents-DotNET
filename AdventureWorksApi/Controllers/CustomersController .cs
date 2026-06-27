using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksApi.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksDbContext _db;

        public CustomersController(
            AdventureWorksDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [EndpointSummary("Get all customers")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(
                await _db.Customers
                    .Take(50)
                    .ToListAsync());
        }

        [HttpGet("{id}")]
        [EndpointSummary("Get a customer by ID")]
        public async Task<IActionResult> Get(int id)
        {
            var customer = await _db.Customers
                .FirstOrDefaultAsync(x => x.CustomerID == id);

            if (customer == null)
                return NotFound();

            return Ok(customer);
        }
    }
}