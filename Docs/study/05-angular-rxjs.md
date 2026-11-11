# Angular e RxJS

Para uma revisão de véspera de entrevista, consulte também o [guia intensivo de Angular](09-angular-interview-review.md).

## O que é Angular?

### Como explicar para uma criança

Angular é um kit para construir o painel de controlo do edifício. Cada parte do painel tem uma função e todas seguem a mesma organização.

### Definição técnica

Angular é um framework frontend baseado em TypeScript. Oferece componentes, templates, dependency injection, router, forms, cliente HTTP e ferramentas de build.

## SPA

Single-Page Application carrega uma aplicação inicial e troca componentes sem pedir uma página HTML completa a cada navegação.

```text
/dashboard
/users
/alerts
```

O Router escolhe o componente. Um refresh direto exige que o servidor devolva `index.html` como fallback.

SPA não significa “sem backend”. Ela continua a consumir API e não substitui autorização no servidor.

## Component

Component controla uma parte da interface:

- classe TypeScript com estado e comportamento;
- template HTML;
- estilos;
- metadados Angular.

Exemplos futuros:

```text
OccupancySummaryComponent
LiveAccessEventsComponent
SecurityAlertsComponent
```

Componentes devem focar apresentação e interação local. Comunicação HTTP e estado partilhado normalmente ficam em services.

## Template e bindings

- interpolação: `{{ value }}`;
- property binding: `[disabled]="isSaving"`;
- event binding: `(click)="save()"`;
- input: dados recebidos;
- output: evento emitido.

O template deve apresentar estado de loading, erro, vazio e sucesso.

## Service e Dependency Injection

Service encapsula comportamento reutilizável, como chamadas HTTP ou autenticação. Angular DI fornece o service ao componente.

```ts
@Injectable({ providedIn: 'root' })
export class OccupancyService {
  private readonly http = inject(HttpClient);
}
```

`providedIn: 'root'` normalmente produz uma instância partilhada na aplicação.

## Observable

### Analogia

Uma Promise é uma encomenda que chega uma vez. Um Observable é uma estação de rádio que pode enviar vários valores ao longo do tempo.

### Definição

Observable representa uma sequência que pode emitir `next`, terminar com `complete` ou falhar com `error`. A execução geralmente começa na subscrição.

`HttpClient` retorna Observables que normalmente emitem uma resposta e completam. Formulários, router e SignalR podem emitir continuamente.

## RxJS operators

| Operator | Função | Exemplo |
|---|---|---|
| `map` | transforma valor | extrair total de ocupação |
| `filter` | seleciona emissões | manter apenas eventos negados |
| `tap` | efeito secundário | telemetria |
| `switchMap` | troca stream e cancela anterior | pesquisa enquanto digita |
| `catchError` | trata falha | apresentar estado de erro |
| `debounceTime` | aguarda pausa | reduzir requests de pesquisa |
| `distinctUntilChanged` | ignora repetição | não consultar termo igual |
| `combineLatest` | combina valores atuais | filtros de piso e período |

`switchMap` é útil quando o resultado anterior deixa de interessar. Para gravações que não devem ser canceladas, a escolha de `concatMap`, `mergeMap` ou `exhaustMap` depende da semântica.

## Subscribe e async pipe

`subscribe` consome um Observable. Subscrições manuais de longa duração precisam ser canceladas.

O `async` pipe subscreve no template, atualiza a UI e limpa a subscrição automaticamente. Evite espalhar `subscribe` apenas para copiar dados entre propriedades.

## Reactive Forms

Reactive Forms representam o formulário em TypeScript:

```ts
readonly form = new FormGroup({
  cardNumber: new FormControl('', {
    nonNullable: true,
    validators: [Validators.required],
  }),
});
```

Vantagens:

- tipagem;
- validação explícita;
- testes simples;
- composição;
- `valueChanges`;
- validação assíncrona.

Validação Angular melhora experiência, mas o backend precisa validar novamente.

## Router

Router associa URL a ecrãs. Lazy loading reduz o código inicial. URLs devem representar estado navegável e permitir refresh e links diretos.

## Guard

Guard decide navegação no cliente. Pode redirecionar utilizador sem token para login.

Guard não protege API: código do browser pode ser alterado. A API deve aplicar autenticação e autorização.

## Interceptor

Interceptor atua sobre requests e responses:

- adiciona `Authorization: Bearer`;
- trata `401` de forma central;
- adiciona correlation ID;
- normaliza erros.

Ele deve clonar requests porque objetos HTTP Angular são imutáveis.

### Exemplo implementado no projeto

O cliente em `src/smart-building-web` já tem um primeiro fluxo completo de
autenticação:

```text
/login → POST /api/auth/login → sessionStorage
       → /dashboard (guard) → interceptor adiciona Bearer nas chamadas /api
```

