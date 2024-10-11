using System.ComponentModel.DataAnnotations;

namespace CineManage.API.DTOs
{
    public class MovieTheaterCreationDTO
    {
        [StringLength(80)] 
        public required string Name { get; set; }

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }
    }
}
