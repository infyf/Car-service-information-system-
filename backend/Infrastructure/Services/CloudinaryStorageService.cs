using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using AutoServ.Core.Interfaces;


namespace AutoServ.Infrastructure.Services
{
    public class CloudinaryStorageService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(IConfiguration config)
        {
            var cloudSettings = config.GetSection("CloudinarySettings");

            var account = new Account(
                cloudSettings["CloudName"],
                cloudSettings["ApiKey"],
                cloudSettings["ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string folder, string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var uniqueName = $"{Guid.NewGuid():N}";

           
            string subFolder = IsVideoExtension(extension) ? "videos" : "images";

            folder = string.IsNullOrWhiteSpace(folder) ? subFolder : $"{folder}/{subFolder}";

            UploadResult result;

            if (IsVideoExtension(extension))
            {
                var uploadParams = new VideoUploadParams()
                {
                    File = new FileDescription(fileName, fileStream),
                    PublicId = uniqueName,
                    Folder = folder,
                    Overwrite = true
                };
                result = await _cloudinary.UploadAsync(uploadParams);
            }
            else
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(fileName, fileStream),
                    PublicId = uniqueName,
                    Folder = folder,
                    Overwrite = true
                };
                result = await _cloudinary.UploadAsync(uploadParams);
            }

            if (result.Error != null)
            {
                throw new Exception($"Cloudinary помилка: {result.Error.Message}");
            }

            return result.SecureUri.ToString();
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                var uri = new Uri(fileUrl);
                string path = uri.AbsolutePath;

               
                ResourceType resourceType;
                if (path.Contains("/video/upload/"))
                {
                    resourceType = ResourceType.Video;
                }
                else
                {
                    resourceType = ResourceType.Image; 
                }

                int uploadIndex = path.IndexOf("/upload/");
                if (uploadIndex > 0)
                {
             
                    string rest = path.Substring(uploadIndex + 8);
                    var segments = rest.Split('/');

                    
                    if (segments.Length >= 2 && segments[0].StartsWith("v"))
                    {
              
                        string pathWithFolder = string.Join("/", segments.Skip(1));

                      
                        string publicId = Path.Combine(
                            Path.GetDirectoryName(pathWithFolder) ?? "",
                            Path.GetFileNameWithoutExtension(pathWithFolder)
                        ).Replace("\\", "/"); 

         
                        var deleteParams = new DelResParams
                        {
                            PublicIds = new List<string> { publicId },
                            ResourceType = resourceType
                        };

                        var destroyResult = await _cloudinary.DeleteResourcesAsync(deleteParams);

                        if (destroyResult.Error != null)
                        {
                            throw new Exception($"Помилка видалення {resourceType}: {destroyResult.Error.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Помилка видалення з Cloudinary: {ex.Message}");
            }
        }

        private bool IsVideoExtension(string extension)
        {
            var ext = extension.ToLowerInvariant();
            return ext == ".mp4" || ext == ".mov" || ext == ".avi" || ext == ".webm";
        }
    }
}