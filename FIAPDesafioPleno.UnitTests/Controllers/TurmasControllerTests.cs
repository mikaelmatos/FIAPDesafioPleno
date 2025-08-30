using System;
using System.Linq;
using System.Threading.Tasks;
using FIAPDesafioPleno.Controllers;
using FIAPDesafioPleno.Data;
using FIAPDesafioPleno.Dtos;
using FIAPDesafioPleno.DTOs;
using FIAPDesafioPleno.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FIAPDesafioPleno.UnitTests.Controllers
{
    public class TurmasControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Banco em memória novo a cada teste
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateTurma_DeveRetornarCreated_WhenDadosValidos()
        {
            // Arrange
            var ctx = GetDbContext();
            var controller = new TurmasController(ctx);

            var dto = new TurmaCreateDto { Nome = "Turma A", Descricao = "Primeira turma" };

            // Act
            var result = await controller.CreateTurma(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var turma = Assert.IsType<Turma>(created.Value);
            Assert.Equal("Turma A", turma.Nome);
            Assert.Equal("Primeira turma", turma.Descricao);
        }

        [Fact]
        public async Task CreateTurma_DeveRetornarBadRequest_SeNomeMuitoCurto()
        {
            var ctx = GetDbContext();
            var controller = new TurmasController(ctx);

            var dto = new TurmaCreateDto { Nome = "AB", Descricao = "Teste" };

            var result = await controller.CreateTurma(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("O nome deve ter pelo menos 3 caracteres.", bad.Value);
        }

        [Fact]
        public async Task GetById_DeveRetornarTurma_SeExistir()
        {
            var ctx = GetDbContext();
            var turma = new Turma { Nome = "Turma X", Descricao = "Teste" };
            ctx.Turmas.Add(turma);
            await ctx.SaveChangesAsync();

            var controller = new TurmasController(ctx);

            var result = await controller.GetById(turma.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var obj = ok.Value as dynamic;
            Assert.Equal("Turma X", (string)obj.Nome);
        }

        [Fact]
        public async Task GetById_DeveRetornarNotFound_SeNaoExistir()
        {
            var ctx = GetDbContext();
            var controller = new TurmasController(ctx);

            var result = await controller.GetById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_DeveAlterarTurma_QuandoValido()
        {
            var ctx = GetDbContext();
            var turma = new Turma { Nome = "Turma Antiga", Descricao = "Teste" };
            ctx.Turmas.Add(turma);
            await ctx.SaveChangesAsync();

            var controller = new TurmasController(ctx);
            var dto = new TurmaUpdateDto { Nome = "Turma Nova", Descricao = "Atualizada" };

            var result = await controller.Update(turma.Id, dto);

            Assert.IsType<NoContentResult>(result);

            var turmaDb = await ctx.Turmas.FindAsync(turma.Id);
            Assert.Equal("Turma Nova", turmaDb.Nome);
        }

        [Fact]
        public async Task Delete_DeveRemoverTurma_SeExistir()
        {
            var ctx = GetDbContext();
            var turma = new Turma { Nome = "Turma X", Descricao = "Teste" };
            ctx.Turmas.Add(turma);
            await ctx.SaveChangesAsync();

            var controller = new TurmasController(ctx);

            var result = await controller.Delete(turma.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(ctx.Turmas);
        }
    }
}
