using LR_2.Data;
using LR_2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LR_2.Controllers
{
    [Route("api/Products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:Guid}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product>> GetProductAsync(Guid id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var product = await _context.Products.FirstOrDefaultAsync(e => e.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsAsync()
        {
            var products = await _context.Products.ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NotFound();
            }
            return Ok(products);
        }

        [HttpPut("{id:Guid}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateProduct(Guid id, [FromBody] Product product)
        {
            if (product == null || id != product.Id)
            {
                return BadRequest();
            }

            var productUp = await _context.Products.FirstOrDefaultAsync(u => u.Id == id);

            productUp.Name = product.Name;
            productUp.Description = product.Description;
            productUp.Kcal = product.Kcal;

            _context.Products.Update(productUp);

            return NoContent();
        }
        [HttpPatch("{id:Guid}", Name = "UpdatePartialProduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdatePartialProductAsync(Guid id, JsonPatchDocument<Product> patch)
        {
            if (patch == null || id == null)
            {
                return BadRequest();
            }

            var product = await _context.Products.FirstOrDefaultAsync(u => u.Id == id);

            if (product == null)
            {
                return BadRequest();
            }

            patch.ApplyTo(product, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{id:Guid}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var product = await _context.Products.FirstOrDefaultAsync(u => u.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            _context.Products.Remove(product);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product) 
        {
            if (_context.Products.FirstOrDefault(u => u.Name.ToLower() == product.Name.ToLower()) != null)
            {
                ModelState.AddModelError("", "Product already exists!");
                return BadRequest(ModelState);
            }
            if (product == null)
            {
                return BadRequest(product);
            }
            product.Id = new Guid();

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
        }

    }
}
