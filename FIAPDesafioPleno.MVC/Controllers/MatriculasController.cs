using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using FIAPDesafioPleno.MVC.Models;
using static FIAPDesafioPleno.Controllers.AdminController;
using static FIAPDesafioPleno.MVC.Controllers.TurmasController;
using FIAPDesafioPleno.MVC.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace FIAPDesafioPleno.MVC.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class MatriculasController : Controller
    {
        private readonly IConfiguration _configuration;
        public MatriculasController(IConfiguration configuration)
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


            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/matriculas");
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

            ViewBag.Alunos = await GetAlunos();
            ViewBag.Turmas = await GetTurmas();

            return View();
        }

        private async Task<List<Aluno>> GetAlunos()
        {
            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/alunos");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                PaginacaoAlunos paginacao = JsonSerializer.Deserialize<PaginacaoAlunos>(json);

                if (paginacao != null && paginacao.items != null)
                    return paginacao.items;
            }
            return new List<Aluno>();
        }

        private async Task<List<TurmaViewModel>> GetTurmas()
        {
            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/turmas");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var paginacao = JsonSerializer.Deserialize<PaginacaoTurmas>(json);

                if (paginacao != null && paginacao.items != null)
                    return paginacao.items;
            }
            return new List<TurmaViewModel>();
        }

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

            var response = await client.PostAsJsonAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/matriculas", matricula);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            TempData["Erro"] = "Erro ao criar matrícula.<br/>Selecione Aluno e Turma";
            return RedirectToAction("Index");
        }

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

            var response = await client.PutAsJsonAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/matriculas/{id}", matricula);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            TempData["Erro"] = "Erro ao editar matrícula.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using var client = new HttpClient();
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/matriculas/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            TempData["Erro"] = "Erro ao excluir matrícula.";
            return RedirectToAction("Index");
        }
    }
}
