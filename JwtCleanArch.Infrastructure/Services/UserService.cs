using JwtCleanArch.Application.Common;
using JwtCleanArch.Application.DTOs;
using JwtCleanArch.Application.Interfaces;
using JwtCleanArch.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtCleanArch.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IFileUploadService _fileUploadService;

        public UserService(ApplicationDbContext context, UserManager<IdentityUser> userManager, IFileUploadService fileUploadService)
        {
            _context = context;
            _userManager = userManager;
            _fileUploadService = fileUploadService;
        }

        public async Task<Result<object>> UpdatePasswordAsync(PasswordUpdateDto passwordUpdateDto)
        {
            var user = await _userManager.FindByIdAsync(passwordUpdateDto.UserId);
            if (user == null)
            {
                return Result<object>.Failure("Invalid user id");
            }

            if (passwordUpdateDto.Password != passwordUpdateDto.PasswordConfirmation)
            {
                return Result<object>.Failure("Retyped password mismatch");
            }

            var result = await _userManager.ChangePasswordAsync(user, passwordUpdateDto.CurrentPassword, passwordUpdateDto.Password);

            if (!result.Succeeded)
            {
                return Result<object>.Failure("Failed to update password");
            }

            return Result<object>.SuccessResult("Password updated successfully");
        }


        public async Task<Result<object>> UploadProfileImage(UserProfileImageUploadDto userProfileImageUploadDto)
        {
            var userId = userProfileImageUploadDto.UserId;


            var applicationUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.IdentityUserId == userId);
            if (applicationUser == null)
            {
                return Result<object>.Failure("invalid user");
            }

            if (applicationUser.ProfileImagePath != null)
            {
                await _fileUploadService.DeleteFileAsync(applicationUser.ProfileImagePath, "UserProfiles");
            }


            var uploadPath = await _fileUploadService.UploadFileAsync(userProfileImageUploadDto.FileUploadDto, "UserProfiles");

            applicationUser.ProfileImagePath = uploadPath.Data as string;
            await _context.SaveChangesAsync();

            return Result<object>.SuccessResult(uploadPath.Data, "Image saved successfully");

        }

        public async Task<Result<object>> UserInfoByIdentityUserIdAsync(string IdentityUserId)
        {
            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(x => x.IdentityUserId == IdentityUserId);

            if (user == null)
                return Result<object>.Failure("Invalid user");

            var userDto = new UserProfileDto();

            userDto.FullName = user.FullName;
            userDto.Email = user.Email;
            userDto.UserId = IdentityUserId;
            if (user.ProfileImagePath != null)
            {
                userDto.ImageUrl = _fileUploadService.ResolvePath(user.ProfileImagePath, "UserProfiles");

            }

            return Result<object>.SuccessResult(userDto);
        }

        public async Task<Result<object>> UserProfileUpdateAsync(UserProfileDto userProfileDto)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.IdentityUserId == userProfileDto.UserId);

            if (user == null)
            {
                return Result<object>.Failure("Invalid User name");
            }

            user.FullName = userProfileDto.FullName;
            user.Email = userProfileDto.Email;

            var identityUser = await _userManager.FindByIdAsync(userProfileDto.UserId);
            if (identityUser == null)
            {
                return Result<object>.Failure("Invalid identity");
            }

            identityUser.Email = userProfileDto.Email;
            identityUser.UserName = userProfileDto.UserName;

            var result = await _userManager.UpdateAsync(identityUser);

            if (!result.Succeeded)
            {
                return Result<object>.Failure(result.Errors.Select(e => e.Description).ToArray());
            }

            await _context.SaveChangesAsync();

            return Result<object>.SuccessResult("User updated successfully");
        }
    }
}
