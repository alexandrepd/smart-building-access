# PostgreSQL, EF Core, DbContext e DbSet

## Banco de dados relacional

### Como explicar para uma criança

Imagine um arquivo com gavetas:

- cada gaveta é uma **tabela**;
- cada ficha é uma **linha**;
- cada espaço da ficha é uma **coluna**;
- o número único da ficha é a **chave primária**;
- uma anotação apontando para outra ficha é uma **chave estrangeira**.

### Definição técnica

Um banco relacional organiza dados em tabelas relacionadas. PostgreSQL é um sistema gerenciador de banco de dados relacional, responsável por armazenar, consultar, proteger e manter consistência dos dados.

## Tabela, linha e coluna

```text
buildings
id | name | address | is_active
```

- tabela representa um conjunto;
- linha representa uma ocorrência;
- coluna representa um atributo com tipo e regras.

## Primary Key e Foreign Key

**Primary Key (PK)** identifica uma linha de forma única. O projeto usa `Guid Id`.

**Foreign Key (FK)** referencia uma linha de outra tabela e protege integridade relacional.

```text
floors.building_id -> buildings.id
```

Sem uma FK, seria possível guardar um piso apontando para um edifício inexistente.

## Constraint

Constraint é uma regra garantida pelo banco:

- `PRIMARY KEY`;
- `FOREIGN KEY`;
- `NOT NULL`;
- `UNIQUE`;
- `CHECK`.

Validação na API melhora a experiência. Constraint no banco protege dados mesmo quando outra aplicação escreve diretamente.

## Índice

### Analogia

É o índice no fim de um livro: ajuda a encontrar um assunto sem ler todas as páginas.

### Definição

Índice é uma estrutura auxiliar que acelera certas consultas. Ele custa espaço e torna escritas mais caras porque também precisa ser atualizado.

No projeto, `occurred_at` será usado para consultar eventos por período. `users.email` tem o índice único `ix_users_email`; número de cartão também tem índice único.

Índice comum não impede duplicação. Índice ou constraint unique impede.

## Transação

### Analogia

Numa troca de cromos, ou os dois entregam, ou nenhum entrega.

### Definição

Transação agrupa operações numa unidade atómica. Propriedades ACID:

- **Atomicity:** tudo ou nada.
- **Consistency:** regras permanecem válidas.
- **Isolation:** operações concorrentes têm separação controlada.
- **Durability:** após commit, dados sobrevivem a falhas esperadas.

Registrar evento e alterar ocupação pode exigir uma transação.

## ORM

ORM é um tradutor entre objetos e tabelas:

```text
classe C# -> tabela
propriedade -> coluna
referência -> foreign key
LINQ -> SQL
```

ORM não elimina a necessidade de saber SQL, índices ou transações. Uma consulta LINQ ruim pode gerar SQL caro.

## EF Core

Entity Framework Core é o ORM usado pelo projeto. Ele:

- constrói o modelo relacional;
- traduz LINQ para SQL;
- materializa linhas como objetos;
- rastreia alterações;
- gera migrations;
- envia comandos ao provider.

## Npgsql

Npgsql é o driver e provider .NET para PostgreSQL. EF Core cria a intenção; Npgsql traduz detalhes para o protocolo e dialeto PostgreSQL.

## DbContext

### Como explicar para uma criança

`DbContext` é o funcionário que abre uma pasta de trabalho com o arquivo, anota tudo que mudou e, quando mandamos guardar, entrega as alterações ao banco.

### Definição técnica

`DbContext` representa uma sessão curta com o banco e implementa comportamentos de Unit of Work e identity map/change tracking.

No projeto, `SmartBuildingDbContext`:

- expõe nove conjuntos de entidades;
- carrega configurações Fluent API;
- constrói o modelo usado pelo provider.

Regras importantes:

- não é thread-safe;
- normalmente é scoped por request;
- deve ter vida curta;
- não deve ser singleton;
- `SaveChangesAsync` confirma alterações rastreadas.

## DbSet

### Como explicar para uma criança

Se `DbContext` é o funcionário do arquivo, `DbSet<Building>` é a porta para a gaveta de edifícios.

