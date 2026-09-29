# Smart Building Access Control & Occupancy Monitoring

## 1. Objetivo do projeto

Construir uma plataforma de controlo de acessos e monitorização de ocupação para um edifício empresarial.

O sistema deve permitir:

- gerir edifícios, pisos e pontos de acesso;
- gerir utilizadores e cartões/badges;
- definir permissões de acesso;
- receber pedidos de entrada/saída;
- registar todas as tentativas de acesso;
- distinguir entrada e saída;
- calcular quanto tempo cada pessoa permaneceu num piso ou edifício;
- saber quantas pessoas estão atualmente num piso/edifício;
- gerar alertas de segurança;
- atualizar dashboards em tempo real;
- simular leitores/cartões físicos;
- executar processamento assíncrono;
- disponibilizar REST API documentada com OpenAPI/Swagger;
- autenticar utilizadores através de JWT;
- usar PostgreSQL + EF Core + migrations;
- executar em Docker;
- possuir CI/CD com GitHub Actions;
- possuir manifests Kubernetes;
- ter testes unitários e de integração.

O projeto deve ser suficientemente realista para servir como laboratório de estudo de:

- C# / .NET 10
- ASP.NET Core
- REST APIs
- OpenAPI / Swagger
- Angular
- RxJS
- Reactive Forms
- Angular Routing / Guards / Interceptors
- PostgreSQL
- Entity Framework Core
- EF Core Migrations
- SignalR
- BackgroundService
- Docker
- GitHub Actions
- Kubernetes
- Git
- testes automatizados
- arquitetura de software
- regras de domínio
- autenticação e autorização

---

# 2. Objetivo de aprendizagem

Este não deve ser tratado apenas como um projeto para "fazer funcionar".

A prioridade é conseguir explicar:

1. por que cada tecnologia foi escolhida;
2. qual problema ela resolve;
3. onde cada responsabilidade deve ficar;
4. como os dados percorrem o sistema;
5. como lidar com erros;
6. como testar as regras de negócio;
7. como executar e publicar a aplicação;
8. quais decisões poderiam ser alteradas numa aplicação de produção.

A IA local deve atuar como assistente de desenvolvimento e estudo, mas não deve simplesmente gerar todo o projeto de uma vez.

Regra de ouro:

> Implementar uma fase por vez, executar, testar, compreender e só depois avançar.

---

# 3. Princípios do projeto

## 3.1 Não criar complexidade artificial

Cada tecnologia deve ter uma razão real.

Exemplos:

- SignalR: necessário para eventos de acesso em tempo real.
- BackgroundService: necessário para processar eventos e gerar alertas/atualizar ocupação.
- PostgreSQL: necessário para relacionamentos, histórico e consultas.
- EF Core: persistência e migrations.
- Docker: ambiente reproduzível.
- GitHub Actions: build/test/quality checks automatizados.
- Kubernetes: estudar deployment/orquestração.
- Angular: dashboard e administração.
- Simulator: representar leitores de cartão/dispositivos físicos.

## 3.2 Manter o domínio independente

O Domain não deve depender de:

- Entity Framework;
- PostgreSQL;
- ASP.NET Core;
- Angular;
- Docker;
- infraestrutura externa.

## 3.3 Manter eventos brutos e estado derivado separados

`AccessEvent` representa o fato ocorrido.

`OccupancySession` representa uma interpretação do histórico.

Exemplo:

```text
08:42:10 - Card CARD-001 entered Floor 2
12:17:43 - Card CARD-001 exited Floor 2

=> OccupancySession:
   EnteredAt = 08:42:10
   ExitedAt  = 12:17:43
```

Nunca apagar o evento bruto para corrigir uma sessão.

---

# 4. Arquitetura geral

```text
                         ┌─────────────────────────┐
                         │        Angular          │
                         │      Web Application    │
                         └────────────┬────────────┘
                                      │
                              HTTPS / REST
                                      │
                         ┌────────────▼────────────┐
                         │      ASP.NET Core       │
                         │           API           │
                         └──────┬─────────┬────────┘
                                │         │
                         REST   │         │ SignalR
                                │         │
                    ┌───────────▼───┐     └──────────────┐
                    │ Application / │                    │
                    │ Domain Logic  │                    ▼
                    └───────┬───────┘             Angular live UI
                            │
                    ┌───────▼────────┐
                    │ Infrastructure  │
                    │ EF Core         │
                    └───────┬────────┘
                            │
                    ┌───────▼────────┐
                    │   PostgreSQL   │
                    └────────────────┘

                    ┌────────────────┐
                    │ Access         │
                    │ Simulator      │
                    └───────┬────────┘
                            │ REST
                            ▼
                          API

                    Background Processing
                            │
                            ▼
                    Occupancy / Alerts
```

---

# 5. Estrutura do repositório

```text
smart-building-access/
│
├── src/
│   ├── SmartBuilding.Api/
│   ├── SmartBuilding.Domain/
│   ├── SmartBuilding.Infrastructure/
│   ├── SmartBuilding.Application/
│   ├── SmartBuilding.Simulator/
│   └── smart-building-web/
│
├── tests/
│   ├── SmartBuilding.UnitTests/
│   └── SmartBuilding.IntegrationTests/
│
├── docs/
│   ├── 00-project-overview.md
│   ├── 01-architecture.md
│   ├── 02-domain-model.md
│   ├── 03-database.md
│   ├── 04-api.md
│   ├── 05-angular.md
│   ├── 06-security.md
│   ├── 07-realtime.md
│   ├── 08-background-processing.md
│   ├── 09-simulator.md
│   ├── 10-docker.md
│   ├── 11-github-actions.md
│   ├── 12-kubernetes.md
│   ├── 13-testing.md
│   ├── 14-study-notes.md
│   └── 15-interview-questions.md
│
├── infrastructure/
│   ├── docker/
│   └── kubernetes/
│
├── docker-compose.yml
├── .gitignore
├── README.md
└── .github/
    └── workflows/
        └── ci.yml
```

O projeto pode começar menor. Os diretórios e documentos devem ser criados progressivamente.

