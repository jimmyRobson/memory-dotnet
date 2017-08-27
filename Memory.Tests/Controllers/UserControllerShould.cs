using System;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Memory.API.Models;
using Memory.API.Controllers;
using Memory.API.Services;
using Memory.API.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Memory.API.Helpers;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;

namespace Memory.Tests.Controllers
{
    public class UserConcontrollerShould
    {
        private Mock<IMemoryRepository> _mockedRepository;
        private UserController _sut;
        private Mock<IMapper> _mockedMapper;
        private Mock<IIdentity> _mockedIdentity;
        private Mock<HttpContext> _mockedContext;

        public UserConcontrollerShould()
        {
            _mockedRepository = new Mock<IMemoryRepository>();
            _mockedMapper = new Mock<IMapper>();
            _mockedIdentity = new Mock<IIdentity>();
            _mockedContext = new Mock<HttpContext>();
            _sut = new UserController(_mockedRepository.Object, _mockedMapper.Object);
        }
        [Fact]
        public void CreateNewUserWhenModelValidAndIdentitySuccess()
        {
            GameUser savedUser = null;
            string userId = "32c88adb-056a-48b9-a898-632ff09806c1";
            var userCreateModel =  new UserCreateModel
            {
                FirstName = "Jess",
                LastName = "Scott",
                Email = "Jess@gmail.com",
                Password = "Password415!"
            };
            var mappedUserEntity = new GameUser
            {
                FirstName = userCreateModel.FirstName,
                LastName = userCreateModel.LastName,
                Email = userCreateModel.Email,
                UserName = userCreateModel.Email
            };
            var mappedUserModel = new UserModel
            {
                FirstName = userCreateModel.FirstName,
                LastName = userCreateModel.LastName,
                Email = userCreateModel.Email,
                Id = userId
            };
            _mockedMapper.Setup(m => m.Map<GameUser>(It.IsAny<UserCreateModel>())).Returns(mappedUserEntity);
            _mockedMapper.Setup(m => m.Map<UserModel>(It.IsAny<GameUser>())).Returns(mappedUserModel);
            _mockedRepository.Setup(x => x.AddGameUser(It.IsAny<GameUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(IdentityResult.Success))
                .Callback((GameUser user, string password) => {
                    savedUser = user;
                    savedUser.Id = userId; // Set the id after save.
                });
            
            IActionResult result = _sut.CreateUser(userCreateModel);

            // Check object saved to the database.
            _mockedRepository.Verify(
                x => x.AddGameUser(It.IsAny<GameUser>(), It.Is<string>(p => p==userCreateModel.Password)), Times.Once);
            Assert.Equal(userCreateModel.FirstName, savedUser.FirstName);
            Assert.Equal(userCreateModel.LastName, savedUser.LastName);
            Assert.Equal(userCreateModel.Email, savedUser.Email);
            Assert.Equal(userCreateModel.Email, savedUser.UserName);

            // Check returned result.
            CreatedAtRouteResult createdAtResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetUser", createdAtResult.RouteName);
            Assert.Equal(mappedUserModel.Id, createdAtResult.RouteValues["id"]);
            Assert.Equal(mappedUserModel, createdAtResult.Value);
        }
        [Fact]
        public void NotCreateUserWhenModelError()
        {
            _sut.ModelState.AddModelError("x", "Model Error");
            var userCreateModel =  new UserCreateModel
            {
                FirstName = "Jess"
            };

            IActionResult result = _sut.CreateUser(userCreateModel);

            _mockedRepository.Verify(
                x => x.AddGameUser(It.IsAny<GameUser>(), It.Is<string>(p => p==userCreateModel.Password)), Times.Never);
            UnprocessableEntityObjectResult badResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
            Assert.Equal(422, badResult.StatusCode);
        }
        [Fact]
        public void NotCreateNewUserWhenModelValidAndIdentityError()
        {
            var userCreateModel =  new UserCreateModel
            {
                FirstName = "Jess",
                LastName = "Scott",
                Email = "Jess@gmail.com",
                Password = "Password415!!"
            };
            var mappedUserEntity = new GameUser
            {
                FirstName = userCreateModel.FirstName,
                LastName = userCreateModel.LastName,
                Email = userCreateModel.Email,
                UserName = userCreateModel.Email
            };
            _mockedMapper.Setup(m => m.Map<GameUser>(It.IsAny<UserCreateModel>())).Returns(mappedUserEntity);
            _mockedRepository.Setup(x => x.AddGameUser(It.IsAny<GameUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(IdentityResult.Failed()));
            
            IActionResult result = _sut.CreateUser(userCreateModel);

            _mockedRepository.Verify(
                x => x.AddGameUser(It.IsAny<GameUser>(), It.Is<string>(p => p==userCreateModel.Password)), Times.Once);

            // Check returned result.
            UnprocessableEntityObjectResult badResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
            Assert.Equal(422, badResult.StatusCode);
        }
        [Theory]
        [InlineData("32c88adb-056a-48b9-a898-632ff09806c1")]
        [InlineData("self")]
        public void GetUserWhenFound(string id)
        {
            string userId = "32c88adb-056a-48b9-a898-632ff09806c1";
            var userEntity = new GameUser
            {
                Id = userId,
                UserName = "coolperson2020@gmail.com",
                FirstName = "Jess",
                LastName = "Scott",
                Email = "coolperson2020@gmail.com"
            };
            var mappedUserModel =  new UserModel
            {
                Id = userId,
                FirstName = userEntity.FirstName,
                LastName = userEntity.LastName,
                Email = userEntity.Email
            };
            _mockedContext.SetupGet(x => x.User.Identity).Returns(_mockedIdentity.Object);
            _mockedMapper.Setup(m => m.Map<UserModel>(It.IsAny<GameUser>())).Returns(mappedUserModel);
            _mockedRepository.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);
            _mockedRepository.Setup(x => x.GetUser(It.IsAny<string>()))
                .Returns(Task.FromResult(userEntity));
            
            IActionResult result = _sut.GetUser(id);

            // Check repo was queried userId.
            _mockedRepository.Verify(
                x => x.GetUser(It.Is<string>(p => p==userId)), Times.Once);
            // Check returned results.
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(mappedUserModel, okResult.Value);
        }
        [Theory]
        [InlineData("32c88adb-056a-48b9-a898-632ff09806c1")]
        [InlineData("self")]
        public void NotGetUserWhenNotFound(string id)
        {
            string userId = "32c88adb-056a-48b9-a898-632ff09806c1";
            GameUser userEntity = null;
            _mockedContext.SetupGet(x => x.User.Identity).Returns(_mockedIdentity.Object);
            _mockedRepository.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);
            _mockedRepository.Setup(x => x.GetUser(It.IsAny<string>()))
                .Returns(Task.FromResult(userEntity));
            
            IActionResult result = _sut.GetUser(id);

            // Check repo was queried.
            _mockedRepository.Verify(
                x => x.GetUser(It.Is<string>(p => p==userId )), Times.Once);

            NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
        [Fact]
        public void GetAllUsers()
        {
            var userEntityList =  new List<GameUser>
            {
                new GameUser{
                    Id = "32c88adb-056a-48b9-a898-632ff09806c1",
                    UserName = "coolperson2020@gmail.com",
                    FirstName = "Jess",
                    LastName = "Scott",
                    Email = "coolperson2020@gmail.com"
                }
            };
            var mappedUserModelList =  new List<UserModel>
            {
                new UserModel
                    {
                        Id = userEntityList[0].Id,
                        FirstName = userEntityList[0].FirstName,
                        LastName = userEntityList[0].LastName,
                        Email = userEntityList[0].Email
                    }
            };
            _mockedRepository.Setup(x => x.GetUsers())
                .Returns(userEntityList);
            _mockedMapper.Setup(m => m.Map<IEnumerable<UserModel>>(It.IsAny<IEnumerable<GameUser>>())).Returns(mappedUserModelList);
            
            IActionResult result = _sut.GetUsers();

            // Check repo was queried userId.
            _mockedRepository.Verify(
                x => x.GetUsers(), Times.Once);
            // Check returned results.
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(mappedUserModelList, okResult.Value);
        }
        [Theory]
        [InlineData("32c88adb-056a-48b9-a898-632ff09806c1")]
        [InlineData("self")]
        public void DeleteUserWhenFound(string id)
        {
            string userId = "32c88adb-056a-48b9-a898-632ff09806c1";
            var userEntity = new GameUser
            {
                Id = userId,
                UserName = "coolperson2020@gmail.com",
                FirstName = "Jess",
                LastName = "Scott",
                Email = "coolperson2020@gmail.com"
            };
            _mockedContext.SetupGet(x => x.User.Identity).Returns(_mockedIdentity.Object);
            _mockedRepository.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);
            _mockedRepository.Setup(x => x.GetUser(It.IsAny<string>()))
                .Returns(Task.FromResult(userEntity));
            _mockedRepository.Setup(x => x.DeleteUser(It.IsAny<GameUser>()))
                .Returns(Task.FromResult(IdentityResult.Success));
            
            IActionResult result = _sut.DeleteUser(id);

            _mockedRepository.Verify(
                x => x.GetUser(It.Is<string>(p => p==userId)), Times.Once);
            _mockedRepository.Verify(
                x => x.DeleteUser(It.Is<GameUser>(p => p==userEntity)), Times.Once);
            
            NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }
        [Theory]
        [InlineData("32c88adb-056a-48b9-a898-632ff09806c1")]
        [InlineData("self")]
        public void DeleteUserReturn404WhenNotFound(string id)
        {
            string userId = "32c88adb-056a-48b9-a898-632ff09806c1";
            GameUser userEntity = null;
            _mockedContext.SetupGet(x => x.User.Identity).Returns(_mockedIdentity.Object);
            _mockedRepository.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);
            _mockedRepository.Setup(x => x.GetUser(It.IsAny<string>()))
                .Returns(Task.FromResult(userEntity));
            _mockedRepository.Setup(x => x.DeleteUser(It.IsAny<GameUser>()))
                .Returns(Task.FromResult(IdentityResult.Success));
            
