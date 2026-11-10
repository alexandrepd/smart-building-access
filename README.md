# Smart Building Access

Plataforma de controlo de acessos e monitorização de ocupação para um edifício empresarial.

Preparação para entrevista: [README-ENTREVISTA.md](README-ENTREVISTA.md).

## Estado atual

- solução .NET 10;
- projetos API, Application, Domain, Infrastructure e UnitTests;
- modelo inicial de domínio;
- OpenAPI configurado na API;
- frontend Angular iniciado com login JWT, rota de painel protegida e interceptor Bearer;
- desenvolvimento organizado por fases em [Docs/smart-building-project-plan.md](Docs/smart-building-project-plan.md).

## Pré-requisitos

- .NET SDK 10.0.102 ou patch compatível.
- PostgreSQL 18 para executar a API localmente.
- Node.js compatível com a versão Angular e npm para executar a interface.

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

Noutra sessão de terminal, execute a interface Angular:

```bash
npm start --prefix src/smart-building-web
```

O servidor de desenvolvimento encaminha `/api` para `http://localhost:5077`, o perfil HTTP da API. Para autenticar localmente, configure a chave JWT e o Admin inicial através de User Secrets conforme [Autenticação e autorização](Docs/authentication-authorization.md).

## Executar com Docker Compose

O Compose inicia PostgreSQL, API e a interface Angular. A interface fica
disponível em `http://localhost:8080` e encaminha `/api` para a API na rede
interna. A API aplica migrations e cria o Admin inicial durante este arranque
de desenvolvimento.

```bash
cp .env.example .env
# Edite .env e substitua todos os valores de exemplo.
docker compose up --build
```

Para gerar `AUTHENTICATION_SIGNING_KEY`, use `openssl rand -base64 48`. Nunca
adicione o ficheiro `.env` ao Git. Para parar os serviços, execute
`docker compose down`; acrescente `--volumes` apenas quando quiser eliminar os
dados locais do PostgreSQL. O volume segue o layout exigido pela imagem oficial
do PostgreSQL 18, que guarda os dados numa subpasta versionada.
