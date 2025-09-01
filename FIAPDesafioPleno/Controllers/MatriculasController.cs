using FIAPDesafioPleno.Data;
using FIAPDesafioPleno.Dtos;
using FIAPDesafioPleno.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FIAPDesafioPleno.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatriculasController : ControllerBase
    {
        private readonly ApplicationDbContext _ctx;
        public MatriculasController(ApplicationDbContext ctx) => _ctx = ctx;

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult<Matricula>> CreateMatricula([FromBody] MatriculaCreateDto dto)
        {
            var existe = await _ctx.Matriculas
                .AnyAsync(m => m.AlunoId == dto.AlunoId && m.TurmaId == dto.TurmaId);

            if (existe)
                return BadRequest("O aluno já está matriculado nesta turma.");

            var matricula = new Matricula
            {
                AlunoId = dto.AlunoId,
                TurmaId = dto.TurmaId,
                DataMatricula = DateTime.UtcNow
            };

            _ctx.Matriculas.Add(matricula);
            await _ctx.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMatricula), new { id = matricula.Id }, matricula);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var matriculas = await _ctx.Matriculas
                .Include(m => m.Aluno)
                .Include(m => m.Turma)
                .Select(m => new
                {
                    m.Id,
                    Aluno = new { m.AlunoId, m.Aluno.Nome, m.Aluno.Email },
                    Turma = new { m.TurmaId, m.Turma.Nome, m.Turma.Matriculas.Count },
                    m.DataMatricula
                })
                .ToListAsync();

            return Ok(matriculas);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetMatricula(int id)
        {
            var matricula = await _ctx.Matriculas
                .Include(m => m.Aluno)
                .Include(m => m.Turma)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matricula == null)
                return NotFound();

            return Ok(new
            {
                matricula.Id,
                Aluno = new { matricula.AlunoId, matricula.Aluno.Nome, matricula.Aluno.Email },
                Turma = new { matricula.TurmaId, matricula.Turma.Nome },
                matricula.DataMatricula
            });
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("turma/{turmaId:int}")]
        public async Task<IActionResult> GetByTurma(int turmaId)
        {
            var turma = await _ctx.Turmas.FindAsync(turmaId);
            if (turma == null) return NotFound();

            var alunos = await _ctx.Matriculas
                .Where(m => m.TurmaId == turmaId)
                .Include(m => m.Aluno)
                .Select(m => new
                {
                    m.AlunoId,
                    m.Aluno.Nome,
                    m.Aluno.Email,
                    m.DataMatricula
                })
                .OrderBy(a => a.Nome)
                .ToListAsync();

            return Ok(new { turmaId, turma.Nome, alunos });
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MatriculaCreateDto dto)
        {
            var matricula = await _ctx.Matriculas.FindAsync(id);
            if (matricula == null)
                return NotFound();

            var existe = await _ctx.Matriculas
                .AnyAsync(m => m.AlunoId == dto.AlunoId && m.TurmaId == dto.TurmaId && m.Id != id);

            if (existe)
                return BadRequest("O aluno já está matriculado nesta turma.");

            matricula.AlunoId = dto.AlunoId;
            matricula.TurmaId = dto.TurmaId;

            await _ctx.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var matricula = await _ctx.Matriculas.FindAsync(id);
            if (matricula == null)
                return NotFound();

            _ctx.Matriculas.Remove(matricula);
            await _ctx.SaveChangesAsync();

            return NoContent();
        }
    }
}
