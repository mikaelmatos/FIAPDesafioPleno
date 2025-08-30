using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FIAPDesafioPleno.Models
{
    public class Aluno
    {
        public int Id { get; set; }

        [Required, MinLength(3)]
        public string Nome { get; set; }

        [Required]
        public DateTime DataNascimento { get; set; }

        [Required, StringLength(11, MinimumLength = 11)]
        public string CPF { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}
