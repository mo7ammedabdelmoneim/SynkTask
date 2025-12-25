using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using SynkTask.Models.DTOs;
using SynkTask.API.Services.IService;

namespace SynkTask.API.Services
{
    public class CloudinaryFileStorageService : IFileStorageService
    {
        private readonly Cloudinary cloudinary;

        public CloudinaryFileStorageService(Cloudinary cloudinary)
        {
            this.cloudinary = cloudinary;
        }
        public async Task<FileUploadResult> UploadAsync(IFormFile file, Guid taskId)
        {
            var folder = $"task-attachments/{taskId}";

            if (IsImage(file))
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = folder
                };

                var result = await cloudinary.UploadAsync(uploadParams);

                return new FileUploadResult
                {
                    Url = result.SecureUrl.ToString(),
                    PublicId = result.PublicId,
                    ResourceType = "image"
                };
            }
            else
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = folder
                };

                var result = await cloudinary.UploadAsync(uploadParams);

                return new FileUploadResult
                {
                    Url = result.SecureUrl.ToString(),
                    PublicId = result.PublicId,
                    ResourceType = "raw"
                };
            }
        }
        private bool IsImage(IFormFile file)
        {
            return file.ContentType.StartsWith("image/");
        }

    }
}
