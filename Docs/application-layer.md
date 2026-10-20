# Camada Application

## Propósito

`SmartBuilding.Application` é responsável por executar casos de uso. Ela coordena regras do Domain e operações externas através de contratos, sem conhecer HTTP, PostgreSQL ou detalhes de interface.

## Estado atual

Os vertical slices CRUD de `Buildings`, `Floors` e `AccessPoints` estão implementados. A camada contém:

- `IBuildingService` e `BuildingService` para os cinco casos de uso CRUD;
- `CreateBuildingCommand` e `UpdateBuildingCommand`;
- `BuildingDto` como saída da Application;
- `IBuildingRepository` como contrato de persistência;
- `DeleteBuildingResult` para distinguir remoção, ausência e conflito com pisos;
- `IFloorService` e `FloorService` para os cinco casos de uso CRUD;
- commands, DTO e contrato de persistência próprios de `Floor`;
- `FloorSaveResult` e `FloorSaveStatus` para distinguir `Success`, `FloorNotFound` e `BuildingNotFound`;
- `DeleteFloorResult` para distinguir remoção, ausência e conflito com dependentes;
- `IAccessPointService` e `AccessPointService` para os cinco casos de uso CRUD;
- `CreateAccessPointCommand` e `UpdateAccessPointCommand`;
- `AccessPointDto` como saída da Application;
- `IAccessPointRepository` como contrato de persistência;
- `AccessPointSaveResult` e `AccessPointSaveStatus` para distinguir sucesso e ausência do ponto ou piso;
- `AccessPointPersistenceResult` e `DeleteAccessPointResult` para representar persistência e remoção bloqueada por dependentes;
- `AddApplication` para registrar os serviços com lifetime scoped.
- `IUserService` e `UserService` para os cinco casos de uso CRUD;
- commands, `UserDto`, `IUserRepository` e resultados explícitos de gravação e remoção;
- `IPasswordHashService` como abstração para hash da password na criação.
- `IAccessPermissionService` e `AccessPermissionService` para consultar, criar, substituir e remover `AccessPermission`s;
- commands, `AccessPermissionDto`, `IAccessPermissionRepository` e resultados explícitos para persistência e referências inexistentes.
- `IAccessControlService` e `AccessControlService` para processar pedidos, consultar eventos e obter um evento por ID;
- commands, DTOs, contexto da decisão e resultados explícitos do fluxo de acesso;
- `IAuthenticationService` e `AuthenticationService` para normalizar email, verificar credenciais de User ativo e solicitar emissão de token;
- commands e resultados explícitos para login, além do contrato de `IAccessTokenService` fornecido pela API;
- extensão do caso de uso de Users para atribuir papel, incluindo a proteção contra rebaixar o último Admin;
- `TimeProvider` para obter o instante UTC de forma substituível em testes.

O processamento/decisão, auditoria por `AccessEvent` e caso de uso de login estão implementados. A Application valida credenciais e delega a criação do token ao contrato `IAccessTokenService`; assinatura e transporte JWT pertencem à API. A atualização de `OccupancySession`, notificações SignalR e processamento de alertas permanecem planejados para fases futuras. A gestão CRUD de `AccessPermission` e a decisão no momento do pedido são casos de uso distintos.

No slice `Users`, `UserService` normaliza nome e email, converte email para lowercase, valida o formato do endereço e exige password entre 12 e 128 caracteres na criação. A password é encaminhada para `IPasswordHashService`; a atualização recebe apenas nome, email e `IsActive`, nunca uma password.

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

O fluxo implementado de decisão:

```text
1. Validar e normalizar o número do cartão; validar AccessPointId e AccessDirection.
2. Obter um único instante UTC e carregar os dados de decisão para o AccessPoint.
3. Se o AccessPoint não existir, devolver resultado de ausência; não criar evento.
4. Avaliar na ordem: cartão ausente, cartão inativo, cartão expirado,
   User inativo, AccessPoint inativo, direção não suportada, sem permissão
   ativa, permissão ativa fora da validade atual e, por fim, concedido.
5. Criar e persistir um AccessEvent para qualquer decisão processada num
   AccessPoint existente, concedida ou negada.
6. Devolver o evento/result; a API mapeia o resultado para HTTP.
```

