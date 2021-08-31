using ASPNET3015.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPNET3015.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }
        public DbSet<ASPNET3015.Models.Item> Item { get; set; }
        public DbSet<ASPNET3015.Models.Pin> Pin { get; set; }
        public DbSet<ASPNET3015.Models.Downvote> Downvote { get; set; }
        public DbSet<ASPNET3015.Models.ApplicationUser> ApplicationUser { get; set; }
    }
}
