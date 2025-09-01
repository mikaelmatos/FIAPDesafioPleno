using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using FIAPDesafioPleno.Controllers;
using FIAPDesafioPleno.Data;
using FIAPDesafioPleno.DTOs;
using FIAPDesafioPleno.Models;
using FIAPDesafioPleno.Services;

namespace FIAPDesafioPleno.UnitTests.Controllers
{
    public class AlunosControllerTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Create_DeveRetornarCreated_QuandoValido()
        {
            using var ctx = CreateContext();
            var mockAuth = new Mock<IAuthService>();
            mockAuth.Setup(a => a.HashPassword(It.IsAny<string>()))
                .Returns("hash123");

            var controller = new AlunosController(ctx, mockAuth.Object);
            var dto = new AlunoCreateDto
            {
                Nome = "Maria",
                CPF = "06465417308", // válido
                Email = "maria@email.com",
                Password = "Senha@123",
                DataNascimento = DateTime.Now.AddYears(-20)
            };

            var result = await controller.Create(dto);

            var created = result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.StatusCode.Should().Be(201);

            var readDto = created.Value as AlunoReadDto;
            readDto.Should().NotBeNull();
            readDto!.Nome.Should().Be("Maria");
        }

        [Fact]
        public async Task Create_DeveRetornarBadRequest_QuandoSenhaFraca()
        {
            using var ctx = CreateContext();
            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var dto = new AlunoCreateDto
            {
                Nome = "Maria",
                CPF = "06465417308",
                Email = "maria@email.com",
                Password = "123", // fraca
                DataNascimento = DateTime.Now.AddYears(-20)
            };

            var result = await controller.Create(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Create_DeveRetornarConflict_QuandoCpfOuEmailJaExiste()
        {
            using var ctx = CreateContext();
            ctx.Alunos.Add(new Aluno { Nome = "João", CPF = "06465417308", Email = "joao@email.com", PasswordHash = "hash" });
            await ctx.SaveChangesAsync();

            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var dto = new AlunoCreateDto
            {
                Nome = "Maria",
                CPF = "06465417308", // mesmo CPF
                Email = "maria@email.com",
                Password = "Senha@123",
                DataNascimento = DateTime.Now.AddYears(-20)
            };

            var result = await controller.Create(dto);

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Create_DeveRetornarConflict_QuandoCpfInvalido()
        {
            using var ctx = CreateContext();
            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var dto = new AlunoCreateDto
            {
                Nome = "Maria",
                CPF = "12345678900", // inválido
                Email = "maria@email.com",
                Password = "Senha@123",
                DataNascimento = DateTime.Now.AddYears(-20)
            };

            var result = await controller.Create(dto);

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task GetById_DeveRetornarNotFound_QuandoNaoExiste()
        {
            using var ctx = CreateContext();
            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var result = await controller.GetById(99);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_DeveRetornarAluno_QuandoExiste()
        {
            using var ctx = CreateContext();
            var aluno = new Aluno { Nome = "João", CPF = "06465417308", Email = "j@a.com", PasswordHash = "hash" };
            ctx.Alunos.Add(aluno);
            await ctx.SaveChangesAsync();

            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var result = await controller.GetById(aluno.Id);

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var dto = ok!.Value as AlunoReadDto;
            dto!.Nome.Should().Be("João");
        }
                
        [Fact]
        public async Task Update_DeveRetornarNotFound_QuandoNaoExiste()
        {
            using var ctx = CreateContext();
            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var dto = new AlunoUpdateDto { Nome = "Teste", CPF = "06465417308", Email = "t@t.com", DataNascimento = DateTime.Now.AddYears(-30) };
            var result = await controller.Update(99, dto);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_DeveRetornarNoContent_QuandoSucesso()
        {
            using var ctx = CreateContext();
            var aluno = new Aluno { Nome = "Carlos", CPF = "06465417308", Email = "c@c.com", PasswordHash = "hash" };
            ctx.Alunos.Add(aluno);
            await ctx.SaveChangesAsync();

            var mockAuth = new Mock<IAuthService>();
            mockAuth.Setup(a => a.HashPassword(It.IsAny<string>())).Returns("hashNovo");

            var controller = new AlunosController(ctx, mockAuth.Object);

            var dto = new AlunoUpdateDto
            {
                Nome = "Carlos Atualizado",
                CPF = "06465417308",
                Email = "novo@c.com",
                DataNascimento = DateTime.Now.AddYears(-25),
                Password = "Senha@123"
            };

            var result = await controller.Update(aluno.Id, dto);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_DeveRetornarBadRequest_QuandoIdIgualA1()
        {
            using var ctx = CreateContext();
            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var result = await controller.Delete(1);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_DeveRetornarNotFound_QuandoNaoExiste()
        {
            using var ctx = CreateContext();
            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var result = await controller.Delete(99);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_DeveRetornarNoContent_QuandoSucesso()
        {
            using var ctx = CreateContext();

            // Garante que o Id usado não será 1
            ctx.Alunos.Add(new Aluno { Nome = "Fake", CPF = "11111111111", Email = "fake@a.com", PasswordHash = "hash" });
            await ctx.SaveChangesAsync();

            var aluno = new Aluno { Nome = "Apagar", CPF = "06465417308", Email = "a@a.com", PasswordHash = "hash" };
            ctx.Alunos.Add(aluno);
            await ctx.SaveChangesAsync();

            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var result = await controller.Delete(aluno.Id);

            result.Should().BeOfType<NoContentResult>();
        }
    }
}