Uma permissão só está dentro da sua janela temporal quando
`ValidFrom <= now < ValidUntil`; `ValidUntil == null` não impõe um fim.
Application usa `TimeProvider`, define a ordem e o resultado de negócio, mas
não decide como os dados são lidos do PostgreSQL nem como a resposta vira HTTP.
O pedido processado retorna `200 OK` tanto para `Granted` como para qualquer
resultado de negação: o cliente inspeciona `result` e `reason` no evento.

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
├── AccessPoints/
│   ├── AccessPointDto.cs
│   ├── AccessPointPersistenceResult.cs
│   ├── AccessPointSaveResult.cs
│   ├── AccessPointSaveStatus.cs
│   ├── AccessPointService.cs
│   ├── CreateAccessPointCommand.cs
│   ├── DeleteAccessPointResult.cs
│   ├── IAccessPointRepository.cs
│   ├── IAccessPointService.cs
│   └── UpdateAccessPointCommand.cs
├── AccessPermissions/
│   ├── AccessPermissionDto.cs
│   ├── AccessPermissionService.cs
│   ├── CreateAccessPermissionCommand.cs
│   ├── IAccessPermissionRepository.cs
│   ├── IAccessPermissionService.cs
│   └── UpdateAccessPermissionCommand.cs
├── AccessControl/
│   ├── AccessControlService.cs
│   ├── AccessDecisionContext.cs
│   ├── AccessDecisionStatus.cs
│   ├── AccessEventDto.cs
│   ├── AccessRequestCommand.cs
│   ├── AccessRequestResult.cs
│   ├── IAccessControlRepository.cs
│   └── IAccessControlService.cs
├── Common/
│   └── ApplicationValidationException.cs
└── DependencyInjection.cs
```

As áreas de `Occupancy` e `SecurityAlerts` continuam futuras; a estrutura
mostrada acima para `AccessControl` corresponde ao código desta branch.

## Contratos de persistência

Quando um caso de uso precisa de dados, a Application define o contrato mínimo necessário. Infrastructure implementa esse contrato.

`IBuildingRepository`, `IFloorRepository` e `IAccessControlRepository`
demonstram essa direção: oferecem operações necessárias sem expor `DbSet`,
`IQueryable`, `DbContext` ou tipos HTTP. O contrato de Access Control fornece
contexto para uma decisão e operações de gravação/consulta de `AccessEvent`;
o serviço pode ser testado com uma implementação em memória.

Contrato de leitura ilustrativo:

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

Nos slices de `Buildings`, `Floors`, `AccessPoints` e `Users`, commands representam entrada de criação e atualização, enquanto DTOs próprios representam saída. `UserDto` não contém password nem hash. `CreateUserCommand` transporta a password somente até ao serviço de criação; `UpdateUserCommand` não possui esse campo. Para `AccessPoint`, o serviço remove espaços nas extremidades de nome e localização, valida os limites de 150 e 250 caracteres e preserva `SupportsEntry` e `SupportsExit`. A API mantém requests e responses próprios e faz o mapeamento na borda.

Capacidades ainda planejadas:

- requests de criação e atualização;
- responses de consulta;
- paginação e filtros;
- resumos de ocupação.

A API pode reutilizar DTOs de Application quando o contrato do caso de uso e o contrato HTTP forem equivalentes. Quando não forem, a API deve fazer o mapeamento na borda.

## Validação

Application valida requisitos do caso de uso, como formato, presença de dados e combinações inválidas. Domain protege invariantes que devem ser verdadeiras independentemente do caso de uso.

`BuildingService` rejeita nome ou endereço vazios ou compostos apenas por whitespace, remove espaços nas extremidades e limita nome a 200 e endereço a 500 caracteres. `FloorService` exige `BuildingId` diferente de `Guid.Empty`, rejeita nome vazio ou composto apenas por whitespace, remove espaços nas extremidades e limita o nome a 100 caracteres. Para essas falhas, os serviços lançam `ApplicationValidationException` com o nome da propriedade inválida. Essa proteção continua válida quando o serviço é chamado sem passar pela validação HTTP.

`UserService` exige nome não vazio de até 200 caracteres, email não vazio e válido de até 320 caracteres e password de 12 a 128 caracteres na criação. Espaços externos são removidos e o email é convertido para lowercase invariável. O serviço encaminha a password original para `IPasswordHashService` e recebe o hash antes de persistir; nenhum DTO de saída contém material de password.

`AccessPermissionService` exige `UserId` e `AccessPointId` diferentes de `Guid.Empty`, converte `ValidFrom` e `ValidUntil` de `DateTimeOffset` para `DateTime` UTC e rejeita `ValidUntil` quando for igual ou anterior ao instante de `ValidFrom`. Um `ValidUntil` nulo é permitido. A criação não recebe `IsActive`; o repositório cria a permissão ativa. O command de atualização inclui `IsActive`, pois o `PUT` substitui também o estado ativo. User ou AccessPoint inexistente são resultados explícitos do contrato de persistência, convertidos pela Application sem referenciar tipos ou status HTTP.

Exemplos:

- Application valida que um request contém `cardNumber` e `accessPointId`.
- Application aplica a ordem da decisão usando as entidades e enums do Domain.
- API transforma falhas de validação em respostas HTTP adequadas.

## Assincronismo e cancelamento

Operações que realizam I/O devem ser assíncronas, terminar em `Async` e receber `CancellationToken`. Regras puras em memória não precisam ser assíncronas.

## Transações

Application define o limite lógico da operação. Infrastructure fornece o mecanismo transacional.

Atualmente `AccessControlRepository` persiste o `AccessEvent` da decisão.
Quando a atualização de ocupação for implementada, a gravação do evento e a
alteração de `OccupancySession` poderão precisar fazer parte da mesma unidade
de trabalho; essa consistência ainda é futura.

## Erros

Application usa resultados ou exceções específicas do caso de uso, sem retornar `IResult`, `ActionResult` ou códigos HTTP. `ApplicationValidationException` representa entrada inválida na fronteira da Application, enquanto exceções inesperadas não são classificadas como erro do cliente. No delete de edifício, `DeleteBuildingResult` informa `Deleted`, `NotFound` ou `HasFloors`. Para pisos, `FloorSaveStatus` informa `Success`, `FloorNotFound` ou `BuildingNotFound`, e `DeleteFloorResult` informa `Deleted`, `NotFound` ou `HasDependents`. Para pontos de acesso, `AccessPointSaveStatus` distingue sucesso, `AccessPointNotFound` e `FloorNotFound`; `DeleteAccessPointResult` distingue `Deleted`, `NotFound` e `HasDependents`. A API traduz esses resultados para a semântica HTTP correspondente.

Para permissões, `AccessPermissionSaveStatus` distingue sucesso, permissão inexistente, User inexistente e AccessPoint inexistente. A remoção distingue `Deleted` de `NotFound`. Esses resultados deixam a API mapear IDs/referências ausentes para `404`; não representam uma decisão de permitir ou negar entrada.

`FloorPersistenceResult` e `FloorSaveResult` possuem factories restritas: sucesso sempre transporta uma entidade ou DTO, enquanto falhas não podem carregar esses valores. Isso evita estados contraditórios antes do mapeamento HTTP.

`AccessPointPersistenceResult` e `AccessPointSaveResult` seguem a mesma proteção: os construtores são privados, sucesso é criado com `Succeeded` e os resultados `AccessPointNotFound` ou `FloorNotFound` são criados sem entidade/DTO. Assim, cada factory mantém consistente o status com o valor transportado.

Para Users, `UserSaveResult` distingue sucesso, utilizador inexistente e email duplicado; `DeleteUserResult` distingue remoção, ausência e dependências. Esses resultados mantêm a Application independente dos status HTTP.

## Testes

Os nove casos de teste de `BuildingService` usam um repositório fake e cobrem mapeamento completo da lista, consulta inexistente, normalização na criação, nome vazio, atualização inexistente, atualização bem-sucedida, os limites máximos de nome e endereço e conflito de remoção com pisos.

Os oito casos de teste de `FloorService` também usam um repositório fake e cobrem mapeamento da lista, normalização na criação, `BuildingId` vazio, edifício inexistente, substituição do estado editável, conflito de remoção e nomes vazio ou composto apenas por whitespace.

Os dez casos de teste de `AccessPointService` usam um repositório fake e cobrem mapeamento dos indicadores de direção, normalização de nome e localização, `FloorId` vazio, piso inexistente, substituição do estado editável, ponto inexistente, conflito de remoção e texto obrigatório vazio ou composto apenas por whitespace.

Os 15 casos de `UserService` cobrem normalização de email e persistência somente do hash, DTOs sem material de password, email inválido ou duplicado na criação e atualização, consulta e atualização de utilizador inexistente, atualização sem alterar o hash existente, remoção com e sem dependentes, nome/email/password fora dos limites e nome composto apenas por whitespace. Testes separados verificam o hash Identity e a validação do atributo HTTP; o conjunto focado em User/hash/API totaliza 20 casos. A verificação de uma password correta ou incorreta testa a primitiva do hasher, não um fluxo de login.

## Próximos passos da camada

Implementar progressivamente os casos de uso restantes. A gestão CRUD de
`AccessPermission` e a decisão/auditoria de Access Control estão
implementadas. Atualização de `OccupancySession` e processamento de alertas
permanecem planejados e não devem ser inferidos a partir do fluxo atual.
