using FIAPDesafioPleno.Models;

namespace FIAPDesafioPleno.Services
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashed, string provided);
        string GenerateJwtToken(Aluno aluno);
    }
}
