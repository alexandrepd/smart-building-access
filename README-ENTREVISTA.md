# Guia de Entrevista Full-Stack

Guia de revisão rápida, em português, para a vaga Full-Stack Angular/.NET. Cada resposta foi escrita para ser dita em voz alta em cerca de 30–60 segundos. Ajuste os exemplos à sua experiência real; não apresente conteúdo planeado como trabalho implementado.

## Índice

- [Apresentação](#apresentação)
- [Angular e UX](#angular-e-ux)
- [RxJS](#rxjs)
- [REST e OpenAPI](#rest-e-openapi)
- [.NET e Backend](#net-e-backend)
- [PostgreSQL e EF Core](#postgresql-e-ef-core)
- [Git e Colaboração](#git-e-colaboração)
- [Containers e Entrega](#containers-e-entrega)
- [Windows Server](#windows-server)
- [Comportamentais](#comportamentais)
- [Revisão de Véspera](#revisão-de-véspera)

## Apresentação

### “Fale-me sobre si e sobre o seu projeto.”

> Estou a desenvolver uma plataforma de controlo de acessos e monitorização de ocupação com .NET 10, ASP.NET Core, PostgreSQL e EF Core. Separei Domain, Application, Infrastructure e API para manter as regras de negócio independentes da tecnologia. Já implementei persistência e CRUDs REST iniciais para Buildings, Floors, AccessPoints e Users, com validação, tratamento de erros e testes. O projeto evolui por fases; Angular, autenticação, processamento de acessos em tempo real e deployment continuam no roadmap.

### “O que está implementado e o que ainda falta?”

> Estão implementados o modelo de domínio, a migration PostgreSQL, o seed de desenvolvimento e CRUDs iniciais de Buildings, Floors, AccessPoints e Users. O endpoint de criação de User usa PasswordHasher e nunca devolve o hash. Ainda faltam Cards e Permissions, a decisão de acesso, login/JWT, Angular, ocupação, SignalR, processamento em background e a infraestrutura Docker/CI/Kubernetes.

Nota: CRUD de Users não significa autenticação implementada. Ainda não existe fluxo de login ou emissão de JWT.

## Angular e UX

### “O que é Angular?”

> Angular é um framework TypeScript para construir aplicações web. Organiza a interface em componentes e oferece templates, dependency injection, router, formulários, cliente HTTP e ferramentas de build. Eu usaria componentes para as páginas e partes visuais, services para integração e estado partilhado, e a API para regras e segurança.

### “O que é uma SPA?”

> É uma aplicação que carrega a estrutura inicial e troca o conteúdo com navegação no cliente, sem pedir uma página HTML completa para cada rota. O servidor ainda fornece os assets e a aplicação continua a depender do backend. As rotas diretas precisam de fallback para `index.html` no servidor.

### “O que é um component?”

> É uma unidade de interface composta por estado/comportamento TypeScript, template e estilos. Eu manteria o componente focado em apresentação e interação local; chamadas reutilizáveis à API ficariam num service.

### “Qual a diferença entre component, directive e pipe?”

> Component tem template e representa uma parte da interface. Directive altera comportamento ou apresentação de um elemento existente. Pipe transforma um valor para apresentação no template, como formatar uma data. Evitaria colocar lógica de negócio num pipe.

### “O que são standalone components?”

> São componentes que declaram diretamente as dependências que importam e não precisam ser declarados num NgModule. São o estilo normal em Angular moderno e reduzem configuração indireta. Ainda posso encontrar NgModules em aplicações e bibliotecas existentes.

### “Para que serve Dependency Injection no Angular?”

> Permite que um componente peça uma dependência e que o Angular forneça a instância. Isso reduz acoplamento e facilita testes. `providedIn: 'root'` normalmente cria uma instância partilhada; providers numa rota ou componente podem criar um escopo mais local.

### “O que é Change Detection? E OnPush?”

> Change Detection é o mecanismo que atualiza a view quando o estado relevante muda. `OnPush` reduz verificações desnecessárias e funciona bem com referências imutáveis, Signals, eventos e Observables ligados ao template. Não quer dizer que o componente nunca será verificado.

### “O que são Signals?”

> Signals representam estado reativo síncrono. `signal` guarda um valor, `computed` calcula um valor derivado e `effect` executa um efeito secundário. São convenientes para estado de UI; não substituem RxJS para streams assíncronos ou operações temporais.

### “Como tornaria um portal de self-service fácil de usar?”

> Começaria pelas tarefas que a pessoa precisa concluir. Reduziria passos, pediria só os dados necessários, preservaria campos válidos quando há erro e mostraria loading, sucesso e falha com mensagens claras. Também verificaria navegação por teclado, labels, leitores de ecrã e uso em mobile. UX é a capacidade de concluir a tarefa, não apenas a aparência.

### “Como trataria estados de loading, vazio e erro?”

> Modelaria os estados explicitamente: loading, sucesso com dados, sucesso sem dados e erro recuperável. Não converteria uma falha numa lista vazia, porque “não há resultados” e “não consegui consultar” são situações diferentes.

### “Como testa um component?”

> Testaria comportamento observável: conteúdo apresentado, interação, estado vazio/erro e chamadas feitas ao service. Substituiria dependências externas por doubles de teste quando o objetivo é isolar a UI. Evitaria testar detalhes privados da classe.

## RxJS

### “Observable e Promise são iguais?”

> Promise resolve uma vez. Observable pode emitir vários valores, compõe-se com operators e permite cancelar a subscrição. Muitos Observables são lazy, mas isso depende do produtor. O HttpClient retorna Observables que normalmente emitem uma resposta e completam.

### “Para que serve switchMap?”

> Mapeia cada valor para outro Observable e deixa de acompanhar o anterior quando chega um novo. É útil numa pesquisa: a consulta do termo anterior fica obsoleta. Com HttpClient, cancelar a subscrição também pode cancelar o request. Não o usaria automaticamente para gravações que não podem ser descartadas.

### “switchMap, mergeMap, concatMap ou exhaustMap?”

> `switchMap` acompanha o mais recente; `mergeMap` permite concorrência; `concatMap` processa em sequência; `exhaustMap` ignora novas emissões enquanto a operação atual decorre. Escolho pela semântica: pesquisa, tarefas paralelas, fila ordenada ou prevenção de duplo submit.

### “O que faz catchError?”

> Interceta uma falha do stream e permite devolver outro Observable, transformar o erro ou propagá-lo. A posição importa: dentro de um `switchMap`, pode falhar uma consulta sem terminar o stream de pesquisa inteiro.

### “Como evita memory leaks em subscriptions?”

> Prefiro `async` pipe, que subscreve e limpa automaticamente. Em subscriptions imperativas de longa duração uso `takeUntilDestroyed()` ou gestão explícita do ciclo de vida. Também verifico se a subscrição é mesmo necessária.

### “Quando usaria Signals em vez de RxJS?”

> Signals para estado síncrono e valores derivados da interface. RxJS para streams, eventos, cancelamento e composição temporal. Podem coexistir; a escolha depende do problema, não de uma regra de substituir um pelo outro.

## REST e OpenAPI

### “O que é uma REST API?”

> É uma API HTTP organizada em recursos, com uso coerente de métodos e status codes, comunicação stateless e representações como JSON. Usar JSON sozinho não torna uma API RESTful.

### “Endpoint e API são a mesma coisa?”

> Não. API é o contrato/conjunto de operações. Endpoint é uma operação concreta identificada por método e rota, por exemplo `GET /api/users/{id}`.

### “OpenAPI e Swagger são iguais?”

> OpenAPI é a especificação do contrato: rotas, schemas, parâmetros, segurança e respostas. Swagger é uma família de ferramentas para editar ou visualizar essa especificação, como Swagger UI. Neste projeto o documento OpenAPI é code-first; Swagger UI ainda não está instalado.

### “Quando usa 401, 403, 404 ou 409?”

> `401` quando a identidade não foi estabelecida; `403` quando está autenticada mas não autorizada; `404` quando o recurso não existe; `409` quando a operação válida conflita com o estado, como email duplicado ou tentativa de apagar um User que ainda tem cartões.

### “O que é DTO e por que não devolver a entidade EF?”

> DTO é um objeto de transporte que define os dados públicos da request/response. Evita expor campos internos ou sensíveis, ciclos de navegação e acoplamento entre contrato HTTP e esquema do banco. No endpoint de User, por exemplo, `PasswordHash` nunca faz parte do response.

### “O que significa idempotência?”

> Repetir a mesma operação produz o mesmo efeito final. Uma transação garante atomicidade de uma execução, mas não impede duplicação entre retries. Para um leitor físico, um identificador externo único poderia permitir reconhecer o mesmo evento reenviado.

### “Como documenta erros?”

> Declaro no OpenAPI os status e formatos de resposta. Para falhas uso Problem Details; para validação, Validation Problem Details com erros por campo. Também mantenho exemplos `.http` e testo que content type e status correspondem ao contrato.

## .NET e Backend

### “Como explica as camadas?”

> Domain contém vocabulário e regras de negócio. Application coordena casos de uso e define contratos. Infrastructure implementa persistência e integrações. API traduz HTTP e liga dependências no composition root. As dependências apontam para o núcleo; Domain não conhece EF nem ASP.NET.

### “O que é Dependency Injection?”

> É fornecer uma dependência a um componente em vez de o componente a criar diretamente. Isso reduz acoplamento, centraliza configuração e facilita substituir implementações em testes. No ASP.NET Core, `DbContext` é normalmente scoped por request.

### “Por que async/await numa API?”

> Para aguardar operações de I/O sem manter uma thread bloqueada enquanto banco ou rede respondem. Isso ajuda a escalabilidade. Não significa paralelismo automático; propago `CancellationToken` e evito `.Result`/`.Wait()`.

### “O que é middleware?”

> É uma etapa transversal do pipeline HTTP que pode agir antes e depois do endpoint. É usado para tratamento de erros, autenticação, autorização, logging e HTTPS. A ordem importa: autenticação deve ocorrer antes da autorização.

### “Como investigaria um bug?”

> Reproduzo e registo os passos, recolho evidência, identifico a primeira camada onde o resultado diverge do esperado, formulo uma hipótese e testo. Corrijo a causa, adiciono uma regressão automatizada e valido os fluxos adjacentes. No browser começo também pela Console e Network tab.

## PostgreSQL e EF Core

### “O que é DbContext?”

> É uma sessão curta de trabalho com o banco. Constrói o modelo, gere entidades rastreadas e persiste alterações com `SaveChanges`. Não é thread-safe e normalmente tem lifetime scoped.

### “O que é DbSet?”

> É a entrada do EF Core para consultar e alterar entidades de um tipo. `DbSet<User>` não é literalmente a tabela; é uma abstração que o provider traduz para queries e comandos SQL.

### “O que faz AsNoTracking?”

> Desativa o change tracking numa consulta de leitura, reduzindo memória e trabalho quando não vou atualizar as entidades.

### “O que é uma migration?”

> É uma alteração versionada do schema do banco, gerada a partir do modelo EF. `migrations add` cria a alteração; `database update` aplica-a. Migration compartilhada e aplicada deve evoluir por uma nova migration, não ser reescrita.

### “Índice sempre melhora performance?”

> Não. Acelera consultas específicas, mas ocupa espaço e aumenta custo de escrita. Escolho índices com base nas consultas reais e nos planos de execução.

### “Transação resolve concorrência ou duplicação?”

> Não sozinha. Transação garante atomicidade de uma unidade de trabalho. Concorrência exige controlar operações simultâneas; idempotência impede que retries repetidos dupliquem efeitos. Constraints e estratégia de retry podem participar da solução.

## Git e Colaboração

### “fetch e pull?”

> `fetch` atualiza referências remotas sem integrar no branch atual. `pull` faz fetch e depois integra por merge ou rebase, conforme configuração.

### “merge e rebase?”

> Merge combina históricos e preserva a bifurcação. Rebase reaplica commits e cria novos hashes. Evito rebase de commits já compartilhados sem coordenação.

### “Como faz code review?”

> Verifico comportamento, casos de erro, testes, segurança, legibilidade, compatibilidade e escopo. Escrevo comentários específicos, separando bloqueios de sugestões, e explico o risco em vez de impor preferência pessoal.

### “Como trabalha com outras equipas?”

> Alinho contratos e responsabilidades cedo, documento mudanças, mantenho PRs pequenos, comunico bloqueios e confirmo que frontend e backend concordam sobre payloads e erros. Se o contrato mudar, atualizo OpenAPI e exemplos de consumo.

## Containers e Entrega

### “CI, Continuous Delivery e Continuous Deployment?”

> CI integra alterações e executa build/testes. Continuous Delivery mantém artefactos prontos para publicar, normalmente com aprovação. Continuous Deployment publica automaticamente cada alteração aprovada.

### “Image e container?”

> Image é um artefacto imutável construído em camadas. Container é uma execução da image. Dados que precisam sobreviver à substituição do container ficam em volumes ou serviços externos.

### “Por que multi-stage Docker build?”

> Uso SDK/Node para compilar numa etapa e copio só o resultado para uma image final menor. Isso reduz tamanho, ferramentas presentes em runtime e superfície de ataque.

### “O que fariam readiness e liveness probes?”

> Readiness diz se a instância pode receber tráfego. Liveness indica se está bloqueada e deve reiniciar. Uma aplicação pode estar viva, mas ainda não pronta para requests.

### “Service e Ingress em Kubernetes?”

> Service dá um endereço estável e distribui tráfego para Pods. Ingress define regras de entrada HTTP/HTTPS e precisa de um Ingress Controller.

### “Como publicar o frontend Angular em container?”

> Compilaria assets estáticos numa etapa Node e serviria a saída por Nginx numa image final. Configuraria fallback das rotas da SPA para `index.html`, cache adequado e endpoint da API por uma estratégia explícita de configuração.

## Windows Server

### “O que sabe de IIS?”

> IIS hospeda sites e pode atuar como reverse proxy. Para ASP.NET Core, o Hosting Bundle integra IIS com Kestrel. Conheço os conceitos de Site, Application Pool, bindings, certificados TLS, permissões NTFS e logs. No projeto, Windows Server é estudo complementar, não o destino de deployment planeado.

## Comportamentais

### “Conte sobre um bug difícil.”

Use STAR: situação, tarefa, ação e resultado. Explique como reproduziu, que evidências recolheu, a causa raiz, o teste que impediu regressão e o que aprendeu. Use uma história verdadeira; se ainda não teve um caso profissional, descreva um caso real de estudo/projeto e identifique-o como tal.

### “Como reage a feedback de code review?”

> Primeiro procuro entender o risco ou requisito por trás do comentário. Se concordo, ajusto e adiciono teste quando necessário. Se discordo, apresento evidência e trade-offs de forma respeitosa. O objetivo é melhorar o produto, não defender o primeiro código escrito.

### “Como contribui para melhoria contínua?”

> Observo fricções repetidas, proponho uma melhoria pequena com impacto claro, valido-a com a equipa e acompanho o resultado. Evito grandes refactors sem problema ou métrica que os justifique.

## Inglês: Respostas Curtas

- “I use `switchMap` for searches because a newer query makes the previous result obsolete.”
- “Route guards improve navigation, but authorization must be enforced by the backend.”
- “The API returns Problem Details so clients receive a consistent error format.”
- “I reproduce the issue, inspect the network request, isolate the failing layer, and add a regression test.”
- “I have not implemented Angular in this project yet, but I can explain the architecture and demonstrate the concepts with a focused exercise.”

## Revisão de Véspera

Se tiver 90 minutos:

1. **30 min:** Angular components, DI, forms e Router.
2. **25 min:** Observable e escolha entre `switchMap`, `concatMap`, `mergeMap` e `exhaustMap`.
3. **15 min:** REST status, OpenAPI e autenticação/autorização.
4. **10 min:** Git, CI, Docker e Kubernetes: diferença entre os termos.
5. **10 min:** apresentação do projeto e respostas em inglês em voz alta.

Se não souber algo, explique o que conhece, o limite e como investigaria. Honestidade técnica e raciocínio claro são melhores do que fingir experiência.