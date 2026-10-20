# JWT, autenticação e autorização

## O que é autenticação e autorização?

### 1. Analogia simples

Pense num edifício com receção. A receção confere o crachá para saber quem você
é: isso é autenticação. Depois, a porta verifica se aquele tipo de crachá
permite entrar na sala: isso é autorização. Um crachá válido não dá acesso a
todas as salas.

### 2. Definição técnica

**Autenticação** valida a identidade apresentada pelo cliente. No Smart
Building, `POST /api/auth/login` valida email/password de um User ativo e
emite um JWT assinado. O cliente apresenta-o no header HTTP de autorização
usando o esquema Bearer.

**Autorização** avalia as claims da identidade autenticada contra regras
aplicadas pelo servidor. A API valida assinatura, issuer, audience e validade
do token e então verifica as policies de papel. JWT é assinado, não
encriptado: não é apropriado colocar segredos no payload.

`401 Unauthorized` significa que falta uma identidade autenticada válida.
`403 Forbidden` significa que a identidade foi validada, mas não satisfaz a
policy exigida.

### 3. Exemplo concreto no Smart Building

`POST /api/auth/login` é anónimo. Uma resposta bem-sucedida inclui
`accessToken` e `expiresAt`; as credenciais inválidas, User inexistente e User
inativo partilham o mesmo `401` e a mensagem `Invalid email or password.`
Campos malformados são erros de validação `400`.

| Policy | Papéis aceites | Exemplo |
|---|---|---|
| `AdminOnly` | `Admin` | gestão de Buildings, Floors, AccessPoints, Cards, Permissions e Users |
| `AccessOperations` | `Admin`, `SecurityOperator` | pedido de acesso e consulta de `AccessEvent`s |

`GET /` e o login permitem acesso anónimo. Os novos Users recebem
`SecurityOperator` por omissão. Apenas um `Admin` pode alterar papéis através
de `PUT /api/users/{id}/role`; a API impede a remoção do último Admin.
Também não é possível excluir o último Admin.

O primeiro Admin é criado no arranque apenas quando ainda não existe um Admin.
Nesse caso, `Authentication:BootstrapAdmin:Email` e
`Authentication:BootstrapAdmin:Password` devem estar definidos por provider
seguro. Para contas locais, use User Secrets; em execução, use configuração
segura do ambiente. Não copie credenciais reais para exemplos ou ficheiros
versionados.

Configuração importante:

- `Authentication:SigningKey` é obrigatória e precisa de pelo menos 32 bytes
  UTF-8; somente User Secrets/variável de ambiente.
- `Authentication:Issuer` e `Authentication:Audience` são obrigatórias e
  devem corresponder aos valores usados para emitir e validar tokens.
- `Authentication:AccessTokenLifetimeMinutes` usa 15 por omissão e aceita
  valores de 5 a 60 minutos.
- O bearer valida assinatura HMAC-SHA-256, issuer, audience e expiração; a
  tolerância de relógio configurada é 30 segundos.
- Alterar o papel ou desativar o User não revoga JWTs já emitidos; estes
  continuam válidos até expirar. Revogação imediata não está implementada.

A migration `AddUserRole` adiciona `users.role` e atribui
`SecurityOperator` às linhas existentes. Apenas em `Development` a API aplica
migrations automaticamente. Em outros ambientes, a equipa de deployment deve
aplicar a migration antes de iniciar esta versão.

### 4. Resposta de entrevista

> “Autenticação confirma a identidade; autorização decide se essa identidade
> pode executar uma operação. Neste projeto, o endpoint de login valida as
> credenciais e emite um JWT de curta duração, assinado com HMAC-SHA-256. A API
> valida assinatura, issuer, audience e expiração antes de aplicar policies:
> AdminOnly para administração e AccessOperations para `Admin` ou
> `SecurityOperator`. Um token válido não implica acesso a todos os endpoints.
> O JWT é assinado, não cifrado, por isso evito claims sensíveis e mantenho a
> chave de assinatura em configuração segura. A UI ainda não está implementada
> e a migration de papéis precisa ser aplicada operacionalmente fora de
> Development.”

### 5. Exercício prático

1. Sem iniciar requests nem alterar dados, desenhe o fluxo
   `login → credenciais → JWT → validação do esquema Bearer → policy → endpoint`.
2. Para cada rota na tabela acima, escreva o papel mínimo e preveja `401`,
   `403` ou acesso permitido para: token ausente, token válido de
   `SecurityOperator` e token válido de `Admin`.
3. Explique por que um email inexistente e uma password incorreta devem dar a
   mesma mensagem, e por que campos inválidos são um erro diferente.
4. Identifique quais configurações são secretas. Explique por que issuer e
   audience precisam coincidir, mas não precisam ser tratados como chaves
   secretas.
5. Explique como `AddUserRole` preserva Users existentes e por que um
   ambiente de produção precisa de migração antes de iniciar a versão nova.
6. Liste duas capacidades não implementadas (por exemplo, refresh token ou
   UI de login) sem as apresentar como experiência já construída.

Consulte a [documentação técnica de autenticação e autorização](../authentication-authorization.md)
e a [camada API](../api-layer.md).
