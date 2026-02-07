using JwtCleanArch.Application.DTOs;

namespace JwtCleanArch.Application.Common
{
    public interface IFileUploadService
    {
        Task<Result<object>> UploadFileAsync(FileUploadDto uploadDto, string relativeFolder, bool isReplace = false);
        Task<Result<bool>> DeleteFileAsync(string relativePath, string relativeFolder);

        string ResolvePath(string relativePath, string relativeFolder);


    }
}
