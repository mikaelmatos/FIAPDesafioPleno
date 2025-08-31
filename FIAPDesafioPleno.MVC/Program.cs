var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- AUTENTICAÇÃO POR COOKIE ---
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";        // Página de login
        options.LogoutPath = "/Login/Logout";      // Página de logout
        options.AccessDeniedPath = "/Login/Index"; // Opcional: redireciona se acesso negado
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Expiração do cookie
    });

// Autorização (opcional, mas recomendado)
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// **IMPORTANTE: autenticação e autorização**
app.UseAuthentication();  // <- necessário
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
