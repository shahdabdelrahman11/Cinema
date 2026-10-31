using System;
using System.IO;
using System.Linq;

namespace Cinema.Services
{
    public class ImageService : IImageService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxSize = 5 * 1024 * 1024;

        private readonly IWebHostEnvironment _environment;

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public bool IsValid(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return file.Length <= MaxSize && AllowedExtensions.Contains(extension);
        }

        public string Save(IFormFile file, string folder)
        {
            var folderPath = Path.Combine(_environment.WebRootPath, "images", folder);
            Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return $"/images/{folder}/{fileName}";
        }

        public void Delete(string? imagePath, string folder)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            var fileName = Path.GetFileName(imagePath);

            var folderPath = Path.GetFullPath(
                Path.Combine(_environment.WebRootPath, "images", folder));

            var filePath = Path.GetFullPath(Path.Combine(folderPath, fileName));

            if (filePath.StartsWith(folderPath + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase)
                && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public string SaveFile(IFormFile file)
        {
            var webRoot = _environment.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var folderPath = Path.Combine(webRoot, "images");
            Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = File.Create(filePath);
            file.CopyTo(stream);

            return fileName;
        }

        public void DeleteFile(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var webRoot = _environment.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var filePath = Path.Combine(webRoot, "images", Path.GetFileName(fileName));

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}