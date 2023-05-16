using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class ForgetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
