# Smart Building Access

Plataforma de controlo de acessos e monitorização de ocupação para um edifício empresarial.

Preparação para entrevista: [README-ENTREVISTA.md](README-ENTREVISTA.md).

## Estado atual

- solução .NET 10;
- projetos API, Application, Domain, Infrastructure e UnitTests;
- modelo inicial de domínio;
- OpenAPI configurado na API;
- desenvolvimento organizado por fases em [Docs/smart-building-project-plan.md](Docs/smart-building-project-plan.md).

## Pré-requisitos

- .NET SDK 10.0.102 ou patch compatível.
- PostgreSQL 18 para executar a API localmente.

## Configuração local

Restaure a ferramenta EF Core fixada pelo repositório:

```bash
dotnet tool restore
```

Configure a connection string com User Secrets. Substitua `<local-password>` pela password do seu PostgreSQL local:

```bash
dotnet user-secrets set \
	--project src/SmartBuilding.Api \
	"ConnectionStrings:SmartBuilding" \
	"Host=localhost;Port=5432;Database=smart_building;Username=smart_building;Password=<local-password>"
```

Em Development, a API aplica migrations pendentes e executa o seed mínimo automaticamente. Credenciais reais não devem ser adicionadas aos ficheiros `appsettings`.

## Comandos

```bash
dotnet restore SmartBuilding.slnx
dotnet build SmartBuilding.slnx --no-restore
dotnet test SmartBuilding.slnx --no-build
dotnet run --project src/SmartBuilding.Api
```
