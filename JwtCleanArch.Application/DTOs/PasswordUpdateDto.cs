using System.ComponentModel.DataAnnotations;

namespace JwtCleanArch.Application.DTOs
{
    public class PasswordUpdateDto
    {
        public string UserId { get; set; }
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordConfirmation { get; set; }
    }

}
