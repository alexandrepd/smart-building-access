# .NET e Arquitetura em Camadas

## O que é .NET?

### Como explicar para uma criança

.NET é uma caixa de ferramentas e uma máquina capaz de executar programas construídos com essas ferramentas.

### Definição técnica

.NET é uma plataforma de desenvolvimento. Inclui runtime, bibliotecas, SDK, compilador e ferramentas. C# é uma das linguagens usadas para escrever aplicações .NET.

- **SDK:** compila, testa, publica e cria projetos.
- **Runtime:** executa a aplicação compilada.
- **BCL:** biblioteca padrão com coleções, I/O, rede, datas e outros recursos.
- **NuGet:** ecossistema de pacotes .NET.

O Smart Building usa .NET 10, fixado por `global.json`.

## Projeto e solução

Um ficheiro `.csproj` define um projeto: target framework, pacotes, referências e opções de compilação. A solução `.slnx` organiza múltiplos projetos.

No Smart Building:

```text
SmartBuilding.slnx
├── SmartBuilding.Domain
├── SmartBuilding.Application
├── SmartBuilding.Infrastructure
├── SmartBuilding.Api
└── SmartBuilding.UnitTests
```

Separar projetos permite ao compilador impor direções de dependência.

## Por que usar camadas?

### Analogia

Num restaurante, o cozinheiro não recebe pagamentos e o caixa não prepara pratos. Cada papel tem uma responsabilidade.

### Definição

Separação de responsabilidades reduz acoplamento. Cada camada muda por um motivo diferente:

- Domain muda quando regras do negócio mudam.
- Application muda quando casos de uso mudam.
- Infrastructure muda quando tecnologia externa muda.
- API muda quando o contrato HTTP muda.

Camadas não são apenas pastas. A direção das dependências precisa refletir a separação.

## Domain

Domain representa o negócio: edifícios, pisos, pontos de acesso, cartões, permissões, eventos, ocupação e alertas.

Ele não conhece EF Core, PostgreSQL ou HTTP. Assim, uma regra como “cartão expirado não permite acesso” pode ser testada sem banco ou servidor.

## Application

Application orquestra casos de uso. Nos slices implementados, `BuildingService`, `FloorService`, `AccessPointService` e `UserService` normalizam entradas, chamam contratos de repositório e devolvem DTOs sem conhecer HTTP ou EF Core. `UserService` normaliza email para lowercase e usa `IPasswordHashService` para não persistir password em texto simples. Updates de Users alteram perfil e estado, não a password. Os serviços preservam resultados explícitos para sucesso e recursos ou pais inexistentes. Quando a entrada viola um requisito, lançam `ApplicationValidationException` com a propriedade inválida, em vez de usar uma `ArgumentException` genérica.

Um fluxo futuro de controlo de acesso terá uma sequência como:

```text
buscar cartão
  -> verificar utilizador
  -> verificar ponto de acesso
  -> avaliar permissão
  -> criar evento
  -> persistir resultado
```

Application não deve saber se os dados vieram de PostgreSQL, memória ou outro serviço.

## Infrastructure

Infrastructure implementa detalhes externos. Atualmente contém EF Core, Npgsql, o registro de `SmartBuildingDbContext`, a migration inicial, o seed de desenvolvimento e repositórios dos CRUDs implementados. `UserRepository` persiste Users e trata duplicidade por índice único; `IdentityPasswordHashService` usa `PasswordHasher<User>` do ASP.NET Core Identity. A verificação de credenciais e o login continuam planejados.

Trocar PostgreSQL por outra tecnologia deveria afetar principalmente Infrastructure, não as regras do Domain.

## API

API é a borda HTTP e o composition root. Ela registra dependências e transforma requests em chamadas de casos de uso.

Um endpoint deve ser fino. Não deve concentrar consultas, regras, persistência e serialização numa única função.

No projeto, a API lê `ConnectionStrings:SmartBuilding` e chama `AddInfrastructure`. Se a configuração estiver ausente, o processo falha cedo. Em `Development`, a API também pede à Infrastructure que aplique migrations e execute o seed antes de servir requests.

Os fluxos de `Buildings`, `Floors`, `AccessPoints` e `Users` mostram a separação completa: Minimal API mapeia HTTP, o serviço coordena o caso de uso e o repositório executa EF Core. Essa divisão permite testar a Application com um fake sem iniciar servidor ou banco. No caso de `AccessPoint`, o endpoint traduz `FloorNotFound` para `404` e `HasDependents` para `409`; em Users, traduz email duplicado e dependências para `409`. A Application não conhece esses códigos HTTP.

Uma analogia útil é um formulário interno de entrega: a Application informa qual campo está incorreto, mas não decide qual envelope HTTP será usado. Tecnicamente, `ApiExceptionHandler` traduz somente `ApplicationValidationException` para `400` com erros por campo; falhas inesperadas permanecem `500`. Assim, a Application comunica significado sem depender de ASP.NET Core.

## Dependency Injection

### Analogia

Em vez de cada trabalhador comprar suas próprias ferramentas, alguém entrega a ferramenta certa quando ele começa o trabalho.

### Definição

Dependency Injection fornece dependências externamente. Isso torna dependências explícitas e substituíveis.

```csharp
public sealed class AccessService(IAccessEventStore eventStore)
{
}
```

O serviço pede `IAccessEventStore`; não cria um banco diretamente.

Tempos de vida comuns:

- **Transient:** nova instância a cada resolução.
- **Scoped:** uma instância por request HTTP.
- **Singleton:** uma instância para toda a aplicação.

