using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FIAPDesafioPleno.Controllers;
using FIAPDesafioPleno.Data;
using FIAPDesafioPleno.Dtos;
using FIAPDesafioPleno.Models;
using Microsoft.AspNetCore.Http;
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
                .EnableSensitiveDataLogging()
                .Options;

            return new ApplicationDbContext(options);
        }

        private MatriculasController BuildAdminController(ApplicationDbContext ctx)
        {
            var controller = new MatriculasController(ctx);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Administrator")
            }, authenticationType: "Test");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            return controller;
        }

        private string RandomDigits(int length)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            return string.Concat(Enumerable.Range(0, length).Select(_ => rnd.Next(0, 10).ToString()));
        }

        private async Task<Aluno> SeedAluno(ApplicationDbContext ctx, string nome, string? email = null)
        {
            var aluno = new Aluno
            {
                Nome = nome,
                CPF = RandomDigits(11),
                Email = email ?? $"aluno_{Guid.NewGuid():N}@test.com",
                PasswordHash = $"hash_{Guid.NewGuid():N}",
                DataNascimento = DateTime.UtcNow.AddYears(-20)
            };

            ctx.Alunos.Add(aluno);
            await ctx.SaveChangesAsync();
            return aluno;
        }

        private async Task<Turma> SeedTurma(ApplicationDbContext ctx, string nome, string? descricao = null)
        {
            var turma = new Turma
            {
                Nome = nome,
                Descricao = descricao ?? $"Descrição {Guid.NewGuid():N}"
            };

            ctx.Turmas.Add(turma);
            await ctx.SaveChangesAsync();
            return turma;
        }

        private async Task<Matricula> SeedMatricula(ApplicationDbContext ctx, int alunoId, int turmaId)
        {
            var m = new Matricula
            {
                AlunoId = alunoId,
                TurmaId = turmaId,
                DataMatricula = DateTime.UtcNow
            };

            ctx.Matriculas.Add(m);
            await ctx.SaveChangesAsync();
            return m;
        }

        private static T GetProp<T>(object obj, string propName)
        {
            var p = obj.GetType().GetProperty(propName);
            Assert.NotNull(p);
            var value = p!.GetValue(obj);
            return (T)value!;
        }

        private static string GetAnonStringProp(object obj, string propName)
        {
            var p = obj.GetType().GetProperty(propName);
            Assert.NotNull(p);
            var val = p!.GetValue(obj);
            return Assert.IsType<string>(val);
        }


        [Fact]
        public async Task CreateMatricula_DeveRetornarCreated_QuandoAlunoETurmaExistem()
        {
            using var ctx = GetDbContext();
            var aluno = await SeedAluno(ctx, "João");
            var turma = await SeedTurma(ctx, "Turma A");
            var controller = BuildAdminController(ctx);

            var dto = new MatriculaCreateDto { AlunoId = aluno.Id, TurmaId = turma.Id };

            var action = await controller.CreateMatricula(dto);

            var created = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(MatriculasController.GetMatricula), created.ActionName);

            var matricula = Assert.IsType<Matricula>(created.Value);
            Assert.True(matricula.Id > 0);
            Assert.Equal(aluno.Id, matricula.AlunoId);
            Assert.Equal(turma.Id, matricula.TurmaId);

            var saved = await ctx.Matriculas.AsNoTracking().FirstOrDefaultAsync(m => m.Id == matricula.Id);
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task CreateMatricula_DeveRetornarBadRequest_QuandoDuplicada()
        {
            using var ctx = GetDbContext();
            var aluno = await SeedAluno(ctx, "João");
            var turma = await SeedTurma(ctx, "Turma A");
            await SeedMatricula(ctx, aluno.Id, turma.Id); // já existe

            var controller = BuildAdminController(ctx);
            var dto = new MatriculaCreateDto { AlunoId = aluno.Id, TurmaId = turma.Id };

            var action = await controller.CreateMatricula(dto);

            Assert.IsType<BadRequestObjectResult>(action.Result);
        }

        [Fact]
        public async Task GetAll_DeveRetornarOk_ComLista()
        {
            using var ctx = GetDbContext();
            var aluno = await SeedAluno(ctx, "João");
            var turma = await SeedTurma(ctx, "Turma A");
            await SeedMatricula(ctx, aluno.Id, turma.Id);

            var controller = BuildAdminController(ctx);

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var lista = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);
            Assert.NotEmpty(lista);
        }

        [Fact]
        public async Task GetMatricula_DeveRetornarOk_QuandoExiste()
        {
            using var ctx = GetDbContext();
            var aluno = await SeedAluno(ctx, "João");
            var turma = await SeedTurma(ctx, "Turma A");
            var matricula = await SeedMatricula(ctx, aluno.Id, turma.Id);

            var controller = BuildAdminController(ctx);

            var result = await controller.GetMatricula(matricula.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);

            var payload = ok.Value!;
            var id = GetProp<int>(payload, "Id");
            Assert.Equal(matricula.Id, id);
        }

        [Fact]
        public async Task GetMatricula_DeveRetornarNotFound_QuandoNaoExiste()
        {
            using var ctx = GetDbContext();
            var controller = BuildAdminController(ctx);

            var result = await controller.GetMatricula(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetByTurma_DeveRetornarOk_ComAlunosOrdenadosPorNome()
        {
            using var ctx = GetDbContext();
            var turma = await SeedTurma(ctx, "Turma X");

            var a3 = await SeedAluno(ctx, "Zeca");
            var a1 = await SeedAluno(ctx, "Ana");
            var a2 = await SeedAluno(ctx, "Bruno");

            await SeedMatricula(ctx, a3.Id, turma.Id);
            await SeedMatricula(ctx, a1.Id, turma.Id);
            await SeedMatricula(ctx, a2.Id, turma.Id);

            var controller = BuildAdminController(ctx);

            var action = await controller.GetByTurma(turma.Id);
            var ok = Assert.IsType<OkObjectResult>(action);

            var body = ok.Value!;
            var turmaIdRet = GetProp<int>(body, "turmaId");
            Assert.Equal(turma.Id, turmaIdRet);

            var alunosObj = GetProp<IEnumerable<object>>(body, "alunos");
            var nomes = alunosObj.Select(o => GetAnonStringProp(o, "Nome")).ToList();

            Assert.Equal(new[] { "Ana", "Bruno", "Zeca" }, nomes);
        }

        [Fact]
        public async Task GetByTurma_DeveRetornarNotFound_QuandoTurmaNaoExiste()
        {
            using var ctx = GetDbContext();
            var controller = BuildAdminController(ctx);

            var result = await controller.GetByTurma(123456);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_DeveRetornarNoContent_QuandoValida()
        {
            using var ctx = GetDbContext();
            var turma = await SeedTurma(ctx, "Turma A");
            var aluno1 = await SeedAluno(ctx, "João");
            var aluno2 = await SeedAluno(ctx, "Maria");

            var matricula = await SeedMatricula(ctx, aluno1.Id, turma.Id);

            var controller = BuildAdminController(ctx);
            var dto = new MatriculaCreateDto { AlunoId = aluno2.Id, TurmaId = turma.Id };

            var result = await controller.Update(matricula.Id, dto);

            Assert.IsType<NoContentResult>(result);

            var saved = await ctx.Matriculas.AsNoTracking().FirstAsync(m => m.Id == matricula.Id);
            Assert.Equal(aluno2.Id, saved.AlunoId);
            Assert.Equal(turma.Id, saved.TurmaId);
        }

        [Fact]
        public async Task Update_DeveRetornarNotFound_QuandoNaoExiste()
        {
            using var ctx = GetDbContext();
            var turma = await SeedTurma(ctx, "Turma A");
            var aluno = await SeedAluno(ctx, "João");

            var controller = BuildAdminController(ctx);
            var dto = new MatriculaCreateDto { AlunoId = aluno.Id, TurmaId = turma.Id };

            var result = await controller.Update(9999, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_DeveRetornarBadRequest_QuandoDuplicada()
        {
            using var ctx = GetDbContext();
            var turma1 = await SeedTurma(ctx, "Turma A");
            var turma2 = await SeedTurma(ctx, "Turma B");
            var aluno1 = await SeedAluno(ctx, "João");
            var aluno2 = await SeedAluno(ctx, "Maria");

            var m1 = await SeedMatricula(ctx, aluno1.Id, turma1.Id);
            var m2 = await SeedMatricula(ctx, aluno2.Id, turma2.Id);

            var controller = BuildAdminController(ctx);
            var dto = new MatriculaCreateDto { AlunoId = aluno1.Id, TurmaId = turma1.Id };

            var result = await controller.Update(m2.Id, dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Delete_DeveRetornarNoContent_QuandoExiste()
        {
            using var ctx = GetDbContext();
            var turma = await SeedTurma(ctx, "Turma A");
            var aluno = await SeedAluno(ctx, "João");
            var matricula = await SeedMatricula(ctx, aluno.Id, turma.Id);

            var controller = BuildAdminController(ctx);

            var result = await controller.Delete(matricula.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(ctx.Matriculas);
        }

        [Fact]
        public async Task Delete_DeveRetornarNotFound_QuandoNaoExiste()
        {
            using var ctx = GetDbContext();
            var controller = BuildAdminController(ctx);

            var result = await controller.Delete(8888);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
