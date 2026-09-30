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

## Exemplos implementados: Buildings, Floors e AccessPoints

Pense no recurso como uma ficha de edifício: `GET` consulta, `POST` cria uma ficha, `PUT` substitui os campos editáveis e `DELETE` tenta removê-la.

Tecnicamente, o grupo `/api/buildings` usa Minimal APIs e contratos separados das entidades. `POST` retorna `201 Created` com body e `Location`; IDs desconhecidos retornam `404`; DataAnnotations com `AddValidation` rejeitam inclusive valores compostos apenas por whitespace e produzem `400` com `ValidationProblemDetails` e erros por campo. Remover um edifício com pisos retorna `409` por meio de `ProblemHttpResult`; a remoção bem-sucedida retorna `204`.

O recurso `Floor` segue o mesmo CRUD em `/api/floors`. `POST` retorna `404` quando o `BuildingId` não existe; `PUT` distingue piso e edifício pai inexistentes; `DELETE` retorna `409` quando existem `AccessPoints` ou `OccupancySessions`. Requests e responses são DTOs próprios, e o nome é validado como obrigatório, não composto apenas por whitespace e limitado a 100 caracteres.

`AccessPoint` também possui CRUD em `/api/access-points`. Pense nele como a ficha de um leitor físico: descreve em que piso está, onde fica e se suporta entrada ou saída. Tecnicamente, `POST` devolve `201` com `Location`, `PUT` devolve `200` com o recurso atualizado e `DELETE` devolve `204`; o piso pai inexistente resulta em `404`, enquanto permissões, eventos ou alertas dependentes impedem o delete com `409` e Problem Details. Os contratos próprios validam nome obrigatório até 150 caracteres e localização obrigatória até 250; `SupportsEntry` e `SupportsExit` são explícitos no request e response.

Os metadados `.WithName`, `.WithSummary`, `.Produces`, `.ProducesValidationProblem` e `.ProducesProblem` alimentam o documento OpenAPI code-first. `404` e `409` são descritos como Problem Details, enquanto `400` é descrito como Validation Problem Details. O ficheiro `SmartBuilding.Api.http` contém requests manuais para os cenários principais dos três recursos. CRUD de cartões e outros recursos continuam planejados; a decisão e o processamento de pedidos de acesso também não estão implementados.

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

## Exercício

Abra o documento OpenAPI em Development e compare as cinco operações de `/api/access-points` com `SmartBuilding.Api.http`. Explique o body, `Location`, status e content type de cada cenário. Trace como sucesso, `FloorNotFound` e dependentes se tornam respostas HTTP e por que o delete bloqueado usa `409`. Depois especifique em papel o futuro `POST /api/access/requests`, incluindo autenticação, retry e idempotência, sem descrevê-lo como implementado.
