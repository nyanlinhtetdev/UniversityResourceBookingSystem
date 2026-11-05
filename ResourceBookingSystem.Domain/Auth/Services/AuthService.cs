using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ResourceBookingSystem.Database.AppDbContextModels;
using ResourceBookingSystem.Domain.Auth.DTOs;
using ResourceBookingSystem.Domain.Auth.ServiceInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ResourceBookingSystem.Domain.Auth.Services
{
    public class AuthService(AppDbContext dbContext, IConfiguration configuration): IAuthService
    {
        public async Task<User?> Register(RegisterRequestDto registerRequestDto)
        {
            var existingUser = await dbContext.Users.AnyAsync(user => user.Email == registerRequestDto.Email);
            if(existingUser)
            {
                return null;
            }
            User user = new User
            {
                FullName = registerRequestDto.FullName,
                Email = registerRequestDto.Email,
                StudentId = registerRequestDto.StudentId,
                ContactNumber = registerRequestDto.ContactNumber,
                IsActive = true
            };

            var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, registerRequestDto.Password);

            user.PasswordHash = hashedPassword;

            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            return user;
        }

        
        public async Task<LoginServiceResponseDto?> Login(LoginRequestDto request)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(dbUser => dbUser.Email == request.Email);
            if (user is null)
            {
                return null;
            }     

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var accessToken = CreateToken(user);

            var refreshToken = GenerateRefreshToken();
            var tokenHash = HashRefreshToken(refreshToken);

            dbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = tokenHash,
                ExpiredDatetime = DateTime.UtcNow.AddDays(7)
            });

            await dbContext.SaveChangesAsync();

            return new LoginServiceResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    UserId= user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    ContactNumber = user.ContactNumber,
                    StudentId = user.StudentId,
                    IsActive = user.IsActive
                }
            };
        }

        private string CreateToken(User user)
        {
            var tokenValue = configuration["AppSettings:Token"]
                ?? throw new InvalidOperationException(
                    "JWT token key is not configured.");

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(tokenValue));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["AppSettings:Issuer"],
                audience: configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(tokenDescriptor);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes);
        }

        private string HashRefreshToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToBase64String(hashBytes);
        }

        public async Task<LoginServiceResponseDto?> Refresh(string refreshToken)
        {

            var tokenHash = HashRefreshToken(refreshToken);

            RefreshToken? storedToken = await dbContext.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (storedToken == null ||
                storedToken.RevokedDatetime != null ||
                storedToken.ExpiredDatetime <= DateTime.UtcNow)
            {
                return null;
            }

            var user = storedToken.User ?? await dbContext.Users.FindAsync(storedToken.UserId);
            if (user == null) return null;

            storedToken.RevokedDatetime = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            var newRefreshHash = HashRefreshToken(newRefreshToken);

            dbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = newRefreshHash,
                ExpiredDatetime = DateTime.UtcNow.AddDays(7)
            });

            await dbContext.SaveChangesAsync();

            var newAccessToken = CreateToken(user);

            return new LoginServiceResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = new UserDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    ContactNumber = user.ContactNumber,
                    StudentId = user.StudentId,
                    IsActive = user.IsActive
                }
            };
        }

        public async Task<int?> RevokeRefreshToken(string refreshToken)
        {
            RefreshToken? token = await dbContext.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == HashRefreshToken(refreshToken));
            if (token is null)
            {
                return null; 
            }
            token.RevokedDatetime = DateTime.UtcNow;
            return await dbContext.SaveChangesAsync();
        }

    }
}

