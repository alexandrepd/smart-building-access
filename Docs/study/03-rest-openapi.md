# REST, HTTP, OpenAPI e Idempotência

## O que é uma API?

### Como explicar para uma criança

Uma API é como o balcão de uma biblioteca. Você não entra no arquivo para procurar sozinho. Faz um pedido permitido, o funcionário procura e entrega uma resposta.

### Definição técnica

API significa Application Programming Interface. É um contrato usado por sistemas para comunicar. Uma Web API disponibiliza operações através de protocolos web, geralmente HTTP.

API não é sinónimo de REST, JSON ou servidor. Esses elementos podem participar da implementação.

## O que é um endpoint?

Endpoint é uma operação acessível através da combinação de método HTTP e rota.

```text
GET  /api/buildings
POST /api/buildings
```

As rotas são iguais, mas os endpoints são diferentes porque o método muda.

## Request e response

Uma request pode conter:

- método;
- URL;
- headers;
- query string;
- route parameters;
- body.

Uma response contém:

- status code;
- headers;
- body opcional.

Exemplo planejado para controlo de acesso:

```http
POST /api/access/requests
Content-Type: application/json
Authorization: Bearer <token>

{
  "cardNumber": "CARD-001",
  "accessPointId": "00000000-0000-0000-0000-000000000001",
  "direction": "Entry"
}
```

## JSON

JSON é um formato textual de dados. HTTP é o protocolo; JSON é uma representação possível do body.

JSON possui objetos, arrays, strings, números, booleanos e `null`. Uma data ou `Guid` normalmente viaja como string e é convertida na aplicação.

## Métodos HTTP

| Método | Uso típico | Seguro | Idempotente |
|---|---|---|---|
| `GET` | Consultar | Sim | Sim |
| `POST` | Criar ou executar comando | Não | Geralmente não |
| `PUT` | Substituir recurso | Não | Sim |
| `PATCH` | Alterar parcialmente | Não | Depende |
| `DELETE` | Remover | Não | Sim em efeito esperado |

“Seguro” significa que a intenção não é alterar estado. “Idempotente” significa que repetir produz o mesmo efeito final.

## Idempotência

### Analogia

Imagine um botão de elevador. Apertar uma vez ou cinco vezes deve chamar o mesmo elevador, não cinco elevadores diferentes.

### Definição técnica

Uma operação é idempotente quando múltiplas execuções equivalentes têm o mesmo efeito observável no estado do sistema que uma execução.

Idempotência não significa:

- mesma resposta byte por byte;
- ausência de logs;
- ausência de requests repetidos;
- transação.

### Exemplo no Smart Building

Um leitor envia um evento, não recebe resposta por timeout e tenta novamente. Sem proteção, dois `AccessEvent`s podem ser criados e duas sessões podem ser abertas.

Uma solução futura:

```text
ExternalEventId = DEVICE-01-000123
```

O banco aplica uma constraint `UNIQUE`. Ao receber novamente o mesmo ID, o sistema retorna o resultado existente ou ignora a duplicação de forma definida.

Uma transação garante “tudo ou nada” dentro de uma execução. Ela não impede duas transações separadas de processarem a mesma mensagem.

## REST

REST é um estilo arquitetural baseado em recursos, interface uniforme e comunicação stateless.

Rotas orientadas a recursos:

```text
/api/buildings
/api/access-points
/api/access/events
```

Evitar rotas como:

```text
/api/getAllBuildings
/api/createNewCard
```

Nem toda ação cabe num CRUD puro. `POST /api/access/requests` representa um comando do domínio e continua sendo uma API coerente.

## Status codes

| Código | Uso |
|---|---|
| `200 OK` | Sucesso com resposta |
| `201 Created` | Recurso criado; idealmente inclui `Location` |
| `202 Accepted` | Aceito para processamento futuro |
| `204 No Content` | Sucesso sem body |
| `400 Bad Request` | Formato ou dados de entrada inválidos |
| `401 Unauthorized` | Não autenticado |
| `403 Forbidden` | Autenticado sem autorização |
| `404 Not Found` | Recurso não encontrado |
| `409 Conflict` | Conflito com estado ou unicidade |
| `500 Internal Server Error` | Falha inesperada |

Apesar do nome histórico, `401 Unauthorized` representa problema de autenticação. Falta de permissão após autenticação é `403`.

## DTO

DTO é um objeto criado para transportar dados numa fronteira.

Por que não retornar entidades diretamente?

- evita expor dados sensíveis;
- evita ciclos de navegação;
- desacopla contrato público do banco;
- permite evoluir os dois modelos separadamente;
- torna a intenção de cada endpoint explícita.

Exemplos:

```text
CreateBuildingRequest
BuildingResponse
AccessRequest
AccessResponse
```

## Paginação