### Definição técnica

`DbSet<TEntity>` é a entrada do EF Core para consultar e alterar entidades de um tipo.

```csharp
public DbSet<Building> Buildings => Set<Building>();
```

Ele não é literalmente a tabela. É uma abstração que participa da construção de queries e do tracking.

```csharp
Building? building = await context.Buildings
    .AsNoTracking()
    .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
```

O EF traduz a expressão LINQ para SQL. A query geralmente só é executada quando enumerada ou quando um método terminal é chamado.

## Change Tracking

O contexto registra estados:

- `Added`;
- `Modified`;
- `Deleted`;
- `Unchanged`;
- `Detached`.

Alterar um objeto rastreado não atualiza imediatamente o banco. O SQL é enviado em `SaveChanges` ou `SaveChangesAsync`.

Para leitura, `AsNoTracking()` evita tracking desnecessário e reduz custo.

Nos repositórios de `Building`, `Floor` e `AccessPoint`, listagem e consulta por ID usam `AsNoTracking` porque apenas materializam respostas. As atualizações e remoções carregam entidades rastreadas, alteram o estado e chamam `SaveChangesAsync`.

## Fluent API

Fluent API configura o modelo sem colocar atributos EF no Domain:

```csharp
builder.HasOne(floor => floor.Building)
    .WithMany(building => building.Floors)
    .HasForeignKey(floor => floor.BuildingId)
    .OnDelete(DeleteBehavior.Restrict);
```

Ela define tabelas, colunas, tamanhos, índices, relações e comportamento de exclusão.

## DeleteBehavior

- `Cascade`: dependentes são apagados.
- `Restrict`/`NoAction`: exclusão é bloqueada enquanto houver dependentes.
- `SetNull`: FK opcional vira `null`.

O projeto protege histórico com `Restrict`. Em `AccessEvent`, relações opcionais com cartão e utilizador usam `SetNull` para preservar auditoria.

## Migration

### Analogia

Migration é uma planta numerada de reforma do arquivo: “crie esta gaveta”, “adicione esta coluna”, “crie este índice”.

### Definição

Migration é uma alteração versionada do esquema gerada a partir da diferença entre o modelo atual e o snapshot anterior.

Configurar entidades não cria automaticamente o banco. É necessário criar e aplicar migrations.

Uma migration aplicada e compartilhada não deve ser editada. Crie outra migration para evoluir o esquema.

No projeto, a ferramenta local `dotnet-ef` 10.0.12 torna o comando reproduzível após `dotnet tool restore`. A migration `InitialCreate` cria as nove tabelas do domínio. Em `Development`, `InitializeDevelopmentDatabaseAsync` chama `MigrateAsync` antes do seed.

## Seed

Seed fornece dados iniciais previsíveis. Não é:

- schema;
- backup;
- carga completa de produção;
- lugar para secrets;
- substituto de teste.

Seed deve ser determinístico e evitar duplicações ao ser executado novamente.

O seed atual usa GUIDs fixos e consulta cada identificador antes de inserir `HQ Lisbon`, `Ground Floor` e `Main Entrance`. Reiniciar a API mantém uma linha de cada: idempotência significa que repetir a inicialização não duplica o efeito.

### Concorrência no seed

Imagine uma sala de preparação com uma única chave: antes de conferir e repor os materiais, uma equipa pega a chave; as outras aguardam. Isso evita que duas equipas vejam a sala vazia e reponham o mesmo material ao mesmo tempo.

Tecnicamente, `DevelopmentDataSeeder` abre uma transação e executa `pg_advisory_xact_lock(1937001)` antes das verificações e inserções. Esse advisory lock do PostgreSQL é associado à transação e libertado automaticamente no seu fim. Instâncias concorrentes que usam a mesma chave serializam o bootstrap completo, fechando a corrida entre o `AnyAsync` e o `SaveChangesAsync`.

A chave `1937001` identifica este protocolo de coordenação; o banco não associa significado de negócio a ela. Um advisory lock é cooperativo: protege contra outros processos que também adquiram a mesma chave, não contra qualquer escrita arbitrária nas tabelas.

