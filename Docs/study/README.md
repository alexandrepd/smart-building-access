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

## Mapa da job description

| Requisito da vaga | Onde estudar | Evidência/ligação ao projeto | Estado |
|---|---|---|---|
| Frontend Angular sólido | Módulos 5 e 9 | Arquitetura planejada do dashboard | Angular ainda não implementado; estudar conceitos e demonstrar exercícios separados |
| UX intuitiva para self-service | Módulos 1, 5 e 9 | Fluxos futuros de administração e ocupação | Princípios documentados; UI ainda não implementada |
| Backend .NET ou Java | Módulo 2 | API, Application, Domain e Infrastructure em .NET 10 | .NET aplicado no projeto; Java é alternativa da vaga, não requisito para duplicar a stack |
| RESTful APIs | Módulo 3 | CRUDs iniciais de Buildings, Floors, AccessPoints e Users | Implementado parcialmente; acesso, ocupação e alertas continuam planejados |
| OpenAPI/Swagger specifications | Módulo 3 | Metadados OpenAPI gerados a partir dos endpoints | Documento OpenAPI code-first; Swagger UI não está implementado |
| Manutenção, bugs e novas features | Módulos 2, 7 e 9 | testes, branches focadas, tratamento de erros e exemplos de debugging | Praticar cada alteração com reprodução, causa raiz e teste de regressão |
| Git fundamental | Módulo 6 | branches, Conventional Commits, PRs e reviews | Fluxo usado neste repositório |
| PostgreSQL e EF migrations | Módulo 4 | EF Core, Npgsql, migration e seed | Persistência implementada e validada em PostgreSQL |
| Colaboração e qualidade | Módulos 6 e 7 | PR pequeno, revisão, testes e documentação | Praticar comunicação técnica e resposta a feedback |
| Containers e deployment frontend | Módulo 6 | Angular em Nginx é um caminho planejado | Fase Docker ainda não implementada |
| CI/CD e GitHub Actions | Módulo 6 | pipeline planejado | Fase CI ainda não implementada |
| Kubernetes | Módulo 6 | manifests planejados | Fase futura; conhecimento valorizado, não baseline atual |
| Windows Server | Módulo 6 | IIS, Kestrel, TLS e logs | Estudo conceitual complementar |
| Inglês fluente | Workbook e módulo 9 | respostas técnicas e vocabulário em inglês | Preparar exemplos próprios e praticar em voz alta |

### Como falar das lacunas

Este repositório prova trabalho prático em .NET, APIs, PostgreSQL, EF Core, migrations e testes. Não prova experiência profissional ou experiência Angular em produção: o frontend ainda não foi iniciado. Não transforme conteúdo estudado em experiência que não teve. Explique o que já construiu, demonstre o modelo Angular com um exercício e seja claro sobre o que ainda precisa praticar.

## O que a vaga também avalia

- **Self-service UX:** reduzir passos, preservar dados válidos, explicar erros, mostrar loading/sucesso e suportar teclado, acessibilidade e mobile.
- **Manutenção e bugs:** reproduzir o problema, recolher evidências, identificar a camada, corrigir a causa e criar teste de regressão.
- **Swagger/OpenAPI:** manter o contrato sincronizado com implementação e discutir mudanças incompatíveis.
- **Colaboração:** PRs pequenos, review respeitoso, comunicação de bloqueios e coordenação de contratos entre frontend/backend.
- **Entrega:** explicar build, teste, image/container, CI/CD e promoção a ambientes, distinguindo o que já praticou do que só estudou.
- **Inglês:** comunicar decisões e trade-offs de forma clara; treine as respostas em voz alta, não apenas vocabulário isolado.

## O que não precisamos adicionar ao projeto

A vaga aceita backend com .NET **ou** Java. Como o projeto usa .NET, não é necessário adicionar Java. Numa entrevista, você pode explicar que os princípios de API, arquitetura, banco e containers são transferíveis entre stacks.

Windows Server também será estudado conceitualmente. O deployment principal do projeto continua planejado com Docker e Kubernetes.

## Ordem recomendada

Estado atual do produto: APIs CRUD iniciais de Buildings, Floors, AccessPoints e Users estão implementadas; Cards e Permissions faltam à Fase 4. Access Control e Authentication também estão por implementar. O frontend Angular ainda não começou e continua planeado para a Fase 7; Docker, CI/CD e Kubernetes continuam nas fases posteriores.

Para preparação de entrevista, pode estudar Angular agora sem alterar a ordem de implementação:

1. revisar o [módulo Angular](05-angular-rxjs.md);
2. usar a [revisão intensiva](09-angular-interview-review.md) para perguntas e respostas em voz alta;
3. ligar os conceitos ao futuro dashboard Smart Building;
4. continuar a implementação das fases backend sem antecipar Angular no produto;
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
