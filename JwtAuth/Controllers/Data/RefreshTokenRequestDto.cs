namespace JwtAuth.Controllers.Data
{
    public class TokenResponseRequestDto
    {
        public Guid UserId { get; set; }
        public required string RefreshToken { get; set; }
    }
}