# Trilha de Estudo para Entrevistas

## Objetivo

Esta trilha explica as tecnologias do Smart Building desde o nível mais simples até ao nível esperado numa entrevista Full-Stack.

O material não altera o [plano do projeto](../smart-building-project-plan.md). O plano continua definindo a ordem de implementação; esta pasta define a ordem de aprendizagem e revisão.

## Método de estudo

Cada conceito deve ser aprendido em cinco níveis:

1. **Analogia:** explicar sem vocabulário técnico.
2. **Definição:** usar os termos corretos.
3. **Projeto:** apontar onde aparece no Smart Building.
4. **Entrevista:** responder com clareza e trade-offs.
5. **Prática:** demonstrar com código, comando ou diagrama.

Se você não consegue explicar um conceito com palavras simples, ainda não o domina. Se consegue apenas repetir a analogia, também não: a entrevista exigirá precisão técnica.

## Módulos

Para ler a preparação completa num único ficheiro, abra o [README de Entrevista](../../README-ENTREVISTA.md).

1. [Web e desenvolvimento Full-Stack](01-web-full-stack.md)
2. [.NET e arquitetura em camadas](02-dotnet-architecture.md)
3. [REST, HTTP, OpenAPI e idempotência](03-rest-openapi.md)
4. [PostgreSQL, EF Core, DbContext e DbSet](04-databases-ef-core.md)
5. [Angular e RxJS](05-angular-rxjs.md)
6. [Git, CI/CD, containers e operação](06-git-delivery-operations.md)
7. [Preparação prática para a entrevista](07-interview-workbook.md)
8. [Glossário rápido](08-glossary.md)
9. [Angular: revisão intensiva para entrevista](09-angular-interview-review.md)
10. [JWT, autenticação e autorização](10-authentication-authorization.md)

## Mapa da job description

| Requisito da vaga | Onde estudar | Evidência/ligação ao projeto | Estado |
|---|---|---|---|
| Frontend Angular sólido | Módulos 5 e 9 | Login, dashboard de `AccessEvent`, guard e CRUD Admin de Buildings/Floors/AccessPoints/Users/AccessCards/AccessPermissions | Fase 7 em desenvolvimento; console, simulação e cobertura mais ampla faltam; ocupação e tempo real pertencem a fases seguintes |
| UX intuitiva para self-service | Módulos 1, 5 e 9 | Dashboard de eventos e formulários administrativos parciais | Melhorias e fluxos restantes ainda requerem implementação e validação |
| Backend .NET ou Java | Módulo 2 | API, Application, Domain e Infrastructure em .NET 10 | .NET aplicado no projeto; Java é alternativa da vaga, não requisito para duplicar a stack |
| RESTful APIs | Módulos 3 e 10 | CRUDs administrativos, Access Control e `POST /api/auth/login` | Autenticação JWT e policies de autorização implementadas; ocupação, alertas e SignalR continuam futuros |
| OpenAPI/Swagger specifications | Módulo 3 | Metadados OpenAPI gerados a partir dos endpoints | Documento OpenAPI code-first; Swagger UI não está implementado |
| Manutenção, bugs e novas features | Módulos 2, 7 e 9 | testes, branches focadas, tratamento de erros e exemplos de debugging | Praticar cada alteração com reprodução, causa raiz e teste de regressão |
| Git fundamental | Módulo 6 | branches, Conventional Commits, PRs e reviews | Fluxo usado neste repositório |
| PostgreSQL e EF migrations | Módulo 4 | EF Core, Npgsql, migration e seed | Persistência implementada e validada em PostgreSQL |
| Colaboração e qualidade | Módulos 6 e 7 | PR pequeno, revisão, testes e documentação | Praticar comunicação técnica e resposta a feedback |
| Containers e deployment frontend | Módulo 6 | Compose local com Angular/Nginx, API e PostgreSQL | Execução local validada; CI/CD e Kubernetes ainda estão planejados |
| CI/CD e GitHub Actions | Módulo 6 | pipeline planejado | Fase CI ainda não implementada |
| Kubernetes | Módulo 6 | manifests planejados | Fase futura; conhecimento valorizado, não baseline atual |
| Windows Server | Módulo 6 | IIS, Kestrel, TLS e logs | Estudo conceitual complementar |
| Inglês fluente | Workbook e módulo 9 | respostas técnicas e vocabulário em inglês | Preparar exemplos próprios e praticar em voz alta |

### Como falar das lacunas

