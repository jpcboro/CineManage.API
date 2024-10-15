using CineManage.API.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CineManage.API.DTOs
{
    public class MovieCreationDTO
    {
        [Required]
        [StringLength(300)]
        public required string Title { get; set; }
        public string? Trailer { get; set; }
        public DateTime ReleaseDate { get; set; }
        public IFormFile? Poster { get; set; }
        
        [ModelBinder(BinderType = typeof(CustomDataTypeBinder))]
        public List<int>? GenresIds { get; set; }

        [ModelBinder(BinderType = typeof(CustomDataTypeBinder))]
        public List<int>? MovieTheatersIds { get; set; }

        [ModelBinder(BinderType = typeof(CustomDataTypeBinder))]
        public List<MovieActorCreationDTO>? Actors { get; set; }   


    }
}
