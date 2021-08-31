using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASPNET3015.Data;
using ASPNET3015.Models;

namespace ASPNET3015.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RecentlyViewedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecentlyViewedController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RecentlyViewed/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItem()
        {
            string cookieIds = Request.Cookies["recentlyviewed"];
            System.Diagnostics.Debug.WriteLine("cookieIds are " + cookieIds);
            if (cookieIds!=null)
            {
                string[] itemIds = cookieIds.Split("|");
                return await _context.Item
                    .Include(i => i.User)
                    .Where(i => itemIds.Contains(i.Id.ToString()))
                    .OrderBy(i => i.User.FirstName).ThenByDescending(i => i.Price)
                    .ToArrayAsync();
            }
            else
            {
                return NoContent();
            }
            //return await _context.Item.ToListAsync();
        }
    }
}
