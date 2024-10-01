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
            ConfigureActors();
        }

        private void ConfigureActors()
        {
            CreateMap<ActorCreationDTO, Actor>()
                .ForMember(a => a.Picture, options => options.Ignore());
            CreateMap<Actor, ActorReadDTO>();
        }

        private void ConfigureGenres()
        {
            CreateMap<GenreCreationDTO, Genre>();
            CreateMap<Genre, GenreReadDTO>();
        }
    }
}
