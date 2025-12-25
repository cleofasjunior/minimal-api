using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Extensoes;

public static class AppExtensao
{
    public static void ConfigurarAmbiente(this WebApplication app)
    {
        // Swagger só em desenvolvimento
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Habilita Autenticação e Autorização
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public static void GerarCargaInicialBanco(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
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
}