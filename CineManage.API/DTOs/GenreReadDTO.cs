using CineManage.API.Validations;
using System.ComponentModel.DataAnnotations;

namespace CineManage.API.DTOs
{
    public class GenreReadDTO : IId
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
