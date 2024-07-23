using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventManagementApi.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
            InitializeContainers().GetAwaiter().GetResult();
        }

        private async Task InitializeContainers()
        {
            var eventImagesContainer = _blobServiceClient.GetBlobContainerClient("eventimages");
            await eventImagesContainer.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var eventDocumentsContainer = _blobServiceClient.GetBlobContainerClient("eventdocuments");
            await eventDocumentsContainer.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string containerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, true);
            return blobClient.Uri.ToString();
        }

        public async Task<Stream> DownloadFileAsync(string containerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

    }
}