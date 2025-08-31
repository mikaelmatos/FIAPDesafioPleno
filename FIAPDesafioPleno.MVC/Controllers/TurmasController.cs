using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using FIAPDesafioPleno.MVC.Models;
using System.Text.Json;
using static FIAPDesafioPleno.Controllers.AdminController;

namespace FIAPDesafioPleno.MVC.Controllers
{
    public partial class TurmasController : Controller
    {
        private readonly string _apiBaseUrl = "https://localhost:7131"; // URL da sua API

        // Recupera token salvo no cookie (igual AlunosController)
        private string? GetAccessToken()
        {
            return User.FindFirst("AccessToken")?.Value;
        }

        // Lista de turmas
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

                var response = await client.GetAsync($"{_apiBaseUrl}/api/turmas");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    PaginacaoTurmas paginacao = JsonSerializer.Deserialize<PaginacaoTurmas>(json);

                    var turmas = paginacao.items;

                    ViewBag.Turmas = turmas;

                    return View();
                }
                else
                {
                    ViewBag.Erro = "Não foi possível carregar as turmas.";
                    return View(new List<Turma>());
                }
            }
        }

        // Buscar por nome
        public async Task<IActionResult> Buscar(string nome)
        {
            using (var client = new HttpClient())
            {
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"{_apiBaseUrl}/api/turmas?busca=" + nome);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    PaginacaoTurmas paginacao = JsonSerializer.Deserialize<PaginacaoTurmas>(json);

                    var turmas = paginacao.items;

                    ViewBag.Turmas = turmas;

                    return View("Index");
                }
                else
                {
                    ViewBag.Erro = "Não foi possível carregar as turmas.";
                    return View("Index", new List<Turma>());
                }
            }
        }

        // Criar nova turma
        [HttpPost]
        public async Task<IActionResult> Create(Turma turma)
        {
            using (var client = new HttpClient())
            {
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/turmas", turma);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Erro"] = "Erro ao criar turma.";
                    return RedirectToAction("Index");
                }
            }
        }

        // Editar turma
        [HttpPost]
        public async Task<IActionResult> Edit(Turma turma)
        {
            using (var client = new HttpClient())
            {
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/turmas/{turma.Id}", turma);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Erro"] = "Erro ao editar turma.";
                    return RedirectToAction("Index");
                }
            }
        }

        // Excluir turma
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/turmas/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Erro"] = "Erro ao excluir turma.";
                    return RedirectToAction("Index");
                }
            }
        }

    }
}
