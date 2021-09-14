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
using System.IO;
using System.Globalization;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace ASPNET3015.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly int MAX_RECENTYLVIEWED = 4;

        private readonly ApplicationDbContext _context;

        public ItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Items/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItem()
        {
            string term = Request.Query["term"];
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //System.Diagnostics.Debug.WriteLine("user id is" + userId);
            if (userId!=null)
            {
                if (term != null && term.Trim() != "")
                {
                    return await _context.Item
                        .Include(i => i.User)
                        //.Where(i => i.CreatedDate >= DateTime.Now.AddHours(-1))
                        .Where(i => !_context.Pin.Any(p => p.UserId == userId && p.ItemId == i.Id))
                        .Where(i => i.Title.Contains(term.Trim()) || i.User.FirstName.Contains(term.Trim()) || i.User.LastName.Contains(term.Trim()) || i.Description.Contains(term.Trim()) || i.Price.ToString().Contains(term.Trim()))
                        .ToArrayAsync();
                }
                else
                {
                    return await _context.Item
                        .Include(i => i.User)
                        //.Where(i => i.CreatedDate >= DateTime.Now.AddHours(-1))
                        .Where(i => !_context.Pin.Any(p => p.UserId == userId && p.ItemId == i.Id))
                        .ToArrayAsync();
                    //return null;
                }
            }
            else
            {
                if (term != null && term.Trim() != "")
                {
                    return await _context.Item
                        .Include(i => i.User)
                        //.Where(i => i.CreatedDate >= DateTime.Now.AddHours(-1))
                        .Where(i => i.Title.Contains(term.Trim()) || i.User.FirstName.Contains(term.Trim()) || i.User.LastName.Contains(term.Trim()) || i.Description.Contains(term.Trim()) || i.Price.ToString().Contains(term.Trim()))
                        .ToArrayAsync();
                }
                else
                {
                    return await _context.Item
                                .Include(i => i.User)
                                //.Where(i => i.CreatedDate >= DateTime.Now.AddHours(-1))
                                .ToArrayAsync();
                }
            }
            //return await _context.Item.ToListAsync();
        }

        // GET: Items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            if (Request.Cookies["recentlyviewed"]!=null)
            {
                string cookieIds = Request.Cookies["recentlyviewed"];
                string[] itemIds = cookieIds.Split("|");
                if (itemIds.Length < MAX_RECENTYLVIEWED && !itemIds.Contains(id.ToString()))
                {
                    string newCookieIds = itemIds[0];
                    for (int i = 1; i < itemIds.Length; i++)
                    {
                        newCookieIds += "|" + itemIds[i];
                    }
                    Response.Cookies.Append("recentlyviewed", newCookieIds+"|"+id.ToString(), new CookieOptions() { Expires = DateTime.Now.AddMinutes(60) });
                }
                else if (itemIds.Length >= MAX_RECENTYLVIEWED && !itemIds.Contains(id.ToString()))
                {
                    string newCookieIds = itemIds[1] + "|" + itemIds[2] + "|" + itemIds[3] + "|" + id.ToString();
                    Response.Cookies.Append("recentlyviewed", newCookieIds, new CookieOptions() { Expires = DateTime.Now.AddMinutes(60) });
                }
                else
                {
                    //do nothing
                }
            }
            else
            {
                Response.Cookies.Append("recentlyviewed", id.ToString(), new CookieOptions() { Expires = DateTime.Now.AddMinutes(60) });
            }
            var item = await _context.Item
                .Include(i => i.User)
                //.FirstOrDefaultAsync(i => i.Id == id && i.CreatedDate >= DateTime.Now.AddHours(-1));
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // PUT: Items/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutItem(int id, Item item)
        //{
        //    if (id != item.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(item).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ItemExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: Items/
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostItem(IFormCollection formCollection, IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string title = formCollection["title"];
            string stringPrice = formCollection["price"];
            string description = formCollection["description"];
            if (title.Trim() != "" && stringPrice.Trim() != "" && description.Trim() != "" && file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var uniqueFileName = DateTime.Now.ToString("ddMMyyyyHHmmssfff", CultureInfo.InvariantCulture) + fileName;
                var supportedTypes = new[] { "jpg", "png", "jpeg", "gif", "bmp", "svg", "webp" };
                var fileExt = Path.GetExtension(file.FileName).Substring(1);
                if (!supportedTypes.Contains(fileExt) && file.Length<(4*1024))
                {
                    return Content("Invalid picture file or file is over 4Mb");
                }
                Item item = new Item { Title = title.Trim(), Price = decimal.Parse(stringPrice.Trim(), CultureInfo.InvariantCulture), Description = description.Trim(), Picture = uniqueFileName, UserId = userId, CreatedDate = DateTime.Now };
                _context.Item.Add(item);
                await _context.SaveChangesAsync();
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\pictures", uniqueFileName);
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                return Content("Item Uploaded");
            }
            else
            {
                return Content("All fields are required");
            }
        }

        // DELETE: Items/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.Item.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            if (item.UserId==userId)
            {
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
                return Content("Delete failed! Unauthorized deletion");
            }
        }

        private bool ItemExists(int id)
        {
            return _context.Item.Any(e => e.Id == id);
        }

    }
}
