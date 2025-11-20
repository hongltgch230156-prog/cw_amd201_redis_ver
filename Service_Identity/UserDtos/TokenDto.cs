namespace Service_Identity.UserDtos
{
    public class FirebaseVerifyResponseDto
    {
        public string Uid { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime Expiration { get; set; }
    }
}