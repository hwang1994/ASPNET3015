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
using System.IO;

namespace ASPNET3015.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DownvotesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DownvotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Downvotes/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Downvote>>> GetDownvote()
        {
            return await _context.Downvote.ToListAsync();
        }

        // GET: Downvotes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Downvote>> GetDownvote(int id)
        {
            var downvote = await _context.Downvote.FindAsync(id);

            if (downvote == null)
            {
                return NotFound();
            }

            return downvote;
        }

        // PUT: Downvotes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutDownvote(int id, Downvote downvote)
        // {
        //     if (id != downvote.Id)
        //     {
        //         return BadRequest();
        //     }

        //     _context.Entry(downvote).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!DownvoteExists(id))
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

        // POST: Downvotes/:itemId
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost("{id}")]
        public async Task<ActionResult<Downvote>> PostDownvote(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var downvote = _context.Downvote.Where(d => d.ItemId == id && d.UserId == userId).FirstOrDefault();
            if (downvote == null)
            {
                var count = _context.Downvote.Count(d => d.ItemId == id);
                if (count >= 4)
                {
                    var item = await _context.Item.FindAsync(id);
                    var pictureFile = item.Picture;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\pictures", pictureFile);
                    FileInfo file = new FileInfo(filePath);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                    _context.Item.Remove(item);
                    await _context.SaveChangesAsync();
                    string cookieIds = Request.Cookies["recentlyviewed"];
                    if (cookieIds != null)
                    {
                        string[] itemIds = cookieIds.Split("|");
                        if (itemIds.Contains(id.ToString()))
                        {
                            int index = Array.IndexOf(itemIds, id.ToString());
                            string newCookieIds = itemIds[0];
                            if (index == 0)
                            {
                                newCookieIds = itemIds[1] + "|" + itemIds[2] + "|" + itemIds[3];
                            }
                            else
                            {
                                for (int i = 1; i < itemIds.Length; i++)
                                {
                                    if (i != index)
                                    {
                                        newCookieIds += "|" + itemIds[i];
                                    }
                                }
                            }
                            Response.Cookies.Append("recentlyviewed", newCookieIds, new CookieOptions() { Expires = DateTime.Now.AddMinutes(60) });
                        }
                    }
                    return Content("Item deleted!");
                }
                else
                {
                    Downvote newDownvote = new Downvote { ItemId = id, UserId = userId, CreatedDate = DateTime.Now };
                    _context.Downvote.Add(newDownvote);
                    await _context.SaveChangesAsync();
                    return Content("Downvoted!");
                }
            }
            else
            {
                return Content("No downvoting more than once on same product!");
            }
        }

        // DELETE: Downvotes/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteDownvote(int id)
        // {
        //     var downvote = await _context.Downvote.FindAsync(id);
        //     if (downvote == null)
        //     {
        //         return NotFound();
        //     }

        //     _context.Downvote.Remove(downvote);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        private bool DownvoteExists(int id)
        {
            return _context.Downvote.Any(e => e.Id == id);
        }
    }
}
