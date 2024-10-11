using AutoFixture;
using AutoMapper;
using CineManage.API.Controllers;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using CineManage.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineManage.API.Tests.Controllers
{
    public class ActorsControllerTests
    {
        private readonly Mock<IOutputCacheStore> _mockOutputCacheStore;
        private readonly ApplicationContext _appContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ActorsController _controller;
        private readonly Mock<IFileStorage> _mockFileStorage;

        public ActorsControllerTests()
        {
            _mockOutputCacheStore = new Mock<IOutputCacheStore>();

            var options = new DbContextOptionsBuilder<ApplicationContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _appContext = new ApplicationContext(options);

            List<Actor> fakeActorsList = new List<Actor>
        {
            new Actor
            {
                Id = 1,
                Name = "Robert Downey Jr.",
                DateOfBirth = new DateTime(1965, 4, 4),
                Picture = "https://example.com/robert.jpg"
            },
            new Actor
            {
                Id = 2,
                Name = "Scarlett Johansson",
                DateOfBirth = new DateTime(1984, 11, 22),
                Picture = "https://example.com/scarlett.jpg"
            },
            new Actor
            {
                Id = 3,
                Name = "Chris Hemsworth",
                DateOfBirth = new DateTime(1983, 8, 11),
                Picture = "https://example.com/chris.jpg"
            }
        };

            _appContext.Actors.AddRange(fakeActorsList);
            _appContext.SaveChanges();

            _mockMapper = new Mock<IMapper>();
            _mockFileStorage = new Mock<IFileStorage>();

            _controller = new ActorsController(_appContext, _mockMapper.Object,
                _mockOutputCacheStore.Object, _mockFileStorage.Object );

            _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

        }

        public void Dispose()
        {
            _appContext.Database.EnsureDeleted();
            _appContext.Dispose();
        }

        [Fact]
        public async Task Get_ShouldReturnActorsList_WhenActorsExist()
        {
            //Arrange
            var pagination = new PaginationDTO()
            {
                PageNumber = 1,
                RecordsPerPage = 10
            };


            //var actorReadDTOs = fixture.CreateMany<ActorReadDTO>();

            _mockMapper.Setup(m => m.ConfigurationProvider)
                .Returns(new MapperConfiguration(g => g.CreateMap<Actor, ActorReadDTO>()));
           

            //Act
            var result = await _controller.Get(pagination);


            //Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Chris Hemsworth", result[0].Name);
            Assert.Equal(3, result[0].Id);
        }

        [Fact]
        public async Task Get_ById_ShouldReturnActor()
        {
            // Arrange
            
            int id = 3;

            _mockMapper.Setup(m => m.ConfigurationProvider)
                .Returns(new MapperConfiguration(g => g.CreateMap<Actor, ActorReadDTO>()));


            // Act

            var result = await _controller.Get(id);

            //Assert

            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ActorReadDTO>>(result);
            var actorReturnVal = Assert.IsType<ActorReadDTO>(actionResult.Value);
            Assert.Equal("Chris Hemsworth", actorReturnVal.Name);
        }

        [Fact]
        public async Task Get_ById_ShouldReturnNotFound_WhenActorDoesNotExist()
        {
            //Arrange

            int id = 10;
            _mockMapper.Setup(m => m.ConfigurationProvider)
                .Returns(new MapperConfiguration(g => g.CreateMap<Actor, ActorReadDTO>()));

            //Act

            var result = await _controller.Get(id);

            //Assert

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Post_Returns_CreatedAtRouteResult_WhenActorHasDetailsAndPicture()
        {
            // Arrange

            var actorCreationDTO = new ActorCreationDTO
            {
                Name = "Mark Ruffalo",
                Picture = new FormFile(Stream.Null, 0, 0, "Picture", "markruffalo.jpg")
            };

            var actor = new Actor { Id = 5, Name = "Mark Ruffalo" };
            var actorReadDto = new ActorReadDTO { Id = 5, Name = "Mark Ruffalo" };
            var pictureUrl = "http://example.com/markruffalo.jpg";

            _mockMapper.Setup(m => m.Map<Actor>(actorCreationDTO)).Returns(actor);
            _mockFileStorage.Setup(f => f.SaveFile(It.IsAny<string>(), It.IsAny<IFormFile>()))
                .ReturnsAsync(pictureUrl);

            _mockMapper.Setup(m => m.Map<ActorReadDTO>(actor)).Returns(actorReadDto);

            //Act

            var result = await _controller.Post(actorCreationDTO);

            //Assert
            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetActorById", createdAtRouteResult.RouteName);
            Assert.Equal("Mark Ruffalo", ((ActorReadDTO)createdAtRouteResult.Value!).Name);
            Assert.Equal(5, ((ActorReadDTO)createdAtRouteResult.Value!).Id);
            _mockFileStorage.Verify(f => f.SaveFile(It.IsAny<string>(), It.IsAny<IFormFile>())
            , Times.Once);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async Task Post_Returns_CreatedAtRouteResult_WhenActorHasDetailsAndWithoutAPicture()
        {
            // Arrange

            var actorCreationDTO = new ActorCreationDTO
            {
                Name = "Mark Ruffalo",
                
            };

            var actor = new Actor { Id = 5, Name = "Mark Ruffalo" };
            var actorReadDto = new ActorReadDTO { Id = 5, Name = "Mark Ruffalo" };

            _mockMapper.Setup(m => m.Map<Actor>(actorCreationDTO)).Returns(actor);
            _mockMapper.Setup(m => m.Map<ActorReadDTO>(actor)).Returns(actorReadDto);

            //Act

            var result = await _controller.Post(actorCreationDTO);

            //Assert
            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetActorById", createdAtRouteResult.RouteName);
            Assert.Equal("Mark Ruffalo", ((ActorReadDTO)createdAtRouteResult.Value!).Name);
            Assert.Equal(5, ((ActorReadDTO)createdAtRouteResult.Value!).Id);
            _mockFileStorage.Verify(f => f.SaveFile(It.IsAny<string>(), It.IsAny<IFormFile>()), 
                Times.Never);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async Task Put_ReturnsNoContent_WhenActorIsUpdated()
        {
            //Arrange
            var actorId = 3;

            var actorCreationDTO = new ActorCreationDTO
            {
                Name = "Chris Hemsworth EDITED",
                Picture = new FormFile(null, 0, 0, null, "chrishemsworthEDITED.jpg")
            };


            _mockMapper.Setup(m => m.Map(It.IsAny<ActorCreationDTO>(), It.IsAny<Actor>())).Callback<ActorCreationDTO, Actor>(
                (dto, a) =>
                {
                    a.Name = dto.Name;
                });


            _mockFileStorage.Setup(m => m.SaveEditedFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormFile>()))
                .ReturnsAsync("chrishemsworthEDITED.jpg");


            //Act
            var result = await _controller.Put(actorId, actorCreationDTO);

            //Assert
            var actorSaved = _appContext.Actors.Find(actorId)?.Name;
            Assert.Equal(actorCreationDTO.Name, actorSaved);
            Assert.IsType<NoContentResult>(result);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_WhenActorIsNotFound()
        {
            //Arrange
            var actorId = 10;
          

            var actorCreationDTO = new ActorCreationDTO
            {
                Name = "Chris Hemsworth EDITED",
                Picture = new FormFile(null, 0, 0, null, "chrishemsworthEDITED.jpg")
            };


            //Act
            var result = await _controller.Put(actorId, actorCreationDTO);

            //Assert
            var noContentResult = Assert.IsType<NotFoundResult>(result);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task Delete_ShouldRemoveActor_ReturnNoContent()
        {
            //Arrange
            int actorId = 1;
            var actor = _appContext.Actors.FirstOrDefault(a => a.Id == actorId);
            _mockFileStorage.Setup(f => f.Delete(actor.Picture, It.IsAny<string>()))
                .Returns(Task.CompletedTask);
           
            //Act
            
            var result = await _controller.Delete(1);

            //Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _appContext.Actors.FindAsync(actorId));
            _mockFileStorage.Verify(f => f.Delete(actor.Picture, It.IsAny<string>()),
                Times.Once);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async Task Delete_ActorDoesNotExist_ReturnNotFound()
        {
            //Arrange
            int actorId = 10;

            //Act
            var result = await _controller.Delete(actorId);

            //Assert
            Assert.IsType<NotFoundResult>(result);

            _mockFileStorage.Verify(f => f.Delete(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), 
                Times.Never);

        }

    }
}
