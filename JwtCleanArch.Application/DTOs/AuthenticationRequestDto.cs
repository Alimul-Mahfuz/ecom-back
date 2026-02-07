using System.ComponentModel.DataAnnotations;

namespace JwtCleanArch.Application.DTOs
{
    public class AuthenticationRequestDto
    {
        public string? FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
