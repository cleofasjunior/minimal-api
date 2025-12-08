# 🚗 API de Gestão de Veículos (Minimal API .NET 9)

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple)
![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen)
![Tests](https://img.shields.io/badge/Tests-100%25-success)
![License](https://img.shields.io/badge/License-MIT-blue)

## 📖 Sobre o Projeto

Este projeto é uma API RESTful robusta desenvolvida para resolver o problema de **Gestão de Frotas e Controle de Acesso**. O objetivo foi ir além do básico, criando uma solução segura, escalável e testável, utilizando as tecnologias mais modernas do ecossistema .NET.

A aplicação não apenas gerencia veículos, mas implementa um sistema completo de **Controle de Acesso Baseado em Funções (RBAC)**, garantindo que apenas usuários autorizados (Administradores ou Editores) possam realizar operações sensíveis.

---

## 🏗️ Arquitetura e Organização (Organograma)

O projeto segue os princípios de **Clean Code** e **Separação de Responsabilidades**. A estrutura foi desenhada para facilitar a manutenção e a escalabilidade.

```mermaid
graph TD;
    Solution-->Src(Código Fonte);
    Solution-->Tests(Testes Automatizados);
    
    Src-->MinimalApi;
    MinimalApi-->Dominio(Camada de Domínio);
    MinimalApi-->Infra(Camada de Infraestrutura);
    MinimalApi-->Services(Serviços de Aplicação);
    
    Dominio-->Entidades(Modelos de Banco);
    Dominio-->DTOs(Objetos de Transporte);
    Dominio-->Enums(Regras de Negócio);
    
    Infra-->DbContexto(Entity Framework);
    
    Tests-->Unitarios(Testes de Unidade);
    Tests-->Integracao(Testes de Integração);

📦 MinimalApi.sln
 ┣ 📂 src
 ┃ ┗ 📂 MinimalApi
 ┃ ┃ ┣ 📂 Dominio
 ┃ ┃ ┃ ┣ 📂 DTOs        # Segurança: Dados que entram/saem da API
 ┃ ┃ ┃ ┣ 📂 Entidades   # O "Coração" do negócio (Veículo, Adm)
 ┃ ┃ ┃ ┗ 📂 Enums       # Regras fortes (Perfil: Adm/Editor)
 ┃ ┃ ┣ 📂 Infraestrutura
 ┃ ┃ ┃ ┗ 📂 Db          # Contexto do Entity Framework
 ┃ ┃ ┣ 📂 Services      # Lógica complexa (ex: Gerador de Token JWT)
 ┃ ┃ ┗ 📜 Program.cs    # Configuração de DI, Middleware e Rotas
 ┣ 📂 tests
 ┃ ┗ 📂 MinimalApi.Tests
 ┃ ┃ ┣ 📂 Dominio       # Testes de Entidades isoladas
 ┃ ┃ ┣ 📂 Infra         # Testes de Persistência (Banco em Memória)
 ┃ ┃ ┗ 📂 Integration   # Testes de Requisição HTTP (Simulação Real)

🚀 Tecnologias e Decisões Técnicas

Tecnologia,Função no Projeto,Por que foi escolhida?
.NET 9 (Minimal APIs),Core Framework,"Menor overhead, performance superior e código mais limpo que MVC tradicional."
Entity Framework Core,ORM,"Abstração do banco de dados, facilitando a troca entre SQL Server, MySQL ou InMemory."
JWT Bearer,Segurança,Padrão de mercado para APIs Stateless. Garante autenticação segura entre requisições.
MSTest + WebAppFactory,Testes (QA),Permite subir a API na memória RAM para testar rotas reais sem abrir navegador.
Swagger / OpenAPI,Documentação,Interface visual para testar e documentar os endpoints automaticamente.

🔒 Segurança e Controle de Acesso

O diferencial deste projeto é a implementação rigorosa de segurança:

1. Autenticação JWT: Nenhuma rota crítica é acessível sem um Token válido.

2. Autorização por Claims (RBAC):

- Perfil Adm: Acesso total. Pode criar outros administradores e excluir veículos.

- Perfil Editor: Acesso operacional. Pode cadastrar e editar veículos, mas não pode deletar registros nem acessar dados de usuários.

3. Proteção de Dados:

- Uso de DTOs para evitar Overposting (usuário enviando dados que não deveria).

- Senhas e dados sensíveis nunca são retornados nas rotas de listagem (GET).

🧪 Testes Automatizados

A aplicação possui uma suíte de testes que garante a estabilidade do código (Regressão):

✅ Testes de Unidade: Validam se as Entidades (ex: Administrador) comportam-se como esperado.

✅ Testes de Infraestrutura: Validam se o EF Core está salvando e recuperando dados corretamente.

✅ Testes de Integração: Simulam um cliente HTTP real.

- Cenário: Tenta cadastrar veículo sem token -> Recebe 401.

- Cenário: Faz login, pega token, tenta cadastrar -> Recebe 201.

Para rodar os testes:

Bash

dotnet test

🛠️ Como Executar o Projeto

Pré-requisitos
.NET SDK 9.0 instalado.

Passo a Passo
Clone o repositório:

Bash

git clone [https://github.com/cleofasjunior/minimal-api.git](https://github.com/cleofasjunior/minimal-api.git)
cd minimal-api
Restaure as dependências:

Bash

dotnet restore
Execute a API:

Bash

dotnet run --project src/MinimalApi/MinimalApi.csproj
Acesse a Documentação: Abra seu navegador em: http://localhost:5xxx/swagger

Usuário Padrão (Seed)
Ao iniciar, o sistema cria automaticamente um superusuário para primeiro acesso:

Email: adm@teste.com

Senha: 123456

📝 Aprendizados e Evolução

Durante o desenvolvimento deste projeto, foram consolidados conceitos avançados de engenharia de software:

- Como estruturar uma solução escalável fugindo do "código espaguete" no Program.cs.

- Implementação de Middleware de Autenticação no pipeline do .NET.

- Importância de Testes de Integração para garantir a segurança dos endpoints.

- Uso de Design Patterns (DTO, Repository Pattern via EF Core).

👨‍💻 Autor
Desenvolvido por Cleofas Junior

Foco em desenvolvimento .NET robusto e Arquitetura de Software.

LinkedIn | Portfólio
