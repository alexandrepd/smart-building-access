# Angular: Revisão Intensiva para Entrevista

## Objetivo

Este guia serve para revisão rápida antes de uma entrevista Full-Stack. O projeto ainda não contém frontend Angular; os exemplos abaixo mostram como o dashboard do Smart Building poderá ser construído na Fase 7. Não apresente esses exemplos como funcionalidades já implementadas.

O projeto não fixou uma versão Angular. Os conceitos são estáveis; confirme a versão usada pela empresa antes de afirmar detalhes dependentes da versão.

## Plano de Revisão para Hoje

Se tiver duas horas, priorize:

1. **20 min:** componente, template, service e Dependency Injection.
2. **25 min:** Observable, `switchMap`, `catchError`, subscrição e `async` pipe.
3. **20 min:** Reactive Forms e validação.
4. **15 min:** Router, lazy loading e guards.
5. **15 min:** HttpClient, interceptor e tratamento de `401`.
6. **15 min:** Change Detection, Signals e performance.
7. **10 min:** responder em voz alta às perguntas no fim do guia.

Para cada resposta: definição curta, problema resolvido, exemplo e trade-off.

## 1. Modelo Mental

### Analogia

Angular é um kit organizado para construir o painel de controlo do edifício. Cada parte visual tem uma responsabilidade; os services fornecem dados; o Router escolhe qual parte mostrar.

### Definição técnica

Angular é um framework frontend baseado em TypeScript. Fornece componentes, templates, injeção de dependências, roteamento, formulários, cliente HTTP e ferramentas de compilação.

### No Smart Building

```text
Browser
  -> Angular Dashboard
      -> HttpClient / SignalR
          -> ASP.NET Core API
```

Angular controla apresentação e interação. Regras de autorização e integridade continuam no backend.

## 2. SPA e CSR

Uma **SPA** carrega a aplicação base uma vez e troca componentes durante a navegação. **CSR** significa que a interface principal é renderizada no browser.

```text
/dashboard
/buildings
/users
/access-points
/alerts
```

O Angular Router escolhe o ecrã. Ao atualizar diretamente `/users`, o servidor de ficheiros precisa devolver `index.html` como fallback.

SPA não significa “sem servidor” nem “sem backend”. O browser continua a consumir APIs.

## 3. Componentes e Templates

### Analogia

Um component é uma peça do painel: contador de ocupação, tabela de eventos ou lista de alertas.

### Tecnicamente

Um component associa:

- classe TypeScript com estado e comportamento;
- template HTML;
- estilos;
- metadata Angular.

Exemplos planejados:

```text
OccupancySummaryComponent
LiveAccessEventsComponent
SecurityAlertsComponent
```

Um component deve coordenar apresentação e interação local. Consultas HTTP reutilizáveis e lógica partilhada pertencem normalmente a services.

### Template bindings

```html
<h2>{{ building.name }}</h2>
<button [disabled]="isSaving()" (click)="save()">Save</button>
```

- `{{ value }}`: interpolação;
- `[property]`: property binding;
- `(event)`: event binding;
- `@if` e `@for`: control flow em Angular moderno;
- `input`/`output`: comunicação entre componentes.

Para um portal de self-service, UX significa mais do que aparência:

- reduzir passos e pedir somente dados necessários;
- preservar campos válidos quando há erro;
- indicar loading, sucesso e falha com mensagens acionáveis;
- permitir concluir tarefas por teclado e leitor de ecrã;
- manter layout e ações utilizáveis em mobile;
- impedir submissões duplicadas sem deixar o utilizador sem feedback.

Comece pela tarefa do utilizador e verifique se consegue concluí-la com poucos erros; não avalie UX apenas por cores ou animações.

## 4. Standalone Components

Componentes standalone podem importar diretamente os recursos de que precisam e não precisam estar registados num NgModule. Esse é o estilo recomendado em Angular moderno e reduz configuração indireta.

Não confunda standalone com “sem módulos” em sentido absoluto: bibliotecas antigas e aplicações existentes ainda podem usar NgModules.

## 5. Services e Dependency Injection

Um service encapsula comportamento reutilizável, como chamadas HTTP ou autenticação.

```ts
@Injectable({ providedIn: 'root' })
export class BuildingService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Building[]> {
    return this.http.get<Building[]>('/api/buildings');
  }
}
```

Dependency Injection fornece a instância do service ao componente. `providedIn: 'root'` normalmente cria uma instância partilhada pela aplicação.