`DbContext` normalmente é scoped e não é thread-safe. Um singleton não deve capturar um serviço scoped.

No Smart Building, `AddInfrastructure` usa `AddDbContext`, por isso `SmartBuildingDbContext` recebe o lifetime scoped padrão. A mesma extensão encapsula Npgsql e a convenção `snake_case`, deixando `Program.cs` responsável pela composição, não pelos detalhes do mapeamento.

## Interface

Uma interface define um contrato de comportamento. Ela ajuda quando existe uma fronteira real, múltiplas implementações ou necessidade de substituição em testes.

Não é necessário criar interface para toda classe. Abstração sem propósito aumenta complexidade.

## Middleware

Middleware é uma etapa reutilizável do pipeline HTTP:

```text
Request
  -> tratamento de erros
  -> HTTPS
  -> autenticação
  -> autorização
  -> endpoint
  -> Response
```

A ordem importa. Autorização precisa da identidade produzida pela autenticação.

## Async e await

### Analogia

Enquanto espera a água ferver, o cozinheiro pode preparar outro ingrediente em vez de ficar parado.

### Definição

`async/await` permite aguardar I/O sem bloquear a thread. É útil para banco, ficheiros e chamadas HTTP.

Não significa automaticamente paralelismo nem cria sempre uma nova thread. Para I/O, evita desperdiçar threads durante espera.

Boas práticas:

- sufixar métodos assíncronos com `Async`;
- propagar `CancellationToken`;
- evitar `.Result` e `.Wait()`;
- não usar `Task.Run` para esconder I/O síncrono.

## SOLID em termos simples

- **S:** uma classe deve ter uma responsabilidade coerente.
- **O:** extensões devem evitar alterar comportamento estável desnecessariamente.
- **L:** implementações devem respeitar o contrato do tipo base.
- **I:** interfaces pequenas são melhores que contratos gigantes.
- **D:** regras dependem de abstrações, não de detalhes externos.

SOLID é orientação, não uma meta para maximizar quantidade de classes.

## Pergunta de entrevista

**Por que Domain não referencia Infrastructure?**

> Porque regras de negócio devem permanecer independentes de EF Core, PostgreSQL e HTTP. Infrastructure conhece o Domain para mapeá-lo, mas o Domain não conhece Infrastructure. Isso reduz acoplamento, facilita testes e permite trocar detalhes técnicos sem reescrever regras.

**Por que validar a connection string no arranque?**

> Para falhar cedo com uma mensagem clara quando uma dependência obrigatória não foi configurada. No Smart Building, a API valida `ConnectionStrings:SmartBuilding` antes de construir o host e passa o valor para `AddInfrastructure`. Credenciais locais ficam em User Secrets, não no repositório.

**Como a inversão de dependência aparece nos CRUDs de Buildings e Floors?**

> A Application declara `IBuildingRepository` e `IFloorRepository` porque esses são os contratos exigidos pelos casos de uso. Infrastructure referencia Application e implementa os contratos; os serviços não conhecem EF Core. A API registra as ligações no composition root. Assim, a política depende das abstrações e os detalhes técnicos dependem delas.

**Por que usar uma exceção específica para validação da Application?**

> Porque `ApplicationValidationException` comunica uma falha esperada do caso de uso e identifica a propriedade inválida. A API pode convertê-la em `400` com `ValidationProblemDetails`, sem transformar qualquer `ArgumentException` ou falha inesperada em erro do cliente. Isso preserva a separação entre significado da Application e transporte HTTP.

### Como o CRUD de AccessPoint mantém a separação de camadas?

Analogia: um formulário informa os dados do ponto de acesso; um coordenador valida o pedido e um funcionário de arquivo verifica o piso e grava a ficha.

Definição técnica: a API converte requests em commands e os resultados da Application em HTTP. `AccessPointService` normaliza e valida os dados e chama `IAccessPointRepository`. `AccessPointRepository` implementa o contrato com EF Core e Npgsql. A Application não conhece Problem Details nem constraints PostgreSQL.

Exemplo do projeto: `POST /api/access-points` transforma `FloorNotFound` em `404`; o delete bloqueado por permissões, eventos ou alertas vira `409`. Os flags `SupportsEntry` e `SupportsExit` são dados administrativos do recurso; eles não significam que a decisão de autorização de acesso já foi implementada.

### Como guardar passwords sem expô-las na API?

Analogia: guardar uma impressão da chave, não uma cópia. Tecnicamente, `UserService` envia a password de criação a `IPasswordHashService`, cuja implementação Infrastructure usa `PasswordHasher<User>`; DTOs e `UserResponse` omitem password e hash, e o endpoint de update não recebe password. Testes confirmam que a primitiva Identity verifica o hash com a password correta e rejeita uma incorreta. Isso não é um fluxo de verificação de credenciais da aplicação: login, verificação no caso de uso, JWT e autorização continuam planejados para a fase 6.

No request HTTP, `TrimmedEmailAddressAttribute` valida o formato do email depois de remover whitespace externo. Em seguida, `UserService` remove esses espaços e converte o email para lowercase antes de persistir; assim, a validação do contrato e a normalização da Application são coerentes.

## Exercício

Explique, sem olhar o código, por que API referencia Application e Infrastructure, por que Infrastructure referencia Application, por que Application referencia Domain e por que Domain não referencia ninguém. Depois desenhe o percurso de `POST /api/access-points` desde o request até `SaveChangesAsync` e de volta ao `201 Created`. Acrescente os percursos alternativos de `ApplicationValidationException` até `400`, `FloorNotFound` até `404`, dependentes no delete até `409` e uma exceção inesperada até `500`.
