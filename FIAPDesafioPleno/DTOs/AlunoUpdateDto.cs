using System.ComponentModel.DataAnnotations;
namespace FIAPDesafioPleno.DTOs
{
    public class AlunoUpdateDto
    {
        [Required, MinLength(3)]
        public string Nome { get; set; }

        [Required]
        public DateTime DataNascimento { get; set; }

        [Required, StringLength(11, MinimumLength = 11)]
        public string CPF { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string? Password { get; set; }
        public bool? IsAdmin { get; set; }
    }
}