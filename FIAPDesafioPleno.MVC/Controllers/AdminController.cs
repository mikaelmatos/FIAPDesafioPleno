using FIAPDesafioPleno.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FIAPDesafioPleno.Controllers
{
    public partial class AdminController : Controller
    {
        private readonly string _apiBaseUrl = "https://localhost:7131"; // URL da sua API

        // Método auxiliar para recuperar o token salvo no Cookie
        private string? GetAccessToken()
        {
            return User.FindFirst("AccessToken")?.Value;
        }

        // Lista de alunos
        public async Task<IActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"{_apiBaseUrl}/api/alunos");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    PaginacaoAlunos paginacao = JsonSerializer.Deserialize<PaginacaoAlunos>(json);

                    var alunos = paginacao.items;

                    ViewBag.Alunos = alunos;

                    return View();
                }
                else
                {
                    ViewBag.Erro = "Não foi possível carregar os alunos.";
                    return View(new List<Aluno>());
                }
            }
        }
    }
}