## Validação real e ambiente descartável

A migration, o seed e os slices de `Buildings`, `Floors` e `AccessPoints` foram validados num PostgreSQL 18 real, executado temporariamente num container Docker na porta `55432`. Foram confirmadas nove tabelas do domínio, a migration em `__EFMigrationsHistory`, a aquisição do advisory lock e contagens `1|1|1` após o seed. O smoke de Floors confirmou `200/201/200/200/404/409/204/204`, incluindo a FK do edifício pai e o bloqueio de delete por dependente. O smoke de AccessPoints confirmou `200/201/200/200/404/409/204`, incluindo piso pai ausente, evento dependente e remoção após limpar o evento; os recursos temporários foram limpos.

No smoke de Users, um email uppercase com espaços externos foi criado com `201` e persistido em lowercase; duplicidade retornou `409`, e consulta/update/delete produziram `200/200/204`. O hash do `PasswordHasher<User>` foi armazenado sem aparecer na response. Cartões bloquearam a remoção com `409`, enquanto `AccessEvent` sobreviveu com `UserId` nulo. É validação manual de API e PostgreSQL, não teste automatizado de integração.

Aqui, Docker foi apenas a forma de fornecer um banco isolado e descartável para a validação. A aplicação ainda não ganhou imagem, Compose ou fluxo operacional de containers; portanto, a fase Docker continua planejada.

## Concorrência

Duas requests podem ler o mesmo estado e tentar alterá-lo. No Smart Building, duas entradas simultâneas poderiam abrir duas sessões.

Estratégias possíveis:

- constraints únicas;
- isolamento transacional;
- concorrência otimista com token;
- locking quando necessário;
- retry apenas para falhas apropriadas.

`DbUpdateConcurrencyException` informa um conflito detectado; não decide sozinho qual resultado é correto para o negócio.

### Corrida entre verificação e delete

Imagine conferir que uma sala está vazia e, antes de fechar a porta, alguém entrar. A primeira conferência não garante que a situação continuará igual.

No delete de `Building`, o repositório primeiro usa `AnyAsync` para devolver rapidamente `HasFloors`. Ainda assim, um `Floor` pode ser criado antes de `SaveChangesAsync`. A FK com `DeleteBehavior.Restrict` é a garantia final; `BuildingRepository` captura especificamente a `ForeignKeyViolation` do PostgreSQL e converte a corrida para o mesmo resultado `HasFloors`.

`FloorRepository` aplica a mesma defesa para `AccessPoints` e `OccupancySessions`: consulta dependentes para o fluxo comum e converte uma violação concorrente de FK em `HasDependents`. Na criação e atualização, uma FK inválida para `BuildingId` torna-se `BuildingNotFound`. Capturar qualquer `DbUpdateException` esconderia falhas não relacionadas.

Por isso, o repositório também compara `ConstraintName`: somente as três FKs conhecidas são traduzidas para resultados de negócio. Depois de uma falha esperada, a entidade é removida do change tracker; caso contrário, um segundo `SaveChangesAsync` no mesmo request tentaria repetir a alteração inválida. Se outro request já tiver eliminado o piso, `DbUpdateConcurrencyException` é traduzida para `NotFound`.

`AccessPointRepository` usa `AsNoTracking` para leitura e tracking para atualização e delete. A constraint específica `fk_access_points_floors_floor_id` converte piso pai inexistente em `FloorNotFound`. O delete verifica `AccessPermissions`, `AccessEvents` e `SecurityAlerts`; se surgir uma dependência entre a consulta e a gravação, as FKs `Restrict` protegem o banco e as violações conhecidas viram `HasDependents`. O repositório destaca a entidade do tracker após conflitos esperados, e delete concorrente de um ponto já removido resulta em `NotFound`.

`UserRepository` também usa `AsNoTracking` nas leituras e trata a violação do índice único `ix_users_email` como email duplicado. Antes de remover um `User`, verifica `AccessCards`, `AccessPermissions` e `OccupancySessions`; as FKs `Restrict` protegem a mesma regra contra concorrência. `AccessEvent.UserId`, ao contrário, é opcional e usa `SetNull`: o user pode ser removido sem apagar o `AccessEvent`, cuja referência fica nula para preservar auditoria.

