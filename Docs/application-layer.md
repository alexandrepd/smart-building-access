# Camada Application

## Propósito

`SmartBuilding.Application` é responsável por executar casos de uso. Ela coordena regras do Domain e operações externas através de contratos, sem conhecer HTTP, PostgreSQL ou detalhes de interface.

## Estado atual

O primeiro vertical slice da Fase 4 está implementado para `Buildings` na branch `feat/building-api`. A camada contém:

- `IBuildingService` e `BuildingService` para os cinco casos de uso CRUD;
- `CreateBuildingCommand` e `UpdateBuildingCommand`;
- `BuildingDto` como saída da Application;
- `IBuildingRepository` como contrato de persistência;
- `DeleteBuildingResult` para distinguir remoção, ausência e conflito com pisos;
- `AddApplication` para registrar o serviço com lifetime scoped.

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

`IBuildingRepository` demonstra essa direção: oferece listagem, consulta por ID, criação, atualização e remoção sem expor `DbSet`, `IQueryable`, `DbContext` ou tipos HTTP. `BuildingService` depende dessa interface e pode ser testado com uma implementação em memória.

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

No slice de `Buildings`, commands representam entrada de criação e atualização, enquanto `BuildingDto` representa saída. A API mantém requests e responses próprios e faz o mapeamento na borda.

Categorias ainda planejadas:

- requests de criação e atualização;
- responses de consulta;
- resultado de pedido de acesso;
- paginação e filtros;
- resumos de ocupação.

A API pode reutilizar DTOs de Application quando o contrato do caso de uso e o contrato HTTP forem equivalentes. Quando não forem, a API deve fazer o mapeamento na borda.

## Validação

Application valida requisitos do caso de uso, como formato, presença de dados e combinações inválidas. Domain protege invariantes que devem ser verdadeiras independentemente do caso de uso.

`BuildingService` rejeita nome ou endereço vazios ou compostos apenas por whitespace, remove espaços nas extremidades e limita nome a 200 e endereço a 500 caracteres. Para essas falhas, lança `ApplicationValidationException` com o nome da propriedade inválida. Essa proteção continua válida quando o serviço é chamado sem passar pela validação HTTP.

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

Application usa resultados ou exceções específicas do caso de uso, sem retornar `IResult`, `ActionResult` ou códigos HTTP. `ApplicationValidationException` representa entrada inválida na fronteira da Application, enquanto exceções inesperadas não são classificadas como erro do cliente. No delete, `DeleteBuildingResult` informa `Deleted`, `NotFound` ou `HasFloors`; a API traduz esses valores para `204`, `404` ou `409`.

## Testes

Os nove casos de teste de `BuildingService` usam um repositório fake e cobrem mapeamento completo da lista, consulta inexistente, normalização na criação, nome vazio, atualização inexistente, atualização bem-sucedida, os limites máximos de nome e endereço e conflito de remoção com pisos.

## Próximos passos da camada

Completar os CRUDs dos demais recursos de forma incremental. O fluxo de controlo de acesso, ocupação e alertas continua planejado e não deve ser inferido a partir do CRUD de `Buildings`.
