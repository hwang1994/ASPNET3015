using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASPNET3015.Data;
using ASPNET3015.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ASPNET3015.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PinsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PinsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pins/
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pin>>> GetPin()
        {
            string term = Request.Query["term"];
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (term != null && term.Trim() != "")
            {
                return await _context.Pin
                    .Include(p => p.Item)
                        .ThenInclude(u => u.User)
                    //.Where(i => i.CreatedDate >= DateTime.Now.AddHours(-1))
                    .Where(p => p.UserId==userId)
                    .Where(p => p.Item.Title.Contains(term.Trim()) || p.Item.User.FirstName.Contains(term.Trim()) || p.Item.User.LastName.Contains(term.Trim()) || p.Item.Description.Contains(term.Trim()) || p.Item.Price.ToString().Contains(term.Trim()))
                    .ToArrayAsync();
            }
            else
            {
                return await _context.Pin
                    .Include(p => p.Item)
                        .ThenInclude(u => u.User)
                    //.Where(i => i.CreatedDate >= DateTime.Now.AddHours(-1))
                    .Where(p => p.UserId == userId)
                    .ToArrayAsync();
            }
            //return await _context.Pin.ToListAsync();
        }

        // GET: Pins/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pin>> GetPin(int id)
        {
            var pin = await _context.Pin.FindAsync(id);

            if (pin == null)
            {
                return NotFound();
            }

            return pin;
        }

        // PUT: Pins/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutPin(int id, Pin pin)
        // {
        //     if (id != pin.Id)
        //     {
        //         return BadRequest();
        //     }

        //     _context.Entry(pin).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!PinExists(id))
        //         {
        //             return NotFound();
        //         }
        //         else
        //         {
        //             throw;
        //         }
        //     }

        //     return NoContent();
        // }

        // POST: Pins/:itemId
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost("{id}")]
        public async Task<ActionResult<Pin>> PostPin(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var pin = _context.Pin.Where(p => p.ItemId == id && p.UserId == userId).FirstOrDefault();
            if (pin==null)
            {
                Pin newPin = new Pin { ItemId = id, UserId = userId, CreatedDate = DateTime.Now };
                _context.Pin.Add(newPin);
                await _context.SaveChangesAsync();
                return Content("Item Pinned");
            }
            else
            {
                return Content("Item already pinned by user");
            }
        }

        // DELETE: Pins/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePin(int id)
        {
            var pin = await _context.Pin.FindAsync(id);
            if (pin == null)
            {
                return NotFound();
            }

            _context.Pin.Remove(pin);
            await _context.SaveChangesAsync();

            return Content("Item unPinned");
        }

        private bool PinExists(int id)
        {
            return _context.Pin.Any(e => e.Id == id);
        }
    }
}
