var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- AUTENTICA��O POR COOKIE ---
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";        // P�gina de login
        options.LogoutPath = "/Login/Logout";      // P�gina de logout
        options.AccessDeniedPath = "/Login/Index"; // Opcional: redireciona se acesso negado
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Expira��o do cookie
    });

// Autoriza��o (opcional, mas recomendado)
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

// **IMPORTANTE: autentica��o e autoriza��o**
app.UseAuthentication();  // <- necess�rio
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
