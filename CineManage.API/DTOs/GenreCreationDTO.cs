using CineManage.API.Validations;
using System.ComponentModel.DataAnnotations;

namespace CineManage.API.DTOs
{
    public class GenreCreationDTO
    {
        [Required(ErrorMessage = "The field {0} is required here.")]
        [StringLength(maximumLength: 50)]
        [FirstLetterUpperCase]
        public required string Name { get; set; }
    }
}
