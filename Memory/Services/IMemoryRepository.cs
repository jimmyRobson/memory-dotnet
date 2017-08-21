using Memory.API.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Memory.API.Services
{
    public interface IMemoryRepository
    {
        Task<IdentityResult> AddGameUser(GameUser gameUser, string password);
        Task<GameUser> GetUser(string id);
        List<GameUser> GetUsers();
        Task<IdentityResult> DeleteUser(GameUser user);
        Task<IdentityResult> UpdateGameUser(GameUser user);
        string GetUserId(ClaimsPrincipal user);
        IEnumerable<GameScore> GetScoresForUser(string userId);
        GameScore GetScoreForUser(string userId, string scoreId);
        Task<SignInResult> SignIn(string email, string password );
        void SignOut();
        void CreateScoreForUser(GameUser user, GameScore score);
        void UpdateScoreForUser(GameScore score);
        void DeleteScore(GameScore score);
        bool Save();
    }
}
