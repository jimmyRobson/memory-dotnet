using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Memory.API.Entities;
using Memory.API.Helpers;
using Memory.API.Models;
using Memory.API.Services;
using Memory.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Memory.API.Controllers
{
    [Route("api/users/{userId}/scores")]
    public class ScoreController: Controller
    {
        private IMemoryRepository _memoryRepository;
        public ScoreController(IMemoryRepository memoryRepository)
        {
            _memoryRepository = memoryRepository;
        }
        [Authorize]
        [MemberAuthorize]
        [HttpGet()]
        public IActionResult GetScoresForUser(string userId)
        {
            if(userId == "self")
                userId = _memoryRepository.GetUserId(this.User);

            if(_memoryRepository.GetUser(userId).Result == null)
            {
                    return NotFound();
            }
            var scoreEntity = _memoryRepository.GetScoresForUser(userId);
            var scoreModel = Mapper.Map<IEnumerable<ScoreModel>>(scoreEntity);
            return Ok(scoreModel);
        }
        [Authorize]
        [MemberAuthorize]
        [HttpGet("{id}", Name = "GetScoreForUser")]
        public IActionResult GetScoreForUser(string userId, string id)
        {
            if(userId == "self")
                userId = _memoryRepository.GetUserId(this.User);

            if(_memoryRepository.GetUser(userId).Result == null)
            {
                    return NotFound();
            }
            var scoreEntity = _memoryRepository.GetScoreForUser(userId, id);
            if(scoreEntity == null)
            {
                return NotFound();
            }
            var scoreModel = Mapper.Map<ScoreModel>(scoreEntity);
            return Ok(scoreModel);
        }
        [Authorize]
        [MemberAuthorize]
        [HttpPost()]
        public IActionResult CreateScoreForUser(string userId, [FromBody] ScoreCreateModel score)
        {
            if(score == null)
            {
                return BadRequest();
            }
            if(!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            if(userId == "self")
                userId = _memoryRepository.GetUserId(this.User);

            var userEntity = _memoryRepository.GetUser(userId).Result;
            if(userEntity == null)
            {
                    return NotFound();
            }
            var scoreEntity = Mapper.Map<GameScore>(score);
            _memoryRepository.CreateScoreForUser(userEntity, scoreEntity);
            if(!_memoryRepository.Save())
            {
                throw new Exception($"Creating a score for user {userEntity.Id} failed on save.");
            }
            var scoreModel = Mapper.Map<ScoreModel>(scoreEntity);
            return CreatedAtRoute("GetScoreForUser", 
                    new { userId = userId, id = scoreModel.Id},
                    scoreModel);
        }
        [Authorize]
        [MemberAuthorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteScore(string id, string userId)
        {
            if(userId == "self")
                userId = _memoryRepository.GetUserId(this.User);
            if(_memoryRepository.GetUser(userId).Result == null)
            {
                    return NotFound();
            }
            var scoreEntity = _memoryRepository.GetScoreForUser(userId, id);
            if(scoreEntity == null)
            {
                return NotFound();
            }
            _memoryRepository.DeleteScore(scoreEntity);
            if(!_memoryRepository.Save())
            {
                throw new Exception($"Deleting a score failed for user {userId} and score {id}");
            }
            return NoContent();
        }
        [Authorize]
        [MemberAuthorize]
        [HttpPut("{id}")]
        public IActionResult UpdateScoreForUser(string id, string userId,[FromBody] ScoreUpdateModel scoreUpdateModel)
        {
            if(scoreUpdateModel == null)
            {
                return BadRequest();
            }
            if(!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            if(userId == "self")
                userId = _memoryRepository.GetUserId(this.User);

            var userEntity = _memoryRepository.GetUser(userId).Result;
            if(userEntity == null)
            {
                    return NotFound();
            }
            var scoreEntity = _memoryRepository.GetScoreForUser(userId, id);
            Mapper.Map(scoreUpdateModel, scoreEntity);
            _memoryRepository.UpdateScoreForUser(scoreEntity);
            if(!_memoryRepository.Save())
            {
                throw new Exception($"Creating a score for user {userEntity.Id} failed on save.");
            }
            return NoContent();
        }
    }
}