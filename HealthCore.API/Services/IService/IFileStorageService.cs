using Microsoft.AspNetCore.Http;
using SynkTask.Models.DTOs;

namespace SynkTask.API.Services.IService
{
    public interface IFileStorageService
    {
        Task<FileUploadResult> UploadAsync(IFormFile file, Guid taskId);
    }

}
