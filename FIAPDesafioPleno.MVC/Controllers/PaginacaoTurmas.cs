using FIAPDesafioPleno.MVC.ViewModel;

namespace FIAPDesafioPleno.MVC.Controllers
{
    public partial class TurmasController
    {
        public class PaginacaoTurmas
        {
            public int total { get; set; }
            public int page { get; set; }
            public int pageSize { get; set; }
            public List<TurmaViewModel> items { get; set; }
        }

    }
}
