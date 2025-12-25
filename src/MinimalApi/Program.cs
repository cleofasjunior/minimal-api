using MinimalApi.Extensoes;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração de Arquitetura (Serviços, Banco, Auth)
builder.AdicionarArquitetura();

var app = builder.Build();

// 2. Configuração de Pipeline (Ambiente, Swagger)
app.ConfigurarAmbiente();

// 3. Mapeamento de Rotas
app.MapearEndpoints();

// 4. Carga Inicial de Dados (Seed)
app.GerarCargaInicialBanco();

app.Run();

// Necessário para os Testes de Integração (WebAppFactory)
public partial class Program { }