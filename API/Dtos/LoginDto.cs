using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "The password must meet the requirements.")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "password must be at least 8 characters.")]
        public string Password { get; set; }
    }
}
