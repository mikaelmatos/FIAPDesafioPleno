using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using FIAPDesafioPleno.MVC.Models;
using static FIAPDesafioPleno.Controllers.AdminController;
using static FIAPDesafioPleno.MVC.Controllers.TurmasController;

namespace FIAPDesafioPleno.MVC.Controllers
{
    public class MatriculasController : Controller
    {
        private readonly string _apiBaseUrl = "https://localhost:7131"; // URL da sua API

        private string? GetAccessToken()
        {
            return User.FindFirst("AccessToken")?.Value;
        }

        // Lista de matrículas
        public async Task<IActionResult> Index()
        {
            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Busca Matrículas
            var response = await client.GetAsync($"{_apiBaseUrl}/api/matriculas");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var matriculas = JsonSerializer.Deserialize<List<MatriculaViewModel>>(json);

                if (matriculas != null)
                    ViewBag.Matriculas = matriculas;
                else
                    ViewBag.Matriculas = new List<MatriculaViewModel>();
            }
            else
            {
                ViewBag.Erro = "Não foi possível carregar as matrículas.";
                ViewBag.Matriculas = new List<MatriculaViewModel>();
            }

            // Buscar alunos e turmas para os combobox
            ViewBag.Alunos = await GetAlunos();
            ViewBag.Turmas = await GetTurmas();

            return View();
        }

        // Método auxiliar para buscar alunos
        private async Task<List<Aluno>> GetAlunos()
        {
            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_apiBaseUrl}/api/alunos");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                PaginacaoAlunos paginacao = JsonSerializer.Deserialize<PaginacaoAlunos>(json);

                if (paginacao != null && paginacao.items != null)
                    return paginacao.items;
            }
            return new List<Aluno>();
        }

        // Método auxiliar para buscar turmas
        private async Task<List<TurmaViewModel>> GetTurmas()
        {
            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_apiBaseUrl}/api/turmas");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var paginacao = JsonSerializer.Deserialize<PaginacaoTurmas>(json);

                if (paginacao != null && paginacao.items != null)
                    return paginacao.items;
            }
            return new List<TurmaViewModel>();
        }

        // Criar matrícula
        [HttpPost]
        public async Task<IActionResult> Create(int AlunoId, int TurmaId)
        {
            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Matricula matricula = new Matricula();
            matricula.AlunoId = AlunoId;
            matricula.TurmaId = TurmaId;
            matricula.DataMatricula = DateTime.Now;

            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/matriculas", matricula);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            TempData["Erro"] = "Erro ao criar matrícula.";
            return RedirectToAction("Index");
        }

        // Editar matrícula
        [HttpPost]
        public async Task<IActionResult> Edit(int id, int AlunoId, int TurmaId)
        {
            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Matricula matricula = new Matricula();
            matricula.AlunoId = AlunoId;
            matricula.TurmaId = TurmaId;
            matricula.DataMatricula = DateTime.Now;

            var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/matriculas/{id}", matricula);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            TempData["Erro"] = "Erro ao editar matrícula.";
            return RedirectToAction("Index");
        }

        // Excluir matrícula
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"{_apiBaseUrl}/api/matriculas/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            TempData["Erro"] = "Erro ao excluir matrícula.";
            return RedirectToAction("Index");
        }
    }
}
