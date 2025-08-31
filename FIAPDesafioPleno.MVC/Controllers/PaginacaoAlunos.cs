using FIAPDesafioPleno.MVC.Models;

namespace FIAPDesafioPleno.Controllers
{
    public partial class AdminController
    {
        public class PaginacaoAlunos
        {
            public int total { get; set; }
            public int page { get; set; }
            public int pageSize { get; set; }
            public List<Aluno> items { get; set; }
        }

    }
}
