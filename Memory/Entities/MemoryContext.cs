using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Memory.API.Entities
{
    public class MemoryContext : IdentityDbContext<IdentityUser>
    {
        public MemoryContext(DbContextOptions<MemoryContext> options)
           : base(options)
        {
            Database.Migrate();
        }

        public DbSet<GameUser> GameUsers { get; set; }
        public DbSet<GameScore> GameScores { get; set; }

    }
}