---

# 6. Stack técnica

## Backend

- .NET 10
- ASP.NET Core Web API
- C#
- Entity Framework Core
- PostgreSQL
- Npgsql
- JWT Bearer Authentication
- SignalR
- BackgroundService
- Swagger/OpenAPI
- xUnit

## Frontend

- Angular
- TypeScript
- RxJS
- Reactive Forms
- Angular Router
- HttpClient
- HTTP Interceptors
- Route Guards
- SignalR client

## DevOps

- Git
- GitHub Actions
- Docker
- Docker Compose
- Kubernetes
- Nginx para servir o Angular em produção

---

# 7. Modelo de domínio

## 7.1 Building

Representa um edifício.

Propriedades:

```csharp
Guid Id
string Name
string Address
bool IsActive
DateTime CreatedAt
```

Relacionamentos:

```text
Building 1:N Floor
```

---

# 7.2 Floor

Representa um piso.

Propriedades:

```csharp
Guid Id
Guid BuildingId
int Number
string Name
bool IsActive
```

Relacionamentos:

```text
Floor N:1 Building
Floor 1:N AccessPoint
Floor 1:N OccupancySession
```

---

# 7.3 AccessPoint

Não tratar simplesmente toda porta como "entrada" ou "saída".

Um `AccessPoint` representa um ponto físico de controlo.

Exemplos:

- Main Entrance
- Floor 2 Entrance
- Floor 2 Exit
- Server Room Door
- Parking Entrance
- Parking Exit

Propriedades sugeridas:

```csharp
Guid Id
Guid FloorId
string Name
string Location
bool IsActive
bool SupportsEntry
bool SupportsExit
DateTime CreatedAt
```

Relacionamentos:

```text
AccessPoint N:1 Floor
AccessPoint 1:N AccessEvent
AccessPoint 1:N AccessPermission
AccessPoint 1:N SecurityAlert
```

### Observação

Para uma versão futura, pode existir um conceito de `Zone` ou `Area` para representar origem/destino de um acesso. Isso não é obrigatório no MVP.

---

# 7.4 User

Representa uma pessoa autorizada a utilizar o sistema.

Propriedades:

```csharp
Guid Id
string Name
string Email
string PasswordHash
bool IsActive
DateTime CreatedAt
```

Relacionamentos:

```text
User 1:N AccessCard
User 1:N AccessPermission
User 1:N AccessEvent
User 1:N OccupancySession
```

Não armazenar password em texto puro.

---

# 7.5 AccessCard

Representa um cartão/badge físico.

Propriedades:

```csharp
Guid Id
Guid UserId
string CardNumber
bool IsActive
DateTime? ExpiresAt
DateTime CreatedAt
```

Relacionamentos:

```text
AccessCard N:1 User
AccessCard 1:N AccessEvent
```

Regras:

- CardNumber deve ser único.
- cartão inativo não pode permitir acesso;
- cartão expirado não pode permitir acesso;
- um cartão deve pertencer a um utilizador;
- no futuro pode ser permitido bloquear/substituir cartão.

---

# 7.6 AccessPermission

Representa a autorização de um utilizador para determinado ponto de acesso.

Propriedades:

```csharp
Guid Id
Guid UserId
Guid AccessPointId
DateTime ValidFrom
DateTime? ValidUntil
bool IsActive
```

Regras:

- User deve existir;
- AccessPoint deve existir;
- `ValidFrom <= ValidUntil` quando `ValidUntil` estiver definido;
- permission inativa não autoriza;
- fora do período de validade não autoriza.

---

# 7.7 AccessEvent

É o evento bruto de acesso.

Todo pedido de acesso deve gerar um evento, seja permitido ou negado.

Propriedades:

```csharp
Guid Id
Guid AccessPointId
Guid? AccessCardId
Guid? UserId
DateTime OccurredAt
AccessDirection Direction
AccessResult Result
string Reason
```

Relacionamentos:

```text
AccessEvent N:1 AccessPoint
AccessEvent N:1 AccessCard
AccessEvent N:1 User
```

`UserId` e `AccessCardId` podem ser null.

Exemplo:

```text
Card desconhecido
        ↓
AccessEvent
UserId = null
AccessCardId = null ou referência técnica se disponível
Result = CardNotFound
```

O evento deve permanecer registrado para auditoria.

---

# 7.8 AccessDirection

Enum:

```csharp
public enum AccessDirection
{
    Entry = 1,
    Exit = 2
}
```

Representa a direção física do acesso.

Não confundir:

- `Entry/Exit` = direção;
- `Granted/Denied` = resultado da autorização.

Um acesso pode ser:

```text
Entry + Granted
Entry + Denied
Exit + Granted
Exit + Denied
```

---

# 7.9 AccessResult

Preferir resultados categorizados:

```csharp
public enum AccessResult
{
    Granted = 1,
    CardNotFound = 2,
    CardInactive = 3,
    CardExpired = 4,
    UserInactive = 5,
    AccessPointInactive = 6,
    NoPermission = 7,
    PermissionExpired = 8
}
```

`Reason` pode conter uma mensagem legível.

Exemplo:

```text
Result = NoPermission
Reason = "User does not have permission for this access point."
```

---

# 7.10 OccupancySession

Representa a presença derivada dos AccessEvents.

Propriedades:

```csharp
Guid Id
Guid UserId
Guid FloorId
DateTime EnteredAt
DateTime? ExitedAt
```

Relacionamentos:

```text
OccupancySession N:1 User
OccupancySession N:1 Floor
```

Regras:

- `ExitedAt == null` significa que a pessoa ainda está no piso;
- `ExitedAt >= EnteredAt`;
- uma sessão fechada representa um intervalo completo;
- sessões são derivadas dos eventos;
- não apagar eventos para corrigir sessões.

Exemplo:

```text
User: Alexandre
Floor: Floor 2
EnteredAt: 08:42
ExitedAt: 12:17
Duration: 3h35m
```

---

# 7.11 SecurityAlert

Representa um alerta gerado pelo sistema.

