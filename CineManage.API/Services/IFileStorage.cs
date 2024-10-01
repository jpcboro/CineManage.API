namespace CineManage.API.Services
{
    public interface IFileStorage
    {
        Task<string> SaveFile(string container, IFormFile file);
        Task Delete(string? route, string container);

        async Task<string> SaveEditedFile(string? route, string container, IFormFile file)
        {
            await Delete(route, container);
            return await SaveFile(container, file);
        }
    }
}
