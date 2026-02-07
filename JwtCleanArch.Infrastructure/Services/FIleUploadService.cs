using JwtCleanArch.Application.Common;
using JwtCleanArch.Application.DTOs;
using Microsoft.Extensions.Configuration;

namespace JwtCleanArch.Infrastructure.Services
{
    public class FIleUploadService : IFileUploadService
    {
        private readonly string _rootPath;
        private readonly string _baseUrl;
        private object? _;

        public FIleUploadService(IConfiguration config)
        {
            _rootPath = Path.Combine(Directory.GetCurrentDirectory(), config["FileStorage:RootPath"]!);
            _baseUrl = config["FileStorage:BaseUrl"] ?? throw new ArgumentNullException(nameof(_baseUrl), "FileStorage:BaseUrl configuration value is missing.");

            Directory.CreateDirectory(_rootPath);
        }

        public Task<Result<bool>> DeleteFileAsync(string relativePath, string relativeFolder)
        {
            try
            {
                relativeFolder = SanitizePath(relativeFolder);
                relativePath = SanitizePath(relativePath);

                var fullPath = Path.Combine(_rootPath, relativeFolder, relativePath);

                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                return Task.FromResult(Result<bool>.SuccessResult(true));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result<bool>.Failure(ex.Message));
            }
        }

        public string ResolvePath(string relativePath, string relativeFolder)
        {
            return Path.Combine(relativeFolder, relativePath);
        }

        public async Task<Result<object>> UploadFileAsync(FileUploadDto uploadDto, string relativeFolder, bool isReplace = false)
        {
            try
            {
                relativeFolder = SanitizePath(relativeFolder);
                var folderPath = Path.Combine(_rootPath, relativeFolder);
                Directory.CreateDirectory(folderPath);

                var safeFileName = Path.GetFileName(uploadDto.FileName);

                var fileName = isReplace ? safeFileName : $"{Guid.NewGuid()}_{safeFileName}";

                var fullPath = Path.Combine(folderPath, fileName);

                await File.WriteAllBytesAsync(fullPath, uploadDto.Content);
                return Result<object>.SuccessResult(fileName);


            }
            catch (Exception ex)
            {
                return Result<object>.Failure(ex.Message);
            }
        }

        private static string SanitizePath(string path)
        {
            return path
                .Replace("..", "")
                .Replace("\\", "")
                .Replace("/", "");
        }




    }
}
