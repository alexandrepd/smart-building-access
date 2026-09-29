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

Exemplo futuro:

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

## Exercício

Especifique em papel `POST /api/access/requests`: request, response, autenticação, status codes, possibilidade de retry e estratégia de idempotência.
