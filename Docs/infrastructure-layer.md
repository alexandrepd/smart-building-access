# Camada Infrastructure

## Propósito

`SmartBuilding.Infrastructure` contém detalhes técnicos externos ao negócio. No estado atual, a sua responsabilidade é mapear o modelo Domain para PostgreSQL através do Entity Framework Core.

## Estado atual

O modelo EF Core está implementado na branch `feat/ef-core-infrastructure`, mas a Fase 3 ainda não foi concluída nem entregue.

Já existem:

- pacotes EF Core e Npgsql;
- `SmartBuildingDbContext`;
- nove `DbSet`s;
- uma configuração Fluent API por entidade;
- nomes de tabelas e colunas em `snake_case`;
- chaves, índices, limites de texto e relacionamentos;
- testes dos metadados do modelo.

Ainda não existem:

- connection string da aplicação;
- registro de `DbContext` na injeção de dependência;
- migration inicial;
- seed de desenvolvimento;
- banco PostgreSQL configurado ou executado;
- implementações de consultas para casos de uso.

## Dependências

### Permitidas

- `SmartBuilding.Domain` para mapear entidades;
- Entity Framework Core;
- provider Npgsql;
- bibliotecas técnicas necessárias à implementação;
- futuramente, `SmartBuilding.Application` para implementar contratos definidos por ela.

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

A API deverá aplicar essa opção ao registrar o contexto. Os testes atuais também a aplicam ao construir o modelo.

## Migrations

Migrations serão o histórico versionado do esquema. A migration inicial deverá ser criada somente depois de configurar o provider e revisar o modelo.

Comandos planejados:

```bash
dotnet ef migrations add InitialCreate \
  --project src/SmartBuilding.Infrastructure \
  --startup-project src/SmartBuilding.Api

dotnet ef database update \
  --project src/SmartBuilding.Infrastructure \
  --startup-project src/SmartBuilding.Api
```

Uma migration já aplicada não deve ser reescrita como se fosse descartável.

## Configuração e segredos

A connection string será lida pela API e fornecida ao registro do contexto. Credenciais reais não devem ser commitadas. Desenvolvimento local poderá usar User Secrets, variáveis de ambiente ou `.env` ignorado pelo Git.

## Seed

O seed de desenvolvimento deverá ser determinístico e mínimo. Ele servirá para demonstração e testes manuais, não para armazenar passwords reais ou substituir migrations.

## Testes atuais

`ModelMetadataTests` constrói o modelo usando Npgsql sem abrir conexão. Ele verifica:

- os nove nomes de tabela;
- unicidade de email e número do cartão;
- obrigatoriedade das relações críticas.

Testes de migration e queries reais pertencerão ao futuro projeto de integração com PostgreSQL.

## Próximos passos

1. registrar `SmartBuildingDbContext` na API;
2. adicionar configuração segura da connection string;
3. criar e revisar a migration inicial;
4. subir PostgreSQL e aplicar a migration;
5. adicionar seed de desenvolvimento;
6. criar testes de integração separados.