Propriedades:

```csharp
Guid Id
Guid AccessPointId
AlertType Type
AlertStatus Status
string Message
DateTime CreatedAt
DateTime? ResolvedAt
```

Relacionamentos:

```text
SecurityAlert N:1 AccessPoint
```

---

# 7.12 AlertType

```csharp
public enum AlertType
{
    MultipleDeniedAttempts = 1,
    ExpiredCardUsed = 2,
    InactiveCardUsed = 3,
    UnauthorizedAccessAttempt = 4
}
```

---

# 7.13 AlertStatus

```csharp
public enum AlertStatus
{
    Open = 1,
    Investigating = 2,
    Resolved = 3
}
```

---

# 8. Relações do domínio

```text
Building
  |
  +-- Floor
       |
       +-- AccessPoint
       |     |
       |     +-- AccessPermission
       |     +-- AccessEvent
       |     +-- SecurityAlert
       |
       +-- OccupancySession

User
  |
  +-- AccessCard
  +-- AccessPermission
  +-- AccessEvent
  +-- OccupancySession

AccessCard
  |
  +-- AccessEvent
```

---

# 9. Regra principal de acesso

Endpoint:

```http
POST /api/access/requests
```

Request:

```json
{
  "cardNumber": "CARD-001",
  "accessPointId": "..."
  "direction": "Entry"
}
```

Fluxo:

```text
Receive request
      |
Find card
      |
Card exists?
      |---- NO ---> CardNotFound
      |
Card active?
      |---- NO ---> CardInactive
      |
Card expired?
      |---- YES --> CardExpired
      |
Find user
      |
User active?
      |---- NO ---> UserInactive
      |
Find access point
      |
Access point active?
      |---- NO ---> AccessPointInactive
      |
Find valid permission
      |
Permission exists?
      |---- NO ---> NoPermission
      |
Permission currently valid?
      |---- NO ---> PermissionExpired
      |
      +----> Granted
```

Independentemente do resultado:

```text
Create AccessEvent
```

Se `Granted`:

```text
Publish SignalR event
Process occupancy
```

Se `Denied`:

```text
Publish SignalR event
Evaluate security rules
```

---

# 10. Entrada e saída

A aplicação precisa diferenciar:

```text
Entry
Exit
```

Exemplo:

```text
08:42 - Entry - Floor 2
12:17 - Exit  - Floor 2
13:05 - Entry - Floor 2
18:02 - Exit  - Floor 2
```

A partir disso:

```text
Session 1:
08:42 -> 12:17 = 3h35

Session 2:
13:05 -> 18:02 = 4h57
```

Tempo total no piso:

```text
8h32
```

---

# 11. Ocupação

O sistema deve conseguir responder:

### Quantas pessoas estão no prédio?

Somar sessões abertas relacionadas ao prédio.

### Quantas pessoas estão no piso?

Somar sessões abertas para o piso.

### Quanto tempo uma pessoa ficou no piso?

Para sessão fechada:

```text
ExitedAt - EnteredAt
```

Para sessão aberta:

```text
Now - EnteredAt
```

### Importante

Não confiar somente em `OccupancySession`.

O `AccessEvent` é a fonte de auditoria.

Se houver inconsistência, o sistema deve sinalizar a situação.

---

# 12. Casos de inconsistência

Exemplo:

```text
08:42 Entry
13:05 Entry
```

Não assumir automaticamente que o primeiro evento foi encerrado.

Gerar uma situação de inconsistência:

```text
Possible inconsistent occupancy state
```

Outra situação:

```text
08:42 Exit
```

sem uma entrada anterior.

Não criar uma sessão negativa.

Registrar o evento e marcar o problema para análise.

Esses casos podem ser tratados inicialmente com logs e posteriormente com uma entidade específica de anomaly.

---

# 13. Background Processing

Criar:

```text
OccupancyProcessor
SecurityAlertWorker
```

Pode começar com um único `BackgroundService` e posteriormente separar responsabilidades.

Responsabilidades:

## Occupancy processing

Processar eventos concedidos:

```text
Entry
  -> create/open OccupancySession

Exit
  -> find open OccupancySession
  -> set ExitedAt
```

## Alert processing

Exemplo:

```text
3 denied attempts
same access point
within 2 minutes
```

Resultado:

```text
SecurityAlert created
```

---

# 14. SignalR

Criar um hub:

```text
/accessHub
```

Eventos possíveis:

```text
AccessEventCreated
SecurityAlertCreated
OccupancyChanged
```

Exemplo:

```text
Card CARD-001 entered Floor 2
```

O Angular recebe imediatamente.

Não utilizar polling contínuo para o dashboard principal.

---

# 15. Dashboard Angular

Dashboard inicial:

```text
+------------------------------------------------------+
| Smart Building                                       |
+------------------------------------------------------+
| People Inside | Active Doors | Today's Events | Alerts|
|      37       |      38      |      1284      |   4   |
+------------------------------------------------------+

LIVE ACCESS EVENTS

08:42:10  ✓ Alexandre   Main Entrance     Entry
08:41:58  ✕ Unknown     Server Room       Entry
08:41:40  ✓ Maria       Floor 2           Exit
08:41:21  ✓ Pedro       Floor 3           Entry
```

O dashboard deve atualizar via SignalR.

---

# 16. Angular estrutura

```text
src/app/
│
├── core/
│   ├── auth/
│   ├── guards/
│   ├── interceptors/
│   ├── models/
│   └── services/
│
├── features/
│   ├── login/
│   ├── dashboard/
│   ├── buildings/
│   ├── floors/
│   ├── access-points/
│   ├── users/
│   ├── cards/
│   ├── permissions/
│   ├── events/
│   ├── occupancy/
│   └── alerts/
│
└── shared/
    ├── components/
    └── pipes/
```

---

# 17. Angular conceitos obrigatórios

Estudar e implementar:

## Components

- lifecycle
- templates
- inputs
- outputs
- event binding
- property binding

## Services

Serviços para comunicação com API.

## Dependency Injection

Utilizar DI do Angular.

