# Camada Infrastructure

## Propósito

`SmartBuilding.Infrastructure` contém detalhes técnicos externos ao negócio. Ela mapeia o modelo Domain para PostgreSQL através do Entity Framework Core e implementa contratos de persistência definidos pela Application.

## Estado atual

O modelo EF Core e o bootstrap da persistência estão implementados. Na branch `feat/building-api`, o primeiro repositório funcional da Fase 4 foi adicionado para `Buildings`; a mudança ainda está em desenvolvimento e não foi entregue.

Já existem:

- pacotes EF Core e Npgsql;
- `SmartBuildingDbContext`;
- nove `DbSet`s;
- uma configuração Fluent API por entidade;
- nomes de tabelas e colunas em `snake_case`;
- chaves, índices, limites de texto e relacionamentos;
- registro do contexto com `UseNpgsql` e `UseSnakeCaseNamingConvention`;
- migration `InitialCreate`;
- aplicação automática de migrations em `Development`;
- seed mínimo e idempotente de desenvolvimento;
- `BuildingRepository` com operações CRUD assíncronas;
- consultas de `Building` com `AsNoTracking`;
- proteção da remoção quando o edifício possui pisos;
- testes dos metadados e do registro do contexto.

Ainda não existem:

- projeto automatizado de testes de integração com PostgreSQL;
- repositórios para os demais recursos do domínio.

## Dependências

### Permitidas

- `SmartBuilding.Domain` para mapear entidades;
- `SmartBuilding.Application` para implementar contratos de persistência;
- Entity Framework Core;
- provider Npgsql;
- bibliotecas técnicas necessárias à implementação;

### Proibidas

- dependência de `SmartBuilding.Api`;
- regras de autorização de acesso;
- decisões de negócio sobre ocupação e alertas;
- tipos HTTP, controllers ou respostas da API.

## Pacotes atuais

| Pacote | Função |
|---|---|
| `Microsoft.EntityFrameworkCore` | Modelo, tracking e acesso a dados |
| `Microsoft.EntityFrameworkCore.Relational` | Metadados e recursos relacionais |
| `Microsoft.EntityFrameworkCore.Design` | Ferramentas de design e migrations |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Provider PostgreSQL |
| `EFCore.NamingConventions` | Convenção automática `snake_case` |

As versões de EF Core foram alinhadas explicitamente para evitar conflitos de assembly entre o provider e as ferramentas de design.

O repositório fixa `dotnet-ef` 10.0.12 como ferramenta local em `dotnet-tools.json`. Assim, a geração e aplicação de migrations usam uma versão reproduzível com `dotnet tool restore` e `dotnet ef`.

## SmartBuildingDbContext

`SmartBuildingDbContext` é a sessão do EF Core com o banco. Ele expõe os conjuntos persistíveis:

```text
Buildings
Floors
AccessPoints
Users
AccessCards
AccessPermissions
AccessEvents
OccupancySessions
SecurityAlerts
```

`OnModelCreating` usa `ApplyConfigurationsFromAssembly`, permitindo que cada entidade tenha uma configuração separada. Assim, o contexto não concentra todos os detalhes do esquema.

## Configurações por entidade

```text
Data/
├── SmartBuildingDbContext.cs
└── Configurations/
    ├── AccessCardConfiguration.cs
    ├── AccessEventConfiguration.cs
    ├── AccessPermissionConfiguration.cs
    ├── AccessPointConfiguration.cs
    ├── BuildingConfiguration.cs
    ├── FloorConfiguration.cs
    ├── OccupancySessionConfiguration.cs
    ├── SecurityAlertConfiguration.cs
    └── UserConfiguration.cs
```

Cada configuração é responsável por:

- nome da tabela;
- chave primária;
- obrigatoriedade e tamanho de campos;
- tipos específicos do PostgreSQL;
- índices;
- relacionamentos e comportamento de exclusão.

## Mapeamento relacional

| Entidade | Tabela | Índices de negócio |
|---|---|---|
| `Building` | `buildings` | chave primária |
| `Floor` | `floors` | `building_id` |
| `AccessPoint` | `access_points` | `floor_id` |
| `User` | `users` | `email` único |
| `AccessCard` | `access_cards` | `card_number` único e `user_id` |
| `AccessPermission` | `access_permissions` | `user_id` e `access_point_id` |
| `AccessEvent` | `access_events` | instante, ponto, cartão e utilizador |
| `OccupancySession` | `occupancy_sessions` | utilizador, piso e saída |
| `SecurityAlert` | `security_alerts` | `access_point_id` |

## Tipos de data

Datas são mapeadas para `timestamp with time zone`. Com Npgsql, os valores devem representar instantes UTC. Datas locais ou com `DateTimeKind.Unspecified` devem ser normalizadas antes da persistência.

## Estratégia de exclusão

Relações obrigatórias usam `DeleteBehavior.Restrict`. Isso evita apagar em cascata histórico de eventos, ocupação ou alertas ao excluir uma entidade principal.

As relações opcionais de `AccessEvent` com cartão e utilizador usam `SetNull`. Assim, o evento de auditoria pode ser preservado mesmo se o vínculo opcional deixar de existir.

Na aplicação final, entidades principais tendem a ser desativadas através de `IsActive` em vez de removidas fisicamente.

## Índices e unicidade

Os índices únicos documentados são:

- `users.email`;
- `access_cards.card_number`.

