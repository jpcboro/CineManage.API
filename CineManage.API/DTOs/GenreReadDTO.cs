using CineManage.API.Validations;
using System.ComponentModel.DataAnnotations;

namespace CineManage.API.DTOs
{
    public class GenreReadDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