## RxJS

Obrigatório compreender:

```text
Observable
subscribe
pipe
map
filter
switchMap
catchError
tap
BehaviorSubject
```

Não utilizar `subscribe` indiscriminadamente dentro de vários componentes.

## Reactive Forms

Utilizar:

```text
FormGroup
FormControl
Validators
valueChanges
```

## Router

Rotas:

```text
/login
/dashboard
/users
/users/:id
/buildings
/floors
/access-points
/cards
/permissions
/events
/occupancy
/alerts
```

## Guards

Proteger áreas autenticadas.

## HTTP Interceptor

Adicionar:

```http
Authorization: Bearer <token>
```

e tratar `401`.

---

# 18. Autenticação

Endpoint:

```http
POST /api/auth/login
```

Request:

```json
{
  "email": "admin@smartbuilding.local",
  "password": "..."
}
```

Response:

```json
{
  "accessToken": "...",
  "expiresAt": "..."
}
```

JWT deve conter claims necessários.

Não colocar dados sensíveis no token.

---

# 19. Autorização

Ter inicialmente dois papéis:

```text
Admin
SecurityOperator
```

Exemplo:

```text
Admin
- gerenciar usuários
- gerenciar cartões
- gerenciar permissões
- gerenciar access points

SecurityOperator
- visualizar eventos
- visualizar ocupação
- visualizar alertas
- resolver alertas
```

A autorização deve ser aplicada no backend.

Angular guards servem para experiência/navegação, mas não substituem autorização no servidor.

---

# 20. REST API

Endpoints mínimos:

## Auth

```http
POST /api/auth/login
```

## Buildings

```http
GET /api/buildings
GET /api/buildings/{id}
POST /api/buildings
PUT /api/buildings/{id}
DELETE /api/buildings/{id}
```

## Floors

```http
GET /api/floors
GET /api/floors/{id}
POST /api/floors
PUT /api/floors/{id}
DELETE /api/floors/{id}
```

## Access Points

```http
GET /api/access-points
GET /api/access-points/{id}
POST /api/access-points
PUT /api/access-points/{id}
DELETE /api/access-points/{id}
```

## Users

```http
GET /api/users
GET /api/users/{id}
POST /api/users
PUT /api/users/{id}
DELETE /api/users/{id}
```

## Cards

```http
GET /api/cards
POST /api/cards
PUT /api/cards/{id}
POST /api/cards/{id}/activate
POST /api/cards/{id}/deactivate
```

## Permissions

```http
GET /api/permissions
POST /api/permissions
PUT /api/permissions/{id}
DELETE /api/permissions/{id}
```

## Access

```http
POST /api/access/requests
GET /api/access/events
GET /api/access/events/{id}
```

## Occupancy

```http
GET /api/occupancy/current
GET /api/occupancy/floors/{floorId}
GET /api/occupancy/users/{userId}
GET /api/occupancy/sessions
```

## Alerts

```http
GET /api/security/alerts
GET /api/security/alerts/{id}
POST /api/security/alerts/{id}/investigate
POST /api/security/alerts/{id}/resolve
```

---

# 21. DTOs

Não retornar entidades EF diretamente.

Criar DTOs:

```text
BuildingResponse
CreateBuildingRequest
UpdateBuildingRequest

FloorResponse
CreateFloorRequest
UpdateFloorRequest

AccessPointResponse
CreateAccessPointRequest
UpdateAccessPointRequest

UserResponse
CreateUserRequest
UpdateUserRequest

AccessCardResponse
CreateAccessCardRequest

AccessPermissionResponse
CreateAccessPermissionRequest

AccessRequest
AccessResponse
AccessEventResponse

OccupancySessionResponse
OccupancySummaryResponse

SecurityAlertResponse
```

---

# 22. Paginação

Eventos podem crescer muito.

Não retornar todos os eventos.

Endpoint:

```http
GET /api/access/events?page=1&pageSize=50
```

Resposta:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 50,
  "totalItems": 1000,
  "totalPages": 20
}
```

Adicionar filtros:

```text
userId
accessPointId
direction
result
from
to
```

---

# 23. PostgreSQL

Banco:

```text
smartbuilding
```

Tabelas principais:

```text
buildings
floors
access_points
users
access_cards
access_permissions
access_events
occupancy_sessions
security_alerts
```

Índices importantes:

```text
access_cards.card_number
access_events.occurred_at
access_events.access_point_id
access_events.user_id
access_events.access_card_id
occupancy_sessions.user_id
occupancy_sessions.floor_id
occupancy_sessions.exited_at
```

Unique constraints:

```text
users.email
access_cards.card_number
```

---

# 24. EF Core

Utilizar:

```text
DbContext
DbSet
Fluent API
Migrations
AsNoTracking
Include
ThenInclude
transactions quando necessário
```

Evitar depender apenas de conventions.

Configurar relacionamentos explicitamente quando isso melhorar clareza.

Separar configurações:

```text
Infrastructure/
└── Data/
    └── Configurations/
        ├── BuildingConfiguration.cs
        ├── FloorConfiguration.cs
        ├── AccessPointConfiguration.cs
        ├── UserConfiguration.cs
        ├── AccessCardConfiguration.cs
        ├── AccessPermissionConfiguration.cs
        ├── AccessEventConfiguration.cs
        ├── OccupancySessionConfiguration.cs
        └── SecurityAlertConfiguration.cs
```

---

# 25. Migrations

Comandos:

```bash
dotnet ef migrations add InitialCreate \
  --project src/SmartBuilding.Infrastructure \
  --startup-project src/SmartBuilding.Api

dotnet ef database update \
  --project src/SmartBuilding.Infrastructure \
  --startup-project src/SmartBuilding.Api
```

Depois de alteração de modelo:

```bash
dotnet ef migrations add AddOccupancySessions \
  --project src/SmartBuilding.Infrastructure \
  --startup-project src/SmartBuilding.Api
