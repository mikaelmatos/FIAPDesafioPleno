using System;

namespace FIAPDesafioPleno.DTOs
{
    public class AlunoReadDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataNascimento { get; set; }
        public string CPF { get; set; }
        public string Email { get; set; }
    }
}
