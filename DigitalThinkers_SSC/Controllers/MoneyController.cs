using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalThinkers_SSC.Models;

namespace DigitalThinkers_SSC.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MoneyController : ControllerBase
    {
        private readonly MoneyContext _context;

        public MoneyController(MoneyContext context)
        {
            _context = context;
        }

        // GET: api/Money
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Money>>> GetMoneyItems()
        {
            return await _context.MoneyItems.ToListAsync();
        }

        // GET: api/Money/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Money>> GetMoney(long id)
        {
            var money = await _context.MoneyItems.FindAsync(id);

            if (money == null)
            {
                return NotFound();
            }

            return money;
        }

        // PUT: api/Money/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMoney(long id, Money money)
        {
            if (id != money.Id)
            {
                return BadRequest();
            }

            _context.Entry(money).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MoneyExists(id))
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

        // POST: api/Money
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Money>> PostMoney(Money money)
        {
            _context.MoneyItems.Add(money);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMoney", new { id = money.Id }, money);
        }

        // DELETE: api/Money/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMoney(long id)
        {
            var money = await _context.MoneyItems.FindAsync(id);
            if (money == null)
            {
                return NotFound();
            }

            _context.MoneyItems.Remove(money);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MoneyExists(long id)
        {
            return _context.MoneyItems.Any(e => e.Id == id);
        }
    }
}
