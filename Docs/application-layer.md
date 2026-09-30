# Camada Application

## Propósito

`SmartBuilding.Application` é responsável por executar casos de uso. Ela coordena regras do Domain e operações externas através de contratos, sem conhecer HTTP, PostgreSQL ou detalhes de interface.

## Estado atual

Os vertical slices de `Buildings` e `Floors` estão implementados. A camada contém:

- `IBuildingService` e `BuildingService` para os cinco casos de uso CRUD;
- `CreateBuildingCommand` e `UpdateBuildingCommand`;
- `BuildingDto` como saída da Application;
- `IBuildingRepository` como contrato de persistência;
- `DeleteBuildingResult` para distinguir remoção, ausência e conflito com pisos;
- `IFloorService` e `FloorService` para os cinco casos de uso CRUD;
- commands, DTO e contrato de persistência próprios de `Floor`;
- `FloorSaveResult` e `FloorSaveStatus` para distinguir `Success`, `FloorNotFound` e `BuildingNotFound`;
- `DeleteFloorResult` para distinguir remoção, ausência e conflito com dependentes;
- `AddApplication` para registrar os serviços com lifetime scoped.

Os casos de uso dos demais recursos continuam planejados e serão adicionados progressivamente.

## Dependências

### Permitidas

- `SmartBuilding.Domain`;
- bibliotecas de abstrações que não introduzam detalhes de transporte ou persistência;
- contratos definidos pela própria Application.

### Proibidas

- `SmartBuilding.Api`;
- `SmartBuilding.Infrastructure`;
- `DbContext`, Npgsql e SQL;
- controllers, endpoints, `HttpContext` e códigos HTTP;
- componentes Angular ou contratos específicos de UI.

## Papel na arquitetura

Domain responde “quais regras são verdadeiras?”. Application responde “em que ordem o caso de uso deve acontecer?”.

Exemplo planejado para um pedido de acesso:

```text
1. Receber AccessRequest.
2. Consultar cartão pelo número.
3. Consultar utilizador e ponto de acesso.
4. Consultar permissões válidas.
5. Aplicar regras do Domain.
6. Criar AccessEvent independentemente do resultado.
7. Persistir a unidade de trabalho.
8. Retornar AccessResponse.
```

Application decide essa sequência, mas não decide como os dados são lidos do PostgreSQL nem como a resposta vira HTTP.

## Estrutura atual e planejada

O slice atual está organizado por funcionalidade:

```text
SmartBuilding.Application/
├── Buildings/
│   ├── BuildingDto.cs
│   ├── BuildingService.cs
│   ├── CreateBuildingCommand.cs
│   ├── DeleteBuildingResult.cs
│   ├── IBuildingRepository.cs
│   ├── IBuildingService.cs
│   └── UpdateBuildingCommand.cs
├── Floors/
│   ├── CreateFloorCommand.cs
│   ├── DeleteFloorResult.cs
│   ├── FloorDto.cs
│   ├── FloorPersistenceResult.cs
│   ├── FloorSaveResult.cs
│   ├── FloorSaveStatus.cs
│   ├── FloorService.cs
│   ├── IFloorRepository.cs
│   ├── IFloorService.cs
│   └── UpdateFloorCommand.cs
├── Common/
│   └── ApplicationValidationException.cs
└── DependencyInjection.cs
```

Outras áreas serão criadas somente conforme os casos de uso surgirem, por exemplo:

```text
SmartBuilding.Application/
├── Abstractions/
│   ├── Persistence/
│   └── Time/
├── AccessControl/
│   ├── AccessRequest.cs
│   ├── AccessResponse.cs
│   └── ProcessAccessRequest.cs
├── Occupancy/
├── SecurityAlerts/
└── Common/
```

Essa estrutura é uma direção, não código já implementado.

## Contratos de persistência

Quando um caso de uso precisa de dados, a Application define o contrato mínimo necessário. Infrastructure implementa esse contrato.

`IBuildingRepository` e `IFloorRepository` demonstram essa direção: oferecem as operações necessárias aos respetivos CRUDs sem expor `DbSet`, `IQueryable`, `DbContext` ou tipos HTTP. Os serviços dependem dessas interfaces e podem ser testados com implementações em memória.

Exemplo conceitual:

