# API de Clínicas

API de exemplo em .NET 10 para aprender a organizar um projeto em camadas (DDD simples).
Ela já sobe com dados e tem o cadastro completo de clínicas.

É um projeto didático: não tem autenticação e o Swagger fica sempre ligado. Não use assim em produção.

## Como rodar

Você só precisa do [.NET 10 SDK](https://dotnet.microsoft.com/download). Não há banco para instalar.

```
dotnet run --project src/GestaoClinica.Api
```

Depois abra <http://localhost:5150> no navegador: você cai no Swagger, onde dá para testar todos os endpoints.
(No Visual Studio, basta apertar F5 que o Swagger abre sozinho.)

Para rodar os testes:

```
dotnet test GestaoClinica.sln
```

O arquivo `src/GestaoClinica.Api/GestaoClinica.Api.http` tem requisições prontas para usar no Visual Studio ou no VS Code (extensão REST Client).

## Endpoints

| Método | Rota | O que faz | Sucesso |
|--------|------|-----------|---------|
| GET | `/api/clinicas` | Lista as clínicas | 200 |
| GET | `/api/clinicas/{id}` | Busca uma clínica | 200 |
| POST | `/api/clinicas` | Cria uma clínica | 201 |
| PUT | `/api/clinicas/{id}` | Atualiza uma clínica | 200 |
| DELETE | `/api/clinicas/{id}` | Exclui uma clínica | 204 |

Atenção a dois comportamentos:

- `DELETE` apaga também os médicos e os pacientes da clínica.
- `PUT` substitui todos os campos. Campo omitido vira nulo, e `ativa` omitida vira `true`.

Erros de regra de negócio, corpo inválido e erros inesperados respondem com um JSON no mesmo formato
(rota que não existe ou id não numérico, como `/api/clinicas/abc`, devolve 404 sem corpo):

```json
{ "mensagem": "Clínica não encontrada." }
```

| Status | Quando acontece |
|--------|-----------------|
| 400 | Uma regra de negócio foi quebrada (CNPJ inválido ou repetido, campo obrigatório vazio) ou o corpo da requisição é inválido (JSON malformado, vazio ou com tipo errado) |
| 404 | A clínica não existe |
| 500 | Erro que ninguém previu. A mensagem é genérica e o detalhe fica só no log |

## Mapa das camadas

```
src/
  GestaoClinica.Api/           Recebe as requisições HTTP e devolve as respostas
    Controllers/           ClinicasController: os 5 endpoints
    DTO/
      Request/             ClinicaRequest: o que o cliente envia
      Response/            ClinicaResponse: o que a API devolve
    Middlewares/           GlobalExceptionMiddleware: transforma exceção em JSON
    Program.cs             Liga tudo: chama AddRepository(), Swagger, dados iniciais

  GestaoClinica.Domain/        O coração do sistema. Não depende de nenhum outro projeto
    Entities/              Clinica, Medico, Paciente
      Base/                Entity: classe mãe de todas as entidades, tem o Id
    Services/              ClinicaService: TODAS as regras de negócio
    Exceptions/            NegocioException e RegistroNaoEncontradoException
    Interfaces/            IClinicaRepository: o que o Domain precisa do banco para clínica
                           IClinicaService: o que o service de clínica oferece ao controller
      Base/                IRepositoryBase<T>: operações comuns de qualquer repositório

  GestaoClinica.Repository.EF/ Conversa com o banco de dados usando Entity Framework
    Context/               ClinicaDbContext
    Mappings/              Como cada entidade vira tabela
    Repositories/          RepositoryBase<T>: operações comuns de qualquer entidade
                           ClinicaRepository: herda da base e implementa IClinicaRepository
    DadosIniciais.cs       Clínicas, médicos e pacientes de exemplo
    DependencyInjection.cs AddRepository() e PopularDadosIniciais(): registra tudo deste projeto

  GestaoClinica.Repository.Dapper/  A mesma coisa usando Dapper e SQL Server (ainda FORA da solução)
    Context/               ConnectionFactory: cria as conexões
    Repositories/          ClinicaRepository: implementa IClinicaRepository com SQL escrito à mão
    DadosIniciais.cs       Cria as tabelas e cadastra os mesmos dados de exemplo
    DependencyInjection.cs AddRepository(connectionString) e PopularDadosIniciais()

tests/
  GestaoClinica.Tests/         Testes do service (sem banco), da API (HTTP de ponta a ponta)
                               e do RepositoryBase (com uma entidade diferente de Clinica)
```

Os nomes de padrão ficam em inglês, como é costume no mercado: `Entities`, `Repository`, `Services`, `Exceptions`, `Mappings`, `Context`, `Controllers`, `DTO`. O que é do negócio fica em português: `Clinica`, `Medico`, `RazaoSocial`, `BuscarPorCnpjAsync`. Por isso uma classe se chama `ClinicaRepository` e um método se chama `ListarAsync`.

Quem depende de quem:

```
GestaoClinica.Api  ──►  GestaoClinica.Repository.EF  ──►  GestaoClinica.Domain
     └────────────────────────────────────────────►┘
```

Os projetos de repositório seguem o nome `Repository.Tecnologia` (`Repository.EF`, `Repository.Dapper`). Cada um tem a sua classe `DependencyInjection`, então a Api só chama `AddRepository()` e não precisa saber qual tecnologia está por trás.

O Domain não conhece ninguém. Isso é de propósito: as regras de negócio não mudam se trocarmos o banco ou a forma de expor a API.

## O caminho de uma requisição

Exemplo: `POST /api/clinicas`

1. O `ClinicasController` recebe o `ClinicaRequest` e o transforma na entidade `Clinica`.
2. O controller chama o `IClinicaService`, e o `ClinicaService` valida as regras (campos obrigatórios, CNPJ com 14 dígitos, CNPJ não repetido).
3. Se estiver tudo certo, o service chama o `IClinicaRepository`, e o `ClinicaRepository` salva no banco.
4. O controller transforma a entidade em `ClinicaResponse` e devolve 201.

Se uma regra for quebrada no passo 2, o service lança `NegocioException`. O controller não trata nada: a exceção sobe até o `GlobalExceptionMiddleware`, que devolve o JSON de erro.

| Exceção | Status |
|---------|--------|
| `RegistroNaoEncontradoException` | 404 |
| `NegocioException` | 400 |
| Qualquer outra | 500 |

A `NegocioException` também tem um construtor que recebe o código HTTP junto com a mensagem. Ele ainda não é usado: o middleware continua respondendo 400.

## Regras da casa

- Regra de negócio fica no `ClinicaService`, nunca no controller nem no repositório.
- Controller não tem `try/catch`.
- O Domain não referencia o Entity Framework nem outro projeto.
- Médico e Paciente existem só como entidades mapeadas. Ainda não têm service, repositório nem endpoint.

## Como criar repositório, service e controller para outra entidade

Médico e Paciente já têm entidade, mapeamento, tabela (`DbSet`) e dados de exemplo. O que falta é o caminho até a API. A base está pronta: o `RepositoryBase<T>` já sabe listar, buscar por Id, adicionar, atualizar e remover qualquer entidade.

Use os arquivos de Clínica como modelo. Exemplo para Médico:

1. **Interface do repositório** — `GestaoClinica.Domain/Interfaces/IMedicoRepository.cs`

   ```csharp
   using GestaoClinica.Domain.Entities;
   using GestaoClinica.Domain.Interfaces.Base;

   namespace GestaoClinica.Domain.Interfaces;

   public interface IMedicoRepository : IRepositoryBase<Medico>
   {
       // Só o que for específico de médico. Pode ficar vazia.
   }
   ```

2. **Repositório** — `GestaoClinica.Repository.EF/Repositories/MedicoRepository.cs`

   ```csharp
   using GestaoClinica.Domain.Entities;
   using GestaoClinica.Domain.Interfaces;
   using GestaoClinica.Repository.EF.Context;

   namespace GestaoClinica.Repository.EF.Repositories;

   public class MedicoRepository : RepositoryBase<Medico>, IMedicoRepository
   {
       public MedicoRepository(ClinicaDbContext contexto) : base(contexto)
       {
       }
   }
   ```

3. **Registro** — em `GestaoClinica.Repository.EF/DependencyInjection.cs`, no lugar indicado pelo comentário:

   ```csharp
   services.AddScoped<IMedicoRepository, MedicoRepository>();
   ```

4. **Service** — a interface `IMedicoService` em `GestaoClinica.Domain/Interfaces` e a classe `MedicoService` em `GestaoClinica.Domain/Services`, com as regras de negócio, seguindo `IClinicaService` e `ClinicaService`. Registre no `Program.cs`: `builder.Services.AddScoped<IMedicoService, MedicoService>();`

5. **DTOs** — `MedicoRequest` em `GestaoClinica.Api/DTO/Request` e `MedicoResponse` em `GestaoClinica.Api/DTO/Response`.

6. **Controller** — `GestaoClinica.Api/Controllers/MedicosController.cs`, seguindo o `ClinicasController`. Ele recebe o `IMedicoService`, não a classe.

7. **Testes** — siga `ClinicaServiceTests` e `ClinicasApiTests`. As operações herdadas já são testadas em `RepositoryBaseTests`; o repositório novo só precisa de teste próprio para o que ele acrescentar ou trocar.

8. **Exemplos** — acrescente requisições em `GestaoClinica.Api.http` e atualize este README (tabela de endpoints e "Regras da casa").

Para uma entidade totalmente nova, antes do passo 1:

- crie a classe em `GestaoClinica.Domain/Entities` herdando de `Entity` (é dela que vem o `Id`; precisa de `using GestaoClinica.Domain.Entities.Base;`);
- crie o mapeamento em `GestaoClinica.Repository.EF/Mappings` (ele é aplicado sozinho);
- adicione o `DbSet` em `ClinicaDbContext`;
- se quiser dados de exemplo, acrescente em `DadosIniciais.cs`.

Se a entidade precisar de uma consulta própria, declare o método na interface dela e implemente no repositório usando `Tabela`, como o `BuscarPorCnpjAsync` de clínica. Se uma operação comum precisar se comportar diferente, use `override`, como o `RemoverAsync` de clínica. Isso é obrigatório para entidade que tem filhos: o banco em memória não apaga os filhos sozinho.

O `Repository.Dapper` não tem classe base. Para a Api poder usá-lo, cada interface nova também precisa ser implementada lá, com o SQL escrito à mão.

## Sobre o banco de dados

A API usa o banco **em memória** do Entity Framework. Os dados são recriados toda vez que a aplicação inicia e somem quando ela para.

Duas diferenças para um banco de verdade, que explicam trechos do código:

- O banco em memória não respeita índice único. Por isso o `ClinicaService` confere se o CNPJ já existe.
- Ele também não apaga os registros filhos sozinho. Por isso o `ClinicaRepository` carrega médicos e pacientes antes de excluir a clínica.

Para usar SQL Server com o Entity Framework não basta trocar uma linha. Os passos (também comentados em `DependencyInjection.cs` do `Repository.EF`) são:

1. Instalar o pacote `Microsoft.EntityFrameworkCore.SqlServer`.
2. Colocar a connection string no `appsettings.json`.
3. Trocar `UseInMemoryDatabase` por `UseSqlServer` em `DependencyInjection.cs`.
4. Criar o banco (`EnsureCreated` ou migrations).
5. Tirar os Ids fixos de `DadosIniciais.cs`, porque o SQL Server gera os Ids.

### Repository.Dapper

O projeto `GestaoClinica.Repository.Dapper` implementa o mesmo `IClinicaRepository` com Dapper. Ele ainda não faz parte da solução e a Api não o usa.

O Dapper não tem banco em memória: ele precisa de um SQL Server de verdade (o LocalDB que vem com o Visual Studio serve). Para a Api passar a usá-lo:

1. Adicionar o projeto à solução e trocar a referência da Api de `Repository.EF` para `Repository.Dapper`.
2. Colocar a connection string `ClinicaDb` no `appsettings.json`. Exemplo para o LocalDB:
   `Server=(localdb)\MSSQLLocalDB;Database=GestaoClinica;Integrated Security=true;TrustServerCertificate=true`
3. No `Program.cs`, trocar o `using` e chamar
   `builder.Services.AddRepository(builder.Configuration.GetConnectionString("ClinicaDb"))`.
4. Ajustar os testes de API: hoje eles trocam o banco em memória do `Repository.EF` por um banco isolado por teste, e isso não existe no Dapper.

O `PopularDadosIniciais()` cria as tabelas se não existirem e cadastra os dados de exemplo sempre que a tabela de clínicas estiver vazia. O banco em si precisa existir antes.
