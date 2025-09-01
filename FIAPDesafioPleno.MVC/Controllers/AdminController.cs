using FIAPDesafioPleno.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace FIAPDesafioPleno.Controllers
{
    [Authorize(Roles = "Administrator")]

    public partial class AdminController : Controller
    {
        private readonly string _apiBaseUrl = "https://localhost:7131";
        private string? GetAccessToken()
        {
            return User.FindFirst("AccessToken")?.Value;
        }
        private int? GetIdLogado()
        {
            return Convert.ToInt32(User.FindFirst("UserId")?.Value);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Erro = TempData["Erro"];
            ViewBag.Sucesso = TempData["Sucesso"];

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
                    ViewBag.IdLogado = GetIdLogado();

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
