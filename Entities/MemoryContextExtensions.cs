using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Memory.API.Entities
{
    public static class MemoryContextExtensions
    {
        public static async Task EnsureSeedDataForContext(this MemoryContext context, UserManager<GameUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            // Delete all users and clear data.
            context.GameUsers.RemoveRange(context.GameUsers);
            context.SaveChanges();
            
            var user = new GameUser
            {
                FirstName = "Jimmy",
                LastName = "Jay",
                UserName = "jimmy@hotmail.com",
                Email = "jimmy@hotmail.com",
                GameScores = new List<GameScore>()
                {
                    new GameScore(){
                        Id = new Guid("25320c5e-f58a-4b1f-b63a-8ee07a840bdf"),
                        Score = 80,
                        ScoreDate = new DateTime()
                    }
                }
            };
            await userManager.CreateAsync(user, "WebbieF415!!");

            var role = new IdentityRole("Admin");
            role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = "IsAdmin", ClaimValue = "True" });
            await roleManager.CreateAsync(role);
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
