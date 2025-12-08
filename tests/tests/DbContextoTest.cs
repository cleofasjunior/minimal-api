using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Tests.Infra.Db;

[TestClass]
public class DbContextoTest
{
    [TestMethod]
    public void TestarSalvarAdministradorNoBanco()
    {
        // 1. Arrange - Configura o banco em memória SOMENTE para este teste
        var options = new DbContextOptionsBuilder<DbContexto>()
            .UseInMemoryDatabase(databaseName: "DbTeste_Contexto_Admin")
            .Options;

        using var context = new DbContexto(options);
        
        var adm = new Administrador 
        { 
            Email = "contexto@teste.com", 
            Senha = "123", 
            Perfil = "Adm" 
        };

        // 2. Act - Salva
        context.Administradores.Add(adm);
        context.SaveChanges();

        // 3. Assert - Busca e Valida
        var admSalvo = context.Administradores.FirstOrDefault(a => a.Email == "contexto@teste.com");
        
        Assert.IsNotNull(admSalvo);
        Assert.AreEqual("Adm", admSalvo.Perfil);
    }
}