Os demais índices suportam navegação por foreign keys e consultas futuras de eventos e ocupação. Índices adicionais devem nascer de consultas reais e medições, não de antecipação.

## Convenção snake_case

As configurações definem os nomes de tabela e a opção `UseSnakeCaseNamingConvention` converte propriedades e constraints. Exemplos:

```text
AccessPointId -> access_point_id
OccurredAt -> occurred_at
OccupancySessions -> occupancy_sessions
```

`AddInfrastructure` registra `SmartBuildingDbContext` como scoped e aplica `UseNpgsql(connectionString)` seguido de `UseSnakeCaseNamingConvention()`.

## BuildingRepository

`BuildingRepository` implementa `IBuildingRepository`, contrato pertencente à Application. A listagem e a consulta por ID usam `AsNoTracking`; a listagem também ordena por nome. Criação e atualização usam entidades rastreadas e persistem com `SaveChangesAsync`.

Antes do delete, o repositório verifica se existem `Floor`s associados. Se existirem, retorna `HasFloors` sem remover o edifício. A verificação melhora a resposta normal, mas não elimina a corrida entre consulta e gravação: se um piso for inserido nesse intervalo, a constraint FK com `Restrict` continua sendo a autoridade. O repositório captura especificamente a `ForeignKeyViolation` do PostgreSQL e também retorna `HasFloors`.

Essa proteção está implementada somente para o CRUD de `Buildings`. Consultas e comandos dos demais recursos continuam planejados.

## Migrations

Migrations são o histórico versionado do esquema. A migration `InitialCreate` foi gerada para as nove tabelas do domínio, respetivas chaves, foreign keys e índices.

Comandos usados pelo fluxo local:

```bash
dotnet tool restore

dotnet ef migrations add InitialCreate \
  --project src/SmartBuilding.Infrastructure \
  --startup-project src/SmartBuilding.Api

dotnet ef database update \
  --project src/SmartBuilding.Infrastructure \
  --startup-project src/SmartBuilding.Api
```

Uma migration já aplicada não deve ser reescrita como se fosse descartável.

## Configuração e segredos

A API lê `ConnectionStrings:SmartBuilding` e falha no arranque com `InvalidOperationException` quando o valor está ausente ou vazio. A connection string é fornecida a `AddInfrastructure`; credenciais reais não são guardadas no código nem em ficheiros versionados.

User Secrets foi inicializado no projeto API para configuração local. O `appsettings.json` mantém apenas uma entrada vazia, sem segredo versionado. Variáveis de ambiente continuam sendo uma alternativa válida.

Para executar a API localmente, é necessário PostgreSQL 18 e a connection string deve ser configurada fora dos ficheiros versionados:

```bash
dotnet tool restore

dotnet user-secrets set \
  --project src/SmartBuilding.Api \
  "ConnectionStrings:SmartBuilding" \
  "Host=localhost;Port=5432;Database=smart_building;Username=smart_building;Password=<local-password>"
```

Em `Development`, `InitializeDevelopmentDatabaseAsync` cria um scope, executa `MigrateAsync` e só depois chama `DevelopmentDataSeeder`. Fora de `Development`, a API não aplica migrations nem executa esse seed automaticamente.

## Seed

O seed de desenvolvimento é determinístico e mínimo. GUIDs fixos identificam:

- edifício `HQ Lisbon`;
- piso `Ground Floor`;
- ponto de acesso `Main Entrance`.

Antes de inserir cada registo, o seeder consulta o respetivo GUID. Por isso, reiniciar a API não duplica esses dados. O seed serve para demonstração e testes manuais; não armazena passwords nem substitui migrations.

`SeedAsync` abre uma transação e adquire `pg_advisory_xact_lock(1937001)` antes das consultas e inserções. O PostgreSQL mantém esse lock até ao fim da transação; assim, duas instâncias que inicializem ao mesmo tempo não executam em paralelo a sequência de verificar e inserir. O lock usa uma chave exclusiva deste bootstrap e coordena apenas participantes que adotem a mesma chave.

## Testes atuais

`ModelMetadataTests` constrói o modelo usando Npgsql sem abrir conexão. Ele verifica:

- os nove nomes de tabela;
- unicidade de email e número do cartão;
- obrigatoriedade das relações críticas.

`DependencyInjectionTests` resolve o contexto em dois scopes e verifica a mesma instância dentro de um scope e instâncias diferentes entre scopes. Também confirma o provider Npgsql, a coluna `access_point_id` em `snake_case` e a rejeição de connection string vazia ou composta apenas por espaços.

Também foi feita uma validação manual contra PostgreSQL 18 real num container Docker descartável, exposto apenas em `55432`. Além da migration e do seed, o smoke test do slice de `Buildings` observou, em sequência, os status `200/201/200/200/204/404/400/409`. As respostas `404`, `400` e `409` usaram content type `application/problem+json`. Isso confirmou o CRUD, a validação e o bloqueio real de exclusão do edifício sem transformar a verificação em teste automatizado.

O container foi apenas um ambiente isolado de validação da persistência. Isso não implementa a fase futura de Docker da aplicação, nem substitui testes de integração automatizados.

## Próximos passos

1. criar testes de integração automatizados com PostgreSQL;
2. implementar contratos e consultas de persistência para os demais casos de uso;
3. manter novas evoluções do esquema em migrations adicionais.
