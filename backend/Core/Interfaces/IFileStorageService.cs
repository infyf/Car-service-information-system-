namespace AutoServ.Core.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string folder, string fileName);
        Task DeleteFileAsync(string fileUrl);
    }
}