using WebAPI_1.Model;

namespace WebAPI_1.Interface
{
    public interface IJwtService
    {
        string GenerateToken(User user);

    }

}
