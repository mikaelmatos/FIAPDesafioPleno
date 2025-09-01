namespace FIAPDesafioPleno.MVC.ViewModel
{
    public class AlunoViewModel
    {
        public int id { get; set; }
        public string nome { get; set; }
        public DateTime dataNascimento { get; set; }
        public string cpf { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public bool isAdmin { get; set; }
    }
}
