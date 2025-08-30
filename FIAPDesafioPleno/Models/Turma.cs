using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FIAPDesafioPleno.Models
{
    public class Turma
    {
        public int Id { get; set; }

        [Required, MinLength(3)]
        public string Nome { get; set; }

        [Required]
        public string Descricao { get; set; }

        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}
