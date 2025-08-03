using JwtAuth.Entities;
using JwtAuth.Entities.Models;

namespace JwtAuth.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(UserDto request);
        Task<string> LoginAsync(UserDto request);
    }
}