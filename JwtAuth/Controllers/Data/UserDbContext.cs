

using JwtAuth.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Controllers.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
    }
}