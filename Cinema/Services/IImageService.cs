namespace Cinema.Services
{
    public interface IImageService
    {
        bool IsValid(IFormFile file);
        string Save(IFormFile file, string folder);
        void Delete(string? imagePath, string folder);
        string SaveFile(IFormFile file);
        void DeleteFile(string? fileName);
    }
}