            IActionResult result = _sut.DeleteUser(id);

            _mockedRepository.Verify(
                x => x.GetUser(It.Is<string>(p => p==userId)), Times.Once);
            _mockedRepository.Verify(
                x => x.DeleteUser(It.Is<GameUser>(p => p==userEntity)), Times.Never);
            
            NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
        [Theory]
        [InlineData("32c88adb-056a-48b9-a898-632ff09806c1")]
        [InlineData("self")]
        public void DeleteUserThrowsExceptionWhenDeleteFails(string id)
        {
            string userId = "32c88adb-056a-48b9-a898-632ff09806c1";
            var userEntity = new GameUser
            {
                Id = userId,
                UserName = "coolperson2020@gmail.com",
                FirstName = "Jess",
                LastName = "Scott",
                Email = "coolperson2020@gmail.com"
            };
            _mockedContext.SetupGet(x => x.User.Identity).Returns(_mockedIdentity.Object);
            _mockedRepository.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);
            _mockedRepository.Setup(x => x.GetUser(It.IsAny<string>()))
                .Returns(Task.FromResult(userEntity));
            _mockedRepository.Setup(x => x.DeleteUser(It.IsAny<GameUser>()))
                .Returns(Task.FromResult(IdentityResult.Failed()));

            // The middleware will handle the HTTP Response after the exception is thrown.
            var exception = Record.Exception(() => _sut.DeleteUser(id));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);

