namespace EasyOrderCs.Services.Interfaces;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(IFormFile file, string folder = "");
    Task DeleteFileAsync(string fileUrl);
    string GetFileUrl(string fileName);
}

