# Estratégia de Testes

## Propósito

Os testes fornecem feedback sobre regras, fronteiras arquiteturais, persistência e comportamento HTTP. A suíte deve crescer de acordo com o risco de cada fase.

## Estado atual

Existe um projeto `SmartBuilding.UnitTests` com xUnit. Ele contém:

- um teste arquitetural de independência do Domain;
- testes dos metadados do modelo EF Core;
- testes do registro de persistência em Dependency Injection.
- nove casos de teste unitário de `BuildingService`.

Ainda não existe `SmartBuilding.IntegrationTests`.

## Organização atual

```text
SmartBuilding.UnitTests/
├── Application/
│   └── Buildings/
│       └── BuildingServiceTests.cs
├── Architecture/
│   └── DomainDependencyTests.cs
└── Persistence/
  ├── DependencyInjectionTests.cs
  └── ModelMetadataTests.cs
```

## Testes arquiteturais

`DomainDependencyTests` inspeciona as referências do assembly Domain e impede dependências acidentais de frameworks e camadas externas.

Esse tipo de teste protege uma decisão estrutural que o compilador sozinho não conhece.

Evolução recomendada:

- proibir também dependência de Domain para Application;
- verificar que Application não referencia API nem Infrastructure;
- manter regras pequenas e legíveis, sem construir um framework arquitetural próprio.

## Testes de metadados EF Core

`ModelMetadataTests` cria opções Npgsql com uma connection string fictícia e acessa `context.Model`. Nenhuma conexão é aberta.

Os testes verificam:

1. nomes das nove tabelas;
2. índices únicos de `User.Email` e `AccessCard.CardNumber`;
3. obrigatoriedade das foreign keys.

Esse teste é rápido e detecta regressões de mapeamento. Ele não prova que uma migration aplica corretamente em PostgreSQL.

## Testes de Dependency Injection

`DependencyInjectionTests` constrói um `ServiceProvider` sem abrir conexão. Ele verifica:

1. lifetime scoped de `SmartBuildingDbContext`, com identidade dentro do mesmo scope e instâncias distintas entre scopes;
2. provider `Npgsql.EntityFrameworkCore.PostgreSQL`;
3. mapeamento de `AccessEvent.AccessPointId` para `access_point_id` em `snake_case`;
4. rejeição de connection string vazia ou composta apenas por espaços.

## Testes unitários de BuildingService

`BuildingServiceTests` substitui `IBuildingRepository` por um fake em memória. Oito métodos, incluindo uma teoria com dois cenários de limite, executam nove casos de teste que verificam:

1. mapeamento completo dos edifícios existentes para DTOs;
2. `null` ao consultar um ID desconhecido;
3. remoção de espaços de nome e endereço na criação;
4. rejeição de nome composto apenas por whitespace com `ApplicationValidationException`;
5. `null` ao atualizar um edifício inexistente;
6. normalização e substituição completa do estado editável numa atualização bem-sucedida;
7. rejeição de nome acima de 200 caracteres;
8. rejeição de endereço acima de 500 caracteres;
9. resultado `HasFloors` quando a remoção é bloqueada.

Esses testes isolam a orquestração da Application. Eles não exercitam Minimal APIs, EF Core ou PostgreSQL.

## Validação manual com PostgreSQL

A persistência também foi validada contra PostgreSQL 18 real num container Docker descartável, exposto na porta isolada `55432`. A verificação confirmou:

- criação das nove tabelas do domínio;
- registo de `InitialCreate` em `__EFMigrationsHistory`;
- resposta do endpoint `GET /` após a inicialização;
- aquisição de `pg_advisory_xact_lock(1937001)` pelo seed dentro da transação;
- uma linha em `buildings`, `floors` e `access_points`;
- manutenção das contagens `1|1|1` após a execução do seed.

Essa execução é evidência manual da mudança, não uma suíte de integração repetível. O uso de Docker foi apenas para isolar o PostgreSQL de validação e não representa a implementação da fase Docker.

