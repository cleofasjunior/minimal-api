using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinimalApi.Dominio.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace MinimalApi.Tests.Integration;

[TestClass]
public class LoginTest
{
    private WebApplicationFactory<Program> _factory = default!;
    private HttpClient _client = default!;

    [TestInitialize]
    public void Initialize()
    {
        // Sobe a API na memória antes de cada teste
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task TestarLoginComCredenciaisValidas_DeveRetornar200OK()
    {
        // 1. Arrange (Preparar o DTO correto)
        var loginDto = new LoginDTO 
        { 
            Email = "adm@teste.com", 
            Senha = "123456" 
        };

        // 2. Act (Enviar POST para /login)
        // PostAsJsonAsync já serializa o objeto para JSON automaticamente
        var response = await _client.PostAsJsonAsync("/login", loginDto);

        // 3. Assert (Validar se deu certo)
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task TestarLoginComSenhaErrada_DeveRetornar401Unauthorized()
    {
        // 1. Arrange (Senha errada)
        var loginDto = new LoginDTO 
        { 
            Email = "adm@teste.com", 
            Senha = "senha_errada" 
        };

        // 2. Act
        var response = await _client.PostAsJsonAsync("/login", loginDto);

        // 3. Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}