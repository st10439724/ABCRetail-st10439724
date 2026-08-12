using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABCRetail.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var containerName = configuration["AzureStorage:BlobContainerName"] ?? "productimages";

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container with public blob access if it doesn't exist
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        // Uploads an image file and returns the public blob URL
        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            // Use a unique name to avoid overwriting existing blobs
            var blobName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            using var stream = imageFile.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = imageFile.ContentType
            });

            return blobClient.Uri.ToString();
        }

        // Deletes a blob by its full URL
        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var uri = new Uri(imageUrl);
            var blobName = uri.Segments.Last();
            var blobClient = _containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }

        // Returns a list of all blob URLs in the container
        public async Task<List<string>> GetAllImagesAsync()
        {
            var urls = new List<string>();
            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                urls.Add(blobClient.Uri.ToString());
            }
            return urls;
        }
    }
}