Eventos crescem continuamente. Retornar todos causa uso excessivo de memória, rede e banco.

```http
GET /api/access/events?page=1&pageSize=50
```

A resposta deve incluir itens e metadados como página, tamanho e total. O backend deve limitar `pageSize`.

## OpenAPI e Swagger

### Analogia

OpenAPI é a receita escrita. Swagger é uma família de ferramentas que lê e apresenta essa receita.

### Definição

**OpenAPI Specification** define um formato para descrever APIs HTTP: rotas, métodos, parâmetros, schemas, autenticação e respostas.

**Swagger** pode referir-se a ferramentas como Swagger UI e Swagger Editor. Dizer “Swagger specification” é comum, mas o nome atual da especificação é OpenAPI.

No projeto, `AddOpenApi` e `MapOpenApi` geram o documento em Development. Swagger UI ainda não está implementado.

## Exemplos implementados: recursos administrativos

Pense no recurso como uma ficha de edifício: `GET` consulta, `POST` cria uma ficha, `PUT` substitui os campos editáveis e `DELETE` tenta removê-la.

Tecnicamente, o grupo `/api/buildings` usa Minimal APIs e contratos separados das entidades. `POST` retorna `201 Created` com body e `Location`; IDs desconhecidos retornam `404`; DataAnnotations com `AddValidation` rejeitam inclusive valores compostos apenas por whitespace e produzem `400` com `ValidationProblemDetails` e erros por campo. Remover um edifício com pisos retorna `409` por meio de `ProblemHttpResult`; a remoção bem-sucedida retorna `204`.

O recurso `Floor` segue o mesmo CRUD em `/api/floors`. `POST` retorna `404` quando o `BuildingId` não existe; `PUT` distingue piso e edifício pai inexistentes; `DELETE` retorna `409` quando existem `AccessPoints` ou `OccupancySessions`. Requests e responses são DTOs próprios, e o nome é validado como obrigatório, não composto apenas por whitespace e limitado a 100 caracteres.

`AccessPoint` também possui CRUD em `/api/access-points`. Pense nele como a ficha de um leitor físico: descreve em que piso está, onde fica e se suporta entrada ou saída. Tecnicamente, `POST` devolve `201` com `Location`, `PUT` devolve `200` com o recurso atualizado e `DELETE` devolve `204`; o piso pai inexistente resulta em `404`, enquanto permissões, eventos ou alertas dependentes impedem o delete com `409` e Problem Details. Os contratos próprios validam nome obrigatório até 150 caracteres e localização obrigatória até 250; `SupportsEntry` e `SupportsExit` são explícitos no request e response.

Os metadados `.WithName`, `.WithSummary`, `.Produces`, `.ProducesValidationProblem` e `.ProducesProblem` alimentam o documento OpenAPI code-first. `404` e `409` são descritos como Problem Details, enquanto `400` é descrito como Validation Problem Details. O ficheiro `SmartBuilding.Api.http` contém requests manuais para os cenários dos recursos administrativos; também inclui exemplos de listagem, criação, consulta, substituição e mudança de estado de cartões. O gerenciamento de cartões está implementado; permissões e a decisão/processamento de pedidos de acesso ainda não estão.

O recurso `Users` também possui CRUD em `/api/users`: listagem e consulta retornam `200`, criação retorna `201` com `Location`, update retorna `200` e delete retorna `204`. Email duplicado na criação ou atualização resulta em `409`; IDs inexistentes em consulta/update/delete resultam em `404`; dados inválidos resultam em `400`. `CreateUserRequest` exige nome de até 200 caracteres, email de até 320 e password de 12 a 128 caracteres. `TrimmedEmailAddressAttribute` valida o email após `Trim`, então whitespace externo é aceito; `UserService` remove-o e canonicaliza para lowercase antes de persistir. `UpdateUserRequest` não inclui password e `UserResponse` nunca inclui password ou hash. Hashing e a primitiva de verificação do Identity estão testados, mas login, verificação de credenciais no fluxo da aplicação, JWT e autorização continuam planejados.

### AccessCard: HTTP e separação em camadas

**Analogia simples:** pense num cartão de acesso como um crachá numa receção. A receção consulta e altera o registo, mas pede ao setor responsável que confirme os dados e ao arquivo que os guarde. Cada papel faz uma parte do trabalho.

**Definição técnica:** o endpoint HTTP traduz rota, body e status codes; o serviço da Application coordena o caso de uso e as regras; o contrato de repositório abstrai o acesso a dados; a Infrastructure implementa esse contrato com EF Core/PostgreSQL. A API não consulta `DbContext` diretamente. `DateTimeOffset` representa uma data/hora junto com o offset e identifica um instante; normalizar para UTC conserva o instante, não o offset escrito no request.

