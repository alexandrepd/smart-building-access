# Git, CI/CD, Containers e Operação

## Git

### Como explicar para uma criança

Git é um álbum de fotografias do projeto. Cada commit guarda uma fotografia. Uma branch permite experimentar sem desenhar diretamente na versão principal.

### Conceitos

- **Working tree:** ficheiros atuais.
- **Staging area:** seleção preparada para o próximo commit.
- **Commit:** snapshot com identidade e mensagem.
- **Branch:** ponteiro móvel para uma sequência de commits.
- **Remote:** referência a outro repositório.
- **Pull Request:** proposta de integração e espaço de revisão no GitHub.

PR não é um objeto nativo do Git; é uma funcionalidade da plataforma.

## Branches e commits

O projeto usa:

```text
feat/access-control
fix/occupancy-session-close
chore/update-dependencies
```

Commits seguem Conventional Commits:

```text
feat(domain): add access validation
fix(api): return not found for unknown card
```

Branches e commits pequenos facilitam review, rollback e diagnóstico.

## Merge e rebase

- **Merge:** combina históricos e preserva a bifurcação.
- **Rebase:** reaplica commits sobre nova base e reescreve hashes.

Não faça rebase de histórico compartilhado sem coordenação. A política da equipa determina quando usar cada estratégia.

## CI/CD

### Analogia

CI é um professor automático que corrige cada entrega. CD prepara ou publica o trabalho aprovado.

### Definições

- **Continuous Integration:** build, testes e verificações frequentes.
- **Continuous Delivery:** mantém artefacto pronto para publicação, normalmente com aprovação.
- **Continuous Deployment:** publica automaticamente alterações aprovadas.

## GitHub Actions

Um workflow YAML responde a eventos:

```text
workflow
  -> jobs
      -> steps
```

- runner executa um job;
- steps do mesmo job partilham workspace;
- cache acelera dependências;
- artifact preserva uma saída deliberada;
- secrets guardam valores sensíveis.

Pipeline planejado:

```text
restore -> build -> tests -> Angular build -> Docker build
```

## Docker

### Analogia

Uma image é a receita e embalagem. Um container é uma execução criada a partir dela.

### Conceitos

- **Dockerfile:** receita de build.
- **Image:** artefacto imutável em camadas.
- **Container:** processo isolado baseado numa image.
- **Registry:** repositório de images.
- **Volume:** armazenamento persistente.
- **Network:** comunicação e descoberta entre containers.
- **Bind mount:** pasta do host montada no container.

Container não é uma VM: normalmente partilha o kernel do host.

`EXPOSE` documenta uma porta; não a publica sozinho.

## Multi-stage build

Uma etapa usa SDK para compilar. A etapa final copia apenas o resultado para uma image menor de runtime.

Benefícios:

- image menor;
- menos ferramentas em produção;
- menor superfície de ataque;
- build reproduzível.

## Docker Compose

Compose descreve vários serviços locais:

```text
postgres
api
web
simulator
```

`depends_on` organiza inicialização, mas não garante prontidão. Health checks e retry continuam necessários.

PostgreSQL precisa de volume para persistência. Secrets reais não entram no compose versionado.

### Validação local do Smart Building

#### Explicação simples

Pense no Compose como uma planta que liga as partes da aplicação. O browser
entra pela porta do Nginx; o Nginx serve o Angular e encaminha pedidos `/api`
para a API. A base de dados e a API executam em containers separados.

#### Definição técnica

Docker Compose configura e executa serviços relacionados, redes e portas. Uma
porta publicada liga uma porta do host à porta interna do container; a variável
`WEB_PORT` escolhe a porta do host, enquanto o serviço web escuta internamente
na porta `8080`. O proxy reverso Nginx encaminha os pedidos `/api` pela rede
interna do Compose.

#### Exemplo do projeto

Na validação local, `WEB_PORT=8081` foi aceite e o container Angular iniciou
com o mapeamento `0.0.0.0:8081->8080`. `GET http://localhost:8081/` retornou
`200`, e `GET http://localhost:8081/api/auth/login` foi encaminhado pelo Nginx
e retornou `405`, esperado para um `GET` num endpoint de login que suporta
`POST`. Os containers da API e da base de dados estavam saudáveis. O Dockerfile
da API usa o SDK `10.0.102`, alinhado com `global.json`.

#### Resposta de entrevista

> Validei localmente o ambiente com Docker Compose. O Nginx serve a aplicação
> Angular e encaminha `/api` para a API; a porta externa é configurável por
> `WEB_PORT`. Com a porta `8081`, a página respondeu `200` e o pedido GET ao
> endpoint de login retornou `405`, como esperado porque a operação aceita
> POST. A API e a base de dados estavam saudáveis. Isto comprova o smoke test
> local, não uma pipeline CI/CD nem um deployment Kubernetes.

#### Exercício prático

Explique o percurso de `GET /` e de um pedido `/api` desde o host até ao
container correto. Depois, altere `WEB_PORT` para uma porta disponível,
confirme o mapeamento e explique por que um `GET` ao endpoint de login pode
retornar `405` sem significar que o proxy falhou.

## Kubernetes

### Analogia

Docker cria caixas. Kubernetes coordena muitas caixas, substitui as quebradas e encaminha visitantes.

### Conceitos

- **Pod:** menor unidade executável.
- **Deployment:** controla réplicas e atualizações.
- **ReplicaSet:** mantém quantidade de Pods.
- **Service:** endereço estável para Pods.
- **Ingress:** regras HTTP externas.
- **ConfigMap:** configuração não sensível.
- **Secret:** dados sensíveis; base64 não é criptografia.
- **PersistentVolumeClaim:** pedido de armazenamento.
- **Namespace:** separação lógica.

### Probes

- **startup:** aplicação terminou de iniciar?
- **readiness:** pode receber tráfego?
- **liveness:** está viva ou precisa reiniciar?

Readiness falhando remove o Pod do tráfego; liveness falhando pode reiniciá-lo.

## Deployment Angular

Angular vira ficheiros estáticos. Uma image pode usar Node para build e Nginx para servir o resultado.

O servidor precisa de fallback para `index.html` nas rotas da SPA. Assets versionados podem usar cache longo; `index.html` deve atualizar rapidamente.

## Windows Server e IIS

Conceitos básicos valorizados pela vaga:

- IIS hospeda sites e atua como reverse proxy;
- Site combina conteúdo, bindings e configuração;
- Application Pool isola processos e identidade;
- ASP.NET Core Hosting Bundle integra IIS e Kestrel;
- bindings associam host, porta e TLS;
- permissões NTFS controlam acesso a ficheiros;
- Event Viewer e logs IIS ajudam no diagnóstico;
- certificados precisam de instalação e renovação.

Para Angular, IIS serve estáticos e precisa de fallback de rota. Para ASP.NET Core, IIS encaminha tráfego ao processo Kestrel.

## Segurança operacional

- branch protection e review;
- secrets fora do Git;
- permissões mínimas;
- actions e images versionadas;
- dependency e image scanning;
- containers sem root quando possível;
- logs sem tokens ou passwords;
- TLS e rotação de certificados;
- backups e teste de restore.

## Pergunta de entrevista

**Qual a diferença entre image e container?**

> Image é o artefacto imutável construído em camadas. Container é uma instância executável dessa image, com processo, rede e filesystem gravável efémero. Dados persistentes devem ficar em volumes ou serviços externos.

## Exercício

Explique o caminho de uma mudança desde `feat/...` até produção: commit, PR, CI, image, registry, Deployment, readiness e tráfego pelo Service/Ingress.
