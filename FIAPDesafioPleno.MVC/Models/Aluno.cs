using System.ComponentModel.DataAnnotations;

namespace FIAPDesafioPleno.MVC.Models
{
    public class Aluno
    {
        public int id { get; set; }
        public string nome { get; set; }
        public DateTime dataNascimento { get; set; }
        public string cpf { get; set; }
        public string email { get; set; }
    }
}
