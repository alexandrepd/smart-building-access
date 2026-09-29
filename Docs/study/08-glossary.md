# Glossário Rápido

## Web e API

- **API:** contrato de comunicação entre sistemas.
- **Endpoint:** operação definida por método e rota.
- **HTTP:** protocolo de requests e responses usado na Web.
- **HTTPS:** HTTP protegido por TLS.
- **JSON:** formato textual de troca de dados.
- **REST:** estilo arquitetural orientado a recursos e interface uniforme.
- **DTO:** objeto de transporte entre fronteiras.
- **Middleware:** etapa transversal do pipeline HTTP.
- **CORS:** política do browser para requests entre origens.
- **JWT:** formato de token assinado com claims.
- **Claim:** informação sobre uma identidade.
- **Authentication:** provar quem é.
- **Authorization:** decidir o que pode fazer.
- **OpenAPI:** especificação para descrever APIs HTTP.
- **Swagger UI:** ferramenta que apresenta e permite experimentar uma especificação OpenAPI.
- **Idempotência:** repetição mantém o mesmo efeito final.
- **Paginação:** dividir resultados em páginas limitadas.

## .NET e arquitetura

- **.NET SDK:** ferramentas para criar, compilar, testar e publicar.
- **Runtime:** ambiente que executa a aplicação.
- **NuGet:** gestor e repositório de pacotes .NET.
- **Dependency Injection:** fornecimento externo de dependências.
- **Scoped:** uma instância por escopo, normalmente request.
- **Singleton:** uma instância durante a aplicação.
- **Transient:** nova instância a cada resolução.
- **Domain:** regras e vocabulário do negócio.
- **Application:** orquestração de casos de uso.
- **Infrastructure:** detalhes externos como banco.
- **Composition root:** local onde dependências são conectadas.
- **CancellationToken:** sinal cooperativo de cancelamento.

## Banco e EF Core

- **PostgreSQL:** sistema de banco relacional.
- **Tabela:** conjunto de linhas com colunas definidas.
- **Primary Key:** identificador único da linha.
- **Foreign Key:** referência protegida a outra linha.
- **Constraint:** regra garantida pelo banco.
- **Índice:** estrutura que acelera consultas específicas.
- **Unique:** regra que impede duplicação.
- **ORM:** mapeamento entre objetos e banco relacional.
- **EF Core:** ORM .NET usado pelo projeto.
- **Npgsql:** provider .NET para PostgreSQL.
- **DbContext:** sessão e unit of work do EF Core.
- **DbSet:** entrada para consultar e alterar um tipo de entidade.
- **Change Tracking:** acompanhamento de alterações em entidades.
- **LINQ:** API de consultas integrada ao C#.
- **Migration:** alteração versionada do schema.
- **Seed:** dados iniciais previsíveis.
- **Transação:** grupo atómico de operações.
- **Concorrência:** operações simultâneas sobre estado relacionado.
- **AsNoTracking:** consulta sem tracking para leitura.

## Angular

- **SPA:** aplicação que navega sem recarregar página completa.
- **Component:** unidade de interface e interação.
- **Template:** marcação visual do component.
- **Service:** comportamento reutilizável ou integração.
- **Observable:** sequência de valores ao longo do tempo.
- **RxJS:** biblioteca de programação reativa usada pelo Angular.
- **Operator:** função que transforma um Observable.
- **Reactive Form:** formulário modelado em TypeScript.
- **Router:** navegação baseada em URL.
- **Guard:** decisão de navegação no cliente.
- **Interceptor:** tratamento transversal de requests HTTP.
- **SignalR:** comunicação em tempo real entre servidor e clientes.

## Git e entrega

- **Commit:** snapshot versionado.
- **Branch:** linha de desenvolvimento apontando para commits.
- **Merge:** combinação de históricos.
- **Rebase:** reaplicação de commits sobre nova base.
- **Pull Request:** proposta de integração e revisão.
- **CI:** integração com build e testes automáticos.
- **CD:** entrega ou deployment contínuo.
- **Artifact:** saída preservada de um pipeline.
- **Secret:** dado sensível gerido fora do código.

## Containers e Kubernetes

- **Image:** artefacto imutável para executar containers.
- **Container:** processo isolado criado de uma image.
- **Dockerfile:** receita de construção de image.
- **Volume:** armazenamento persistente.
- **Registry:** repositório de images.
- **Docker Compose:** definição de aplicação multi-container.
- **Pod:** menor unidade executável Kubernetes.
- **Deployment:** controlador de réplicas e atualizações.
- **Service:** endpoint estável para Pods.
- **Ingress:** roteamento HTTP externo.
- **ConfigMap:** configuração não sensível.
- **Kubernetes Secret:** objeto para dados sensíveis, sujeito a políticas de segurança.
- **Readiness probe:** indica se pode receber tráfego.
- **Liveness probe:** indica se precisa reiniciar.

## Operação

- **IIS:** servidor web da Microsoft para Windows Server.
- **Kestrel:** servidor HTTP multiplataforma do ASP.NET Core.
- **Reverse proxy:** servidor que recebe tráfego e encaminha ao backend.
- **TLS:** proteção criptográfica usada pelo HTTPS.
- **Health check:** verificação de saúde da aplicação ou dependências.
- **Observabilidade:** uso de logs, métricas e traces para entender o sistema.