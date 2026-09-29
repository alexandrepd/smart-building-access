# Web e Desenvolvimento Full-Stack

## O que é uma aplicação web?

### Como explicar para uma criança

Imagine um restaurante:

- o cliente vê o salão e o menu;
- o empregado recebe o pedido;
- a cozinha prepara o resultado;
- o armazém guarda os ingredientes.

Numa aplicação web:

- o **frontend** é o salão e o menu;
- a **API** é o empregado que leva pedidos;
- o **backend** é a cozinha;
- o **banco de dados** é o armazém.

### Definição técnica

Uma aplicação web usa tecnologias da Web para disponibilizar funcionalidades. O browser executa HTML, CSS e JavaScript. O frontend comunica com servidores por HTTP. O backend aplica regras, controla segurança, integra serviços e persiste dados.

## Frontend

Frontend é o código executado na interface do utilizador. Ele não é apenas “a parte bonita”. Também trata:

- componentes e navegação;
- estado visual;
- formulários;
- acessibilidade;
- feedback de loading e erro;
- integração com APIs;
- atualização em tempo real.

No Smart Building, Angular apresentará dashboard, utilizadores, cartões, permissões, eventos, ocupação e alertas.

## Backend

Backend é o código executado no servidor. Ele trata:

- regras de negócio;
- autenticação e autorização;
- persistência;
- transações;
- integrações;
- logging e observabilidade;
- contratos HTTP.

No Smart Building, ASP.NET Core receberá pedidos e Application/Domain decidirão o comportamento.

## O que significa Full-Stack?

Um Full-Stack Developer consegue trabalhar verticalmente num fluxo:

```text
Interface Angular
  -> chamada HTTP
  -> endpoint ASP.NET Core
  -> caso de uso
  -> regra de domínio
  -> EF Core
  -> PostgreSQL
  -> resposta
  -> atualização da interface
```

Não significa saber tudo com a mesma profundidade. Significa compreender as fronteiras, diagnosticar o fluxo completo e colaborar com especialistas.

## Cliente e servidor

O **cliente** inicia uma comunicação. O **servidor** escuta e responde.

No projeto:

- Angular é cliente da API;
- Simulator será cliente da API;
- API é servidora HTTP;
- API também é cliente do PostgreSQL através do Npgsql.

Os papéis dependem da relação observada. Um backend pode ser servidor para o browser e cliente de outro serviço.

## Browser

O browser:

- interpreta HTML;
- aplica CSS;
- executa JavaScript;
- mantém cookies e armazenamento local conforme políticas;
- aplica regras de segurança como CORS;
- envia requests HTTP.

O utilizador controla o browser. Por isso, nenhuma validação ou autorização apenas no frontend é confiável para segurança.

## Servidor web

Um servidor web recebe tráfego HTTP. No desenvolvimento ASP.NET Core, Kestrel hospeda a API. Em produção, ele pode ficar atrás de Nginx, IIS, um Ingress Controller ou load balancer.

O servidor web não substitui o código da aplicação: ele recebe e encaminha requests, gere conexões e participa de TLS, headers, limites e logging.

## Stateless

HTTP é normalmente usado de forma stateless: cada request carrega informação suficiente para ser entendido. Isso facilita escalabilidade porque outra instância da API pode atender o próximo request.

Stateless não significa “o sistema não guarda dados”. O estado pode estar no banco, cache ou token. Significa que a conversa não depende de memória privada escondida numa única instância do servidor.

## Escalabilidade

Escalabilidade é a capacidade de suportar mais carga.

- **Vertical:** aumentar CPU e memória de uma máquina.
- **Horizontal:** executar mais instâncias.

APIs stateless facilitam escala horizontal. Banco, cache, filas e sessões exigem estratégias próprias.

## Robustez

Uma solução robusta lida de forma previsível com falhas:

- valida entrada;
- define timeouts;
- propaga cancelamento;
- evita duplicações;
- usa transações quando necessário;
- registra logs úteis;
- não expõe informações sensíveis;
- responde com erros consistentes.

## Experiência do utilizador

Uma UI user-friendly:

- mostra o estado atual;
- oferece feedback após ações;
- evita perder dados;
- funciona com teclado e tecnologias assistivas;
- adapta-se a diferentes ecrãs;
- usa mensagens que ajudam a corrigir erros.

No dashboard, um evento em tempo real não deve deslocar controles nem impedir o operador de investigar um alerta.

## Pergunta de entrevista

**Qual é a diferença entre frontend e backend?**

Resposta forte:

> O frontend executa a experiência no cliente: componentes, navegação, estado e integração. O backend executa regras, segurança, persistência e integrações no servidor. A fronteira normalmente é uma API. Validações de experiência podem existir no frontend, mas segurança e invariantes precisam ser garantidas no backend.

## Exercício

Desenhe o fluxo de um operador que abre a página de alertas. Identifique browser, Angular, endpoint, caso de uso, EF Core e PostgreSQL. Depois explique onde loading, autorização e paginação são tratados.
