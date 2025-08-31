using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FIAPDesafioPleno.MVC.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FIAPDesafioPleno.MVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly string _apiBaseUrl = "https://localhost:7131"; // URL da sua API

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // Monta o JSON para o POST
                    var content = new StringContent(
                        JsonSerializer.Serialize(model),
                        Encoding.UTF8,
                        "application/json"
                    );

                    // Chama o endpoint de login
                    var response = await client.PostAsync("/api/auth/login", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Erro"] = "E-mail ou senha inválidos.";
                        return View(model);
                    }

                    // Lê o token da resposta
                    var responseString = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseString);
                    var token = jsonDoc.RootElement.GetProperty("token").GetString();

                    if (string.IsNullOrEmpty(token))
                    {
                        TempData["Erro"] = "Token não recebido.";
                        return View(model);
                    }

                    // Cria as claims do usuário
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Email),
                        new Claim(ClaimTypes.Email, model.Email),
                        new Claim("AccessToken", token) // <--- JWT armazenado como claim
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) // expira em 8h
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
                }

                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro de login: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}
