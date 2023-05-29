using System.ComponentModel.DataAnnotations;

namespace Core.Entities.Identity
{
    public class AdminAddress
    {
        public int Id { get; set; }

        public string FirstName { get;set; }
        
        public string? MiddleName { get;set; }   

        public string LastName { get;set; }

        public string State { get;set; }    

        public string City { get;set; }

        public string Street { get;set; }

        [Required]
        public string AdminId { get; set; }

        public Admin Admin { get;set; }

    }
}