**Exemplo do projeto:** `AccessCardEndpoints` expõe `GET /api/cards` (lista ordenada pelo número), `GET /api/cards/{id}`, `POST /api/cards`, `PUT /api/cards/{id}` e `POST /api/cards/{id}/activate` ou `/deactivate`. A criação retorna `201 Created` com body e `Location`. O `PUT` substitui User, número do cartão e expiração opcional; enviar `null` remove a expiração. Número duplicado retorna `409`; User desconhecido retorna `404`; cartão desconhecido também resulta em `404`. Requests usam `DateTimeOffset`; a Application converte `ExpiresAt` para UTC e a resposta expõe datas com offset UTC. O percurso é `AccessCardEndpoints` → `IAccessCardService`/`AccessCardService` → `IAccessCardRepository`/`AccessCardRepository`.

**Resposta de entrevista:** “A API de AccessCard mantém o endpoint fino. Ele converte HTTP para o caso de uso, enquanto o serviço valida e normaliza os dados e depende de uma interface de repositório. Infrastructure implementa essa interface com EF Core. Assim, HTTP e persistência ficam separados; os resultados explícitos permitem mapear cartão/User ausente para `404` e número duplicado para `409`. Datas de entrada são `DateTimeOffset` e são normalizadas para UTC antes da persistência.”

**Exercício prático:** use o ficheiro `SmartBuilding.Api.http` para listar, criar, consultar, substituir, ativar e desativar um cartão. Tente um número já existente e um `UserId` desconhecido e identifique os status esperados. Envie `ExpiresAt` com um offset diferente de `Z`, depois consulte a resposta e explique por que o instante permanece igual, embora a representação seja UTC. Desenhe o percurso do request pelas quatro partes (endpoint, serviço, interface do repositório e implementação) e indique em que camada cada status HTTP é escolhido.

O gerenciamento de `AccessCards` está **implementado nesta branch**. A gestão CRUD de permissões também está implementada, mas isso não significa que a autorização de acesso esteja pronta: decidir/processar pedidos, criar `AccessEvent`s e atualizar `OccupancySession`s continuam por implementar.

No smoke PostgreSQL, `  Alex@Example.com  ` foi criado com `201` e armazenado como `alex@example.com`; repetição do email retornou `409`. O fluxo também confirmou update `200`, delete `204`, hash persistido e ausência de password/hash na response. Cartões bloqueiam o delete com `409`; `AccessEvent` sobrevive à remoção de User com `UserId` nulo.

### AccessPermission: gerir uma autorização não é decidir uma entrada

**Analogia simples:** é como o funcionário que mantém a lista de pessoas autorizadas a entrar numa sala. Cadastrar ou corrigir a lista não é o mesmo que verificar, no momento da tentativa, o crachá, o leitor e todas as regras para abrir a porta.

**Definição técnica:** `AccessPermission` é um recurso administrativo que associa um `User` a um `AccessPoint`, com início, fim opcional e estado ativo. A API expõe operações CRUD sobre esse recurso; elas não implementam o caso de uso de processamento/decisão de um pedido de acesso. A decisão ainda deverá avaliar os dados relevantes e produzir os resultados do fluxo de acesso.

**Exemplo do projeto:** em `/api/permissions`, `GET` lista ou consulta por ID (`200`, ou `404` para ID desconhecido); `POST` cria e retorna `201 Created`, body e `Location`; `PUT /{id}` substitui User, AccessPoint, intervalo e `IsActive`, retornando `200`; `DELETE /{id}` retorna `204`, ou `404` se a permissão não existir. No POST e PUT, User ou AccessPoint inexistente também resulta em `404`. Criar não recebe estado ativo e a permissão é criada ativa. Os requests usam `DateTimeOffset`; `ValidFrom` e o `ValidUntil` opcional são normalizados para UTC. Se `ValidUntil` estiver presente, deve ser estritamente posterior a `ValidFrom`; igualdade ou término anterior é inválido (`400`).

**Resposta de entrevista:** “O endpoint de AccessPermission implementa gestão CRUD, não a decisão de acesso. A API mapeia HTTP para `IAccessPermissionService`; a Application valida referências e intervalo e normaliza `DateTimeOffset` para UTC; o contrato `IAccessPermissionRepository` separa o caso de uso do repositório EF Core. O POST cria a permissão ativa e responde `201` com `Location`, enquanto o PUT substitui também `IsActive`. Referências ausentes retornam `404`. A avaliação de um pedido do leitor, a gravação de `AccessEvent` e a atualização de `OccupancySession` continuam fora deste CRUD e ainda não estão implementadas.”

