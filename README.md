# Smart Building Access

Plataforma de controlo de acessos e monitorização de ocupação para um edifício empresarial.

## Estado atual

- solução .NET 10;
- projetos API, Domain, Infrastructure e UnitTests;
- modelo inicial de domínio;
- OpenAPI configurado na API;
- desenvolvimento organizado por fases em [Docs/smart-building-project-plan.md](Docs/smart-building-project-plan.md).

## Pré-requisitos

- .NET SDK 10.0.102 ou patch compatível.

## Comandos

```bash
dotnet restore SmartBuilding.slnx
dotnet build SmartBuilding.slnx --no-restore
dotnet test SmartBuilding.slnx --no-build
```
