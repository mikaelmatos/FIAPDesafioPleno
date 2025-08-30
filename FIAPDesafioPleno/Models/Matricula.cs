using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIAPDesafioPleno.Models
{
    public class Matricula
    {
        public int Id { get; set; }

        public int AlunoId { get; set; }
        public Aluno Aluno { get; set; }

        public int TurmaId { get; set; }
        public Turma Turma { get; set; }

        public DateTime DataMatricula { get; set; } = DateTime.UtcNow;
    }
}
