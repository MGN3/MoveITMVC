using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoveITMVC.Models;

namespace MoveITMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartProductsController : ControllerBase
    {
        private readonly MoveITDbContext _context;

        public ShoppingCartProductsController(MoveITDbContext context)
        {
            _context = context;
        }

        // GET: api/ShoppingCartProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShoppingCartProduct>>> GetShoppingCartProducts()
        {
            return await _context.ShoppingCartProducts.ToListAsync();
        }

        // GET: api/ShoppingCartProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingCartProduct>> GetShoppingCartProduct(Guid id)
        {
            var shoppingCartProduct = await _context.ShoppingCartProducts.FindAsync(id);

            if (shoppingCartProduct == null)
            {
                return NotFound();
            }

            return shoppingCartProduct;
        }

        // PUT: api/ShoppingCartProducts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShoppingCartProduct(Guid id, ShoppingCartProduct shoppingCartProduct)
        {
            if (id != shoppingCartProduct.ShoppingCartId)
            {
                return BadRequest();
            }

            _context.Entry(shoppingCartProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShoppingCartProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ShoppingCartProducts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ShoppingCartProduct>> PostShoppingCartProduct(ShoppingCartProduct shoppingCartProduct)
        {
            _context.ShoppingCartProducts.Add(shoppingCartProduct);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ShoppingCartProductExists(shoppingCartProduct.ShoppingCartId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetShoppingCartProduct", new { id = shoppingCartProduct.ShoppingCartId }, shoppingCartProduct);
        }

        // DELETE: api/ShoppingCartProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShoppingCartProduct(Guid id)
        {
            var shoppingCartProduct = await _context.ShoppingCartProducts.FindAsync(id);
            if (shoppingCartProduct == null)
            {
                return NotFound();
            }

            _context.ShoppingCartProducts.Remove(shoppingCartProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ShoppingCartProductExists(Guid id)
        {
            return _context.ShoppingCartProducts.Any(e => e.ShoppingCartId == id);
        }
    }
}
