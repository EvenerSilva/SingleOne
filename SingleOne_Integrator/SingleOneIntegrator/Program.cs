using SingleOneIntegrator;
using SingleOneIntegrator.Middleware;
using SingleOneIntegrator.Options;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços customizados
builder.Services.AddCustomServices(builder.Configuration);

// Adicionar suporte a controllers
builder.Services.AddControllers();

// Adicionar Memory Cache
builder.Services.AddMemoryCache();

// Adicionar Worker Service (mantém funcionalidade original)
builder.Services.AddHostedService<Worker>();

// Adicionar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SingleOne Integrator API",
        Version = "v1",
        Description = "API para integração de folha de pagamento com o SingleOne",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Suporte SingleOne",
            Email = "suporte@singleone.com.br"
        }
    });

    // Incluir comentários XML se existirem
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure o pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SingleOne Integrator API v1");
        options.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

app.UseCors("AllowAll");

// Middleware de autenticação HMAC (aplicado em /api/integracao/*)
app.UseMiddleware<HmacAuthenticationMiddleware>();

app.UseRouting();

app.MapControllers();

// Log de inicialização
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== SingleOne Integrator Iniciado ===");
logger.LogInformation("Worker Service: ATIVO (leitura de VIEW)");
logger.LogInformation("Web API: ATIVA (integração via API)");
logger.LogInformation("Swagger UI: http://localhost:5000");
logger.LogInformation("====================================");

app.Run();
