using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjTask.Controllers;
using ProjTask.Enum;
using ProjTask.Repositores;
using ProjTask.UserDTOs;
using System.Text;

DotNetEnv.Env.Load(); // Carrega variáveis do arquivo .env

var builder = WebApplication.CreateBuilder(args);

// Configuração da Connection String
var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__POSTGRESQL")
                      ?? builder.Configuration.GetConnectionString("PostgreSQL");

// Configuração dos serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuração do Swagger com autenticação JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API de Tarefas", Version = "v1" });

    // Configuração do esquema de segurança JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Registro dos repositórios
builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
builder.Services.AddScoped<ITaskRepository>(_ => new TaskRepository(connectionString));

// Configuração da autenticação JWT
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
             ?? builder.Configuration["Jwt:Key"]
             ?? "MinhaChaveSecretaMuitoForteCom32Chars!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configuração do pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Tarefas v1");

    // Rota padrão para a UI do Swagger
    c.RoutePrefix = string.Empty;

    // Configuração para exibir o botão de Authorize
    c.ConfigObject.AdditionalItems["filter"] = true;
});

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Criação do usuário de teste automático
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

    var testEmail = "test@example.com";
    var testUser = await userRepo.GetUserByEmailAsync(testEmail);

    if (testUser == null)
    {
        await userRepo.CreateUserAsync(new User
        {
            Id = Guid.NewGuid(),
            Name = "Usuário Teste",
            Email = testEmail,
            Password = "123456", // Será hasheado automaticamente no repositório
            Role = RoleType.Admin
        });

        Console.WriteLine($"Usuário de teste criado: {testEmail} / 123456");
    }
}

// Rota pública de teste
app.MapGet("/api/public-test", () =>
{
    return Results.Ok(new
    {
        Message = "Esta é uma rota pública para testes",
        Status = "API está funcionando",
        DateTime = DateTime.UtcNow,
        TestCredentials = new
        {
            Email = "test@example.com",
            Password = "123456",
            Note = "Disponível apenas em ambiente de desenvolvimento"
        }
    });
}).AllowAnonymous().WithTags("Test").WithName("PublicTestEndpoint");

app.Run();