Após o primeiro slice da Fase 4, um novo smoke test manual atravessou API, Application, Infrastructure e PostgreSQL real. A sequência observada foi `200/201/200/200/204/404/400/409`, cobrindo listagem, criação, consultas, atualização, remoção, recurso ausente, validação e conflito por pisos. Nos três cenários de erro observados, `400`, `404` e `409`, o content type foi `application/problem+json`. Essa evidência confirma o comportamento atual de `Buildings`, mas não substitui testes HTTP e de persistência automatizados.

## Categorias planejadas

### Testes unitários de Domain

Devem verificar regras puras sem banco, rede ou host HTTP:

- cartão ativo e permissão válida concedem acesso;
- cartão desconhecido, inativo ou expirado nega acesso;
- utilizador e ponto inativos negam acesso;
- permissão ausente ou expirada nega acesso;
- saída fecha sessão aberta;
- entrada duplicada e saída sem entrada geram inconsistência;
- transições de alerta respeitam o ciclo permitido.

### Testes unitários de Application

Devem verificar orquestração com contratos controlados:

- ordem das consultas e persistência;
- criação de evento para acessos permitidos e negados;
- propagação de cancelamento;
- resultado correto para cada cenário.

### Testes de integração de persistência

Devem usar PostgreSQL real, preferencialmente containerizado:

- migration cria o esquema;
- constraints e índices existem;
- unicidade de email e cartão é aplicada pelo banco;
- relações e `DeleteBehavior` funcionam;
- queries e transações produzem o resultado esperado.

Esses testes devem ficar num futuro projeto `SmartBuilding.IntegrationTests`, não em UnitTests.

### Testes de integração HTTP

Devem iniciar a API em memória e atravessar a cadeia completa:

```text
HTTP -> API -> Application -> Infrastructure -> PostgreSQL
```

Casos principais incluem login, CRUD, pedido de acesso, persistência do evento, ocupação e alertas.

### Testes de frontend

Quando Angular existir, serão necessários testes de componentes, services, guards e fluxos críticos. Testes end-to-end devem cobrir somente jornadas de maior valor.

## Convenção de nomes

Usar:

```text
Subject_Scenario_ExpectedResult
```

Exemplos:

```text
AccessCard_Expired_DeniesAccess
OccupancySession_ExitWithoutEntry_ReturnsInconsistency
AccessRequest_ValidPermission_CreatesGrantedEvent
```

O nome deve explicar comportamento, cenário e resultado, sem depender da implementação interna.

## Estrutura Arrange, Act, Assert

Testes de comportamento devem separar claramente:

```text
Arrange: preparar estado e dependências.
Act: executar uma única ação observável.
Assert: verificar resultado e efeitos importantes.
```

Não é necessário escrever comentários `Arrange`, `Act` e `Assert` quando a separação visual já for clara.

## Determinismo

Testes não devem depender do relógio real, ordem de execução, rede externa ou dados compartilhados. IDs e instantes relevantes devem ser definidos explicitamente.

## Comandos

Executar toda a suíte:

```bash
dotnet test SmartBuilding.slnx
```

Executar somente os metadados:

```bash
dotnet test tests/SmartBuilding.UnitTests/SmartBuilding.UnitTests.csproj \
  --filter 'FullyQualifiedName~ModelMetadataTests'
```

Executar somente os testes de registro:

```bash
dotnet test tests/SmartBuilding.UnitTests/SmartBuilding.UnitTests.csproj \
  --filter 'FullyQualifiedName~DependencyInjectionTests'
```

Executar somente os testes de `BuildingService`:

```bash
dotnet test tests/SmartBuilding.UnitTests/SmartBuilding.UnitTests.csproj \
  --filter 'FullyQualifiedName~BuildingServiceTests'
```

Build final da solução:

```bash
dotnet build SmartBuilding.slnx --no-restore
```

## Critério de entrega

Antes de solicitar commit:

1. executar o teste mais específico da alteração;
2. executar toda a suíte relevante;
3. compilar a solução completa;
4. verificar warnings e vulnerabilidades quando dependências mudarem;
5. reportar comandos e resultados.

## Próximos passos

Criar testes de integração automatizados para repetir a aplicação da migration, o seed e o CRUD de `Buildings` em PostgreSQL isolado. Os demais CRUDs e respetivos testes continuam planejados.
