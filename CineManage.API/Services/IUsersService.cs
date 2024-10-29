namespace CineManage.API.Services;

public interface IUsersService
{
    Task<string?> GetUserId();
}