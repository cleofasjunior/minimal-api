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
    private static WebApplicationFactory<global::Program> _factory = default!;

    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        _factory = new WebApplicationFactory<global::Program>();
    }

    [TestMethod]
    public async Task TestarCadastroVeiculo_ComTokenAdm_DeveRetornar201()
    {
        // 1. ARRANGE
        var client = _factory.CreateClient();

        // Passo A: Fazer Login
        var loginDto = new LoginDTO { Email = "adm@teste.com", Senha = "123456" };
        var responseLogin = await client.PostAsJsonAsync("/login", loginDto);
        
        // 🚑 CORREÇÃO: Ler o JSON {"token": "..."} usando um record auxiliar
        var resultado = await responseLogin.Content.ReadFromJsonAsync<TokenResult>();
        
        // Agora temos o token limpo (sem chaves {} ou aspas extras)
        var tokenLimpo = resultado?.token; 

        // Passo B: Configurar o Header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

        var veiculoDto = new VeiculoDTO 
        { 
            Nome = "Fusca Teste", 
            Marca = "VW", 
            Ano = 2025 
        };

        // 2. ACT
        var response = await client.PostAsJsonAsync("/veiculos", veiculoDto);

        // 3. ASSERT
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task TestarCadastroVeiculo_SemToken_DeveRetornar401()
    {
        var client = _factory.CreateClient();
        var veiculoDto = new VeiculoDTO { Nome = "Ghost Car", Marca = "Ghost", Ano = 2022 };

        var response = await client.PostAsJsonAsync("/veiculos", veiculoDto);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [ClassCleanup]
    public static void ClassCleanup()
    {
        _factory.Dispose();
    }
}

// 📦 Record auxiliar para ler o retorno do TokenService
// O nome da propriedade "token" deve ser igual ao do objeto anônimo no TokenService
public record TokenResult(string token);