// ДЛЯ ЛОКАЛЬНОГО ТЕСТУВАННЯ

using System.IO;
using AutoServ.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AutoServ.Infrastructure.Services
{
    public class LocalStorageService : IFileStorageService
    {
        private readonly string _basePath;

        public LocalStorageService(IConfiguration config)
        {
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string folder, string fileName)
        {
            var folderPath = Path.Combine(_basePath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

           
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".jpg";
            }

         
            var uniqueName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, uniqueName);

            using (var fileStreamOnDisk = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOnDisk);
            }

            return $"/uploads/{folder}/{uniqueName}";
        }

        public Task DeleteFileAsync(string fileUrl)
        {
            var relativePath = fileUrl.TrimStart('/');
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
    }
}