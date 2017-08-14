using System;
using System.Threading.Tasks;
using Memory.API.Entities;
using Memory.API.Helpers;
using Memory.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Memory.API.Controllers
{
    public class AuthController : Controller
    {
        private MemoryContext _context;
        private SignInManager<GameUser> _signInManager;
        private ILogger<AuthController> _logger;

        public AuthController(MemoryContext context, SignInManager<GameUser> signInManager, ILogger<AuthController> logger)
        {
            _context = context;
            _signInManager = signInManager;
        }
        [HttpPost("api/auth/login")]
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            if(!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, false);
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest("Failed to login");

        }
        [Authorize]
        [HttpPost("api/auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}