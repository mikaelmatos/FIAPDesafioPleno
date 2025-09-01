using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using FIAPDesafioPleno.MVC.Models;
using System.Text.Json;
using static FIAPDesafioPleno.Controllers.AdminController;
using FIAPDesafioPleno.MVC.Util;
using Microsoft.AspNetCore.Authorization;

namespace FIAPDesafioPleno.MVC.Controllers
{
    [Authorize(Roles = "Administrator")]
    public partial class TurmasController : Controller
    {
        private readonly IConfiguration _configuration;
        public TurmasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private string? GetAccessToken()
        {
            return User.FindFirst("AccessToken")?.Value;
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

                var response = await client.GetAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/turmas");
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

                var response = await client.GetAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/turmas?busca=" + nome);
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

                var response = await client.PostAsJsonAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/turmas", turma);
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

                var response = await client.PutAsJsonAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/turmas/{turma.Id}", turma);
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

                var response = await client.DeleteAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/turmas/{id}");
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
