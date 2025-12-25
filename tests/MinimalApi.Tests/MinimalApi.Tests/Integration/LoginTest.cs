using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinimalApi.Dominio.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace MinimalApi.Tests.Integration;

[TestClass]
public class LoginTest
{
    // WebApplicationFactory sobe a aplicação inteira em memória
    private WebApplicationFactory<Program> _factory = default!;
    private HttpClient _client = default!;

    [TestInitialize]
    public void Initialize()
    {
        // A factory usa as configurações do seu Program.cs
        // Como lá está "UseInMemoryDatabase", o teste não suja banco real.
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task TestarLoginComCredenciaisValidas_DeveRetornarToken()
    {
        // 1. Arrange
        var loginDto = new LoginDTO 
        { 
            Email = "adm@teste.com", 
            Senha = "123456" 
        };

        // 2. Act
        var response = await _client.PostAsJsonAsync("/login", loginDto);

        // 3. Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Validação Extra (Nível Sênior):
        // Garante que não veio um 200 OK vazio, mas sim um texto (o Token)
        var token = await response.Content.ReadAsStringAsync();
        Assert.IsFalse(string.IsNullOrEmpty(token), "O token não deveria ser vazio");
    }

    [TestMethod]
    public async Task TestarLoginComSenhaErrada_DeveRetornarUnauthorized()
    {
        // 1. Arrange
        var loginDto = new LoginDTO 
        { 
            Email = "adm@teste.com", 
            Senha = "senha_totalmente_errada" 
        };

        // 2. Act
        var response = await _client.PostAsJsonAsync("/login", loginDto);

        // 3. Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        // Boa prática: Liberar memória após cada teste
        _client.Dispose();
        _factory.Dispose();
    }
}