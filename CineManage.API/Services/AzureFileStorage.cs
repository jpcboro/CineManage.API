
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace CineManage.API.Services
{
    public class AzureFileStorage : IFileStorage
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public AzureFileStorage(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("AzureStorageConnection") ?? string.Empty;
        }
        public async Task Delete(string? route, string containerName)
        {
            if (string.IsNullOrEmpty(route))
            {
                return;
            }

            var client = new BlobContainerClient(_connectionString, containerName);

            await client.CreateIfNotExistsAsync();

            var fileName = Path.GetFileName(route);
            var blob = client.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();

        }

        public async Task<string> SaveFile(string containerName, IFormFile file)
        {
            var client = new BlobContainerClient(_connectionString, containerName);

            await client.CreateIfNotExistsAsync();
            client.SetAccessPolicy(PublicAccessType.Blob);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var blob = client.GetBlobClient(fileName);
            var blobHttpHeaders = new BlobHttpHeaders();
            blobHttpHeaders.ContentType = file.ContentType;
            await blob.UploadAsync(file.OpenReadStream(), blobHttpHeaders);
            return blob.Uri.ToString();
        }
    }
}
