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
            if(!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            var userEntity = Mapper.Map<GameUser>(userModel);
            var userResult = _memoryRepository.AddGameUser(userEntity, userModel.Password).Result;
            if (!userResult.Succeeded)
            {
                if (userResult.Errors != null)
                {
                    this.AddErrors(userResult);
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
            var userName = this.User.Identity.Name;
            GameUser repoUser = (id == "self") ?
                                 _memoryRepository.GetCurrentUser(userName).Result:
                                 _memoryRepository.GetUser(id).Result;
            if(repoUser == null)
            {
                    return NotFound();
            }
            if(id == "self" || repoUser.UserName == userName || this.User.IsInRole("Admin"))
            {
                var user = Mapper.Map<UserModel>(repoUser);
                return Ok(user);
            }
            else
            {
                return Forbid();
            }
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
            var userName = this.User.Identity.Name;
            GameUser repoUser = (id == "self") ?
                                 _memoryRepository.GetCurrentUser(userName).Result:
                                 _memoryRepository.GetUser(id).Result;
            if(repoUser == null)
            {
                return NotFound();
            }
            if(id == "self" || repoUser.UserName == userName || this.User.IsInRole("Admin"))
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
        [HttpPut("{id}")]
        public IActionResult UpdateUser(string id, [FromBody]UserUpdateModel userModel)
        {
            if(userModel == null){
                return BadRequest();
            }
            var userName = this.User.Identity.Name;
            var repoUser = (id == "self") ?
                                 _memoryRepository.GetCurrentUser(userName).Result:
                                 _memoryRepository.GetUser(id).Result;
            if(repoUser == null)
            {
                return NotFound();
            }
            if(!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            if(id == "self" || repoUser.UserName == userName || this.User.IsInRole("Admin"))
            { // Is deleting self or is Admin
                Mapper.Map(userModel, repoUser);
                var userResult = _memoryRepository.UpdateGameUser(repoUser).Result;
                if (!userResult.Succeeded)
                {
                    if (userResult.Errors != null)
                    {
                        this.AddErrors(userResult);
                    }
                    return new UnprocessableEntityObjectResult(ModelState);
                }
                return NoContent();
            }
            else
            {
                return Forbid();
            }
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
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

    }
}