```

Nunca editar uma migration aplicada como se fosse uma migration descartável.

---

# 26. Validation

Validar:

- email;
- nome obrigatório;
- CardNumber obrigatório;
- datas;
- IDs;
- paginação;
- pageSize máximo;
- permissões;
- access point ativo.

Erros devem utilizar respostas HTTP apropriadas.

Exemplos:

```text
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
422 Unprocessable Entity quando fizer sentido
500 Internal Server Error
```

---

# 27. Error handling

Criar middleware global para exceções.

Resposta padronizada:

```json
{
  "title": "An unexpected error occurred.",
  "status": 500,
  "traceId": "..."
}
```

Não retornar stack trace para o cliente.

Utilizar logs estruturados.

---

# 28. Logging

Registrar:

- login;
- erros;
- pedidos de acesso;
- falhas de autorização;
- alertas;
- erros de background processing.

Não registrar:

- passwords;
- tokens completos;
- informações sensíveis desnecessárias.

---

# 29. Access Simulator

Criar projeto:

```text
SmartBuilding.Simulator
```

Responsabilidade:

simular leitores físicos.

Configuração:

```text
API URL
Card numbers
Access points
interval
```

Exemplo:

```text
Every 5 seconds:

CARD-001 -> Main Entrance -> Entry
CARD-001 -> Floor 2 Door  -> Entry
CARD-002 -> Server Room   -> Entry
CARD-002 -> Server Room   -> Exit
```

Pode existir um modo aleatório:

```text
Random card
Random access point
Random direction
```

Mas também um modo determinístico para testes.

---

# 30. Docker Compose

Serviços:

```text
postgres
api
web
simulator
```

Exemplo conceitual:

```text
docker compose up -d
```

Verificar:

```text
API      http://localhost:5000
Swagger  http://localhost:5000/swagger
Web      http://localhost:4200
Postgres localhost:5432
```

Não hardcodar secrets reais no compose.

Utilizar `.env` local quando necessário.

---

# 31. Docker API

Usar multi-stage build:

```text
SDK image
   |
restore
   |
build
   |
publish
   |
runtime image
```

Objetivo:

imagem final menor e sem SDK desnecessário.

---

# 32. Docker Angular

Build:

```text
Node
  |
npm ci
  |
ng build
  |
Nginx
```

Nginx serve os ficheiros estáticos.

Configurar fallback para Angular Router.

---

# 33. GitHub Actions

Pipeline mínimo:

```text
push / pull request
       |
checkout
       |
setup .NET
       |
restore
       |
build
       |
unit tests
       |
setup Node
       |
npm ci
       |
Angular build
```

Depois evoluir para:

```text
Docker build
```

Opcionalmente:

```text
Docker image push
```

Não é necessário configurar cloud deployment para o MVP.

---

# 34. Kubernetes

Criar manifests:

```text
infrastructure/kubernetes/
├── namespace.yaml
├── configmap.yaml
├── secret.yaml
├── postgres-statefulset.yaml
├── postgres-service.yaml
├── api-deployment.yaml
├── api-service.yaml
├── web-deployment.yaml
├── web-service.yaml
└── ingress.yaml
```

Estudar:

- Pod
- Deployment
- StatefulSet
- Service
- Ingress
- ConfigMap
- Secret
- replicas
- readiness probe
- liveness probe

Não colocar passwords reais no Git.

---

# 35. Testes unitários

Testar principalmente regras de negócio.

Casos obrigatórios:

```text
Active card + valid permission => Granted

Unknown card => CardNotFound

Inactive card => CardInactive

Expired card => CardExpired

Inactive user => UserInactive

Inactive access point => AccessPointInactive

No permission => NoPermission

Expired permission => PermissionExpired
```

---

# 36. Testes de Occupancy

Casos:

```text
Entry with no open session
=> creates session

Exit with open session
=> closes session

Exit without open session
=> inconsistency

Second Entry while already inside
=> inconsistency

Open session
=> current occupancy

Closed session
=> duration calculated
```

---

# 37. Testes de alertas

Exemplo:

```text
Denied
Denied
Denied
within 2 minutes
=> SecurityAlert
```

Mas:

```text
Denied
Denied
Denied
over several hours
=> no alert for this rule
```

---

# 38. Testes de integração

Usar uma infraestrutura de teste real ou containerizada para PostgreSQL quando possível.

Testar:

```text
HTTP request
  |
Controller
  |
Service
  |
EF Core
  |
PostgreSQL
```

Casos:

- login;
- criar user;
- criar card;
- criar permission;
- request access;
- event persisted;
- occupancy persisted;
- alert persisted.

---

# 39. Seed data

Criar dados iniciais para desenvolvimento.

Exemplo:

```text
Building:
    HQ Lisbon

Floors:
    Floor 0
    Floor 1
    Floor 2
    Floor 3

Users:
    Admin
    Alexandre
    Maria
    Pedro

Cards:
    CARD-001
    CARD-002
    CARD-003

Access Points:
    Main Entrance
    Floor 1 Entrance
    Floor 2 Entrance
    Server Room
```

Permissões:

```text
Alexandre -> Main Entrance
Alexandre -> Floor 1
Alexandre -> Floor 2

Maria -> Main Entrance
Maria -> Floor 1

Pedro -> Main Entrance
Pedro -> Floor 3
```

---

# 40. Fluxo completo de demonstração

A aplicação deve permitir demonstrar:

## Cenário 1 - entrada

```text
CARD-001
Main Entrance
Entry
```

Resultado:

```text
Access Granted
AccessEvent created
OccupancySession created
SignalR event
Dashboard updated
```

## Cenário 2 - entrada não autorizada

```text
CARD-002
Server Room
Entry
```

Resultado:

```text
Access Denied
AccessEvent created
Dashboard updated
```

## Cenário 3 - saída

```text
CARD-001
Main Entrance
Exit
```

Resultado:

```text
Access Granted
AccessEvent created
OccupancySession closed
Duration calculated
Dashboard updated
```

## Cenário 4 - ataque/suspeita

```text
CARD-999
Server Room
Entry
Denied

CARD-998
Server Room
Entry
Denied

