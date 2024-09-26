using AutoMapper;
using CineManage.API.DTOs;
using CineManage.API.Entities;

namespace CineManage.API.Utilities
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            ConfigureGenres();
        }

        private void ConfigureGenres()
        {
            CreateMap<GenreCreationDTO, Genre>();
            CreateMap<Genre, GenreReadDTO>();
        }
    }
}
