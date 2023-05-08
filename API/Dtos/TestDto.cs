using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class TestDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength =5, ErrorMessage ="{0} min len is 5")]
        public string Name { get; set; } 
    }
}
