using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static FIAPDesafioPleno.Controllers.AdminController;
using System.Net.Http.Headers;
using FIAPDesafioPleno.MVC.Models;
using System.Text.Json;
using FIAPDesafioPleno.MVC.ViewModel;
using FIAPDesafioPleno.MVC.Util;
using Microsoft.AspNetCore.Authorization;

namespace FIAPDesafioPleno.MVC.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AlunosController : Controller
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
                    TempData["Erro"] = "Não foi possível carregar os alunos.";
                    return View("Index", new List<Aluno>());
                }
            }
        }

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
                    string errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Erro"] = TrataErros.TrataMensagemErro(errorContent);

                    return RedirectToAction("Index");
                }
            }
        }

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
                    string errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Erro"] = TrataErros.TrataMensagemErro(errorContent);

                    return RedirectToAction("Index");
                }
            }
        }

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
