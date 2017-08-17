using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
        public async Task<SignInResult> SignIn(string email, string password )
        {
            return await _signInManager.PasswordSignInAsync(email, password, true, false);
        }
        public async void SignOut()
        {
            await _signInManager.SignOutAsync();
        }
        public async Task<IdentityResult> AddGameUser(GameUser gameUser, string password)
        {
            if (gameUser.GameScores.Any())
            {
                foreach (var score in gameUser.GameScores)
                {
                    score.Id = Guid.NewGuid().ToString();
                    score.ScoreDate = DateTime.Now;
                }
            }
            return await _userManager.CreateAsync(gameUser, password);
            
            // return userResult;
        }
        public string GetUserId(ClaimsPrincipal user)
        {
            return _userManager.GetUserId(user);
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
        public IEnumerable<GameScore> GetScoresForUser(string userId)
        {
            return _context.GameScores
                        .Where(s => s.GameUserId == userId).OrderBy(s => s.ScoreDate).ToList();
        }
        public GameScore GetScoreForUser(string userId, string scoreId)
        {
            return _context.GameScores
                        .Where(s => s.GameUserId == userId && s.Id == scoreId).FirstOrDefault();
        }
        public void CreateScoreForUser(GameUser user, GameScore score)
        {
            if(score.Id == null)
            {
                score.Id = Guid.NewGuid().ToString();
            }
            score.ScoreDate = DateTime.Now;
            user.GameScores.Add(score);
        }
        public void UpdateScoreForUser(GameScore score)
        {
            score.ScoreDate = DateTime.Now;
        }
        public void DeleteScore(GameScore score)
        {
            _context.GameScores.Remove(score);
        }
        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}