CARD-997
Server Room
Entry
Denied
```

Resultado:

```text
SecurityAlert created
SignalR notification
Dashboard alert
```

---

# 41. Dashboard de ocupação

Adicionar:

```text
Building occupancy
Floor occupancy
Current sessions
```

Exemplo:

```text
HQ Lisbon

People inside: 37

Floor 0: 8
Floor 1: 12
Floor 2: 14
Floor 3: 3
```

Tabela:

```text
Person       Floor       Entered       Duration
Alexandre    Floor 2     08:42         3h35
Maria        Floor 1     09:10         3h07
Pedro        Floor 3     09:25         2h52
```

---

# 42. Histórico de presença

Página:

```text
/occupancy
```

Filtros:

```text
User
Floor
From
To
```

Resultado:

```text
Alexandre
Floor 2
08:42 - 12:17
Duration: 3h35
```

---

# 43. Eventos de acesso

Página:

```text
/events
```

Colunas:

```text
Timestamp
User
Card
Access Point
Direction
Result
Reason
```

Filtros:

```text
User
Card
Access Point
Entry/Exit
Granted/Denied
Date range
```

---

# 44. Segurança

Página:

```text
/alerts
```

Colunas:

```text
Created
Access Point
Type
Status
Message
```

Ações:

```text
Investigate
Resolve
```

---

# 45. Decisões arquiteturais

## Domain

Contém:

- entidades;
- enums;
- regras de domínio puras quando fizer sentido.

Não contém:

- EF Core;
- controllers;
- HTTP;
- PostgreSQL.

## Application

Contém:

- casos de uso;
- serviços;
- DTOs;
- interfaces;
- orquestração;
- regras que envolvem múltiplas entidades.

Exemplos:

```text
AccessControlService
OccupancyService
SecurityAlertService
```

## Infrastructure

Contém:

- EF Core;
- DbContext;
- repositories quando realmente necessários;
- PostgreSQL;
- configurações de persistência;
- implementações externas.

## API

Contém:

- Controllers;
- authentication;
- middleware;
- SignalR hubs;
- configuração;
- dependency injection.

---

# 46. Repository Pattern

Não criar repository para absolutamente tudo apenas por hábito.

Primeiro utilizar EF Core diretamente através de abstrações de Application quando fizer sentido.

Se uma abstração melhorar testes/separação, criar:

```text
IAccessEventRepository
IOccupancySessionRepository
```

Evitar:

```text
GenericRepository<T>
```

apenas porque é um padrão conhecido.

---

# 47. Transações

O processamento de um acesso concedido deve considerar consistência.

Exemplo:

```text
Create AccessEvent
+
Create/Update OccupancySession
```

Quando necessário, utilizar uma transação.

O evento não deve ser criado como permitido e a sessão ficar inconsistente sem tratamento.

---

# 48. Concorrência

Considerar o seguinte:

```text
dois pedidos de Entry
quase simultaneamente
```

O sistema não deve criar duas sessões abertas para a mesma pessoa no mesmo piso sem detectar o problema.

Considerar:

- transações;
- unique constraints quando aplicável;
- locking quando necessário;
- idempotência.

Não implementar soluções complexas antes de demonstrar o problema.

---

# 49. Idempotência

O simulator pode enviar o mesmo evento mais de uma vez.

Para uma versão evoluída, adicionar:

```text
ExternalEventId
```

ao `AccessEvent`.

Esse valor pode ser único.

Assim:

```text
ExternalEventId = DEVICE-01-000123
```

não pode ser processado duas vezes.

Isso é importante em sistemas distribuídos.

---

# 50. Observabilidade futura

Não é MVP, mas deixar documentado:

- health checks;
- metrics;
- OpenTelemetry;
- distributed tracing;
- structured logging.

Health endpoints:

```http
GET /health
GET /health/ready
```

---

# 51. Segurança futura

Não precisa implementar tudo no MVP, mas estudar:

- JWT;
- refresh tokens;
- password hashing;
- roles;
- claims;
- authorization policies;
- rate limiting;
- CORS;
- HTTPS;
- secret management;
- audit logging.

Nunca:

```text
password = "123456"
```

em produção.

---

# 52. Performance

Problemas que devem ser estudados:

## N+1 queries

Evitar carregar relações em loops.

## AsNoTracking

Usar em consultas somente leitura quando apropriado.

## Pagination

Obrigatória para eventos.

## Indexes

Principalmente em:

```text
OccurredAt
UserId
AccessPointId
CardId
```

## SignalR

Não enviar dados desnecessários.

## Background processing

Não bloquear requests HTTP esperando tarefas demoradas.

---

# 53. Git strategy

Cada fase deve terminar com commit.

Commits sugeridos:

```text
01 - Initialize repository
02 - Create .NET solution
03 - Add domain entities
04 - Add domain enums
05 - Add application project
06 - Add EF Core infrastructure
07 - Configure PostgreSQL
08 - Add initial migration
09 - Add seed data
10 - Add building endpoints
11 - Add access point endpoints
12 - Add user endpoints
13 - Add card endpoints
14 - Add permission endpoints
15 - Implement access control
16 - Add access event persistence
17 - Add JWT authentication
18 - Add authorization policies
19 - Initialize Angular app
20 - Add Angular routing
21 - Add Angular authentication
22 - Add HTTP interceptor
23 - Add dashboard
24 - Add user management
25 - Add card management
26 - Add permissions management
27 - Add access events
28 - Add occupancy dashboard
29 - Add SignalR hub
30 - Add real-time dashboard updates
31 - Add occupancy processing
32 - Add security alert processing
33 - Add simulator
34 - Add unit tests
35 - Add integration tests
36 - Add Docker
37 - Add Docker Compose
38 - Add GitHub Actions
39 - Add Kubernetes manifests
40 - Add documentation
```

---

# 54. Desenvolvimento por fases

## Phase 1 - Foundation

Objetivo:

- repository;
- solution;
- projects;
- references;
- build.

Definition of Done:

```bash
dotnet build
```

funciona sem erros.

---

## Phase 2 - Domain

Implementar:

- Building;
- Floor;
- AccessPoint;
- User;
- AccessCard;
- AccessPermission;
- AccessEvent;
- OccupancySession;
- SecurityAlert;
- enums.

Definition of Done:

Domain compila sem dependências de Infrastructure/API.

---

## Phase 3 - Database

Implementar:

- DbContext;
- configurations;
- PostgreSQL;
- migration;
- seed.

Definition of Done:

Banco sobe e migration cria todas as tabelas.

---

## Phase 4 - API

Implementar:

- CRUD;
- DTOs;
- validation;
- Swagger;
- error handling.

Definition of Done:

Endpoints principais funcionam pelo Swagger.

---

## Phase 5 - Access Control

Implementar:

```text
POST /api/access/requests
```

Definition of Done:

Access granted/denied funciona e gera AccessEvent.

---

## Phase 6 - Authentication

Implementar:

- login;
- JWT;
- roles;
- authorization;
- protected endpoints.

Definition of Done:

Endpoints protegidos rejeitam requests sem token.

---

## Phase 7 - Angular

Implementar:

- login;
- routing;
- dashboard;
- CRUD;
- forms;
- RxJS;
- guards;
- interceptor.

Definition of Done:

Frontend consegue operar o sistema principal.

---

## Phase 8 - Occupancy

Implementar:

- Entry/Exit;
- OccupancySession;
- current occupancy;
- duration;
- floor occupancy.

Definition of Done:

Entrar e sair altera corretamente a ocupação.

---

## Phase 9 - Real-time

Implementar:

- SignalR;
- access event notifications;
- occupancy updates;
- alert notifications.

Definition of Done:

Dashboard atualiza sem refresh.

---

## Phase 10 - Background Processing

Implementar:

- occupancy processor;
- security rules;
- alert worker.

Definition of Done:

Eventos são processados assincronamente.

---

## Phase 11 - Simulator

Implementar:

- configurable cards;
- configurable access points;
- deterministic mode;
- random mode.

Definition of Done:

É possível gerar tráfego automaticamente.

---

## Phase 12 - Testing

Implementar:

- unit tests;
- integration tests.

Definition of Done:

CI executa testes automaticamente.

---

## Phase 13 - Docker

Implementar:

- Dockerfile API;
- Dockerfile Angular;
- Docker Compose;
- PostgreSQL container.

Definition of Done:

```bash
docker compose up
```

inicia o ambiente.

---

## Phase 14 - GitHub Actions

Implementar:

- backend build;
- backend tests;
- frontend build;
- Docker build.

Definition of Done:

Pull Request executa CI.

---

## Phase 15 - Kubernetes

Implementar:

- deployments;
- services;
- config;
- secrets;
- ingress;
- probes.

Definition of Done:

Aplicação pode ser executada em Kubernetes local.

---

# 55. Ordem de estudo para entrevista

Prioridade máxima:

```text
1. Angular
2. .NET / ASP.NET Core
3. REST APIs
4. Swagger/OpenAPI
5. RxJS
6. JWT / Authentication
7. EF Core
8. PostgreSQL
```

Prioridade média:

```text
9. SignalR
10. Docker
11. Git
12. GitHub Actions
13. Reactive Forms
14. Guards
15. Interceptors
```

Prioridade complementar:

```text
16. BackgroundService
17. Kubernetes
18. Integration Tests
19. Windows Server concepts
20. deployment
```

---

# 56. Perguntas de entrevista que este projeto deve preparar

## Angular

- What is a component?
- What is dependency injection?
- Observable vs Promise?
- What is RxJS?
- What does switchMap do?
- How does HttpClient work?
- What is an interceptor?
- What is a route guard?
- Reactive Forms vs template-driven forms?
- How do parent and child components communicate?
- How would you optimize a large Angular application?
- What is lazy loading?
- How would you handle errors from an API?

## .NET

- What is dependency injection?
- What is middleware?
- What is async/await?
- How does ASP.NET Core process a request?
- What is a DTO?
- How do you handle exceptions?
- What is scoped vs singleton vs transient?
- How does EF Core track entities?
- What are migrations?
- How would you improve API performance?

## REST

- GET vs POST vs PUT vs PATCH?
- 401 vs 403?
- What is idempotency?
- What is OpenAPI?
- Why use DTOs?
- How do you version an API?

## PostgreSQL

- Primary key?
- Foreign key?
- Index?
- Unique constraint?
- Transactions?
- How would you optimize a slow query?

## Docker

- Image vs container?
- Dockerfile?
- Multi-stage build?
- Docker Compose?
- Environment variables?

## Kubernetes

- Pod?
- Deployment?
- Service?
- Ingress?
- ConfigMap?
- Secret?
- Replica?

## Architecture

- Why separate Domain and Infrastructure?
- Where should business rules live?
- When would you use a repository?
- How would you process access events asynchronously?
- Why SignalR?
- How would you handle duplicate events?
- How would you handle concurrent access events?

---

# 57. Definition of Done global

O projeto só é considerado concluído quando:

- [ ] Domain implementado
- [ ] PostgreSQL configurado
- [ ] EF Core configurado
- [ ] Migrations funcionando
- [ ] Seed funcionando
- [ ] REST API funcionando
- [ ] Swagger funcionando
- [ ] DTOs implementados
- [ ] Validation implementada
- [ ] Error handling implementado
- [ ] JWT implementado
- [ ] Authorization implementada
- [ ] Angular implementado
- [ ] Routing implementado
- [ ] Guards implementados
- [ ] Interceptor implementado
- [ ] Reactive Forms implementados
- [ ] RxJS utilizado conscientemente
- [ ] Access Control funcionando
- [ ] Entry/Exit funcionando
- [ ] Access Events funcionando
- [ ] Occupancy Sessions funcionando
- [ ] Occupancy dashboard funcionando
- [ ] SignalR funcionando
- [ ] Background processing funcionando
- [ ] Security alerts funcionando
- [ ] Simulator funcionando
- [ ] Unit tests funcionando
- [ ] Integration tests funcionando
- [ ] Docker funcionando
- [ ] Docker Compose funcionando
- [ ] GitHub Actions funcionando
- [ ] Kubernetes manifests criados
- [ ] README atualizado
- [ ] docs atualizados

---

# 58. Regras para a IA local

A IA que trabalhar neste repositório deve seguir estas regras:

1. Não alterar arquitetura sem explicar a razão.
2. Não adicionar dependências sem justificar.
3. Não criar abstrações desnecessárias.
4. Não implementar funcionalidades fora do escopo sem aprovação.
5. Não substituir uma solução simples por uma solução excessivamente complexa.
6. Não esconder regras de negócio dentro de Controllers.
7. Não colocar regras de negócio no Angular que deveriam ser validadas no backend.
8. Nunca confiar em Guards Angular como mecanismo de segurança.
9. Não retornar entidades EF diretamente pela API.
10. Não guardar passwords em texto puro.
11. Não colocar secrets no Git.
12. Não criar GenericRepository automaticamente.
13. Não usar `Task.Run` sem uma razão clara em ASP.NET Core.
14. Respeitar cancellation tokens em operações assíncronas de longa duração.
15. Preferir async/await para I/O.
16. Evitar N+1 queries.
17. Usar pagination em coleções potencialmente grandes.
18. Não remover AccessEvents para corrigir OccupancySessions.
19. Tratar AccessEvents como histórico/auditoria.
20. Explicar qualquer alteração de schema do banco.
21. Criar migration sempre que houver mudança persistente no modelo.
22. Antes de modificar código existente, analisar o código relacionado.
23. Executar testes depois das alterações.
24. Não afirmar que algo funciona sem executar ou verificar.
25. Manter documentação atualizada quando a arquitetura mudar.
26. Implementar uma fase de cada vez.
27. Ao terminar uma fase, apresentar:
   - o que foi implementado;
   - arquivos alterados;
   - comandos executados;
   - testes realizados;
   - próximos passos.
28. Se houver ambiguidade arquitetural, apresentar as opções antes de escolher uma.
29. Priorizar código legível em vez de código excessivamente sofisticado.
30. Explicar conceitos novos antes de gerar grandes blocos de código.

---

# 59. Regra de trabalho incremental com a IA

Para cada tarefa:

```text
1. Ler a documentação relevante.
2. Inspecionar o código existente.
3. Explicar o que será alterado.
4. Implementar a menor mudança necessária.
5. Compilar.
6. Executar testes.
7. Corrigir problemas.
8. Atualizar documentação.
9. Resumir o que foi feito.
10. Esperar a próxima tarefa.
```

Não gerar todo o sistema de uma vez.

---

# 60. Primeiro milestone

O primeiro milestone é:

```text
Repository
+
.NET Solution
+
Domain
+
Application
+
Infrastructure
+
API
+
EF Core
+
PostgreSQL
+
Initial Migration
```

Ainda não implementar:

- Angular;
- JWT;
- SignalR;
- Docker;
- Kubernetes.

Primeiro garantir que o backend base está correto.

---

# 61. Primeira tarefa para a IA

A primeira tarefa deve ser exatamente:

> Analise este documento e crie a estrutura inicial do projeto Smart Building Access Control. Crie a solution .NET 8, os projetos Domain, Application, Infrastructure, Api e os testes. Configure corretamente as referências entre projetos de acordo com a arquitetura descrita. Não implemente controllers, banco, Angular, autenticação ou funcionalidades de negócio ainda. Primeiro crie apenas a estrutura, namespaces, referências e configurações mínimas necessárias para compilar. No final execute `dotnet build` e apresente os arquivos criados, referências entre projetos e resultado do build.

---

# 62. Segundo milestone

Depois da estrutura:

> Implemente somente o Domain Model descrito neste documento. Crie as entidades, enums e relacionamentos de domínio. Não adicione Entity Framework, PostgreSQL ou atributos de persistência ao Domain. Execute os testes/build e explique cada entidade criada.

---

# 63. Terceiro milestone

Depois:

> Implemente a persistência com EF Core e PostgreSQL. Crie DbContext e EntityTypeConfigurations no projeto Infrastructure. Configure todos os relacionamentos, foreign keys, indexes e unique constraints descritos neste documento. Crie a migration inicial. Não implemente API ou Angular ainda.

---

# 64. Estado final desejado

A demonstração final deve ser:

```text
User logs in
      ↓
