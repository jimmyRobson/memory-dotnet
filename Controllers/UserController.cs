using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
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
    [Route("api/users")]
    public class UserController : Controller
    {
        private IMemoryRepository _memoryRepository;
        private ILogger<UserController> _logger;

        public UserController( IMemoryRepository memoryRepository, ILogger<UserController> logger)
        {
            _memoryRepository = memoryRepository;
            _logger = logger;
        }
        [HttpPost]
        public IActionResult CreateUser([FromBody]UserCreateModel userModel)
        {
            if(userModel == null){
                return BadRequest();
            }
            var userEntity = Mapper.Map<GameUser>(userModel);
            var userResult = _memoryRepository.AddGameUser(userEntity, userModel.Password).Result;
            if (!userResult.Succeeded)
            {
                if (userResult.Errors != null)
                {
                    foreach (IdentityError error in userResult.Errors)
                    {
                        if(error.Code == "DuplicateUserName")
                        {
                            ModelState.AddModelError(error.Code, "An account already exists for this email.");
                        }
                        else
                        {
                            ModelState.AddModelError(error.Code, error.Description);
                        }
                    }
                }
                return new UnprocessableEntityObjectResult(ModelState);
            }
            var userToReturn = Mapper.Map<UserModel>(userEntity);
            return CreatedAtRoute("GetUser", new { id = userToReturn.Id}, userToReturn);
        }
        [HttpGet("{id}", Name = "GetUser")]
        [Authorize]
        public IActionResult GetUser(string id)
        {
            var userName = this.User.Identity.Name; // Check to make sure this happens.
            GameUser repoUser = _memoryRepository.GetCurrentUser(userName).Result;
            if(id != "self" && id != repoUser.Id)
            {
                if(this.User.IsInRole("Admin"))
                {
                    repoUser = _memoryRepository.GetUser(id).Result;
                }
                else
                {
                    return Forbid();
                }
            }
            if(repoUser == null)
            {
                    return NotFound();
            }
            var user = Mapper.Map<UserModel>(repoUser);
            return Ok(user);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult GetUsers()
        {
            var usersFromRepo = _memoryRepository.GetUsers();
            var authors = Mapper.Map<IEnumerable<UserModel>>(usersFromRepo);

            return Ok(authors);
        }
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteUser(string id)
        {
            var userName = this.User.Identity.Name; // Check to make sure this happens.
            GameUser repoUser;
            if(this.User.IsInRole("Admin"))
            {
                if(id == "self")
                {
                     repoUser = _memoryRepository.GetCurrentUser(userName).Result;
                }
                else
                {
                    repoUser = _memoryRepository.GetUser(id).Result;
                }
            }
            else
            { // Not Admin can only delete self.
                 repoUser = _memoryRepository.GetCurrentUser(userName).Result;
            }
            if(repoUser == null)
            {
                return NotFound();
            }
            if(id == "self" || id == repoUser.Id || this.User.IsInRole("Admin"))
            { // Is deleting self or is Admin
                var result = _memoryRepository.DeleteUser(repoUser).Result; 
                if(!result.Succeeded)
                {
                    throw new Exception($"Deleting user {id} failed on save.");
                }
                return NoContent();
            }
            else
            {
                return Forbid();
            }
        }

    }
}