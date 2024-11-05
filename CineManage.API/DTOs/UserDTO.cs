namespace CineManage.API.DTOs;

public class UserDTO
{
    public required string Email { get; set; }
    public required bool IsAdmin { get; set; }
}