namespace API.Dtos
{
    public class UserWithAddressDto
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Phone { get; set; }
        public AddressDto Address { get; set; }
    }
}
