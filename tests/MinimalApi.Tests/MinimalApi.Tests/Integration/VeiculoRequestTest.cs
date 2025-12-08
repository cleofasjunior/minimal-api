using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinimalApi.Dominio.DTOs;

namespace MinimalApi.Tests.Integration;

[TestClass]
public class VeiculoRequestTest
{
    // O "global::Program" garante que estamos pegando a classe Program.cs da raiz
    private static WebApplicationFactory<global::Program> _factory = default!;

    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        _factory = new WebApplicationFactory<global::Program>();
    }

    [TestMethod]
    public async Task TestarCadastroVeiculo_ComTokenAdm_DeveRetornar201()
    {
        // ARRANGE --------------------------------------------
        var client = _factory.CreateClient();

        // 1. Fazer Login para pegar o Token
        var loginDto = new LoginDTO { Email = "adm@teste.com", Senha = "123456" };
        var responseLogin = await client.PostAsJsonAsync("/login", loginDto);
        
        // Lê o token do JSON
        var loginResult = await responseLogin.Content.ReadFromJsonAsync<TokenResult>();
        var token = loginResult?.token;

        // 2. Configurar o Cliente HTTP com o Token no Header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // ACT ------------------------------------------------
        var veiculoDto = new VeiculoDTO 
        { 
            Nome = "Test Car", 
            Marca = "Test Brand", 
            Ano = 2025 
        };

        var response = await client.PostAsJsonAsync("/veiculos", veiculoDto);

        // ASSERT ---------------------------------------------
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task TestarCadastroVeiculo_SemToken_DeveRetornar401()
    {
        // ARRANGE
        var client = _factory.CreateClient();
        // Não adicionamos token propositalmente

        var veiculoDto = new VeiculoDTO { Nome = "Ghost Car", Marca = "Ghost", Ano = 2022 };

        // ACT
        var response = await client.PostAsJsonAsync("/veiculos", veiculoDto);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Classe auxiliar para ler o JSON do login
    public record TokenResult(string token);
}