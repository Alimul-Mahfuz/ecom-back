using JwtCleanArch.Application.Common;
using JwtCleanArch.Application.DTOs;

namespace JwtCleanArch.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<object>> UserInfoByIdentityUserIdAsync(string IdentityUserId);
        Task<Result<object>> UserProfileUpdateAsync(UserProfileDto userProfileDto);
        Task<Result<object>> UpdatePasswordAsync(PasswordUpdateDto passwordUpdateDto);

        Task<Result<object>> UploadProfileImage(UserProfileImageUploadDto userProfileImageUploadDto);
    }
}
