using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class ConfirmEmailOtpDto
    {
        [EmailAddress]
        public string Email { get; set; }

        public string Otp { get; set; }
    }
}
