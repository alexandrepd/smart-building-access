# Camada Domain

## Propósito

`SmartBuilding.Domain` representa o núcleo conceitual do sistema. A camada define os termos do negócio e deve continuar válida mesmo que PostgreSQL, ASP.NET Core ou Angular sejam substituídos.

## Dependências

### Permitidas

- biblioteca padrão do .NET;
- tipos internos do próprio Domain.

### Proibidas

- `SmartBuilding.Application`;
- `SmartBuilding.Infrastructure`;
- `SmartBuilding.Api`;
- Entity Framework Core e Npgsql;
- ASP.NET Core e tipos HTTP;
- atributos de persistência ou serialização usados para adaptar frameworks.

O teste arquitetural `DomainDependencyTests` protege parte dessa fronteira automaticamente.

## Estrutura atual

```text
SmartBuilding.Domain/
├── Entities/
│   ├── AccessCard.cs
│   ├── AccessEvent.cs
│   ├── AccessPermission.cs
│   ├── AccessPoint.cs
│   ├── Building.cs
│   ├── Floor.cs
│   ├── OccupancySession.cs
│   ├── SecurityAlert.cs
│   └── User.cs
└── Enums/
    ├── AccessDirection.cs
    ├── AccessResult.cs
    ├── AlertStatus.cs
    └── AlertType.cs
```

## Entidades

### Building

Representa um edifício. É a raiz estrutural que contém pisos. Possui nome, endereço, estado ativo e data de criação.

### Floor

Representa um piso dentro de um edifício. Relaciona-se com pontos de acesso e sessões de ocupação.

### AccessPoint

Representa um ponto físico controlado, como entrada principal, saída de piso ou acesso à sala de servidores. O termo correto é `AccessPoint`, e não `Door`, porque um ponto de controlo pode representar mais do que uma porta.

As propriedades `SupportsEntry` e `SupportsExit` indicam as direções físicas aceitas pelo ponto.

### User

Representa uma pessoa autorizada. Mantém os vínculos com cartões, permissões, eventos e sessões de ocupação. `PasswordHash` deve armazenar somente hash seguro, nunca password em texto puro.

### AccessCard

Representa um cartão ou badge. Um cartão pertence a um utilizador, pode ser desativado e pode expirar.

### AccessPermission

Representa a autorização configurada para um `User` num `AccessPoint`, com `ValidFrom`, `ValidUntil` opcional e `IsActive`. A entidade tem estado ativo por omissão; a gestão CRUD aplica a validação do intervalo na Application. No fluxo implementado de decisão, a permissão só é considerada válida quando está ativa e `ValidFrom <= now < ValidUntil`; quando `ValidUntil` é nulo, não existe limite final.

### AccessEvent

Representa uma tentativa de acesso num `AccessPoint` conhecido, permitida ou negada. É um registro de auditoria; cada pedido processado para um ponto existente cria um `AccessEvent`, mesmo quando a decisão é negar.

`AccessCardId` e `UserId` são opcionais porque um cartão desconhecido não possui vínculos internos. `AccessPointId` é obrigatório por chave estrangeira; por isso, um pedido que referencia um AccessPoint desconhecido retorna `404` e não pode gerar evento.

### OccupancySession

Representa um intervalo derivado de permanência de um utilizador num piso. `ExitedAt == null` indica uma sessão aberta.

### SecurityAlert

Representa uma situação que exige investigação, como tentativas negadas repetidas ou uso de cartão expirado.

## Enums

### AccessDirection

- `Entry`: entrada.
- `Exit`: saída.

Direção não é resultado. Uma entrada pode ser concedida ou negada.

### AccessResult

Categoriza o resultado da avaliação: `Granted`, `CardNotFound`, `CardInactive`, `CardExpired`, `UserInactive`, `AccessPointInactive`, `DirectionNotSupported`, `NoPermission` ou `PermissionExpired`. O resultado também inclui a razão textual persistida no `AccessEvent`.

### AlertType e AlertStatus

Definem, respectivamente, a causa do alerta e o seu ciclo de tratamento: aberto, em investigação ou resolvido.

## Relações

```mermaid
erDiagram
    BUILDING ||--o{ FLOOR : contains
    FLOOR ||--o{ ACCESS_POINT : contains
    FLOOR ||--o{ OCCUPANCY_SESSION : tracks
    USER ||--o{ ACCESS_CARD : owns
    USER ||--o{ ACCESS_PERMISSION : receives
    USER ||--o{ ACCESS_EVENT : produces
    USER ||--o{ OCCUPANCY_SESSION : occupies
    ACCESS_POINT ||--o{ ACCESS_PERMISSION : controls
    ACCESS_POINT ||--o{ ACCESS_EVENT : records
    ACCESS_POINT ||--o{ SECURITY_ALERT : raises
    ACCESS_CARD ||--o{ ACCESS_EVENT : identifies
```

## Decisão de acesso implementada e limites

O fluxo de decisão implementado avalia os dados das entidades na Application, na ordem: cartão inexistente; cartão inativo; cartão expirado; utilizador inativo; AccessPoint inativo; direção não suportada; nenhuma permissão ativa; permissão ativa fora da validade atual; acesso concedido. A primeira condição aplicável determina o `AccessResult`. Os pedidos com AccessPoint conhecido são auditados em todos esses resultados, incluindo negativas. A decisão não atualiza `OccupancySession`, não envia notificações SignalR e não processa alertas.

As entidades continuam sendo modelos mutáveis com propriedades públicas, valores padrão e coleções de navegação; não encapsulam todas as invariantes. Regras ainda planejadas incluem:

- garantir `ExitedAt >= EnteredAt`;
- detectar segunda entrada com sessão já aberta;
- detectar saída sem sessão aberta;
- controlar transições válidas de `AlertStatus`.

Essas regras devem ser adicionadas juntamente com testes unitários. Não devem ser implementadas em endpoints ou configurações EF. O Domain não autentica o cliente: o login e a autorização HTTP são implementados nas camadas Application/API; a decisão de acesso continua sendo uma regra distinta e não é alterada pela identidade do chamador.

## Datas e tempo

O código usa `DateTime.UtcNow` nos valores padrão. O processamento de acesso
obtém `now` por `TimeProvider`, mantém a decisão e `AccessEvent.OccurredAt` em
UTC e converte para horário local somente nas bordas de apresentação.
Injetar o relógio permite testes determinísticos em vez de depender
diretamente de `DateTime.UtcNow` durante a avaliação.

## Convenções

- tipos, membros e ficheiros em PascalCase;
- parâmetros e variáveis locais em camelCase;
- termos consistentes com o plano;
- coleções inicializadas para evitar referências nulas;
- nenhuma regra dependente de banco, HTTP ou UI.

## Próximos passos da camada

Os resultados e enumerações usados na decisão de Access Control estão
implementados. A evolução comportamental restante nesta área envolve
`OccupancySession` e alertas; suas invariantes devem entrar com os respetivos
casos de uso e testes, sem colocar regras em endpoints ou configurações EF.
