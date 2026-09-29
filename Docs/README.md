# Documentação Técnica

Este diretório reúne o plano do produto e a documentação da arquitetura implementada.

## Como ler

1. [Visão geral da arquitetura](architecture-overview.md)
2. [Camada Domain](domain-layer.md)
3. [Camada Application](application-layer.md)
4. [Camada Infrastructure](infrastructure-layer.md)
5. [Camada API](api-layer.md)
6. [Estratégia de testes](testing-layer.md)
7. [Plano completo do projeto](smart-building-project-plan.md)
8. [Trilha de estudo para entrevistas](study/README.md)

## Estado da documentação

Os documentos distinguem explicitamente:

- **Implementado:** comportamento existente no código atual.
- **Em desenvolvimento:** código presente numa branch de trabalho, ainda sem entrega concluída.
- **Planejado:** intenção arquitetural para fases futuras.

Essa distinção evita tratar o plano como se já fosse comportamento disponível.

## Regra de atualização

Quando uma fase alterar responsabilidades, dependências ou fluxo de dados, o documento da camada correspondente deve ser atualizado no mesmo pull request.
