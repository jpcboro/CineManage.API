using CineManage.API.DTOs;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace CineManage.API.Entities
{
    public class MovieTheater : IId
    {
        public int Id { get; set; }
        [Required]
        [StringLength(80)]
        public required string Name { get; set; }
        public required Point Location { get; set; }

    }
}
