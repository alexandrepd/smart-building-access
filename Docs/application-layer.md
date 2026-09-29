# Camada Application

## Propósito

`SmartBuilding.Application` será responsável por executar casos de uso. Ela coordena regras do Domain e operações externas através de contratos, sem conhecer HTTP, PostgreSQL ou detalhes de interface.

## Estado atual

O projeto está criado e referencia somente `SmartBuilding.Domain`. Ainda não contém serviços, DTOs, interfaces ou casos de uso.

Esse estado é intencional: a camada foi criada como fronteira arquitetural, mas será preenchida progressivamente quando os primeiros fluxos funcionais forem implementados.

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

## Estrutura planejada

Uma organização possível, criada somente conforme os casos de uso surgirem:

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

Quando um caso de uso precisar de dados, a Application deve definir o contrato mínimo necessário. Infrastructure implementa esse contrato.

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

Categorias planejadas:

- requests de criação e atualização;
- responses de consulta;
- resultado de pedido de acesso;
- paginação e filtros;
- resumos de ocupação.

A API pode reutilizar DTOs de Application quando o contrato do caso de uso e o contrato HTTP forem equivalentes. Quando não forem, a API deve fazer o mapeamento na borda.

## Validação

Application valida requisitos do caso de uso, como formato, presença de dados e combinações inválidas. Domain protege invariantes que devem ser verdadeiras independentemente do caso de uso.

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

Application deve usar resultados ou exceções específicas do caso de uso, sem retornar `IResult`, `ActionResult` ou códigos HTTP. A API traduz esses resultados para o protocolo HTTP.

## Testes

Casos de uso devem ser testados isoladamente com implementações controladas dos contratos. Os testes devem cobrir sucesso, negação, dados inexistentes, limites de validade e cancelamento quando relevante.

## Próximos passos da camada

A Application permanece estruturalmente vazia durante a conclusão da Fase 3. O primeiro caso de uso real será introduzido na fase apropriada, sem antecipar toda a arquitetura de uma vez.
