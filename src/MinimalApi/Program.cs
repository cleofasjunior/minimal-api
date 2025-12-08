using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.Dominio.Enums;
using Microsoft.OpenApi.Models;
using MinimalApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

#region Builder & Configuração de Serviços

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração de Autenticação e Segurança (JWT)
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!);

builder.Services.AddAuthentication(x =>
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

// 2. Configuração de Autorização (Policies)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Adm", policy => policy.RequireRole("Adm"));
    options.AddPolicy("Editor", policy => policy.RequireRole("Editor"));
});

// 3. Banco de Dados
builder.Services.AddDbContext<DbContexto>(options => 
    options.UseInMemoryDatabase("MinimalApiDb"));

// 4. Swagger e OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Insira o token JWT desta maneira: Bearer {seu token}"
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
            new string[] {}
        }
    });
});

var app = builder.Build();

#endregion

#region Pipeline & Middleware

// Configuração do ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ordem vital para segurança
app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Database Seed (Carga Inicial)

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbContexto>();
    
    if (!db.Administradores.Any())
    {
        db.Administradores.Add(new Administrador 
        { 
            Email = "adm@teste.com", 
            Senha = "123456", 
            Perfil = "Adm"
        });
        
        db.SaveChanges();
        Console.WriteLine("Seed: Administrador padrão criado com sucesso!");
    }
}

#endregion

#region Endpoints

    #region Home & Autenticação

    app.MapGet("/", () => Results.Json(new {
        Mensagem = "Bem-vindo à API de Veículos - Minimal API",
        Documentacao = "/swagger"
    }))
    .WithTags("Home");

    app.MapPost("/login", ([FromBody] LoginDTO loginDTO, DbContexto db, IConfiguration config) =>
    {
        var adm = db.Administradores.FirstOrDefault(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);

        if(adm == null)
            return Results.Unauthorized();

        var tokenKey = config["Jwt:Key"] ?? throw new Exception("Chave JWT ausente");
        var token = TokenService.GerarToken(adm, tokenKey);

        return Results.Ok(token);
    })
    .WithTags("Autenticação");

    #endregion

    #region Administradores (Área Restrita)

    app.MapPost("/administradores", ([FromBody] AdministradorDTO admDto, DbContexto db) =>
    {
        if(string.IsNullOrEmpty(admDto.Email) || string.IsNullOrEmpty(admDto.Senha) || 
           !Enum.TryParse<Perfil>(admDto.Perfil, true, out _))
        {
            return Results.BadRequest("Perfil inválido. Use: Adm ou Editor.");
        }

        var adm = new Administrador
        {
            Email = admDto.Email,
            Senha = admDto.Senha,
            Perfil = admDto.Perfil
        };

        db.Administradores.Add(adm);
        db.SaveChanges();

        return Results.Created($"/administradores/{adm.Id}", new 
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    })
    .RequireAuthorization("Adm")
    .WithTags("Administradores");

    app.MapGet("/administradores", (int? pagina, DbContexto db) =>
    {
        int page = pagina ?? 1;
        
        var lista = db.Administradores
            .Skip((page - 1) * 10)
            .Take(10)
            .Select(a => new { Id = a.Id, Email = a.Email, Perfil = a.Perfil })
            .ToList();

        return Results.Ok(lista);
    })
    .RequireAuthorization("Adm")
    .WithTags("Administradores");

    app.MapGet("/administradores/{id}", (int id, DbContexto db) =>
    {
        var adm = db.Administradores.Find(id);

        if(adm == null) return Results.NotFound();

        return Results.Ok(new { Id = adm.Id, Email = adm.Email, Perfil = adm.Perfil });
    })
    .RequireAuthorization("Adm")
    .WithTags("Administradores");

    #endregion

    #region Veículos (CRUD)

    app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDto, DbContexto db) =>
    {
        if(string.IsNullOrEmpty(veiculoDto.Nome) || string.IsNullOrEmpty(veiculoDto.Marca) || veiculoDto.Ano < 1900)
            return Results.BadRequest("Dados inválidos");

        var veiculo = new Veiculo
        {
            Nome = veiculoDto.Nome,
            Marca = veiculoDto.Marca,
            Ano = veiculoDto.Ano
        };

        db.Veiculos.Add(veiculo);
        db.SaveChanges();

        return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
    })
    .RequireAuthorization() // Adm ou Editor
    .WithTags("Veículos");

    app.MapGet("/veiculos", (int? pagina, DbContexto db) =>
    {
        int page = pagina ?? 1;
        var veiculos = db.Veiculos.Skip((page - 1) * 10).Take(10).ToList();
        return Results.Ok(veiculos);
    })
    .RequireAuthorization()
    .WithTags("Veículos");

    app.MapGet("/veiculos/{id}", (int id, DbContexto db) =>
    {
        var veiculo = db.Veiculos.Find(id);

        if(veiculo == null)
            return Results.NotFound();

        return Results.Ok(veiculo);
    })
    .RequireAuthorization()
    .WithTags("Veículos");

    app.MapPut("/veiculos/{id}", (int id, VeiculoDTO veiculoDto, DbContexto db) =>
    {
        var veiculo = db.Veiculos.Find(id);
        if(veiculo == null) return Results.NotFound();

        if(string.IsNullOrEmpty(veiculoDto.Nome) || string.IsNullOrEmpty(veiculoDto.Marca) || veiculoDto.Ano < 1900)
            return Results.BadRequest("Dados inválidos");

        veiculo.Nome = veiculoDto.Nome;
        veiculo.Marca = veiculoDto.Marca;
        veiculo.Ano = veiculoDto.Ano;

        db.Veiculos.Update(veiculo);
        db.SaveChanges();

        return Results.Ok(veiculo);
    })
    .RequireAuthorization()
    .WithTags("Veículos");

    app.MapDelete("/veiculos/{id}", (int id, DbContexto db) =>
    {
        var veiculo = db.Veiculos.Find(id);

        if(veiculo == null)
            return Results.NotFound();

        db.Veiculos.Remove(veiculo);
        db.SaveChanges();

        return Results.NoContent();
    })
    .RequireAuthorization("Adm") // Exclusivo Adm
    .WithTags("Veículos");

    #endregion

#endregion

app.Run();

#region Classe Program (Partial)

public partial class Program { }

#endregion