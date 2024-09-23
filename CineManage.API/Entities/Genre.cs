using System.ComponentModel.DataAnnotations;
using CineManage.API.Validations;

namespace CineManage.API.Entities;

public class Genre
{
    public int Id { get; set; }
    [Required(ErrorMessage = "The field {0} is required here.")] 
    [StringLength(maximumLength: 50)]
    [FirstLetterUpperCase]
    public required string Name { get; set; }
    
}