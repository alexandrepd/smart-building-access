# Visão Geral da Arquitetura

## Objetivo

O Smart Building Access é uma plataforma para controlo de acessos e monitorização de ocupação. A arquitetura separa regras de negócio, orquestração, tecnologia de persistência e transporte HTTP para que cada parte possa evoluir e ser testada com menor acoplamento.

## Camadas atuais

```mermaid
flowchart LR
    Client[Cliente HTTP] --> API[SmartBuilding.Api]
    API --> Application[SmartBuilding.Application]
    API --> Infrastructure[SmartBuilding.Infrastructure]
    Application --> Domain[SmartBuilding.Domain]
    Infrastructure --> Application
    Infrastructure --> Domain

    UnitTests[SmartBuilding.UnitTests] --> Domain
    UnitTests --> Application
    UnitTests --> Infrastructure

    Infrastructure --> PostgreSQL[(PostgreSQL)]
```

As setas representam dependências de compilação ou uso. A camada apontada não conhece a camada de origem.

## Regra de dependência

As dependências devem apontar para o centro da aplicação:

```text
API ------------> Application ------------> Domain
  \                                      ^
   \-----------> Infrastructure ----------/
```

O Domain é o núcleo e não referencia nenhuma outra camada do sistema. Application conhece o Domain, mas não conhece API nem Infrastructure. Infrastructure conhece Domain e Application para mapear entidades e implementar contratos definidos pela Application. API funciona como composition root e conecta as partes.

## Responsabilidades

| Projeto | Responsabilidade principal | Estado |
|---|---|---|
| `SmartBuilding.Domain` | Vocabulário, estado e regras puras do negócio | Modelo estrutural implementado; comportamento ainda será evoluído |
| `SmartBuilding.Application` | Casos de uso, DTOs, contratos e orquestração | Serviço, commands, DTO e contrato de repositório de `Buildings` implementados |
| `SmartBuilding.Infrastructure` | EF Core, PostgreSQL e implementações técnicas | Persistência base e `BuildingRepository` implementados |
| `SmartBuilding.Api` | Host HTTP, endpoints, autenticação e composição | Minimal APIs, validação e Problem Details de `Buildings` implementados; autenticação planejada |
| `SmartBuilding.UnitTests` | Testes unitários, arquiteturais e de persistência | Testes existentes mais nove casos de teste de `BuildingService` |

## Fluxo atual

Além do endpoint de estado, o primeiro fluxo vertical da Fase 4 atravessa todas as camadas:

```text
HTTP /api/buildings
  -> requests e endpoints em SmartBuilding.Api
  -> BuildingService e IBuildingRepository em SmartBuilding.Application
  -> BuildingRepository e SmartBuildingDbContext em SmartBuilding.Infrastructure
  -> PostgreSQL
```

O fluxo implementa listagem, consulta por ID, criação, atualização e remoção de `Building`. O delete retorna conflito quando há `Floor`s associados, inclusive quando uma inserção concorrente causa violação de FK durante a gravação.

Antes de servir endpoints, a API exige `ConnectionStrings:SmartBuilding`. Em `Development`, o bootstrap aplica migrations e executa o seed mínimo. O endpoint `GET /` continua sem acessar o banco, enquanto `/api/buildings` usa a persistência PostgreSQL.

Esse fluxo existe somente para `Buildings`. CRUDs de `Floor`, `AccessPoint` e dos demais recursos permanecem planejados.

## Fluxo planejado de uma solicitação de acesso

```mermaid
sequenceDiagram
    participant Reader as Leitor/Simulator
    participant Api as API
    participant App as Application
    participant Domain as Domain
    participant Infra as Infrastructure
    participant Db as PostgreSQL

    Reader->>Api: POST /api/access/requests
    Api->>App: ExecuteAsync(request)
    App->>Infra: Consultar cartão, utilizador e permissão
    Infra->>Db: SELECT
    Db-->>Infra: Estado persistido
    Infra-->>App: Dados necessários
    App->>Domain: Avaliar regras de acesso
    Domain-->>App: AccessResult
    App->>Infra: Persistir AccessEvent
    Infra->>Db: INSERT
    App-->>Api: AccessResponse
    Api-->>Reader: HTTP response
```

Esse fluxo é planejado. Os casos de uso, contratos de persistência e endpoint ainda não foram implementados.

## Princípios arquiteturais

### Domínio independente

O Domain não pode depender de ASP.NET Core, EF Core, PostgreSQL, serialização, Docker ou detalhes de interface.

### Eventos brutos e estado derivado

`AccessEvent` representa um fato auditável. `OccupancySession` representa estado derivado desses eventos. Uma correção de ocupação não deve apagar ou reescrever o histórico bruto.

### Composition root na API

A API lê a connection string e chama `AddInfrastructure`, que registra `SmartBuildingDbContext` com Npgsql e convenção `snake_case`. Isso não significa que regras de negócio pertencem à API.

### Persistência como detalhe externo

EF Core traduz o modelo para PostgreSQL. Entidades do Domain não devem receber atributos ou comportamentos específicos do banco.

### Evolução por fases

Cada fase deve terminar com build, testes e um pull request focado. Tecnologias futuras não devem ser adicionadas antes de existir uma necessidade da fase atual.

## Estado por fase

| Fase | Estado resumido |
|---|---|
| Foundation | Concluída |
| Domain estrutural | Concluída pelo Definition of Done atual |
| Database | Persistência base implementada e validada |
| API funcional | Primeiro vertical slice implementado para `Buildings` na branch atual; demais CRUDs planejados |
| Access Control em diante | Não iniciado |

PostgreSQL 18 foi executado num container descartável, na porta isolada `55432`, para validar a migration, o arranque, a idempotência do seed e o slice de `Buildings`. O runtime confirmou respostas `400`, `404` e `409` com content type `application/problem+json`. Isso não significa que a fase futura de Docker foi implementada.

Consulte [o plano do projeto](smart-building-project-plan.md) para todas as fases e critérios de conclusão.