Providers também podem ser declarados numa rota ou componente. O injector mais próximo determina a instância; providers locais podem criar instâncias diferentes.

**Resposta de entrevista:** DI reduz acoplamento, torna dependências explícitas e facilita substituição em testes. Não é a mesma coisa que Service Locator: na DI a dependência é declarada e fornecida externamente.

## 6. Inputs, Outputs e Comunicação

- **Input:** o componente filho recebe dados do pai.
- **Output:** o filho comunica um evento ao pai.
- **Service/store:** partilha dados entre componentes que não têm relação direta pai-filho.
- **Router:** partilha estado navegável através do URL.

Escolha a fronteira menor que resolve o problema. Não crie um store global para estado usado por apenas um componente.

## 7. Change Detection e Performance

Change Detection é o mecanismo que decide quando o Angular deve atualizar a view após alterações de estado.

### Default e OnPush

`Default` verifica a árvore com mais frequência. `OnPush` permite reduzir verificações e funciona bem quando alterações são comunicadas por novas referências, Signals, Observables consumidos pelo `async` pipe ou eventos do componente.

`OnPush` não significa “nunca verificar”; significa que as verificações dependem de sinais de mudança definidos pelo framework.

### Signals

Signal é um recipiente reativo de valor atual. Uma leitura estabelece dependência reativa e mudanças notificam consumidores.

```ts
readonly isLoading = signal(false);
readonly title = computed(() => `Building: ${this.name()}`);
```

- `signal`: estado que pode mudar;
- `computed`: valor derivado;
- `effect`: efeito secundário que reage a alterações.

Não use `effect` para substituir operações deriváveis por `computed`, nem para criar fluxos de efeitos difíceis de rastrear.

### Signals versus RxJS

- Signals: estado síncrono da UI e valores derivados.
- RxJS: streams assíncronos, eventos, concorrência e composição temporal.

Podem coexistir. Escolha pela forma do problema, não por preferência tribal.

### Outras otimizações

- lazy loading por rota;
- `@for (...; track item.id)` para identidade estável;
- evitar cálculos pesados em templates;
- paginação em listas grandes;
- imagens e assets otimizados;
- medir antes de introduzir cache ou estado global.

## 8. Observable e RxJS

### Analogia

Uma Promise é uma encomenda que chega uma vez. Um Observable é uma transmissão que pode emitir vários acontecimentos.

### Definição técnica

Um Observable pode emitir `next`, terminar com `complete` ou falhar com `error`. Normalmente a execução começa com uma subscrição. O `HttpClient` retorna Observables que costumam emitir uma resposta e completar.

### Operators principais

| Operator | O que faz | Exemplo no projeto |
|---|---|---|
| `map` | transforma um valor | projetar resposta para estado visual |
| `filter` | deixa passar algumas emissões | mostrar só eventos negados |
| `tap` | executa efeito secundário | telemetria sem alterar o valor |
| `switchMap` | troca para o stream novo e cancela o anterior | pesquisa enquanto o filtro muda |
| `concatMap` | processa um de cada vez, na ordem | fila de comandos dependentes |
| `mergeMap` | permite operações concorrentes | carregar detalhes independentes |
| `exhaustMap` | ignora novas emissões enquanto uma operação está ativa | evitar submissões repetidas de login |
| `catchError` | trata erro no stream | mostrar estado de falha |
| `debounceTime` | espera uma pausa | pesquisa textual |
| `distinctUntilChanged` | ignora valores iguais consecutivos | evitar request repetido |

Não existe um operator “sempre certo”. `switchMap` pode cancelar a subscrição anterior; escolha-o quando o resultado antigo deixou de interessar, não cegamente para gravações.

### Promise versus Observable

- Promise resolve uma vez e não oferece a mesma composição/cancelamento de subscrição.
- Observable pode emitir vários valores, é componível com operators e pode ser cancelado ao cancelar a subscrição.
- `HttpClient` usa Observable mesmo que cada request normalmente produza uma resposta.

## 9. Subscrições e async pipe

Uma subscrição manual inicia o consumo. Streams longos precisam ser cancelados para não manter listeners ou referências à UI.

O `async` pipe subscreve, entrega valores ao template e limpa a subscrição quando o componente deixa de existir:

```html
@if (buildings$ | async; as buildings) {
  @for (building of buildings; track building.id) {
    <p>{{ building.name }}</p>
  }
}
```

