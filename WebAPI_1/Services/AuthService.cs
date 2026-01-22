using BCrypt.Net;
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

       

        public string Login(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.IsActive);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid username");

            // ✅ Correct BCrypt usage
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid password");

            return _jwtService.GenerateToken(user);
        }

        public void Register(RegisterDto dto)
        {
            // 1️⃣ Check if user already exists
            bool exists = _context.Users.Any(u => u.Username == dto.Username);

            if (exists)
                throw new Exception("User already exists");

            // 2️⃣ Hash password (THIS IS THE KEY PART)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3️⃣ Create user entity
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hashedPassword,
                Role = dto.Role,
                IsActive = true
            };

            // 4️⃣ Save to database
            _context.Users.Add(user);
            _context.SaveChanges();
        }

    }
}
