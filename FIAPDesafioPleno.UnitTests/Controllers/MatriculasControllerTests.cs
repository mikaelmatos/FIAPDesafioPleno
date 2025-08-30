using System;
using System.Linq;
using System.Threading.Tasks;
using FIAPDesafioPleno.Controllers;
using FIAPDesafioPleno.Data;
using FIAPDesafioPleno.Dtos;
using FIAPDesafioPleno.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FIAPDesafioPleno.UnitTests.Controllers
{
    public class MatriculasControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private async Task<(Aluno aluno, Turma turma)> SeedAlunoETurma(ApplicationDbContext ctx)
        {
            var aluno = new Aluno { Nome = "João", Email = "joao@email.com" };
            var turma = new Turma { Nome = "Turma Teste", Descricao = "Desc" };
            ctx.Alunos.Add(aluno);
            ctx.Turmas.Add(turma);
            await ctx.SaveChangesAsync();
            return (aluno, turma);
        }

        [Fact]
        public async Task CreateMatricula_DeveRetornarCreated_QuandoValida()
        {
            var ctx = GetDbContext();
            var (aluno, turma) = await SeedAlunoETurma(ctx);

            var controller = new MatriculasController(ctx);
            var dto = new MatriculaCreateDto { AlunoId = aluno.Id, TurmaId = turma.Id };

            var result = await controller.CreateMatricula(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var matricula = Assert.IsType<Matricula>(created.Value);
            Assert.Equal(aluno.Id, matricula.AlunoId);
            Assert.Equal(turma.Id, matricula.TurmaId);
        }

        [Fact]
        public async Task CreateMatricula_DeveRetornarBadRequest_SeDuplicada()
        {
            var ctx = GetDbContext();
            var (aluno, turma) = await SeedAlunoETurma(ctx);

            ctx.Matriculas.Add(new Matricula { AlunoId = aluno.Id, TurmaId = turma.Id, DataMatricula = DateTime.UtcNow });
            await ctx.SaveChangesAsync();

            var controller = new MatriculasController(ctx);
            var dto = new MatriculaCreateDto { AlunoId = aluno.Id, TurmaId = turma.Id };

            var result = await controller.CreateMatricula(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("O aluno já está matriculado nesta turma.", bad.Value);
        }

        [Fact]
        public async Task GetMatricula_DeveRetornarMatricula_SeExistir()
        {
            var ctx = GetDbContext();
            var (aluno, turma) = await SeedAlunoETurma(ctx);

            var matricula = new Matricula { AlunoId = aluno.Id, TurmaId = turma.Id, DataMatricula = DateTime.UtcNow };
            ctx.Matriculas.Add(matricula);
            await ctx.SaveChangesAsync();

            var controller = new MatriculasController(ctx);

            var result = await controller.GetMatricula(matricula.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var obj = ok.Value as dynamic;
            Assert.Equal(aluno.Id, (int)obj.Aluno.AlunoId);
        }

        [Fact]
        public async Task GetMatricula_DeveRetornarNotFound_SeNaoExistir()
        {
            var ctx = GetDbContext();
            var controller = new MatriculasController(ctx);

            var result = await controller.GetMatricula(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_DeveAlterarMatricula_QuandoValida()
        {
            var ctx = GetDbContext();
            var (aluno, turma) = await SeedAlunoETurma(ctx);

            var outroAluno = new Aluno { Nome = "Maria", Email = "maria@email.com" };
            ctx.Alunos.Add(outroAluno);
            await ctx.SaveChangesAsync();

            var matricula = new Matricula { AlunoId = aluno.Id, TurmaId = turma.Id, DataMatricula = DateTime.UtcNow };
            ctx.Matriculas.Add(matricula);
            await ctx.SaveChangesAsync();

            var controller = new MatriculasController(ctx);
            var dto = new MatriculaCreateDto { AlunoId = outroAluno.Id, TurmaId = turma.Id };

            var result = await controller.Update(matricula.Id, dto);

            Assert.IsType<NoContentResult>(result);

            var matriculaDb = await ctx.Matriculas.FindAsync(matricula.Id);
            Assert.Equal(outroAluno.Id, matriculaDb.AlunoId);
        }

        [Fact]
        public async Task Delete_DeveRemoverMatricula_SeExistir()
        {
            var ctx = GetDbContext();
            var (aluno, turma) = await SeedAlunoETurma(ctx);

            var matricula = new Matricula { AlunoId = aluno.Id, TurmaId = turma.Id, DataMatricula = DateTime.UtcNow };
            ctx.Matriculas.Add(matricula);
            await ctx.SaveChangesAsync();

            var controller = new MatriculasController(ctx);

            var result = await controller.Delete(matricula.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(ctx.Matriculas);
        }
    }
}