Quando código imperativo é necessário, `takeUntilDestroyed()` ajuda a encerrar streams com o lifecycle do componente.

## 10. Reactive Forms

Reactive Forms mantêm o modelo do formulário em TypeScript. São úteis para formulários administrativos com regras explícitas e testes.

```ts
readonly form = new FormGroup({
  name: new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(200)],
  }),
  email: new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.email],
  }),
});
```

Estados comuns:

- `valid`/`invalid`;
- `touched`/`untouched`;
- `dirty`/`pristine`;
- `pending` para validadores assíncronos.

Validação frontend oferece feedback imediato. A API valida novamente porque o cliente pode ser contornado.

**Resposta de entrevista:** Reactive Forms são adequados para formulários complexos porque o modelo e validadores são explícitos em TypeScript, facilitando composição e teste. Template-driven pode ser mais simples em formulários pequenos.

## 11. Router, Lazy Loading e Guards

Router associa URL a componente e permite deep links. Lazy loading adia o carregamento de features até serem visitadas, reduzindo o bundle inicial.

Um guard pode redirecionar para login quando a sessão visual não está autenticada. Ele protege navegação/UX, não a API: JavaScript executa no dispositivo do utilizador e pode ser manipulado.

A autorização real deve ser verificada no backend em cada endpoint protegido.

## 12. HttpClient, Interceptors e erros

`HttpClient` envia requests e retorna Observables tipados:

```ts
getUsers(): Observable<User[]> {
  return this.http.get<User[]>('/api/users');
}
```

O tipo TypeScript não valida o JSON em runtime. É uma declaração para o compilador; contratos externos ainda podem precisar validação runtime.

Interceptor trata comportamento transversal:

- adicionar `Authorization: Bearer` quando existir token;
- adicionar correlation ID;
- tratar `401` centralmente;
- converter respostas de erro em estado apresentável.

Requests são imutáveis; para mudar headers, clone o request.

Estados de UI devem ser explícitos:

```text
loading -> success com dados
loading -> success vazio
loading -> error recuperável
```

Não esconda uma falha retornando silenciosamente uma lista vazia: isso confunde “sem dados” com “não consegui carregar”.

## 13. Segurança no Frontend

- Não coloque autorização apenas em guards.
- Não confie em validação feita só no browser.
- Evite inserir HTML não confiável; Angular sanitiza templates, mas APIs como `bypassSecurityTrust...` podem reabrir XSS.
- Tokens em `localStorage` ficam acessíveis a JavaScript e a XSS; escolha armazenamento conforme modelo de ameaça. Cookies `HttpOnly`, `Secure` e `SameSite` reduzem acesso por JavaScript, mas exigem estratégia CSRF.
- Não registe tokens, passwords ou dados pessoais nos logs do browser.
- Use HTTPS e políticas CSP adequadas na implantação.

Uma resposta de entrevista deve mostrar trade-off, não afirmar que existe armazenamento de token universalmente perfeito.

## 14. Estado e SignalR

Use estado local para um componente. Use service/Signals para estado partilhado simples. Introduza store global quando o fluxo e a complexidade realmente justificarem.

No Smart Building, REST fornecerá snapshot/paginação. SignalR poderá entregar `AccessEventCreated`, `SecurityAlertCreated` e `OccupancyChanged`. Depois de reconectar, recarregue um snapshot: eventos podem ter sido perdidos durante desconexão.

## 15. Testes Angular

Teste comportamento visível e efeitos observáveis:

- component mostra loading, vazio, erro e dados;
- service envia URL, método e payload corretos;
- formulário rejeita campos inválidos;
- guard redireciona conforme estado de navegação;
- interceptor adiciona header e trata `401`;
- streams cancelam/ignoram requests antigas conforme operator escolhido.

Não teste detalhes privados do component quando uma asserção sobre UI ou request já comprova o comportamento.

## 16. Manutenção e Debugging

Quando recebe um bug numa aplicação existente:

1. reproduza o problema e registe passos, rota, utilizador e dados;
2. observe Console e Network no browser: request, payload, status e response;
3. siga o caminho component → service/interceptor → API;
4. descubra a primeira camada em que o comportamento diverge do esperado;
5. formule uma hipótese pequena e confirme-a com logs ou breakpoint;
6. corrija a causa e adicione um teste de regressão;
7. reteste o fluxo principal e os casos adjacentes antes do PR.

