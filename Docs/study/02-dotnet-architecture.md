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

Application orquestra casos de uso. Ela decide a sequência de ações, por exemplo:

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

Infrastructure implementa detalhes externos. Atualmente contém EF Core e Npgsql. Futuramente implementará contratos de persistência definidos por Application.

Trocar PostgreSQL por outra tecnologia deveria afetar principalmente Infrastructure, não as regras do Domain.

## API

API é a borda HTTP e o composition root. Ela registra dependências e transforma requests em chamadas de casos de uso.

Um endpoint deve ser fino. Não deve concentrar consultas, regras, persistência e serialização numa única função.

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

## Exercício

Explique, sem olhar o código, por que API referencia Application e Infrastructure, por que Application referencia Domain e por que Domain não referencia ninguém.
