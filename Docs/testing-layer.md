# Estratégia de Testes

## Propósito

Os testes fornecem feedback sobre regras, fronteiras arquiteturais, persistência e comportamento HTTP. A suíte deve crescer de acordo com o risco de cada fase.

## Estado atual

Existe um projeto `SmartBuilding.UnitTests` com xUnit. Ele contém:

- um teste arquitetural de independência do Domain;
- testes dos metadados do modelo EF Core.

Na validação mais recente, a solução executou 15 testes com sucesso.

Ainda não existe `SmartBuilding.IntegrationTests`.

## Organização atual

```text
SmartBuilding.UnitTests/
├── Architecture/
│   └── DomainDependencyTests.cs
└── Persistence/
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

Ao concluir a Fase 3, criar testes de integração para a migration em PostgreSQL. Quando regras de negócio forem adicionadas, expandir UnitTests antes de construir endpoints sobre elas.