Não esconda uma falha convertendo-a em sucesso vazio. Distinga “não existem registos” de “não consegui consultar os registos”.

### Pergunta de entrevista

**O formulário mostra sucesso, mas o dado não aparece. Como investigaria?**

> Reproduziria o cenário e verificaria a Network tab: se o request foi enviado, qual payload saiu e qual status voltou. Depois seguiria o handler e service Angular, o interceptor e o contrato da API. Se a API devolveu sucesso, verificaria persistência e a consulta usada para recarregar a página. Corrigiria a causa e adicionaria um teste de regressão.

## 17. Build e publicação

`ng build` compila TypeScript/templates e produz assets estáticos. Nginx ou IIS servem esses arquivos. Para rotas Angular diretas, o servidor precisa fallback para `index.html`.

Build frontend em Docker costuma usar Node numa etapa e Nginx numa etapa final. Variáveis Docker não alteram automaticamente JS já compilado; configuração de API em runtime precisa de estratégia explícita.

## Respostas Curtas de Entrevista

### O que é um component?

> É uma unidade de UI composta por lógica TypeScript, template e estilos. Eu manteria o componente focado na apresentação e interação e colocaria comunicação reutilizável com API num service.

### Observable versus Promise?

> Promise entrega um resultado. Observable representa uma sequência, compõe-se com operators e permite cancelar a subscrição. Para pesquisa que muda rapidamente uso `switchMap`; para operações enfileiradas escolho operator conforme a semântica.

### `switchMap` versus `mergeMap`?

> `switchMap` deixa de observar o request anterior e acompanha o mais recente, bom para pesquisa. `mergeMap` permite vários trabalhos em paralelo, bom quando todos os resultados são necessários.

### Para que serve um interceptor?

> Centraliza comportamento transversal de HTTP, como correlation IDs, headers de autenticação e tratamento de erros. Não deve conter regras de negócio.

### Um Guard protege dados da API?

> Não. Um guard controla navegação no cliente. O utilizador controla o browser; por isso, autenticação e autorização precisam ser aplicadas no servidor.

### Como lida com erros e loading?

> Represento estados explicitamente e mantenho erros recuperáveis na UI. Uso `catchError` para mapear falhas, mas não converto falha em lista vazia, porque vazio e erro têm significados diferentes.

### Signals substituem RxJS?

> Não. Signals são convenientes para estado síncrono e valores derivados da UI. RxJS continua adequado para streams assíncronos e operações temporais. Podem coexistir.

### Como torna um portal de self-service user-friendly?

> Começo pela tarefa que o utilizador quer concluir. Reduzo passos, explico erros junto ao campo, preservo dados válidos, dou feedback de loading e sucesso e verifico teclado, acessibilidade e mobile. A validação no browser melhora UX; a API continua responsável por validar e autorizar.

## Exemplo de Resposta Full-Stack

> Para a lista de utilizadores, criaria uma rota lazy-loaded e um componente que usa um service com `HttpClient`. O service expõe um Observable; o template usa `async` pipe e apresenta loading, erro e vazio. O request é tipado, mas o backend continua responsável por validar e autorizar. Para pesquisa reativa usaria `debounceTime`, `distinctUntilChanged` e `switchMap`, porque um resultado de pesquisa antigo deixa de ser útil quando chega um termo novo.

## Vocabulário em Inglês

- component: unidade de UI;
- template: estrutura visual;
- dependency injection: injeção de dependências;
- change detection: deteção de alterações;
- reactive form: formulário reativo;
- route guard: proteção de navegação;
- interceptor: tratamento transversal de HTTP;
- subscription cleanup: limpeza de subscrição;
- loading state: estado de carregamento;
- stale request: pedido cujo resultado já ficou obsoleto.

Frase de prática:

> I use `switchMap` for searches because a newer query makes the previous result obsolete, and I keep authorization on the server rather than relying on a route guard.

## Exercício Prático

Sem escrever Angular no produto ainda, explique como desenharia `/users`:

1. Que componentes e services seriam necessários?
2. Como faria loading, empty e error states?
3. Como cancelaria pesquisas antigas?
4. Onde validaria email?
5. O que faria o guard e o que deve fazer a API?
6. Como o utilizador atualiza a página diretamente em `/users`?

Consegue explicar cada escolha e o respetivo trade-off? Essa é a preparação útil para a Fase 7 e para a entrevista.