Angular receives JWT
      ↓
User opens dashboard
      ↓
Simulator sends access request
      ↓
API authenticates request
      ↓
AccessControlService validates:
    card
    user
    access point
    permission
      ↓
AccessEvent created
      ↓
If granted:
    Occupancy processing
      ↓
OccupancySession created/updated
      ↓
SignalR broadcasts event
      ↓
Angular updates dashboard
      ↓
Background worker evaluates rules
      ↓
SecurityAlert if necessary
```

Exemplo final:

```text
08:42:10
CARD-001
Alexandre
Main Entrance
ENTRY
GRANTED

08:42:10
OccupancySession created

08:42:10
Dashboard updated

12:17:43
CARD-001
Alexandre
Main Entrance
EXIT
GRANTED

12:17:43
OccupancySession closed

Duration:
3h35m33s
```

---

# 65. Visão de evolução futura

Depois do projeto principal, possíveis evoluções:

```text
Device Management
   |
   +-- Access Reader
   +-- Device heartbeat
   +-- Device status
   +-- Firmware version

Zones
   |
   +-- Office
   +-- Server Room
   +-- Lobby
   +-- Parking

Advanced analytics
   |
   +-- Average occupancy
   +-- Peak hours
   +-- Time per floor
   +-- Access trends

Notifications
   |
   +-- Email
   +-- Web notifications
   +-- Teams/Slack

Distributed architecture
   |
   +-- Message broker
   +-- Kafka/RabbitMQ
   +-- Event-driven processing

Observability
   |
   +-- OpenTelemetry
   +-- Prometheus
   +-- Grafana
```

Essas funcionalidades não fazem parte do MVP e só devem ser implementadas depois que o núcleo estiver estável.
