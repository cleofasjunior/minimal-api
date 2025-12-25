using Microsoft.AspNetCore.Mvc;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enums;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.Services;

namespace MinimalApi.Extensoes;

public static class EndpointExtensao
{
    public static void MapearEndpoints(this WebApplication app)
    {
        app.MapearEndpointsHome();
        app.MapearEndpointsAutenticacao();
        app.MapearEndpointsAdministradores();
        app.MapearEndpointsVeiculos();
    }

    private static void MapearEndpointsHome(this WebApplication app)
    {
        app.MapGet("/", () => Results.Json(new {
            Mensagem = "Bem-vindo à API de Veículos - Minimal API",
            Documentacao = "/swagger"
        })).WithTags("Home");
    }

    private static void MapearEndpointsAutenticacao(this WebApplication app)
    {
        app.MapPost("/login", ([FromBody] LoginDTO loginDTO, DbContexto db, IConfiguration config) =>
        {
            var adm = db.Administradores.FirstOrDefault(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
            if(adm == null) return Results.Unauthorized();

            var tokenKey = config["Jwt:Key"] ?? throw new Exception("Chave JWT ausente");
            var token = TokenService.GerarToken(adm, tokenKey);

            return Results.Ok(token);
        }).WithTags("Autenticação");
    }

    private static void MapearEndpointsAdministradores(this WebApplication app)
    {
        var grupo = app.MapGroup("/administradores").RequireAuthorization("Adm").WithTags("Administradores");

        grupo.MapPost("/", ([FromBody] AdministradorDTO admDto, DbContexto db) =>
        {
            if(string.IsNullOrEmpty(admDto.Email) || string.IsNullOrEmpty(admDto.Senha) || 
               !Enum.TryParse<Perfil>(admDto.Perfil, true, out _))
                return Results.BadRequest("Perfil inválido.");

            var adm = new Administrador { Email = admDto.Email, Senha = admDto.Senha, Perfil = admDto.Perfil };
            db.Administradores.Add(adm);
            db.SaveChanges();

            return Results.Created($"/administradores/{adm.Id}", new { adm.Id, adm.Email, adm.Perfil });
        });

        grupo.MapGet("/", (int? pagina, DbContexto db) => {
            int page = pagina ?? 1;
            return Results.Ok(db.Administradores.Skip((page - 1) * 10).Take(10).ToList());
        });

        grupo.MapGet("/{id}", (int id, DbContexto db) => {
            var adm = db.Administradores.Find(id);
            return adm == null ? Results.NotFound() : Results.Ok(adm);
        });
    }

    private static void MapearEndpointsVeiculos(this WebApplication app)
    {
        var grupo = app.MapGroup("/veiculos").RequireAuthorization().WithTags("Veículos");

        grupo.MapPost("/", ([FromBody] VeiculoDTO veiculoDto, DbContexto db) => {
            if(string.IsNullOrEmpty(veiculoDto.Nome) || string.IsNullOrEmpty(veiculoDto.Marca) || veiculoDto.Ano < 1900)
                return Results.BadRequest("Dados inválidos");

            var veiculo = new Veiculo { Nome = veiculoDto.Nome, Marca = veiculoDto.Marca, Ano = veiculoDto.Ano };
            db.Veiculos.Add(veiculo);
            db.SaveChanges();
            return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
        });

        grupo.MapGet("/", (int? pagina, DbContexto db) => {
            int page = pagina ?? 1;
            return Results.Ok(db.Veiculos.Skip((page - 1) * 10).Take(10).ToList());
        });

        grupo.MapGet("/{id}", (int id, DbContexto db) => {
            var v = db.Veiculos.Find(id);
            return v == null ? Results.NotFound() : Results.Ok(v);
        });

        grupo.MapPut("/{id}", (int id, VeiculoDTO veiculoDto, DbContexto db) => {
            var v = db.Veiculos.Find(id);
            if(v == null) return Results.NotFound();
            
            v.Nome = veiculoDto.Nome; v.Marca = veiculoDto.Marca; v.Ano = veiculoDto.Ano;
            db.Veiculos.Update(v);
            db.SaveChanges();
            return Results.Ok(v);
        });

        grupo.MapDelete("/{id}", (int id, DbContexto db) => {
            var v = db.Veiculos.Find(id);
            if(v == null) return Results.NotFound();
            db.Veiculos.Remove(v);
            db.SaveChanges();
            return Results.NoContent();
        }).RequireAuthorization("Adm");
    }
}