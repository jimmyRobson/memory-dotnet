using System;
using System.Threading.Tasks;
using Memory.API.Entities;
using Memory.API.Helpers;
using Memory.API.Models;
using Memory.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Memory.API.Controllers
{
    public class AuthController : Controller
    {
        private IMemoryRepository _memoryRepository;

        public AuthController(IMemoryRepository memoryRepository)
        {
            _memoryRepository = memoryRepository;
        }
        [HttpPost("api/auth/login")]
        public IActionResult Login([FromBody] CredentialModel model)
        {
            if(!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            var result = _memoryRepository.SignIn(model.Email, model.Password).Result;
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest("Failed to login");

        }
        [Authorize]
        [HttpPost("api/auth/logout")]
        public IActionResult Logout()
        {
            _memoryRepository.SignOut();
            return Ok();
        }
    }
}