```csharp
public interface IAccessCardLookup
{
    Task<AccessCard?> FindByNumberAsync(
        string cardNumber,
        CancellationToken cancellationToken);
}
```

O contrato expressa a necessidade do caso de uso e não expõe `DbSet`, `IQueryable` ou `DbContext`.

Não será criado um `GenericRepository<T>` apenas por convenção. Abstrações devem representar operações reais do negócio ou limites úteis para testes.

## DTOs

DTOs definem os dados de entrada e saída dos casos de uso. Eles não devem ser entidades EF nem transportar propriedades de navegação.

Nos slices de `Buildings` e `Floors`, commands representam entrada de criação e atualização, enquanto `BuildingDto` e `FloorDto` representam saída. A API mantém requests e responses próprios e faz o mapeamento na borda.

Categorias ainda planejadas:

- requests de criação e atualização;
- responses de consulta;
- resultado de pedido de acesso;
- paginação e filtros;
- resumos de ocupação.

A API pode reutilizar DTOs de Application quando o contrato do caso de uso e o contrato HTTP forem equivalentes. Quando não forem, a API deve fazer o mapeamento na borda.

## Validação

Application valida requisitos do caso de uso, como formato, presença de dados e combinações inválidas. Domain protege invariantes que devem ser verdadeiras independentemente do caso de uso.

`BuildingService` rejeita nome ou endereço vazios ou compostos apenas por whitespace, remove espaços nas extremidades e limita nome a 200 e endereço a 500 caracteres. `FloorService` exige `BuildingId` diferente de `Guid.Empty`, rejeita nome vazio ou composto apenas por whitespace, remove espaços nas extremidades e limita o nome a 100 caracteres. Para essas falhas, os serviços lançam `ApplicationValidationException` com o nome da propriedade inválida. Essa proteção continua válida quando o serviço é chamado sem passar pela validação HTTP.

Exemplos:

- Application valida que um request contém `cardNumber` e `accessPointId`.
- Domain decide se um cartão ativo e uma permissão válida concedem acesso.
- API transforma falhas de validação em respostas HTTP adequadas.

## Assincronismo e cancelamento

Operações que realizam I/O devem ser assíncronas, terminar em `Async` e receber `CancellationToken`. Regras puras em memória não precisam ser assíncronas.

## Transações

Application define o limite lógico da operação. Infrastructure fornece o mecanismo transacional.

Para um acesso concedido, a gravação de `AccessEvent` e a atualização de ocupação podem precisar fazer parte da mesma unidade de trabalho. A decisão será implementada quando esse fluxo existir.

## Erros

Application usa resultados ou exceções específicas do caso de uso, sem retornar `IResult`, `ActionResult` ou códigos HTTP. `ApplicationValidationException` representa entrada inválida na fronteira da Application, enquanto exceções inesperadas não são classificadas como erro do cliente. No delete de edifício, `DeleteBuildingResult` informa `Deleted`, `NotFound` ou `HasFloors`. Para pisos, `FloorSaveStatus` informa `Success`, `FloorNotFound` ou `BuildingNotFound`, e `DeleteFloorResult` informa `Deleted`, `NotFound` ou `HasDependents`. A API traduz esses resultados para a semântica HTTP correspondente.

`FloorPersistenceResult` e `FloorSaveResult` possuem factories restritas: sucesso sempre transporta uma entidade ou DTO, enquanto falhas não podem carregar esses valores. Isso evita estados contraditórios antes do mapeamento HTTP.

## Testes

Os nove casos de teste de `BuildingService` usam um repositório fake e cobrem mapeamento completo da lista, consulta inexistente, normalização na criação, nome vazio, atualização inexistente, atualização bem-sucedida, os limites máximos de nome e endereço e conflito de remoção com pisos.

Os oito casos de teste de `FloorService` também usam um repositório fake e cobrem mapeamento da lista, normalização na criação, `BuildingId` vazio, edifício inexistente, substituição do estado editável, conflito de remoção e nomes vazio ou composto apenas por whitespace.

## Próximos passos da camada

Completar os CRUDs dos recursos seguintes de forma incremental. O fluxo de controlo de acesso, ocupação e alertas continua planejado e não deve ser inferido a partir dos CRUDs de `Buildings` e `Floors`.
