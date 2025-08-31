namespace FIAPDesafioPleno.MVC.Models
{
    public class MatriculaViewModel
    {
        public int id { get; set; }
        public AlunoAux aluno { get; set; }
        public TurmaAux turma { get; set; }
        public DateTime dataMatricula { get; set; }
    }

    public class AlunoAux
    {
        public int alunoId { get; set; }
        public string nome { get; set; }
        public string email { get; set; }
    }

    public class TurmaAux
    {
        public int turmaId { get; set; }
        public string nome { get; set; }
    }
}

