using FIAPDesafioPleno.Data;
using FIAPDesafioPleno.DTOs;
using FIAPDesafioPleno.Models;
using FIAPDesafioPleno.Services;
using FIAPDesafioPleno.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FIAPDesafioPleno.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlunosController : ControllerBase
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IAuthService _auth;

        public AlunosController(ApplicationDbContext ctx, IAuthService auth)
        {
            _ctx = ctx;
            _auth = auth;
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AlunoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!IsStrongPassword(dto.Password))
                return BadRequest("Senha fraca. Deve ter ao menos 8 caracteres, maiúsculas, minúsculas, número e símbolo.");

            var cpf = dto.CPF?.Trim();
            if (await _ctx.Alunos.AnyAsync(a => a.CPF == cpf || a.Email == dto.Email))
                return Conflict("Aluno com mesmo CPF ou e-mail já cadastrado.");

            if (!Validador.ValidaCPF(cpf))
            {
                return Conflict("CPF Inválido");
            }

            var aluno = new Aluno
            {
                Nome = dto.Nome,
                DataNascimento = dto.DataNascimento,
                CPF = cpf,
                Email = dto.Email,
                PasswordHash = _auth.HashPassword(dto.Password)
            };

            _ctx.Alunos.Add(aluno);
            await _ctx.SaveChangesAsync();

            var read = new AlunoReadDto { Id = aluno.Id, Nome = aluno.Nome, DataNascimento = aluno.DataNascimento, CPF = aluno.CPF, Email = aluno.Email };
            return CreatedAtAction(nameof(GetById), new { id = aluno.Id }, read);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var aluno = await _ctx.Alunos.FindAsync(id);
            if (aluno == null) return NotFound();
            return Ok(new AlunoReadDto { Id = aluno.Id, Nome = aluno.Nome, DataNascimento = aluno.DataNascimento, CPF = aluno.CPF, Email = aluno.Email });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string busca = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (pageSize <= 0) pageSize = 10;

            var query = _ctx.Alunos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var t = busca.Trim();
                query = query.Where(a => a.Nome.Contains(t));
            }

            query = query.OrderBy(a => a.Nome);

            var total = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(a => new AlunoReadDto { Id = a.Id, Nome = a.Nome, DataNascimento = a.DataNascimento, CPF = a.CPF, Email = a.Email })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AlunoUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var aluno = await _ctx.Alunos.FindAsync(id);
            if (aluno == null) return NotFound();

            var cpf = dto.CPF.Trim();
            if (await _ctx.Alunos.AnyAsync(a => a.Id != id && (a.CPF == cpf || a.Email == dto.Email)))
                return Conflict("Outro aluno já utiliza esse CPF ou e-mail.");

            if (!Validador.ValidaCPF(cpf))
            {
                return Conflict("CPF Inválido");
            }

            aluno.Nome = dto.Nome;
            aluno.DataNascimento = dto.DataNascimento;
            aluno.CPF = cpf;
            aluno.Email = dto.Email;

            if (dto.IsAdmin.HasValue)
                aluno.IsAdmin = dto.IsAdmin.Value;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (!IsStrongPassword(dto.Password)) return BadRequest("Senha fraca.");
                aluno.PasswordHash = _auth.HashPassword(dto.Password);
            }

            await _ctx.SaveChangesAsync();
            return NoContent();
        }


        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 1)
            {
                return BadRequest("Não pode ser excluído ");
            }

            var aluno = await _ctx.Alunos.FindAsync(id);
            if (aluno == null) return NotFound();
            _ctx.Alunos.Remove(aluno);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("GetUserInfo")]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            var email = User.Identity?.Name;

            return Ok(new { userId, role, email });
        }

        private bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch));
            return hasUpper && hasLower && hasDigit && hasSymbol;
        }
    }
}
