using Memory.API.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Memory.API.Services
{
    public interface IMemoryRepository
    {
        Task<IdentityResult> AddGameUser(GameUser gameUser, string password);
        Task<GameUser> GetCurrentUser(string currentUserName);
        Task<GameUser> GetUser(string id);
        List<GameUser> GetUsers();
        Task<IdentityResult> DeleteUser(GameUser user);
    }
}
