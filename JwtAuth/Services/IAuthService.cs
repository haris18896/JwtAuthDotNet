using JwtAuth.Controllers.Data;
using JwtAuth.Entities;
using JwtAuth.Entities.Models;

namespace JwtAuth.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<TokenResponseDto?> RefreshTokenAsync(TokenResponseRequestDto request);
    }
}