# Documentação Técnica

Este diretório reúne o plano do produto e a documentação da arquitetura implementada.

## Como ler

1. [Visão geral da arquitetura](architecture-overview.md)
2. [Camada Domain](domain-layer.md)
3. [Camada Application](application-layer.md)
4. [Camada Infrastructure](infrastructure-layer.md)
5. [Camada API](api-layer.md)
6. [Autenticação e autorização](authentication-authorization.md)
7. [Estratégia de testes](testing-layer.md)
8. [Plano completo do projeto](smart-building-project-plan.md)
9. [Trilha de estudo para entrevistas](study/README.md)

## Estado da documentação

Os documentos distinguem explicitamente:

- **Implementado:** comportamento existente no código atual.
- **Em desenvolvimento:** código presente numa branch de trabalho, ainda sem entrega concluída.
- **Planejado:** intenção arquitetural para fases futuras.

Essa distinção evita tratar o plano como se já fosse comportamento disponível.

## Regra de atualização

Quando uma fase alterar responsabilidades, dependências ou fluxo de dados, o documento da camada correspondente deve ser atualizado no mesmo pull request.

O fluxo de Access Control desta branch está descrito em
[Camada API](api-layer.md#access-control-pedido-e-eventos),
[Camada Application](application-layer.md#papel-na-arquitetura) e no
[exemplo progressivo para entrevistas](study/03-rest-openapi.md#exemplo-do-projeto-decisão-de-acesso-e-auditoria).

A aplicação Angular já tem dashboard com histórico de `AccessEvent`, console
de pedidos de acesso para Admin/SecurityOperator e CRUD Admin de Buildings,
Floors, AccessPoints, Users, AccessCards e AccessPermissions, além de login,
guards e interceptor Bearer. A Fase 7 continua em desenvolvimento: simulação
e cobertura mais ampla de componentes e E2E ainda não estão implementadas.
A consulta de ocupação por piso e as transições de `OccupancySession` estão
implementadas (Fase 8); SignalR/tempo real pertence à Fase 9. Consulte a
[visão geral da arquitetura](architecture-overview.md), a
[camada API](api-layer.md#consulta-de-ocupação), a
[revisão Angular para entrevistas](study/09-angular-interview-review.md) e o
[módulo de PostgreSQL/EF Core](study/04-databases-ef-core.md#exemplo-progressivo-ocupação-por-piso).
O comando de execução local e o proxy para a API estão no
[README principal](../README.md).

A autenticação JWT e a autorização por papéis implementadas nesta branch
estão descritas em [Autenticação e autorização](authentication-authorization.md)
e no [módulo progressivo de estudo](study/10-authentication-authorization.md).
