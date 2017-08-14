using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memory.API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Memory.API.Services
{
    public class MemoryRepository : IMemoryRepository
    {
        private MemoryContext _context;
        private UserManager<GameUser> _userManager;
        private SignInManager<GameUser> _signInManager;

        public MemoryRepository(MemoryContext context, UserManager<GameUser> userManager, SignInManager<GameUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<IdentityResult> AddGameUser(GameUser gameUser, string password)
        {
            return await _userManager.CreateAsync(gameUser, password);
            
            // return userResult;
        }
        public async Task<GameUser> GetCurrentUser(string currentUserName)
        {
            return await _userManager.FindByNameAsync(currentUserName);
        }
        public async Task<GameUser> GetUser(string id)
        {
             return await _userManager.FindByIdAsync(id);
        }
        public List<GameUser> GetUsers()
        {
            return  _userManager.Users.ToList();
        }
        public async Task<IdentityResult> DeleteUser(GameUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var item in userRoles.ToList())
            { // Delete all roles for user
                var result = await _userManager.RemoveFromRoleAsync(user, item);
            }
            return await _userManager.DeleteAsync(user);
        }
        public async Task<IdentityResult> UpdateGameUser(GameUser gameUser)
        {
            return await _userManager.UpdateAsync(gameUser);
        }
    }
}