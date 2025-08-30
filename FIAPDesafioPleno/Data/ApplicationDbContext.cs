using FIAPDesafioPleno.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FIAPDesafioPleno.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions opts) : base(opts) { }

        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Turma> Turmas { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Aluno>()
                .HasIndex(a => a.CPF)
                .IsUnique();

            builder.Entity<Aluno>()
                .HasIndex(a => a.Email)
                .IsUnique();

            builder.Entity<Turma>()
                .HasIndex(t => t.Nome);

            builder.Entity<Matricula>()
                .HasIndex(m => new { m.AlunoId, m.TurmaId })
                .IsUnique();

            builder.Entity<Matricula>()
                .HasOne(m => m.Aluno)
                .WithMany(a => a.Matriculas)
                .HasForeignKey(m => m.AlunoId);

            builder.Entity<Matricula>()
                .HasOne(m => m.Turma)
                .WithMany(t => t.Matriculas)
                .HasForeignKey(m => m.TurmaId);
        }
    }
}
