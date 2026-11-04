using Microsoft.EntityFrameworkCore;
using PizzariaApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Banco MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30))));

builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<PizzariaApi.Models.Pessoa>,
    Microsoft.AspNetCore.Identity.PasswordHasher<PizzariaApi.Models.Pessoa>>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Permissão CORS para o Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var metodo = context.Request.Method;
    var administrativo =
        (path.StartsWithSegments("/api/atendimentos") && !HttpMethods.IsPost(metodo)) ||
        (path.StartsWithSegments("/api/pessoas") && !HttpMethods.IsPost(metodo) &&
            !path.Equals("/api/pessoas/meus-pedidos", StringComparison.OrdinalIgnoreCase) &&
            !path.Equals("/api/pessoas/perfil", StringComparison.OrdinalIgnoreCase)) ||
        path.StartsWithSegments("/api/estoque") ||
        ((path.StartsWithSegments("/api/itens") || path.StartsWithSegments("/api/categorias")) &&
            !HttpMethods.IsGet(metodo));

    if (administrativo)
    {
        var chave = app.Configuration["AdminKey"];
        if (string.IsNullOrWhiteSpace(chave) || !System.Security.Cryptography.CryptographicOperations
            .FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(chave),
                System.Text.Encoding.UTF8.GetBytes(context.Request.Headers["X-Admin-Key"].ToString())))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
    }

    await next();
});
app.UseAuthorization();
app.MapControllers();

app.Run();