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
        // Cria a fábrica apenas uma vez para todos os testes desta classe (Performance)
        _factory = new WebApplicationFactory<global::Program>();
    }

    [TestMethod]
    public async Task TestarCadastroVeiculo_ComTokenAdm_DeveRetornar201()
    {
        // 1. ARRANGE: Preparar cenário
        var client = _factory.CreateClient();

        // Passo A: Fazer Login para pegar o Token de Adm
        var loginDto = new LoginDTO { Email = "adm@teste.com", Senha = "123456" };
        var responseLogin = await client.PostAsJsonAsync("/login", loginDto);
        
        // Lê o token como string (garante compatibilidade com o retorno da API)
        var token = await responseLogin.Content.ReadAsStringAsync();
        token = token.Trim('"'); // Remove aspas extras se vierem no JSON

        // Passo B: Configurar o Cliente HTTP com o Token no Header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var veiculoDto = new VeiculoDTO 
        { 
            Nome = "Fusca Teste", 
            Marca = "VW", 
            Ano = 2025 
        };

        // 2. ACT: Tentar cadastrar o veículo estando logado
        var response = await client.PostAsJsonAsync("/veiculos", veiculoDto);

        // 3. ASSERT: Deve criar com sucesso
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        
        // Opcional: Verificar se retornou o objeto criado
        var veiculoCriado = await response.Content.ReadFromJsonAsync<VeiculoDTO>();
        Assert.IsNotNull(veiculoCriado);
        Assert.AreEqual("Fusca Teste", veiculoCriado.Nome);
    }

    [TestMethod]
    public async Task TestarCadastroVeiculo_SemToken_DeveRetornar401()
    {
        // 1. ARRANGE
        var client = _factory.CreateClient();
        // Nota: NÃO configuramos o Header Authorization aqui propositalmente

        var veiculoDto = new VeiculoDTO { Nome = "Carro Fantasma", Marca = "Ghost", Ano = 2022 };

        // 2. ACT
        var response = await client.PostAsJsonAsync("/veiculos", veiculoDto);

        // 3. ASSERT
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [ClassCleanup]
    public static void ClassCleanup()
    {
        _factory.Dispose();
    }
}