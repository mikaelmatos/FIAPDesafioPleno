using FIAPDesafioPleno.Data;
using FIAPDesafioPleno.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FIAPDesafioPleno.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _ctx;
        private readonly Services.IAuthService _auth;

        public AuthController(ApplicationDbContext ctx, Services.IAuthService auth)
        {
            _ctx = ctx;
            _auth = auth;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var aluno = await _ctx.Alunos.SingleOrDefaultAsync(a => a.Email == dto.Email);
            if (aluno == null) return Unauthorized("Credenciais inválidas.");

            if (!_auth.VerifyPassword(aluno.PasswordHash, dto.Password)) return Unauthorized("Credenciais inválidas.");

            var token = _auth.GenerateJwtToken(aluno);
            return Ok(new { token });
        }
    }
}
