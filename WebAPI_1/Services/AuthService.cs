using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using WebAPI_1.Data;
using WebAPI_1.Interface;
using WebAPI_1.Model;

namespace WebAPI_1.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // ================= LOGIN =================
        public AuthResponse? Login(string username, string password)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == username);
            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _context.SaveChanges();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        // ================= REGISTER =================
        public void Register(RegisterDto dto)
        {
            bool exists = _context.Users.Any(u => u.Username == dto.Username);
            if (exists)
                throw new Exception("User already exists");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hashedPassword,
                Role = dto.Role,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        // ================= REFRESH TOKEN =================
        public AuthResponse? RefreshToken(RefreshTokenRequestDto dto)
        {
            var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
            var username = principal.Identity?.Name;

            if (username == null)
                return null;

            var user = _context.Users.SingleOrDefault(x => x.Username == username);

            if (user == null ||
                user.RefreshToken != dto.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            var newAccessToken = _jwtService.GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _context.SaveChanges();

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        // ================= HELPERS =================
        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("THIS_IS_YOUR_SECRET_KEY")
                ),
                ValidateLifetime = false // 🔥 allow expired tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out SecurityToken _
            );

            return principal;
        }
    }
}
