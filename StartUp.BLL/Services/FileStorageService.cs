using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace StartUp.BLL.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _uploadPath;
        private readonly long _maxFileSize;

        public FileStorageService(IConfiguration configuration)
        {
            _uploadPath = configuration["FileStorage:UploadPath"];
            var maxFileSizeInMB = configuration["FileStorage:MaxFileSizeInMB"];
            _maxFileSize = long.TryParse(maxFileSizeInMB, out var maxFileSize) ? maxFileSize * 1024 * 1024 : 10 * 1024 * 1024; // Default 10MB
        }

        public async Task<List<string>> UploadFilesAsync(IEnumerable<IFormFile> files)
        {
            var fileNames = new List<string>();

            foreach (var file in files)
            {
                if (file.Length == 0)
                    throw new ArgumentException("No files uploaded.");

                if (file.Length > _maxFileSize)
                    throw new ArgumentException($"File size exceeds {_maxFileSize / 1024 / 1024} MB.");

                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), _uploadPath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                // Generate GUID + Original Filename (with extension)
                var originalFileName = Path.GetFileNameWithoutExtension(file.FileName); // Get the original filename without extension
                var fileExtension = Path.GetExtension(file.FileName); // Get the original file extension
                var fileName = Guid.NewGuid().ToString() + "_" + originalFileName + fileExtension; // Combine GUID + original filename + extension

                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                fileNames.Add(fileName); // Add the newly generated file name to the list
            }

            return fileNames;
        }


        public async Task<FileStreamResult> DownloadFileAsync(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), _uploadPath, fileName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.");

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(fileStream, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }
    }

    public interface IFileStorageService
    {
        Task<List<string>> UploadFilesAsync(IEnumerable<IFormFile> files);
        Task<FileStreamResult> DownloadFileAsync(string fileName);
    }
}
