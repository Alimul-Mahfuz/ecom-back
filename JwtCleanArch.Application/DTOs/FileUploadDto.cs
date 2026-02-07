namespace JwtCleanArch.Application.DTOs
{
    public sealed class FileUploadDto
    {
        public byte[] Content { get; init; } = default!;
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public long Length { get; init; }
    }
}