`LoginPage` usa Reactive Forms para validar email e password antes de enviar o
pedido. `AuthService` guarda o token e a data `expiresAt`; quando a sessão é
lida depois de expirar, remove-a. `authenticatedGuard` evita a navegação para
o painel sem token local e `authInterceptor` clona as requests para adicionar
`Authorization: Bearer <token>`.

O guard e o interceptor facilitam a navegação e evitam repetição no cliente.
A API continua responsável por validar o JWT e aplicar as policies de papel.

## HttpClient

HttpClient envia requests tipados. O tipo TypeScript não valida automaticamente o JSON em runtime; contratos críticos podem exigir validação adicional.

## Estado

Estado local pertence ao componente. Estado partilhado pode ficar em service, Signals ou solução específica quando a complexidade justificar.

Não adicionar uma biblioteca global de estado apenas porque o projeto usa Angular.

## Exemplo progressivo: SignalR no dashboard

### 1. Analogia simples

REST é como pedir à receção uma lista atual do edifício. SignalR é como a
receção telefonar quando há uma nova mudança. Se a chamada cair, você não
recebe o que foi dito nesse intervalo; por isso, ao voltar a ligar, pede uma
lista atualizada.

### 2. Definição técnica

SignalR permite ao servidor enviar notificações a clientes com uma conexão
ativa. A notificação é uma mudança, não um histórico durável: sem mecanismo de
replay, mensagens durante uma desconexão podem não chegar. Um snapshot REST
reconcilia o estado depois da ligação ser restabelecida.

### 3. Exemplo concreto no Smart Building

`AccessHub` está em `/hubs/access` e exige `AccessOperations` (`Admin` ou
`SecurityOperator`). O browser fornece o JWT por `accessTokenFactory`; o
bearer handler aceita o parâmetro `access_token` de query apenas no caminho
do hub, nunca nas rotas REST. A API publica a resposta REST
`AccessEventResponse` como `AccessEventCreated` depois de um pedido
processado ser persistido. Para resultado concedido, consulta então a
ocupação atual e publica o array completo como `OccupancyChanged`; para
resultado negado, não consulta ocupação.

No cliente, `DashboardService` carrega eventos e ocupação por REST no início,
regista os handlers e liga-se com reconexão automática. Atualiza eventos e
substitui o snapshot de ocupação recebido. Ao conectar (para fechar o intervalo
entre a carga inicial e o handshake) e ao reconectar, carrega novamente o
snapshot REST. Se uma chamada REST falha, conserva o tratamento de erro
existente; falha da conexão SignalR não remove a carga REST. Publicações no
servidor são best effort e registadas, sem alterar a resposta REST. Alertas
não estão implementados, portanto não existe `SecurityAlertCreated`.

### 4. Resposta de entrevista

> “Uso REST para a carga inicial e SignalR para atualizações enquanto o
> dashboard está ligado. O hub `/hubs/access` exige a policy
> `AccessOperations`. Cada pedido registado publica `AccessEventCreated` com
> o mesmo contrato `AccessEventResponse`; só uma decisão concedida consulta
> e envia o snapshot completo de ocupação. O Angular tenta reconectar
> automaticamente e busca um novo snapshot REST após reconectar, porque o
> hub não faz replay durável. A publicação é best effort para não transformar
> uma falha de notificação numa falha da resposta de acesso. Alertas ainda
> não estão implementados.”

### 5. Exercício prático

1. Trace `POST /api/access/requests` desde persistência até
   `AccessEventCreated`; explique por que uma negação não consulta ocupação.
2. Desenhe o caminho de autenticação do browser e indique por que
   `access_token` só pode ser lido no caminho `/hubs/access`.
3. Simule conceptualmente uma desconexão e explique como o snapshot REST após
   reconectar reconcilia eventos e ocupação, e o que não consegue recuperar.
4. Explique como a interface mantém o carregamento inicial e o erro REST se o
   hub falhar.
5. Consulte [Camada API](../api-layer.md#signalr-e-atualizações-do-dashboard)
   e [Autenticação e autorização](../authentication-authorization.md) para
   confirmar contrato, policy e limites. Não afirme replay durável, alerta ou
   validação PostgreSQL que não existe.

## Build e deployment

`ng build` transforma TypeScript, templates e estilos em ficheiros estáticos. Nginx ou IIS podem servi-los.

Configuração de ambiente exige estratégia deliberada porque variáveis do container não alteram automaticamente JavaScript já compilado.

## Pergunta de entrevista

**Observable e Promise são iguais?**

> Promise resolve uma vez e começa imediatamente. Observable pode emitir múltiplos valores, é componível com operadores, normalmente é lazy e pode ser cancelado pela subscrição. O HttpClient usa Observable mesmo quando a request emite uma única resposta.

## Exercício

Abra o login do Smart Building e identifique a responsabilidade de
`LoginPage`, `AuthService`, `authenticatedGuard` e `authInterceptor`. Depois,
desenhe o dashboard com quatro componentes. Indique quais dados vêm por REST,
quais atualizações chegam por SignalR e onde ficam loading, erro e reconexão.
