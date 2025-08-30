using FIAPDesafioPleno.Data;
using FIAPDesafioPleno.Dtos;
using FIAPDesafioPleno.DTOs;
using FIAPDesafioPleno.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FIAPDesafioPleno.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TurmasController : ControllerBase
    {
        private readonly ApplicationDbContext _ctx;
        public TurmasController(ApplicationDbContext ctx) => _ctx = ctx;

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult<Turma>> CreateTurma([FromBody] TurmaCreateDto dto)
        {
            if (dto.Nome.Length < 3)
                return BadRequest("O nome deve ter pelo menos 3 caracteres.");

            var turma = new Turma
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao
            };

            _ctx.Turmas.Add(turma);
            await _ctx.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = turma.Id }, turma);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _ctx.Turmas.AsNoTracking()
                        .OrderBy(t => t.Nome)
                        .Select(t => new {
                            t.Id,
                            t.Nome,
                            t.Descricao,
                            AlunosCount = t.Matriculas.Count()
                        });

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var turma = await _ctx.Turmas.Include(t => t.Matriculas).ThenInclude(m => m.Aluno).FirstOrDefaultAsync(t => t.Id == id);
            if (turma == null) return NotFound();
            return Ok(new
            {
                turma.Id,
                turma.Nome,
                turma.Descricao,
                Alunos = turma.Matriculas.Select(m => new { m.AlunoId, m.Aluno.Nome, m.DataMatricula })
            });
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TurmaUpdateDto dto)
        {
            var turma = await _ctx.Turmas.FindAsync(id);
            if (turma == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Nome) || dto.Nome.Length < 3)
                return BadRequest("Nome deve ter ao menos 3 caracteres.");

            turma.Nome = dto.Nome;
            turma.Descricao = dto.Descricao;

            await _ctx.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var turma = await _ctx.Turmas.FindAsync(id);
            if (turma == null) return NotFound();
            _ctx.Turmas.Remove(turma);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}
