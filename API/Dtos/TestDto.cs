using API.Resources;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class TestDto
    {
        [Required(
            ErrorMessageResourceName = "TestDto_Required",
            ErrorMessageResourceType =typeof(ValidationResource))]
        [Display(Name ="test name")]
        [StringLength(100, MinimumLength =5, ErrorMessage =null,
            ErrorMessageResourceName = "TestDto_StringLength", ErrorMessageResourceType = typeof(ValidationResource))]
        public string Name { get; set; } 
    }
}