            _mockedRepository.Verify(
                x => x.GetUser(It.Is<string>(p => p==userId)), Times.Once);
            _mockedRepository.Verify(
                x => x.DeleteUser(It.Is<GameUser>(p => p==userEntity)), Times.Once);
        }
        [Theory]
        [InlineData("32c88adb-056a-48b9-a898-632ff09806c1")]
        [InlineData("self")]
        public void UpdateUserWhenModelValidAndIdentitySuccess(string id)
        {
            GameUser updatedUser = null;
            string userId = "32c88adb-056a-48b9-a898-632ff09806c1";
            var userUpdateModel =  new UserUpdateModel
            {
                FirstName = "Jess",
                LastName = "Brown",
                Email = "JessBrown@gmail.com"
            };
            var foundUserEntity = new GameUser
            {
                Id = userId,
                UserName = "coolperson2020@gmail.com",
                FirstName = "Jess",
                LastName = "Scott",
                Email = "coolperson2020@gmail.com"
            };
            _mockedContext.SetupGet(x => x.User.Identity).Returns(_mockedIdentity.Object);
            _mockedRepository.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);
            _mockedRepository.Setup(x => x.GetUser(It.IsAny<string>()))
                .Returns(Task.FromResult(foundUserEntity));
            _mockedMapper.Setup(m => m.Map<UserUpdateModel, GameUser>(It.IsAny<UserUpdateModel>(), It.IsAny<GameUser>()))
                .Callback((UserUpdateModel userModel, GameUser userEntity) =>{
                    foundUserEntity.FirstName = userModel.FirstName;
                    foundUserEntity.LastName = userModel.LastName;
                    foundUserEntity.Email = userModel.Email;
                    foundUserEntity.UserName = userModel.Email;
                });
            _mockedRepository.Setup(x => x.UpdateGameUser(It.IsAny<GameUser>()))
                .Returns(Task.FromResult(IdentityResult.Success))
                .Callback((GameUser user) => {
                    updatedUser = user;
                });
            
            IActionResult result = _sut.UpdateUser(id, userUpdateModel);

            _mockedRepository.Verify(
                x => x.GetUser(It.IsAny<string>()), Times.Once);
            _mockedRepository.Verify(
                x => x.UpdateGameUser(It.IsAny<GameUser>()), Times.Once);

            Assert.Equal(userUpdateModel.FirstName, updatedUser.FirstName);
            Assert.Equal(userUpdateModel.LastName, updatedUser.LastName);
            Assert.Equal(userUpdateModel.Email, updatedUser.Email);
            Assert.Equal(userUpdateModel.Email, updatedUser.UserName);

            NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }
        [Theory]
        [InlineData("32c88adb-056a-48b9-a898-632ff09806c1")]
        [InlineData("self")]
        public void NotUpdateUserWhenModelValidAndIdentityError(string id)
        {
            string userId = "32c88adb-056a-48b9-a898-632ff09806c1";
            var userUpdateModel =  new UserUpdateModel
            {
                FirstName = "Jess",
                LastName = "Brown",
                Email = "JessBrown@gmail.com"
            };
            var foundUserEntity = new GameUser
            {
                Id = userId,
                UserName = "coolperson2020@gmail.com",
                FirstName = "Jess",
                LastName = "Scott",
                Email = "coolperson2020@gmail.com"
            };
            _mockedContext.SetupGet(x => x.User.Identity).Returns(_mockedIdentity.Object);
            _mockedRepository.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);
            _mockedRepository.Setup(x => x.GetUser(It.IsAny<string>()))
                .Returns(Task.FromResult(foundUserEntity));
            _mockedMapper.Setup(m => m.Map<UserUpdateModel, GameUser>(It.IsAny<UserUpdateModel>(), It.IsAny<GameUser>()))
                .Callback((UserUpdateModel userModel, GameUser userEntity) =>{
                    foundUserEntity.FirstName = userModel.FirstName;
                    foundUserEntity.LastName = userModel.LastName;
                    foundUserEntity.Email = userModel.Email;
                    foundUserEntity.UserName = userModel.Email;
                });
            _mockedRepository.Setup(x => x.UpdateGameUser(It.IsAny<GameUser>()))
                .Returns(Task.FromResult(IdentityResult.Failed()));
            
            IActionResult result = _sut.UpdateUser(id, userUpdateModel);

            _mockedRepository.Verify(
                x => x.GetUser(It.IsAny<string>()), Times.Once);
            _mockedRepository.Verify(
                x => x.UpdateGameUser(It.IsAny<GameUser>()), Times.Once);

            UnprocessableEntityObjectResult badResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
            Assert.Equal(422, badResult.StatusCode);
        }
        [Theory]
        [InlineData("32c88adb-056a-48b9-a898-632ff09806c1")]
        [InlineData("self")]
        public void NotUpdateUserWhenModelInvalid(string id)
        {
            // string userId = "32c88adb-056a-48b9-a898-632ff09806c1";
            var userUpdateModel =  new UserUpdateModel
            {
                FirstName = "Jess",
                LastName = "Brown",
                Email = ""
            };
            _sut.ModelState.AddModelError("x", "Model Error");
            _mockedRepository.Setup(x => x.UpdateGameUser(It.IsAny<GameUser>()));
            
            IActionResult result = _sut.UpdateUser(id, userUpdateModel);
            
            _mockedRepository.Verify(
                x => x.GetUser(It.IsAny<string>()), Times.Never);
            _mockedRepository.Verify(
                x => x.UpdateGameUser(It.IsAny<GameUser>()), Times.Never);
            
            UnprocessableEntityObjectResult badResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
            Assert.Equal(422, badResult.StatusCode);
        }
        [Theory]
        [InlineData("32c88adb-056a-48b9-a898-632ff09806c1")]
        [InlineData("self")]
        public void NotUpdateUserWhenUserIsNotFound(string id)
        {
            string userId = "32c88adb-056a-48b9-a898-632ff09806c1";
            var userUpdateModel =  new UserUpdateModel
            {
                FirstName = "Jess",
                LastName = "Brown",
                Email = "JessBrown@gmail.com"
            };
            GameUser foundUserEntity = null;
            _mockedContext.SetupGet(x => x.User.Identity).Returns(_mockedIdentity.Object);
            _mockedRepository.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);
            _mockedRepository.Setup(x => x.GetUser(It.IsAny<string>()))
                .Returns(Task.FromResult(foundUserEntity));
            _mockedRepository.Setup(x => x.UpdateGameUser(It.IsAny<GameUser>()));
            
            IActionResult result = _sut.UpdateUser(id, userUpdateModel);

            _mockedRepository.Verify(
                x => x.GetUser(It.IsAny<string>()), Times.Once);
            _mockedRepository.Verify(
                x => x.UpdateGameUser(It.IsAny<GameUser>()), Times.Never);

            NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}