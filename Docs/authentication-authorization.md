# Autenticação e autorização

## Estado

**Implementado nesta branch:** autenticação de utilizadores com tokens JWT
no esquema HTTP Bearer,
policies por papel, bootstrap seguro do primeiro Admin e a migration
`AddUserRole`.

**Ainda incompleto ou planeado:** a interface Angular não está implementada;
portanto, não há login no frontend, armazenamento/renovação de tokens no
cliente ou guards/interceptors. Não há refresh tokens, revogação de tokens,
MFA nem integração com um provedor externo. Não trate estas capacidades como
implementadas. O roadmap continua a definir as fases e não é alterado por este
estado do código.

## Conceitos

- **Autenticação** responde “quem está a fazer o pedido?”. O endpoint de login
  valida credenciais e emite um token.
- **Autorização** responde “o que esta identidade pode fazer?”. A API valida o
  bearer token e aplica policies no servidor.
- **JWT no esquema Bearer** é um token assinado enviado no header HTTP de
  autorização. O token assinado não é encriptado: os seus claims podem ser
  lidos por quem o obtiver. Não inclua secrets ou outros dados sensíveis nos
  claims.

## Login

`POST /api/auth/login` é anónimo e recebe `email` e `password`. Com credenciais
válidas para um User ativo, responde `200 OK` com `accessToken` e `expiresAt`.
O token inclui `sub` (ID do User), `email`, `name`, `role` e `jti`; é assinado
com HMAC-SHA-256 e contém `issuer`, `audience`, `notBefore` e `expires`.

Email/password incorretos, User inexistente ou User inativo produzem a mesma
resposta `401 Unauthorized` com o título genérico `Invalid email or password.`
Isto evita revelar se um endereço está registado ou se a conta está inativa.
Erros de formato/campos obrigatórios são erros de validação `400`, não uma
falha de credenciais. Passwords de Users são armazenadas como hash pelo
serviço de hashing; não são devolvidas pela API.

Exemplo de request (sem credenciais reais):

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.invalid",
  "password": "<password fornecida fora deste exemplo>"
}
```

Depois do login, o cliente envia o token recebido no header HTTP de
autorização, usando o esquema Bearer, nas rotas protegidas.

## Papéis e policies

Os papéis do domínio são `Admin` e `SecurityOperator`.

| Policy | Requisito | Aplicação |
|---|---|---|
| `AdminOnly` | papel `Admin` | `/api/buildings`, `/api/floors`, `/api/access-points`, `/api/cards`, `/api/permissions` e `/api/users` |
| `AccessOperations` | papel `Admin` ou `SecurityOperator` | `POST /api/access/requests`, `GET /api/access/events` e `GET /api/access/events/{id}` |
| Anónimo | não requer autenticação | `GET /` e `POST /api/auth/login` |

Sem token válido, uma rota protegida responde `401`; com identidade válida,
mas papel insuficiente, responde `403`. A autorização é aplicada pela API,
não por uma futura interface Angular.

Novos Users não enviam papel no request de criação e recebem
`SecurityOperator` por omissão. O endpoint `PUT /api/users/{id}/role` permite
atribuir `Admin` ou `SecurityOperator`, mas está sob `AdminOnly`. Não é possível
rebaixar nem excluir o último Admin; ambas as operações retornam `409 Conflict`
nesse caso. Alterações de papel, bootstrap e exclusão usam o mesmo advisory
lock para serializar a proteção do último administrador.

## Bootstrap inicial do Admin

Se não existir nenhum User com papel `Admin`, a inicialização exige:

- `Authentication:BootstrapAdmin:Email`;
- `Authentication:BootstrapAdmin:Password`.

Forneça ambos através de um provider de configuração seguro, como User Secrets
no desenvolvimento local ou variáveis de ambiente no ambiente de execução.
Não os escreva em `appsettings.json`, código, logs, documentação ou ficheiros
versionados. A password de bootstrap deve ter entre 12 e 128 caracteres e o
email deve ser válido. Se o email já pertencer a um User, o bootstrap define
password hash, papel `Admin` e estado ativo; caso contrário cria o primeiro
Admin. Depois de existir um Admin, estes valores deixam de ser necessários
para iniciar a API, e o bootstrap não substitui credenciais existentes.

## Configuração JWT

`Authentication:SigningKey` é obrigatória e deve conter pelo menos **32 bytes
UTF-8**. Configure-a apenas com User Secrets ou uma variável de ambiente,
nunca em ficheiro versionado. A chave assina os tokens e também é usada para
validar a assinatura.

| Chave | Comportamento implementado |
|---|---|
| `Authentication:Issuer` | Obrigatória e não vazia; `appsettings.json` contém `SmartBuilding.Api` |
| `Authentication:Audience` | Obrigatória e não vazia; `appsettings.json` contém `SmartBuilding.Client` |
| `Authentication:SigningKey` | Obrigatória; mínimo de 32 bytes UTF-8; somente provider seguro |
| `Authentication:AccessTokenLifetimeMinutes` | Omissão usa 15 minutos; valores permitidos de 5 a 60 minutos |

A validação do bearer token exige assinatura, issuer, audience e validade
temporal, usa HMAC-SHA-256 e aplica tolerância de relógio de 30 segundos.
Issuer e audience não são secrets e os valores de exemplo já versionados não
substituem a chave de assinatura.

O papel e o estado ativo são verificados ao autenticar e emitir o token; um JWT
já emitido é autónomo. Alterar o papel ou desativar um User não revoga tokens
existentes: eles continuam válidos até expirar, respeitando a duração
configurada. O access token não pode durar mais de 60 minutos; o padrão é 15.
Revogação imediata e refresh tokens não estão implementados.

## Migration e operação

A migration `AddUserRole` adiciona a coluna obrigatória `role` à tabela
`users`, como texto até 50 caracteres, com valor default `SecurityOperator`.
Assim os Users existentes recebem o papel operador ao aplicar a migration.

Em `Development`, a inicialização aplica migrations antes do seed de
desenvolvimento e do bootstrap do Admin. Fora de `Development`, a API **não**
aplica migrations automaticamente. A equipa responsável pela implantação
deve aplicar e verificar `AddUserRole` no banco de destino antes de iniciar
esta versão da API; isso também garante que a coluna está disponível para o
bootstrap inicial. A estratégia e a execução de migrations em produção são
responsabilidade operacional do deployment, não um comportamento automático
da API.

## Limites e cuidados

- Tokens de acesso são credenciais: não os registe em logs ou os exponha em
  URLs.
- Claims e payload JWT são legíveis; assinatura protege integridade, não
  confidencialidade.
- A API não implementa refresh, logout com revogação, MFA ou gestão de sessões.
- As rotas CRUD administrativas requerem `Admin`; as rotas de decisão e
  auditoria de `AccessEvent` aceitam `Admin` e `SecurityOperator`.
- A autenticação do chamador não altera a regra de decisão de acesso nem
  atualiza `OccupancySession`; atualização de ocupação, alertas e SignalR
  continuam por implementar.
