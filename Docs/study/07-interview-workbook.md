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

> Estou a construir uma plataforma de controlo de acessos e ocupação com .NET 10, ASP.NET Core, Angular, PostgreSQL e EF Core. Separei Domain, Application, Infrastructure e API para manter regras independentes de frameworks. Atualmente concluí a fundação e o modelo de domínio e estou a implementar a persistência com Fluent API e testes de metadados. O roadmap inclui REST, JWT, SignalR, processamento em background, Docker, GitHub Actions e Kubernetes, sempre uma fase por vez.
> Estou a construir uma plataforma de controlo de acessos e ocupação com .NET 10, ASP.NET Core, PostgreSQL e EF Core. Separei Domain, Application, Infrastructure e API para manter regras independentes de frameworks. Na branch atual implementei o modelo relacional, o registro do DbContext com Npgsql, a migration inicial e um seed idempotente e protegido contra inicializações concorrentes. Validei a persistência num PostgreSQL real. Hoje existe apenas o host HTTP básico com `GET /`; a API REST funcional, Angular, JWT, SignalR, Docker da aplicação, CI/CD e Kubernetes continuam planejados para fases futuras.

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

## Perguntas EF Core e PostgreSQL

### O que é DbContext?

Sessão curta, tracking, modelo, conexão e unit of work. Normalmente scoped, não thread-safe e não singleton.

### O que é DbSet?

Entrada para query e alteração de um tipo de entidade. Não é literalmente uma tabela.

### O que faz AsNoTracking?

Desativa tracking para leitura, reduzindo memória e processamento quando atualização não será feita.

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
