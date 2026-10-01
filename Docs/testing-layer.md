# Estratégia de Testes

## Propósito

Os testes fornecem feedback sobre regras, fronteiras arquiteturais, persistência e comportamento HTTP. A suíte deve crescer de acordo com o risco de cada fase.

## Estado atual

Existe um projeto `SmartBuilding.UnitTests` com xUnit. Ele contém:

- um teste arquitetural de independência do Domain;
- testes dos metadados do modelo EF Core;
- testes do registro de persistência em Dependency Injection;
- nove casos de teste unitário de `BuildingService`;
- oito casos de teste unitário de `FloorService`;
- dez casos de teste unitário de `AccessPointService`;
- 15 casos de teste unitário de `UserService`;
- três casos de `IdentityPasswordHashService`;
- dois casos de validação HTTP do request de User.

Os testes focados em User/hash/API totalizam 20 casos: 15 de Application, três de Infrastructure e dois de API.

Ainda não existe `SmartBuilding.IntegrationTests`.

## Organização atual

```text
SmartBuilding.UnitTests/
├── Application/
│   ├── Buildings/
│   │   └── BuildingServiceTests.cs
│   ├── Floors/
│   │   └── FloorServiceTests.cs
│   ├── AccessPoints/
│   │   └── AccessPointServiceTests.cs
│   └── Users/
│       └── UserServiceTests.cs
├── Api/
│   └── Users/
│       └── UserRequestValidationTests.cs
├── Architecture/
│   └── DomainDependencyTests.cs
├── Infrastructure/
│   └── IdentityPasswordHashServiceTests.cs
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

## Testes unitários de FloorService

`FloorServiceTests` substitui `IFloorRepository` por um fake em memória. Seis métodos de fato e uma teoria com dois cenários executam oito casos de teste que verificam:

1. mapeamento completo dos pisos existentes para DTOs;
2. normalização do nome na criação;
3. rejeição de `BuildingId` vazio;
4. resultado `BuildingNotFound` para edifício pai desconhecido;
5. substituição completa do estado editável numa atualização bem-sucedida;
6. resultado `HasDependents` quando a remoção é bloqueada;
7. rejeição de nome vazio;
8. rejeição de nome composto apenas por whitespace.

Esses testes exercitam a orquestração e os resultados explícitos da Application, mas não exercitam Minimal APIs, EF Core ou PostgreSQL.

## Testes unitários de AccessPointService

`AccessPointServiceTests` usa um fake de `IAccessPointRepository` e cobre dez casos: mapeamento dos indicadores `SupportsEntry` e `SupportsExit`, normalização de texto na criação, `FloorId` vazio, piso inexistente, substituição do estado editável, ponto inexistente, conflito de remoção e três combinações de nome/localização vazios ou compostos apenas por whitespace. Esses testes validam Application isoladamente; não executam endpoint, EF Core nem PostgreSQL.

`UserServiceTests` usa fakes de `IUserRepository` e `IPasswordHashService` e cobre 15 casos, incluindo email canonicalizado e persistência somente do hash, DTO de listagem sem material de password, email inválido ou duplicado tanto na criação quanto na atualização, password abaixo/acima dos limites, nome acima do limite ou composto apenas por whitespace, email acima do limite, atualização sem alterar o hash, utilizador inexistente e delete com/sem dependentes.

`IdentityPasswordHashServiceTests` cobre três casos: hash diferente do texto original, salt aleatório e verificação da password correta versus rejeição da incorreta. `UserRequestValidationTests` cobre dois casos do atributo HTTP: email válido com whitespace externo é aceito e email inválido é rejeitado. Somados, são 20 casos focados em User/hash/API. A validação de hash é teste da primitiva Identity; não existe ainda caso de uso de login ou verificação de credenciais.

## Validação manual com PostgreSQL

A persistência também foi validada contra PostgreSQL 18 real num container Docker descartável, exposto na porta isolada `55432`. A verificação confirmou:

- criação das nove tabelas do domínio;
- registo de `InitialCreate` em `__EFMigrationsHistory`;
- resposta do endpoint `GET /` após a inicialização;
- aquisição de `pg_advisory_xact_lock(1937001)` pelo seed dentro da transação;
- uma linha em `buildings`, `floors` e `access_points`;
- manutenção das contagens `1|1|1` após a execução do seed.

Essa execução é evidência manual da mudança, não uma suíte de integração repetível. O uso de Docker foi apenas para isolar o PostgreSQL de validação e não representa a implementação da fase Docker.

Após o slice de `Buildings`, um smoke test manual atravessou API, Application, Infrastructure e PostgreSQL real. A sequência observada foi `200/201/200/200/204/404/400/409`, cobrindo listagem, criação, consultas, atualização, remoção, recurso ausente, validação e conflito por pisos. Nos três cenários de erro observados, `400`, `404` e `409`, o content type foi `application/problem+json`.

O smoke manual de `Floors` observou `200/201/200/200/404/409/204/204`: listagem, criação, consulta, atualização, edifício pai inexistente, conflito por dependente, remoção do piso temporário e limpeza do edifício temporário. Essa evidência confirma os três slices administrativos então validados, mas não substitui testes HTTP e de persistência automatizados.

O smoke manual de `AccessPoints` observou `200/201/200/200/404/409/204`: listagem, criação, consulta, atualização, piso pai inexistente, conflito por evento dependente e remoção após a limpeza desse evento. Os recursos temporários também foram limpos. A execução confirma o caminho HTTP e a persistência real para esse cenário, mas continua sendo validação manual, não uma suíte de integração repetível.

O smoke manual de `Users` contra PostgreSQL real aceitou email uppercase com espaços externos: criação `201` e valor canonicalizado para lowercase. Email duplicado retornou `409`; consulta, atualização e remoção retornaram `200/200/204`. O banco armazenou hash do `PasswordHasher<User>` e a response não continha password nem hash. Delete com cartão retornou `409`; delete com evento de auditoria retornou `204`, mantendo o evento com `UserId` nulo. Essa validação atravessou API e persistência real, mas não substitui testes de integração automatizados.

Depois da revisão do repositório, os caminhos associados às constraints específicas foram repetidos no PostgreSQL real: parent inexistente permaneceu `404` e delete com dependente permaneceu `409`. Tracking após falha e deletes simultâneos ainda devem receber testes de integração automatizados numa fase posterior.

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

Executar somente os testes de `FloorService`:

```bash
dotnet test tests/SmartBuilding.UnitTests/SmartBuilding.UnitTests.csproj \
  --filter 'FullyQualifiedName~FloorServiceTests'
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

Criar testes de integração automatizados para repetir a aplicação da migration, o seed e os CRUDs administrativos implementados em PostgreSQL isolado. O processamento de autorização, eventos, ocupação e alertas, bem como seus testes, continua planejado.
