using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using FIAPDesafioPleno.MVC.Models;

namespace FIAPDesafioPleno.MVC.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
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

            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var idUsuario = GetIdLogado();

            if (idUsuario <= 0)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("Login", "Login");
            }

            var response = await client.GetAsync($"{_apiBaseUrl}/api/alunos/{idUsuario}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var usuario = JsonSerializer.Deserialize<Aluno>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                ViewBag.Aluno = usuario;
                return View();
            }

            TempData["Erro"] = "Não foi possível carregar seus dados.";
            return View(new Aluno());
        }

        [HttpPost]
        public async Task<IActionResult> Atualizar(Aluno usuario)
        {
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Dados inválidos.";
                return RedirectToAction("Index");
            }

            usuario.id = (int)GetIdLogado();

            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/alunos/{usuario.id}", usuario);

            if (response.IsSuccessStatusCode)
            {
                TempData["Sucesso"] = "Dados atualizados com sucesso!";
                return RedirectToAction("Index");
            }

            TempData["Erro"] = "Erro ao atualizar seus dados. (Sem permisão)";
            return RedirectToAction("Index");
        }
    }
}