Passwords não são guardadas em texto simples. `IdentityPasswordHashService` usa `PasswordHasher<User>` do ASP.NET Core Identity, que gera hashes com salt aleatório; portanto, duas codificações da mesma password não são iguais. Os testes usam `VerifyHashedPassword` para aceitar a password correta e rejeitar a incorreta. Isso valida a primitiva de hash, não autenticação/verificação de credenciais num fluxo da aplicação; login continua planejado.

## Idempotência versus transação

- transação: uma execução é atómica;
- idempotência: repetição da operação não duplica o efeito;
- concorrência: execuções simultâneas disputam estado.

São problemas relacionados, mas diferentes.

## Pergunta de entrevista

**Qual a diferença entre DbContext e DbSet?**

> `DbContext` representa a sessão e unidade de trabalho do EF Core: configura o modelo, gerencia conexão e rastreia alterações. `DbSet<TEntity>` é a entrada para consultar e alterar entidades de um tipo dentro desse contexto. O DbSet não é literalmente a tabela, e as alterações só são persistidas quando o contexto executa SaveChanges.

**Qual a diferença entre migration e seed?**

> Migration versiona a estrutura do banco, como tabelas, constraints e índices. Seed insere dados iniciais previsíveis depois que a estrutura existe. No Smart Building, `InitialCreate` cria o esquema e o seeder de desenvolvimento cria três registos por GUID fixo sem duplicá-los em reinícios.

**Como o seed evita uma corrida entre instâncias?**

> O seeder abre uma transação e adquire `pg_advisory_xact_lock(1937001)` antes de consultar ou inserir. O PostgreSQL mantém o lock até commit ou rollback, então outra instância que use a mesma chave aguarda e só verifica os GUIDs depois da primeira terminar. Isso serializa o bootstrap, mas continua sendo um mecanismo cooperativo específico do PostgreSQL.

**Por que verificar dependentes e ainda tratar a violação de FK?**

> A consulta antecipada permite devolver um conflito esperado sem provocar exceção na situação comum, mas existe uma janela concorrente até o delete. A constraint do PostgreSQL continua sendo a fonte final de integridade. Por isso, o repositório trata especificamente a violação de FK e preserva outras falhas como erros reais.

**Como o repositório diferencia um Floor ausente de um Building pai ausente?**

> Na atualização, primeiro consulta o piso pelo ID; se não existir, retorna `FloorNotFound`. Se o piso existir, ou na criação, `SaveChangesAsync` deixa a FK validar `BuildingId`; uma `ForeignKeyViolation` específica é convertida em `BuildingNotFound`. Assim, o caso de uso recebe resultados explícitos sem conhecer Npgsql.

**Como o repositório trata integridade referencial no CRUD de AccessPoints?**

> Na criação e atualização, a violação específica da FK de `FloorId` vira `FloorNotFound`. No delete, o repositório consulta `AccessPermissions`, `AccessEvents` e `SecurityAlerts`; as FKs `Restrict` fecham a janela concorrente. Só as constraints conhecidas são traduzidas, a entidade é destacada do tracker após falhas esperadas e um delete concorrente já concluído resulta em `NotFound`.

**Por que remover um User não apaga um AccessEvent?**

> O evento é histórico de auditoria, enquanto o vínculo com User é opcional. A FK usa `ON DELETE SET NULL`, preservando o evento e limpando apenas `UserId`. Cartões, permissões e sessões de ocupação são dependências que bloqueiam o delete com conflito.

## Exercício

Abra `SmartBuildingDbContext` e explique cada `DbSet`. Depois percorra `AccessPointRepository`: identifique onde tracking é necessário, onde `AsNoTracking` é usado, como a FK do piso produz `FloorNotFound`, como permissões, eventos e alertas bloqueiam o delete e como as FKs fecham a corrida concorrente. Por fim, descreva como transformaria o smoke `200/201/200/200/404/409/204` num teste de integração repetível com PostgreSQL isolado.
