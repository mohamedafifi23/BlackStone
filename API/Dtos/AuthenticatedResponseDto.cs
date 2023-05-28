namespace API.Dtos
{
    public class AuthenticatedResponseDto
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