Este repositório prova trabalho prático em .NET, APIs, PostgreSQL, EF Core,
migrations e testes, além de uma implementação Angular parcial: login,
dashboard de eventos e gestão de Buildings, Floors, AccessPoints, Users,
AccessCards e AccessPermissions. Não prova
experiência profissional ou experiência Angular em produção. Não transforme
conteúdo estudado em experiência que não teve; explique o que já construiu e
seja claro sobre as UIs, os testes frontend e os fluxos ainda em falta.

## O que a vaga também avalia

- **Self-service UX:** reduzir passos, preservar dados válidos, explicar erros, mostrar loading/sucesso e suportar teclado, acessibilidade e mobile.
- **Manutenção e bugs:** reproduzir o problema, recolher evidências, identificar a camada, corrigir a causa e criar teste de regressão.
- **Swagger/OpenAPI:** manter o contrato sincronizado com implementação e discutir mudanças incompatíveis.
- **Colaboração:** PRs pequenos, review respeitoso, comunicação de bloqueios e coordenação de contratos entre frontend/backend.
- **Entrega:** explicar build, teste, image/container, CI/CD e promoção a ambientes, distinguindo o que já praticou do que só estudou.
- **Inglês:** comunicar decisões e trade-offs de forma clara; treine as respostas em voz alta, não apenas vocabulário isolado.

## O que não precisamos adicionar ao projeto

A vaga aceita backend com .NET **ou** Java. Como o projeto usa .NET, não é necessário adicionar Java. Numa entrevista, você pode explicar que os princípios de API, arquitetura, banco e containers são transferíveis entre stacks.

Windows Server também será estudado conceitualmente. Docker Compose já foi validado para execução local; CI/CD e deployment Kubernetes continuam planejados.

## Ordem recomendada

Estado atual do produto: APIs CRUD de Buildings, Floors, AccessPoints, Users, AccessCards e AccessPermissions estão implementadas. O processamento/decisão de Access Control, auditoria/consulta de `AccessEvent`s, login JWT e autorização por papéis estão implementados nesta branch. A migration `AddUserRole` existe; fora de Development, aplicá-la é responsabilidade operacional antes de iniciar a versão nova. O frontend Angular tem login, dashboard com os 10 `AccessEvent`s mais recentes e CRUD Admin das seis entidades de gestão. O target de testes está configurado com Vitest e há testes focados nos mapeamentos HTTP do service; build/testes desta branch ainda sem resultado reportado. Console de pedidos, simulação e cobertura mais ampla de componentes/E2E ainda não estão implementados na Fase 7; dashboard de ocupação e SignalR/tempo real pertencem às fases seguintes. A aplicação também tem Docker Compose validado para execução local, incluindo Angular servido por Nginx, API e PostgreSQL. CI/CD e Kubernetes continuam planejados; atualização de `OccupancySession` e processamento de alertas continuam futuros.

Para estudar o fluxo implementado em cinco níveis — analogia simples,
definição, exemplo do projeto, resposta de entrevista e exercício — consulte
[Decisão de acesso e auditoria no módulo REST](03-rest-openapi.md#exemplo-do-projeto-decisão-de-acesso-e-auditoria).

Para estudar autenticação JWT e autorização `Admin`/`SecurityOperator` nos
mesmos cinco níveis, consulte o [módulo de autenticação e autorização](10-authentication-authorization.md).

Para rever em cinco níveis o dashboard Angular, os formulários administrativos,
os limites do guard e as lacunas da Fase 7, consulte o [exemplo progressivo de
dashboard e gestão](09-angular-interview-review.md#exemplo-progressivo-dashboard-e-gestão-administrativa).

Para preparação de entrevista, pode estudar Angular em paralelo com a implementação:

1. revisar o [módulo Angular](05-angular-rxjs.md);
2. usar a [revisão intensiva](09-angular-interview-review.md) para perguntas e respostas em voz alta;
3. relacionar os conceitos com o dashboard e a gestão administrativa Angular já implementados;
4. identificar as lacunas restantes sem descrever funcionalidades futuras como existentes;
5. estudar deployment em profundidade nas fases 13 a 15.

## Regra para respostas fortes

Uma boa resposta de entrevista normalmente contém:

```text
Definição curta
    +
Problema que resolve
    +
Exemplo real
    +
Limite ou trade-off
```

Exemplo:

> Um índice é uma estrutura auxiliar que acelera buscas no banco. No Smart Building indexamos `occurred_at` porque o histórico será consultado por período. O custo é espaço adicional e mais trabalho nas escritas, portanto não se deve indexar todas as colunas sem observar as consultas reais.
