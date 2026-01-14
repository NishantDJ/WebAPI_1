using WebAPI_1.Data;
using WebAPI_1.Interface;
using BCrypt.Net;


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
                .FirstOrDefault(x => x.Username == username && x.IsActive);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid username");

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid password");

            return _jwtService.GenerateToken(user);
        }
    }

}
