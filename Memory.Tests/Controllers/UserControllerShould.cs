using System;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Memory.API.Models;
using Memory.API.Controllers;
using Memory.API.Services;

namespace Memory.Tests.Controllers
{
    public class UserConcontrollerShould
    {
        private Mock<IMemoryRepository> _mockedRepository;
        private UserController _sut;

        UserConcontrollerShould()
        {
            _mockedRepository = new Mock<IMemoryRepository>();
            _sut = new UserController(_mockedRepository.Object);
        }
    }
}