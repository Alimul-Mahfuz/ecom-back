using JwtCleanArch.API.Helpers;
using JwtCleanArch.API.Requests;
using JwtCleanArch.Application.Common;
using JwtCleanArch.Application.DTOs;
using JwtCleanArch.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtCleanArch.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("userbyid")]
        public async Task<IActionResult> UserInfoByIdentityId(string userId)
        {
            try
            {
                var result = await _userService.UserInfoByIdentityUserIdAsync(userId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var result = await _userService.UserInfoByIdentityUserIdAsync(userId);

                if (!result.Success)
                    return BadRequest(result);

                var userDto = result.Data as UserProfileDto;

                if (userDto != null && !string.IsNullOrWhiteSpace(userDto.ImageUrl))
                {
                    userDto.ImageUrl = FileUrlHelper.GeneratePublicUrl(
                        userDto.ImageUrl
                    );
                }

                return Ok(Result<UserProfileDto>.SuccessResult(userDto));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile(UserProfileDto userProfileDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            userProfileDto.UserId = userId;
            try
            {
                var result = await _userService.UserProfileUpdateAsync(userProfileDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword(PasswordUpdateDto passwordUpdateDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            passwordUpdateDto.UserId = userId;
            try
            {
                var result = await _userService.UpdatePasswordAsync(passwordUpdateDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("upload-profile-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfileImage(
            [FromForm] UploadProfileImageRequest request)
        {
            var file = request.File;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            var dto = new UserProfileImageUploadDto
            {
                UserId = userId,
                FileUploadDto = new FileUploadDto
                {
                    Content = fileBytes,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length
                }
            };

            var result = await _userService.UploadProfileImage(dto);

            string? fileUrl = null;
            if (result.Data != null)
            {
                fileUrl = FileUrlHelper.GeneratePublicUrl(result.Data.ToString());
            }

            return result.Success ? Ok(result) : BadRequest(result);
        }

    }
}