**Exercício prático:** desenhe o caminho endpoint → service → interface de repositório → implementação. Escreva requests para criar uma permissão sem `ValidUntil`, consultar por ID, substituir e desativar via `PUT`, e remover. Preveja os status para User inexistente, AccessPoint inexistente, `ValidUntil == ValidFrom` e ID desconhecido. Depois use dois offsets diferentes que representem o mesmo instante para explicar por que a Application compara instantes e persiste valores UTC. Por fim, explique por que uma permissão criada não significa, por si só, que uma tentativa real será concedida.

A gestão CRUD de `AccessPermission` está **implementada nesta branch**. A decisão/processamento de acesso, a criação de `AccessEvent`s e a gestão de `OccupancySession`s continuam **planejadas/não implementadas**.

### Validar e normalizar email

Analogia: conferir se um envelope tem um endereço legível é diferente de confirmar quem mora nele. Tecnicamente, `TrimmedEmailAddressAttribute` remove whitespace externo para validar o formato no transporte; `UserService` depois aplica `Trim` e lowercase para produzir o valor canônico persistido. No projeto, `  Alex@Example.com  ` passa pela validação HTTP e fica armazenado como `alex@example.com`. Isso não autentica a pessoa nem confirma que ela controla a caixa postal.

Pense em Problem Details como uma etiqueta de erro com formato previsível: o status muda, mas o cliente sabe onde procurar título, detalhe e erros de campos. No runtime com PostgreSQL real, `400`, `404` e `409` foram confirmados com content type `application/problem+json`, coerente com o contrato OpenAPI.

## Contract-first e code-first

- **Code-first:** o documento é gerado a partir do código.
- **Contract-first:** a especificação é criada primeiro e guia implementação, mocks e clientes.

A vaga menciona APIs baseadas em Swagger specifications, portanto é importante saber ler uma especificação, discutir mudanças e manter implementação e contrato sincronizados.

## Versionamento

APIs evoluem sem quebrar consumidores. Estratégias incluem rota (`/api/v1`), header ou media type.

Antes de versionar, diferencie mudanças compatíveis de breaking changes. Adicionar campo opcional costuma ser compatível; remover ou mudar significado pode quebrar clientes.

## Pergunta de entrevista

**Qual a diferença entre OpenAPI e Swagger?**

> OpenAPI é a especificação que descreve o contrato da API. Swagger é o conjunto de ferramentas historicamente associado a ela, como Swagger UI e Editor. Posso gerar OpenAPI a partir do código ou implementar a API a partir de uma especificação criada primeiro.

**Por que o delete de Building retorna 409 em vez de 400?**

> O request é sintaticamente válido, mas entra em conflito com o estado atual porque existem pisos associados. `409 Conflict` comunica esse impedimento. O endpoint traduz `DeleteBuildingResult.HasFloors` num `ProblemHttpResult`, devolvido como `application/problem+json`, sem levar conceitos HTTP para a Application.

**Por que criar um Floor pode retornar 404 mesmo sendo um POST?**

> Porque o recurso pai indicado por `BuildingId` precisa existir. O payload pode ser válido, mas a relação solicitada aponta para um edifício ausente. `FloorRepository` converte a violação de FK em `BuildingNotFound`, e a API traduz esse resultado para `404` com Problem Details sem expor detalhes do PostgreSQL.

**Por que o delete de AccessPoint retorna `409` quando há um evento dependente?**

> O request é válido, mas remover o ponto apagaria a referência necessária para preservar o histórico do evento. A API retorna `409 Conflict` com Problem Details; a Application comunica `HasDependents` e não depende de HTTP. A verificação antecipada cobre o caminho comum, enquanto a FK `Restrict` protege contra dependências concorrentes.

**Por que password só aparece no POST de Users?**

> A password inicial é necessária para criar a credencial, mas é convertida em hash antes de persistir e nunca aparece na response. `PUT /api/users/{id}` altera somente nome, email e estado ativo; assim, uma atualização de perfil não troca silenciosamente a credencial. Fluxo explícito de alteração de password e login ainda estão planejados.

**Por que aceitar whitespace externo no email?**

> A validação HTTP verifica o formato do valor depois de `Trim`, e a Application remove os espaços e canonicaliza para lowercase antes de persistir. Assim, o contrato aceita uma entrada tolerante sem criar variantes inconsistentes no índice único. Isso não valida a propriedade da caixa postal nem autentica o utilizador.

## Exercício

Abra o documento OpenAPI em Development e compare as cinco operações de `/api/access-points` com `SmartBuilding.Api.http`. Explique o body, `Location`, status e content type de cada cenário. Trace como sucesso, `FloorNotFound` e dependentes se tornam respostas HTTP e por que o delete bloqueado usa `409`. Para Users, envie um email com whitespace externo e explique a diferença entre validação do formato e normalização; descreva como o teste de hash com password errada difere de um login, que ainda não existe. Depois especifique em papel o futuro `POST /api/access/requests`, incluindo autenticação, retry e idempotência, sem descrevê-lo como implementado.
