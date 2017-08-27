using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Memory.API.Entities;
using Memory.API.Filters;
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
        private IMapper _mapper;

        public UserController(IMemoryRepository memoryRepository, IMapper mapper)
        {
            _memoryRepository = memoryRepository;
            _mapper = mapper;
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
            var userEntity = _mapper.Map<GameUser>(userModel);
            var userResult = _memoryRepository.AddGameUser(userEntity, userModel.Password).Result;
            if (!userResult.Succeeded)
            {
                if (userResult.Errors != null)
                {
                    this.AddErrors(userResult);
                }
                return new UnprocessableEntityObjectResult(ModelState);
            }
            var userToReturn = _mapper.Map<UserModel>(userEntity);
            return CreatedAtRoute("GetUser", new { id = userToReturn.Id}, userToReturn);
        }
        [Authorize]
        [MemberAuthorize]
        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetUser(string id)
        {
            if(id == "self")
                id = _memoryRepository.GetUserId(this.User);
            
            var userEntity = _memoryRepository.GetUser(id).Result;
            if(userEntity == null)
            {
                    return NotFound();
            }
            var user = _mapper.Map<UserModel>(userEntity);
            return Ok(user);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult GetUsers()
        {
            var userEntities = _memoryRepository.GetUsers();
            var authors = _mapper.Map<IEnumerable<UserModel>>(userEntities);
            return Ok(authors);
        }
        
        [Authorize]
        [MemberAuthorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(string id)
        {
            if(id == "self")
                id = _memoryRepository.GetUserId(this.User);
            
            var userEntity = _memoryRepository.GetUser(id).Result;
            if(userEntity == null)
            {
                return NotFound();
            }
            var result = _memoryRepository.DeleteUser(userEntity).Result; 
            if(!result.Succeeded)
            {
                throw new Exception($"Deleting user {id} failed on save.");
            }
            return NoContent();
        }
        [Authorize]
        [MemberAuthorize]
        [HttpPut("{id}")]
        public IActionResult UpdateUser(string id, [FromBody]UserUpdateModel userUpdateModel)
        {
            if(userUpdateModel == null){
                return BadRequest();
            }
            if(!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            if(id == "self")
                id = _memoryRepository.GetUserId(this.User);
            
            var userEntity = _memoryRepository.GetUser(id).Result;
            if(userEntity == null)
            {
                return NotFound();
            }
            _mapper.Map<UserUpdateModel, GameUser>(userUpdateModel, userEntity);
            var userResult = _memoryRepository.UpdateGameUser(userEntity).Result;
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