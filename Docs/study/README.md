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

1. [Web e desenvolvimento Full-Stack](01-web-full-stack.md)
2. [.NET e arquitetura em camadas](02-dotnet-architecture.md)
3. [REST, HTTP, OpenAPI e idempotência](03-rest-openapi.md)
4. [PostgreSQL, EF Core, DbContext e DbSet](04-databases-ef-core.md)
5. [Angular e RxJS](05-angular-rxjs.md)
6. [Git, CI/CD, containers e operação](06-git-delivery-operations.md)
7. [Preparação prática para a entrevista](07-interview-workbook.md)
8. [Glossário rápido](08-glossary.md)

## Mapa da job description

| Requisito da vaga | Onde estudar | Onde praticar no projeto |
|---|---|---|
| Frontend Angular | Módulo 5 | Fase 7 |
| Backend .NET | Módulo 2 | Todas as fases backend |
| RESTful APIs | Módulo 3 | Fases 4 e 5 |
| Swagger specifications | Módulo 3 | Fase 4 |
| Manutenção e bugs | Módulos 2 e 7 | Cada branch e pull request |
| Git | Módulo 6 | Desde a primeira fase |
| PostgreSQL | Módulo 4 | Fase 3 |
| EF Migrations | Módulo 4 | Fase 3 |
| Docker e containers | Módulo 6 | Fase 13 |
| GitHub Actions e CI/CD | Módulo 6 | Fase 14 |
| Kubernetes | Módulo 6 | Fase 15 |
| Windows Server | Módulo 6 | Estudo complementar |
| Colaboração | Módulos 6 e 7 | Branches, reviews e PRs |
| Inglês técnico | Módulo 7 | Respostas e vocabulário |

## O que não precisamos adicionar ao projeto

A vaga aceita backend com .NET **ou** Java. Como o projeto usa .NET, não é necessário adicionar Java. Numa entrevista, você pode explicar que os princípios de API, arquitetura, banco e containers são transferíveis entre stacks.

Windows Server também será estudado conceitualmente. O deployment principal do projeto continua planejado com Docker e Kubernetes.

## Ordem recomendada

Enquanto a Fase 3 é implementada:

1. estudar os módulos 1 a 4;
2. explicar o código de persistência sem consultar notas;
3. responder às perguntas do workbook;
4. avançar para Angular quando a Fase 7 começar;
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
