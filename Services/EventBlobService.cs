using EventManagementApi.Common;

namespace EventManagementApi.Services
{
    public class EventBlobService
    {
        private readonly BlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;

        public EventBlobService(BlobStorageService blobStorageService, IConfiguration configuration)
        {
            _blobStorageService = blobStorageService;
            _configuration = configuration;
        }

        public async Task<FileOperationResult> UploadEventImageAsync(Stream fileStream, string fileName)
        {
            try
            {
                var containerName = _configuration["BlobStorage:EventImagesContainer"];
                var url = await _blobStorageService.UploadFileAsync(fileStream, containerName!, fileName);
                return new FileOperationResult { Success = true, Url = url };
            }
            catch (Exception ex)
            {
                return new FileOperationResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<FileOperationResult> UploadEventDocumentAsync(Stream fileStream, string fileName)
        {
            try
            {
                var containerName = _configuration["BlobStorage:EventDocumentsContainer"];
                var url = await _blobStorageService.UploadFileAsync(fileStream, containerName!, fileName);
                return new FileOperationResult { Success = true, Url = url };
            }
            catch (Exception ex)
            {
                return new FileOperationResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<FileOperationResult> DownloadEventImageAsync(string fileName)
        {
            try
            {
                var containerName = _configuration["BlobStorage:EventImagesContainer"];
                var stream = await _blobStorageService.DownloadFileAsync(containerName!, fileName);
                return new FileOperationResult { Success = true, FileStream = stream };
            }
            catch (Exception ex)
            {
                return new FileOperationResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<FileOperationResult> DownloadEventDocumentAsync(string fileName)
        {
            try
            {
                var containerName = _configuration["BlobStorage:EventDocumentsContainer"];
                var stream = await _blobStorageService.DownloadFileAsync(containerName!, fileName);
                return new FileOperationResult { Success = true, FileStream = stream };
            }
            catch (Exception ex)
            {
                return new FileOperationResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}