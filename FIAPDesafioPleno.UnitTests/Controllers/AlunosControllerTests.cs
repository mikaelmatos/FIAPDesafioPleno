using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.Mvc;
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
            // Arrange
            using var ctx = CreateContext();
            var mockAuth = new Mock<IAuthService>();
            mockAuth.Setup(a => a.HashPassword(It.IsAny<string>()))
                    .Returns("hash123");

            var controller = new AlunosController(ctx, mockAuth.Object);
            var dto = new AlunoCreateDto
            {
                Nome = "Maria",
                CPF = "12345678900",
                Email = "maria@email.com",
                Password = "Senha@123",
                DataNascimento = DateTime.Now.AddYears(-20)
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var created = result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.StatusCode.Should().Be(201);

            var readDto = created.Value as AlunoReadDto;
            readDto.Should().NotBeNull();
            readDto!.Nome.Should().Be("Maria");
        }

        [Fact]
        public async Task GetById_DeveRetornarNotFound_QuandoNaoExiste()
        {
            using var ctx = CreateContext();
            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var result = await controller.GetById(99);

            var notFound = result as NotFoundResult;
            notFound.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_DeveRetornarAluno_QuandoExiste()
        {
            using var ctx = CreateContext();
            var aluno = new Aluno { Nome = "João", CPF = "111", Email = "j@a.com", PasswordHash = "hash" };
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
        public async Task Delete_DeveRetornarBadRequest_QuandoIdIgualA1()
        {
            using var ctx = CreateContext();
            var mockAuth = new Mock<IAuthService>();
            var controller = new AlunosController(ctx, mockAuth.Object);

            var result = await controller.Delete(1);

            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
        }
    }
}
