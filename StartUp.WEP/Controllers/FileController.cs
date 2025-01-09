using Microsoft.AspNetCore.Mvc;
using StartUp.BLL.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace StartUp.WEP.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileStorageService _fileStorageService;

        public FileController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        // GET: File/Upload
        public IActionResult Upload()
        {
            return View();
        }

        // POST: File/UploadFiles
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            try
            {
                // Validate that files are selected
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files selected for upload.");
                }

                // Upload files using FileStorageService
                var fileNames = await _fileStorageService.UploadFilesAsync(files);

                // Return success with the file names
                return Ok(new { FileNames = fileNames });
            }
            catch (Exception ex)
            {
                // Return error if exception occurs
                return BadRequest(new { Message = ex.Message });
            }
        }

        // GET: File/Download/{fileName}
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                // Call the DownloadFile method from the service
                var fileStream = await _fileStorageService.DownloadFileAsync(fileName);

                // Return the file for download
                return fileStream;
            }
            catch (Exception ex)
            {
                // Return error if file is not found
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}
