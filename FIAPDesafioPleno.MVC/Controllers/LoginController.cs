using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FIAPDesafioPleno.MVC.ViewModel;
using FIAPDesafioPleno.MVC.Util;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using static FIAPDesafioPleno.MVC.Controllers.LoginController;

namespace FIAPDesafioPleno.MVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index(string ReturnUrl = null)
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);

                    var content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(model),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await client.PostAsync("/api/auth/login", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Erro"] = "E-mail ou senha inválidos.";
                        return RedirectToAction("Index", "Home");
                    }

                    var responseString = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseString);
                    var token = jsonDoc.RootElement.GetProperty("token").GetString();

                    if (string.IsNullOrEmpty(token))
                    {
                        TempData["Erro"] = "Token não recebido.";
                        return RedirectToAction("Index", "Home");
                    }

                    Usuario usuario = await ObterInformacoesLogado(token);

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Email),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim("AccessToken", token),
                new Claim("UserId", usuario.userId),
                new Claim(ClaimTypes.Role, usuario.role)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
                }

                if (User.FindFirst(ClaimTypes.Role)?.Value == "Administrator")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Usuario");
                }
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro de login: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task<Usuario> ObterInformacoesLogado(string token)
        {
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"{_configuration["ApiSettings:BaseUrl"]}/api/alunos/GetUserInfo");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    Usuario usuario = JsonConvert.DeserializeObject<Usuario>(json);

                    return usuario;
                }
                else
                {
                    return null;
                }
            }
        }

        public class Usuario
        {
            public string userId { get; set; }
            public string role { get; set; }
            public string email { get; set; }
        }


    }
}
