# Camada API

## Propósito

`SmartBuilding.Api` é a borda HTTP e o processo executável da aplicação. Ela recebe requests, valida o contrato de transporte, chama casos de uso e transforma resultados em respostas HTTP.

A API também é o composition root: o local onde abstrações são conectadas às implementações concretas.

## Estado atual

A API contém:

- host ASP.NET Core em .NET 10;
- geração do documento OpenAPI em ambiente de desenvolvimento;
- redirecionamento HTTPS;
- endpoint `GET /` para indicar que o processo está ativo;
- referências para Application e Infrastructure.

Ainda não contém:

- controllers ou grupos de endpoints do domínio;
- registro do `SmartBuildingDbContext`;
- endpoints CRUD;
- autenticação JWT;
- autorização por papéis;
- middleware global de erros;
- SignalR;
- Swagger UI interativo;
- health checks completos.

## Dependências

### Permitidas

- `SmartBuilding.Application` para executar casos de uso;
- `SmartBuilding.Infrastructure` para registrar implementações;
- ASP.NET Core e bibliotecas relacionadas ao transporte;
- configuração, logging, autenticação e observabilidade.

### Proibidas como responsabilidade

- implementar regras de concessão de acesso em endpoints;
- consultar `DbContext` diretamente em cada endpoint como padrão arquitetural;
- retornar entidades persistidas diretamente;
- guardar secrets no código ou em ficheiros versionados;
- considerar guards do frontend como autorização suficiente.

## Bootstrap atual

`Program.cs` executa quatro passos:

```text
1. cria WebApplicationBuilder;
2. registra OpenAPI;
3. constrói a aplicação;
4. configura middleware e endpoints.
```

O endpoint atual responde aproximadamente:

```json
{
  "name": "Smart Building API",
  "status": "Running"
}
```

Ele confirma que o host iniciou, mas não confirma conectividade com PostgreSQL.

## Fluxo futuro de request

```mermaid
flowchart LR
    Request[HTTP Request] --> Middleware[Middleware]
    Middleware --> Endpoint[Endpoint ou Controller]
    Endpoint --> App[Application Use Case]
    App --> Endpoint
    Endpoint --> Response[HTTP Response]
```

O endpoint deve ser fino: mapear entrada, chamar o caso de uso e mapear o resultado.

## Composition root

Na Fase 3, a API deverá registrar a persistência:

```csharp
builder.Services.AddDbContext<SmartBuildingDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention());
```

Esse exemplo representa o próximo passo e ainda não existe em `Program.cs`.

Depois, contratos de Application serão registrados com implementações de Infrastructure. A API conhece os dois lados apenas para realizar essa composição.

## Contratos HTTP

Os recursos usam substantivos plurais em kebab-case:

```text
/api/buildings
/api/floors
/api/access-points
/api/access/requests
/api/access/events
/api/occupancy/sessions
/api/security/alerts
```

DTOs devem representar explicitamente requests e responses. Entidades EF não devem ser serializadas diretamente, porque isso acopla o contrato público ao esquema interno e pode expor propriedades de navegação.

## Códigos de resposta

Os endpoints devem usar semântica HTTP consistente:

- `200 OK`: consulta ou operação concluída com representação;
- `201 Created`: criação de recurso;
- `204 No Content`: atualização ou remoção sem corpo;
- `400 Bad Request`: contrato inválido;
- `401 Unauthorized`: autenticação ausente ou inválida;
- `403 Forbidden`: identidade válida sem permissão;
- `404 Not Found`: recurso inexistente;
- `409 Conflict`: violação de unicidade ou conflito de estado;
- `500 Internal Server Error`: falha inesperada tratada globalmente.

## Tratamento de erros

Erros inesperados devem ser convertidos para Problem Details sem stack trace. Logs internos devem conter contexto e `traceId`, mas nunca passwords, hashes, tokens completos ou segredos.

## Validação

A API valida o formato do transporte: JSON, campos obrigatórios, identificadores e limites básicos. Regras de negócio permanecem em Domain/Application.

## OpenAPI e Swagger

Atualmente `MapOpenApi` publica o documento OpenAPI somente em Development. Uma UI interativa ainda deverá ser adicionada na fase da API.

Cada endpoint futuro deve documentar tipos de resposta, status possíveis, autenticação e exemplos úteis.

## Autenticação e autorização

Planejado para fase própria:

- login com password verificada por hash;
- emissão de JWT;
- papéis `Admin` e `SecurityOperator`;
- policies aplicadas no servidor;
- endpoints protegidos por autorização.

JWT não deve carregar dados sensíveis. Autorização da API nunca deve depender apenas da interface Angular.

## Paginação

Eventos de acesso crescerão continuamente. Consultas de histórico devem exigir paginação e aceitar filtros sem carregar todo o conjunto em memória.

## SignalR

O futuro hub `/accessHub` notificará eventos, alertas e alterações de ocupação. SignalR pertence à borda da API; a decisão sobre quando publicar decorre dos casos de uso.

## Próximos passos

O único passo da API pertencente à Fase 3 é configurar a persistência e a connection string. CRUD, validação HTTP e tratamento global de erros pertencem à Fase 4.
