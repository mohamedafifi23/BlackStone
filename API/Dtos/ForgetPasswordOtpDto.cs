using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class ForgetPasswordOtpDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
