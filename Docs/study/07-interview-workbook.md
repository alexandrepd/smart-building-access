# Workbook de Preparação para Entrevista

## Como usar

Para cada pergunta:

1. responda em até 60 segundos;
2. dê um exemplo do Smart Building;
3. mencione um limite ou trade-off;
4. repita em inglês técnico simples.

Evite decorar parágrafos inteiros. Memorize a estrutura da resposta.

## Apresentação do projeto

Modelo de resposta:

> Estou a construir uma plataforma de controlo de acessos e ocupação com .NET 10, ASP.NET Core, PostgreSQL e EF Core. Separei Domain, Application, Infrastructure e API para manter regras independentes de frameworks. Na branch atual implementei o primeiro vertical slice da Fase 4: CRUD REST de `Buildings`, com serviço de Application, repositório EF Core, validação, Problem Details e nove casos de teste unitário. Também validei o fluxo contra PostgreSQL real, incluindo `400`, `404` e `409` com `application/problem+json`. Apenas `Buildings` está implementado; os demais CRUDs, Angular, JWT, SignalR, Docker da aplicação, CI/CD e Kubernetes continuam planejados.

Não diga que uma tecnologia está implementada quando está apenas no roadmap.

## Perguntas Backend

### O que é Dependency Injection?

Pontos esperados:

- dependências fornecidas externamente;
- menor acoplamento;
- composition root;
- lifetimes;
- testabilidade;
- não confundir com Service Locator.

### Por que usar async/await numa API?

Pontos esperados:

- liberar thread enquanto aguarda I/O;
- melhorar escalabilidade;
- não significa paralelismo automático;
- propagar `CancellationToken`;
- evitar bloqueio com `.Result`.

### Por que separar Domain e Infrastructure?

Pontos esperados:

- regras independentes de banco e framework;
- direção de dependência;
- testes rápidos;
- substituição de detalhes;
- evitar lógica de negócio em mappings.

### Como implementou um vertical slice sem acoplar as camadas?

Pontos esperados:

- Minimal API conhece o contrato HTTP;
- `BuildingService` coordena commands, DTOs e normalização;
- `ApplicationValidationException` identifica falhas esperadas e a propriedade inválida;
- `IBuildingRepository` pertence à Application;
- `BuildingRepository` implementa o contrato com EF Core;
- Dependency Injection conecta as implementações;
- o fake do repositório permite testar o serviço isoladamente.

## Perguntas REST

### O que torna uma API RESTful?

Fale sobre recursos, métodos HTTP, stateless, interface uniforme, representações, status codes e cache quando aplicável. Não responda apenas “usa JSON”.

### 401 ou 403?

- `401`: identidade não estabelecida.
- `403`: identidade válida sem permissão.

### O que é idempotência?

Explique repetição com mesmo efeito final. Use retry do leitor e `ExternalEventId` único. Diferencie de transação.

### OpenAPI e Swagger são iguais?

Explique especificação versus ferramentas e code-first versus contract-first.

### Como desenhou os status do CRUD de Buildings?

Explique `200` para leitura e atualização, `201` com `Location` para criação, `204` para remoção, `404` para ID desconhecido, `400` para validação e `409` quando pisos impedem o delete. Diga que whitespace é rejeitado por DataAnnotations com erros por campo, que o handler converte somente `ApplicationValidationException` em `400` e mantém falhas inesperadas como `500`, e que `404` e `409` seguem Problem Details coerente com OpenAPI. Somente esse recurso está implementado.

## Perguntas EF Core e PostgreSQL

### O que é DbContext?

Sessão curta, tracking, modelo, conexão e unit of work. Normalmente scoped, não thread-safe e não singleton.

### O que é DbSet?

Entrada para query e alteração de um tipo de entidade. Não é literalmente uma tabela.

### O que faz AsNoTracking?

Desativa tracking para leitura, reduzindo memória e processamento quando atualização não será feita.

Exemplo do projeto: `BuildingRepository` usa `AsNoTracking` na lista e na consulta por ID, mas mantém tracking na atualização e remoção.

### Como tratou uma corrida no delete de Building?

Primeiro verifico se existem pisos para retornar um conflito esperado. Como um piso ainda pode ser inserido antes do commit, a FK com `Restrict` é a garantia final. Capturo apenas a `ForeignKeyViolation` e converto esse caso em `HasFloors`; outras falhas não são mascaradas.

### Por que migrations?

Versionam e reproduzem evolução do schema. Não editar migration compartilhada e aplicada; criar uma nova.

