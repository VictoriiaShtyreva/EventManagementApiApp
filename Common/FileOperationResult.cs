namespace EventManagementApi.Common
{
    public class FileOperationResult
    {
        public bool Success { get; set; }
        public Stream? FileStream { get; set; }
        public string? Url { get; set; }
        public string? ErrorMessage { get; set; }
    }
}