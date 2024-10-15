using CineManage.API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CineManage.API.Entities
{
    public class Movie
    {
        public int Id { get; set; }
        [Required]
        [StringLength(300)]
        public required string Title { get; set; }
        public string? Trailer { get; set; }
        public DateTime ReleaseDate { get; set; }
        [Unicode(false)]
        public string? Poster { get; set; }
        public List<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public List<CinemaScreening> CinemaScreenings { get; set; } = new List<CinemaScreening>();
        public List<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
