namespace JwtCleanArch.API.Requests
{
    public class UploadProfileImageRequest
    {
        public IFormFile? File { get; set; } = default;
    }
}
