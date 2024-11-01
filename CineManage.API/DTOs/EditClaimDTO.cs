using System.ComponentModel.DataAnnotations;

namespace CineManage.API.DTOs;

public class EditClaimDTO
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}