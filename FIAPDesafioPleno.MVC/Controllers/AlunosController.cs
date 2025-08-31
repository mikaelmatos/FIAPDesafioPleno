using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static FIAPDesafioPleno.Controllers.AdminController;
using System.Net.Http.Headers;
using FIAPDesafioPleno.MVC.Models;
using System.Text.Json;

namespace FIAPDesafioPleno.MVC.Controllers
{
    public class AlunosController : Controller
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

                var response = await client.GetAsync($"{_apiBaseUrl}/api/alunos?busca=" + nome);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    PaginacaoAlunos paginacao = JsonSerializer.Deserialize<PaginacaoAlunos>(json);

                    var alunos = paginacao.items;

                    ViewBag.Alunos = alunos;

                    return View("Index");
                }
                else
                {
                    ViewBag.Erro = "Não foi possível carregar os alunos.";
                    return View("Index", new List<Aluno>());
                }
            }
        }

        // Criar novo aluno
        [HttpPost]
        public async Task<IActionResult> Create(AlunoViewModel aluno)
        {
            using (var client = new HttpClient())
            {
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/alunos", aluno);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Erro"] = "Erro ao criar aluno.";
                    return RedirectToAction("Index");
                }
            }
        }

        // Editar aluno
        [HttpPost]
        public async Task<IActionResult> Edit(Aluno aluno)
        {
            using (var client = new HttpClient())
            {
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/alunos/{aluno.id}", aluno);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Erro"] = "Erro ao editar aluno.";
                    return RedirectToAction("Index");
                }
            }
        }

        // Excluir aluno
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

                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/alunos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Erro"] = "Erro ao excluir aluno.";
                    return RedirectToAction("Index");
                }
            }
        }
    }
}
