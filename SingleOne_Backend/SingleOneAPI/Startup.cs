using DocumentFormat.OpenXml.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SingleOne.Jwt;
using SingleOneAPI;
using SingleOneAPI.DependencyInjection;
using SingleOneAPI.DTOMapping;
using SingleOneAPI.Infra.Contexto;
using SingleOneAPI.Services;
using SingleOneAPI.Services.Interface;
using System;
using System.IO;
using System.Text;
using Wkhtmltopdf.NetCore;
using DotNetEnv;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.FileProviders;

namespace SingleOneAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Carregar arquivo .env se existir
            try
            {
                Env.Load();
                Console.WriteLine("[STARTUP] Arquivo .env carregado com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[STARTUP] Aviso: Não foi possível carregar arquivo .env: {ex.Message}");
            }

            // Configurações de banco de dados
            string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "127.0.0.1";
            string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
            string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "Admin@2025";
            string siteUrl = Environment.GetEnvironmentVariable("SITE_URL") ?? "http://localhost:4200";

            // Criar EnvironmentApiSettings com configurações básicas
            // As configurações SMTP serão carregadas do banco de dados pelo SmtpConfigService
            EnvironmentApiSettings environmentApiSettings = new EnvironmentApiSettings(siteUrl, null, null, null, null, null, false, false);
            environmentApiSettings.DatabaseConfiguration = new DatabaseConfiguration(dbHost, dbUser, dbPassword);
            
            Console.WriteLine($"[STARTUP] Configurações carregadas:");
            Console.WriteLine($"[STARTUP] - DB_HOST: {dbHost}");
            Console.WriteLine($"[STARTUP] - DB_USER: {dbUser}");
            Console.WriteLine($"[STARTUP] - SITE_URL: {siteUrl}");
            Console.WriteLine($"[STARTUP] - SMTP: Será carregado do banco de dados");
            
            // Usar string de conexão das variáveis de ambiente ou fallback para appsettings
            string connectionString;
            if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPassword))
            {
                connectionString = $"Host={environmentApiSettings.DatabaseConfiguration.Host};Database=singleone;Username={environmentApiSettings.DatabaseConfiguration.Username};Password={environmentApiSettings.DatabaseConfiguration.Password};Pooling=true;MinPoolSize=1;MaxPoolSize=100;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Encoding=UTF8;Client Encoding=UTF8;";
            }
            else
            {
                connectionString = Configuration.GetConnectionString("DefaultConnection");
            }
            
            services.AddSingleton(environmentApiSettings);
            services.AddDbContext<SingleOneDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(60);
                    npgsqlOptions.MaxBatchSize(100);
                    npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                // .EnableSensitiveDataLogging(false) // Comentado temporariamente
                .EnableDetailedErrors(false));
            
            //AutoMapper
            services.AddAutoMapper(typeof(ContratoMapping));

            //Serviços da aplicação
            services.AddClassesAntigasDI();
            services.AddCustomRepositories();
            services.AddCustomServices();

            // Serviço de upload de arquivos
            services.AddScoped<IFileUploadService, FileUploadService>();
            
            // Serviço de categorias
            services.AddScoped<ICategoriaService, CategoriaService>();
            
            // Serviço para captura de IP real do cliente
            services.AddScoped<IIpAddressService, IpAddressService>();
            
            // Serviço de Nota Fiscal
            services.AddScoped<INotaFiscalService, NotaFiscalService>();
            
            // 📧 Serviço de Jobs do Hangfire para Campanhas
            services.AddScoped<HangfireJobService>();
            
            // ⏰ Configurar Hangfire com PostgreSQL
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(connectionString, new PostgreSqlStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(15), // Verificar fila a cada 15 segundos
                    JobExpirationCheckInterval = TimeSpan.FromHours(1), // Limpar jobs antigos a cada hora
                    CountersAggregateInterval = TimeSpan.FromMinutes(5), // Agregar contadores a cada 5 minutos
                    PrepareSchemaIfNecessary = false, // Desabilitado temporariamente - tabelas criadas manualmente
                    SchemaName = "hangfire" // Schema separado para tabelas do Hangfire
                })
            );

            // Adicionar servidor Hangfire (processa os jobs em background)
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 5; // Número de workers paralelos
                options.Queues = new[] { "default", "critical" }; // Filas disponíveis
                options.ServerName = "SingleOneServer";
                options.SchedulePollingInterval = TimeSpan.FromSeconds(15); // Verificar agendamentos a cada 15 segundos
            });

            services.AddCors();
            services.AddMvc(o => o.EnableEndpointRouting = true).AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                o.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            });
            //services.AddControllers();
            services.AddControllers().AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            });
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "SingleOne API",
                    Description = "An API for managing SingleOne items",
                    TermsOfService = new Uri("https://singleone.tech/cookies"),
                    Contact = new OpenApiContact
                    {
                        Name = "SingleOne",
                        Url = new Uri("https://singleone.tech/contato")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "SingleOne",
                        Url = new Uri("https://singleone.tech/")
                    }
                });
            });
            services.AddWkhtmltopdf();
            var key = Encoding.ASCII.GetBytes(JwtSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            // Habilitar Swagger também em produção para debug
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SingleOne API v1");
                c.RoutePrefix = "swagger";
            });
            
            // Configurar ForwardedHeaders para capturar IP real quando atrás de proxy/reverse proxy
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                                 Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto | 
                                 Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost,
                RequireHeaderSymmetry = false,
                ForwardLimit = null,
                KnownNetworks = { },
                KnownProxies = { }
            });
            
            // Configurar arquivos estáticos padrão
            app.UseStaticFiles();
            
            // Configurar rota específica para logos via /api/logos/
            app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logos")),
                RequestPath = "/api/logos"
            });
            
            // Middleware para garantir UTF-8 em todas as respostas JSON
            app.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    if (context.Response.ContentType != null && context.Response.ContentType.Contains("application/json"))
                    {
                        context.Response.ContentType = "application/json; charset=utf-8";
                    }
                    return System.Threading.Tasks.Task.CompletedTask;
                });
                await next();
            });
            
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            
            // ⏰ Hangfire Dashboard (acessível em /hangfire)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "SingleOne - Agendamento de Tarefas",
                Authorization = new[] { new HangfireAuthorizationFilter() }, // Filtro customizado para auth
                StatsPollingInterval = 2000, // Atualizar estatísticas a cada 2 segundos
                DisplayStorageConnectionString = false // Não mostrar string de conexão
            });
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
