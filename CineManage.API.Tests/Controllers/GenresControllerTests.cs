using AutoFixture;
using AutoMapper;
using CineManage.API.Controllers;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
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
    public class GenresControllerTests
    {
        private readonly Mock<IOutputCacheStore> _mockOutputCacheStore;
        private readonly ApplicationContext _appContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GenresController _controller;

        public GenresControllerTests()
        {
            _mockOutputCacheStore = new Mock<IOutputCacheStore>();

            var options = new DbContextOptionsBuilder<ApplicationContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _appContext = new ApplicationContext(options);

            var fakeGenreList = new List<Genre>()
            {
                new Genre
                {
                    Id = 1,
                    Name = "Action"
                },
                new Genre
                {
                    Id = 2,
                    Name = "Horror"
                },
                new Genre
                {
                    Id = 3,
                    Name = "Sci-Fi"
                },

            };

            _appContext.Genres.AddRange(fakeGenreList);
            _appContext.SaveChanges();

            _mockMapper = new Mock<IMapper>();

            _controller = new GenresController(_mockOutputCacheStore.Object,
                _appContext,
                _mockMapper.Object);

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
        public async Task Get_ShouldReturnGenresList_WhenGenresExist()
        {
            //Arrange
            var pagination = new PaginationDTO()
            {
                PageNumber = 1,
                RecordsPerPage = 10
            };

            var fixture = new Fixture()
                .Customize(new MultipleCustomization());

            var genreReadDTOs = fixture.CreateMany<GenreReadDTO>();

            _mockMapper.Setup(m => m.ConfigurationProvider)
                .Returns(new MapperConfiguration(g => g.CreateMap<Genre, GenreReadDTO>()));

            //Act
            var result = await _controller.Get(pagination);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Action", result[0].Name);
        }

        [Fact]
        public async Task Post_ShouldCreateGenreAndReturnCreatedAtRouteWithGenre()
        {
            //Arrange

            var genreCreationDTO = new GenreCreationDTO { Name = "Adventure" };
            var genre = new Genre { Id = 5, Name = "Adventure" };
            var genreReadDTO = new GenreReadDTO { Id = 5, Name = "Adventure" };

            _mockMapper.Setup(m => m.Map<Genre>(genreCreationDTO)).Returns(genre);
           
            _mockMapper.Setup(m => m.Map<GenreReadDTO>(genre)).Returns(genreReadDTO);

            //Act
            var result = await _controller.Post(genreCreationDTO);

            //Assert
            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetGenreById", createdAtRouteResult.RouteName);
            Assert.Equal("Adventure", ((GenreReadDTO)createdAtRouteResult.Value!).Name);
            Assert.Equal(5, ((GenreReadDTO)createdAtRouteResult.Value).Id);

            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async Task Put_ShouldUpdateGenreAndReturnNoContent()
        {
            //Arrange

            var genreCreationDTO = new GenreCreationDTO { Name = "Action EDITED" };
            var genre = new Genre { Id = 1, Name = "Action EDITED" };

            _mockMapper.Setup(m => m.Map<Genre>(genreCreationDTO)).Returns(genre);

            var trackedGenre = _appContext.Genres.Find(1);
            _appContext.Entry(trackedGenre).State = EntityState.Detached;

            //Act
            var result = await _controller.Put(1, genreCreationDTO);

            //Assert
            Assert.IsType<NoContentResult>(result);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Once);

        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenGenreDoesNotExist()
        {
            //Arrange
            var genreCreationDTO = new GenreCreationDTO { Name = "Drama" };

            //Act
            var result = await _controller.Put(10, genreCreationDTO);

            //Assert

            Assert.IsType<NotFoundResult>(result);

            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Never);


        }

        [Fact]
        public async Task Delete_ShouldRemoveGenreAndReturnNoContent()
        {
            //Act
            var result = await _controller.Delete(1);

            //Assert
            Assert.IsType<NoContentResult>(result);

            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Once);

        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNoRecordIsDeleted()
        {
            //Act
            var result = await _controller.Delete(10);

            //Assert
            Assert.IsType<NotFoundResult>(result);

            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Never);
        }

    }
}