Exemplo do projeto: `InitialCreate` foi gerada com a ferramenta local `dotnet-ef` 10.0.12 e aplicada a PostgreSQL 18. O histórico em `__EFMigrationsHistory` confirmou qual versão do esquema estava instalada.

### Como tornou o seed idempotente?

Usei GUIDs fixos para `HQ Lisbon`, `Ground Floor` e `Main Entrance`. Antes das consultas, o seeder abre uma transação e adquire `pg_advisory_xact_lock(1937001)`. Assim, inicializações concorrentes com a mesma chave são serializadas; cada entidade só é adicionada quando o respetivo ID não existe, mantendo contagens `1|1|1`.

### Como configurou a persistência sem versionar secrets?

A API exige `ConnectionStrings:SmartBuilding`, falha cedo quando a configuração está ausente e entrega o valor a `AddInfrastructure`. Para desenvolvimento local, inicializei User Secrets; `appsettings.json` contém somente uma entrada vazia e nenhuma credencial foi commitada.

### Usar PostgreSQL em Docker significa que a aplicação está containerizada?

Não. O container foi apenas um ambiente descartável de PostgreSQL na porta isolada `55432` para validar migration, arranque e seed. A aplicação ainda não tem a fase Docker implementada.

### Índice sempre melhora performance?

Não. Acelera padrões específicos de leitura, mas usa espaço e adiciona custo de escrita/manutenção.

### Transação resolve duplicação?

Não necessariamente. Garante atomicidade de uma execução; idempotência e concorrência precisam de identificadores, constraints e estratégia própria.

## Perguntas Angular

### Component versus service

Component cuida de UI e interação local. Service encapsula comunicação, estado partilhado ou comportamento reutilizável.

### Observable versus Promise

Observable pode emitir vários valores, compor operators e ser cancelado. Promise resolve uma vez.

### Para que serve switchMap?

Troca para novo Observable e cancela o anterior. Bom para pesquisa; avaliar semântica antes de usar em gravações.

### Guard protege a API?

Não. Protege navegação e experiência. Backend aplica autorização real.

### Para que serve interceptor?

Tratamento transversal de HTTP: JWT, correlation ID, erros e headers.

## Perguntas Git e colaboração

### Fetch versus pull

`fetch` baixa referências. `pull` baixa e integra por merge ou rebase conforme configuração.

### Merge versus rebase

Merge preserva bifurcação. Rebase reaplica commits e reescreve hashes. Evitar rebase de histórico compartilhado sem coordenação.

### Como revisar um PR?

Verificar correção, testes, segurança, legibilidade, compatibilidade e escopo. Fazer comentários específicos e distinguir bloqueios de sugestões.

## Perguntas DevOps

### CI, Delivery e Deployment

- CI integra e verifica.
- Delivery mantém pronto para publicar.
- Deployment publica automaticamente.

### Image versus container

Image é artefacto imutável; container é execução dessa image.

### Pod versus Deployment versus Service

Pod executa containers. Deployment mantém réplicas e rollouts. Service fornece endereço estável e distribuição.

### Readiness versus liveness

Readiness controla tráfego. Liveness decide reinício.

## Perguntas comportamentais

Use STAR:

- **Situation:** contexto.
- **Task:** responsabilidade.
- **Action:** o que você fez.
- **Result:** resultado mensurável e aprendizagem.

Prepare histórias sobre:

- bug difícil;
- conflito técnico;
- melhoria proativa;
- incidente ou falha;
- aprendizagem rápida;
- feedback recebido;
- decisão com trade-off.

Uma história técnica disponível nesta branch é a proteção em duas camadas do delete: consulta antecipada para o fluxo comum e constraint FK para concorrência. Num PostgreSQL real, o resultado validado foi `409 Conflict` com content type `application/problem+json` quando o edifício possui pisos; `400` e `404` também foram confirmados com esse content type.

## Inglês técnico

Pratique frases curtas:

> I separated business rules from infrastructure so they can be tested without a database.

> The API is stateless, and authorization is enforced on the server.

> EF Core tracks changes through the DbContext and persists them with SaveChangesAsync.

> Our CI pipeline builds the solution and runs tests before a pull request can be merged.

> Idempotency prevents retries from producing duplicate effects.

## Checklist de honestidade técnica

Use três níveis:

- **Implementei:** existe no código e foi validado.
- **Estudei/pratiquei:** você consegue explicar e demonstrar isoladamente.
- **Planejei:** está no roadmap, mas ainda não foi executado.

Essa precisão é mais forte do que fingir experiência. Demonstra consciência de engenharia e capacidade de aprender.
