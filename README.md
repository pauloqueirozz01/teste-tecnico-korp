# Korp_Teste_PauloQueiroz

Projeto de avaliação técnica para a Korp.

O projeto está em desenvolvimento incremental por specs. A fundação backend, o InventoryService, o BillingService, o processamento da nota fiscal e a resiliência HTTP entre microsserviços já foram implementados. O frontend Angular permanece planejado para próximas specs.

## Objetivo

Construir uma aplicação simples para gerenciamento de produtos, estoque e emissão de notas fiscais, demonstrando boas práticas com Angular, ASP.NET Core Web API, Entity Framework Core, PostgreSQL e testes automatizados.

## Arquitetura

- `frontend/korp-web`: frontend Angular funcional com componentes standalone, roteamento, gestão de produtos e clients HTTP preparados.
- `backend/InventoryService`: API ASP.NET Core funcional para produtos, validação de estoque e consumo de estoque.
- `backend/BillingService`: API ASP.NET Core funcional para criação, consulta, processamento e download de notas fiscais.
- `backend/tests`: projetos de teste xUnit.
- `docs/architecture.md`: documentação pública da arquitetura.
- `docs/presentation-script.md`: estrutura inicial do roteiro da apresentação técnica.

As especificações internas de implementação ficam em `docs/specs/`, mas essa pasta é ignorada pelo Git por ser documentação de apoio ao desenvolvimento e a agentes de IA.

## Tecnologias

- .NET SDK `10.0.301`
- ASP.NET Core Web API
- C#
- Entity Framework Core
- PostgreSQL
- xUnit
- Swashbuckle/Swagger UI
- Microsoft.Extensions.Http.Resilience `10.9.0`
- Docker Compose
- dotnet-ef como ferramenta local em `backend/dotnet-tools.json`
- Angular `20.3.30`
- Angular CLI `20.3.35`
- TypeScript `5.9.3`
- RxJS `7.8.2`
- pnpm `11.15.1`

## Idioma do projeto

O idioma oficial do projeto é Português Brasileiro (pt-BR).

Textos de interface, labels, títulos, mensagens de erro, mensagens de validação, feedbacks de processamento, documentação técnica e roteiro de apresentação devem ser escritos em pt-BR.

Convenções próprias das tecnologias devem ser preservadas quando fizer sentido, como `Program.cs`, `Controllers`, `DTO`, `API`, `HTTP`, `Observable`, `HttpClient`, Entity Framework Core, ASP.NET Core, PostgreSQL e RxJS.

## Microsserviços

O sistema será dividido em dois microsserviços independentes:

- InventoryService: produtos, estoque, validação de estoque e consumo de estoque.
- BillingService: notas fiscais, itens da nota, numeração sequencial e ciclo de vida inicial.

Cada serviço possui seu próprio `DbContext` e seu próprio banco PostgreSQL. Não há referência direta entre InventoryService e BillingService.

## InventoryService

Funcionalidades implementadas:

- cadastro de produtos.
- listagem de produtos.
- consulta de produto por ID.
- validação de estoque.
- consumo atômico de estoque.
- reposição de estoque para compensação.
- idempotência persistida para consumo e reposição via header `Idempotency-Key`.
- tratamento de produtos repetidos na mesma solicitação.
- respostas de erro padronizadas em Português Brasileiro.

Entidade persistida:

- `Produto`
  - `Id`
  - `Codigo`
  - `Descricao`
  - `Saldo`
  - `CriadoEm`
  - `AtualizadoEm`
- `OperacaoEstoqueIdempotente`
  - `Id`
  - `Chave`
  - `Tipo`
  - `RespostaJson`
  - `CriadaEm`

Regras implementadas:

- `Codigo` é obrigatório, normalizado com `Trim()` e `ToUpperInvariant()`, possui tamanho máximo de 50 caracteres e índice único no banco.
- `Descricao` é obrigatória e possui tamanho máximo de 200 caracteres.
- `Saldo` aceita zero e rejeita valores negativos.
- consumo de estoque valida todos os itens antes de alterar saldos.
- produtos repetidos são agrupados antes da validação.
- nenhum consumo pode deixar saldo negativo.
- chamadas repetidas com a mesma chave idempotente retornam resposta compatível sem alterar novamente o saldo.

Endpoints:

```text
POST /api/produtos
GET  /api/produtos
GET  /api/produtos/{id}
POST /api/estoque/validar
POST /api/estoque/consumir
POST /api/estoque/repor
```

Exemplo de cadastro:

```bash
curl -X POST http://localhost:5001/api/produtos \
  -H "Content-Type: application/json" \
  -d '{"codigo":"PROD-001","descricao":"Teclado Mecânico","saldo":10}'
```

Exemplo de validação de estoque:

```bash
curl -X POST http://localhost:5001/api/estoque/validar \
  -H "Content-Type: application/json" \
  -d '{"itens":[{"produtoId":"ID_DO_PRODUTO","quantidade":2}]}'
```

Exemplo de consumo de estoque:

```bash
curl -X POST http://localhost:5001/api/estoque/consumir \
  -H "Content-Type: application/json" \
  -d '{"itens":[{"produtoId":"ID_DO_PRODUTO","quantidade":2}]}'
```

## BillingService

Funcionalidades implementadas:

- criação de notas fiscais.
- numeração sequencial gerada pelo backend.
- status inicial `Aberta`.
- múltiplos itens por nota fiscal.
- agrupamento determinístico de produtos repetidos na mesma nota.
- listagem resumida de notas fiscais.
- consulta detalhada por ID.
- respostas de erro padronizadas em Português Brasileiro.

Entidades persistidas:

- `NotaFiscal`
  - `Id`
  - `Numero`
  - `Status`
  - `CriadaEm`
  - `FechadaEm`
  - `GeradaEm`
  - `NomeArquivo`
  - `Itens`
- `ItemNotaFiscal`
  - `Id`
  - `NotaFiscalId`
  - `ProdutoId`
  - `CodigoProduto`
  - `DescricaoProduto`
  - `Quantidade`

Regras implementadas:

- toda nota fiscal inicia com status `Aberta`.
- `Numero` é gerado pelo backend por sequence PostgreSQL e possui índice único.
- o cliente não informa número, status ou datas internas.
- cada nota deve possuir pelo menos um item.
- cada item deve possuir `ProdutoId`, `CodigoProduto`, `DescricaoProduto` e `Quantidade` maior que zero.
- produtos repetidos na mesma requisição são agrupados por `ProdutoId`.
- itens repetidos do mesmo produto devem possuir o mesmo código e a mesma descrição.
- não existe endpoint público para fechar, editar ou excluir nota fiscal nesta etapa.
- o processamento utiliza um marcador técnico persistido para impedir processamento concorrente da mesma nota.

Endpoints:

```text
POST /api/notas-fiscais
GET  /api/notas-fiscais
GET  /api/notas-fiscais/{id}
POST /api/notas-fiscais/{id}/processar
GET  /api/notas-fiscais/{id}/arquivo
```

Exemplo de criação de nota fiscal:

```bash
curl -X POST http://localhost:5002/api/notas-fiscais \
  -H "Content-Type: application/json" \
  -d '{"itens":[{"produtoId":"ID_DO_PRODUTO","codigoProduto":"PROD-001","descricaoProduto":"Teclado Mecânico","quantidade":2}]}'
```

Observação: `CodigoProduto` e `DescricaoProduto` são recebidos no request como snapshot da nota. A consistência do saldo é responsabilidade do InventoryService durante o processamento.

### Processamento e impressão

O processamento está implementado no BillingService por meio de `POST /api/notas-fiscais/{id}/processar`. A operação:

1. aceita somente notas `Aberta`;
2. gera um arquivo temporário com `System.IO`;
3. solicita o consumo ao InventoryService via HTTP;
4. finaliza o arquivo como `NF-000001.txt`;
5. registra metadados e fecha a nota.

Em caso de falha após o consumo, o BillingService tenta compensar a operação usando `POST /api/estoque/repor`. Consumo e compensação usam chaves idempotentes determinísticas no formato `consumo-nota-<id>` e `compensacao-nota-<id>`, permitindo retry seguro sem duplicar movimentações.

Se a compensação também falhar, a nota permanece `Aberta`, o sistema registra log crítico e retorna `FALHA_COMPENSACAO_ESTOQUE`. Essa situação representa uma janela de inconsistência operacional e não é mascarada como sucesso.

O download é feito por `GET /api/notas-fiscais/{id}/arquivo` e retorna `text/plain`. Notas abertas não possuem arquivo definitivo disponível.

O endereço do InventoryService é configurado por `InventoryService__BaseUrl` ou pela seção `InventoryService:BaseUrl` dos arquivos de configuração. O timeout atual é de 5 segundos.

### Resiliência entre microsserviços

O BillingService usa `IHttpClientFactory` com `Microsoft.Extensions.Http.Resilience` `10.9.0` para a comunicação com o InventoryService.

Configuração padrão:

```json
"InventoryService": {
  "BaseUrl": "http://localhost:5001",
  "TimeoutSeconds": 5,
  "Resilience": {
    "RetryCount": 2,
    "RetryBaseDelayMilliseconds": 200,
    "CircuitBreakerFailureRatio": 0.5,
    "CircuitBreakerMinimumThroughput": 4,
    "CircuitBreakerSamplingSeconds": 10,
    "CircuitBreakerBreakSeconds": 15
  }
}
```

Falhas transitórias recebem retry pequeno com backoff exponencial e jitter: erro de conexão, timeout controlado, HTTP `500`, `502`, `503` e `504`.

Falhas de negócio não recebem retry: `400`, `404`, `409`, produto inexistente, saldo insuficiente e requisição inválida.

Quando o InventoryService está indisponível ou o circuit breaker está aberto, o BillingService retorna resposta controlada:

```json
{
  "codigo": "INVENTORY_SERVICE_INDISPONIVEL",
  "mensagem": "O serviço de estoque está temporariamente indisponível. Tente novamente em alguns instantes.",
  "status": 503
}
```

Nessas falhas, a nota permanece `Aberta` e nenhum arquivo definitivo fica disponível. O BillingService continua respondendo endpoints independentes como `GET /health`, `GET /api/notas-fiscais` e `GET /api/notas-fiscais/{id}`.

## Migrations

A primeira migration de negócio do InventoryService é `AdicionarProdutos`.
A primeira migration de negócio do BillingService é `AdicionarNotasFiscais`.

Para instalar/restaurar ferramentas locais:

```bash
cd backend
dotnet tool restore
```

Para aplicar migrations do InventoryService:

```bash
cd backend
dotnet tool run dotnet-ef database update \
  --project InventoryService/InventoryService.csproj \
  --startup-project InventoryService/InventoryService.csproj
```

Para aplicar migrations do BillingService:

```bash
cd backend
dotnet tool run dotnet-ef database update \
  --project BillingService/BillingService.csproj \
  --startup-project BillingService/BillingService.csproj
```

A migration `AdicionarMetadadosProcessamentoNota` adiciona os metadados do arquivo e o controle otimista do processamento. A migration `AdicionarOperacoesEstoqueIdempotentes` adiciona a persistência das chaves idempotentes do InventoryService.

## Estrutura backend

```text
backend/
├── KorpTeste.sln
├── InventoryService/
│   ├── Controllers/
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/Persistence/
│   └── Middleware/
├── BillingService/
│   ├── Controllers/
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/Persistence/
│   └── Middleware/
└── tests/
    ├── InventoryService.Tests/
    └── BillingService.Tests/
```

## Banco de dados

O Docker Compose da raiz define dois bancos PostgreSQL independentes:

- `postgres-inventory`
  - database: `korp_inventory`
  - porta local: `5433`
- `postgres-billing`
  - database: `korp_billing`
  - porta local: `5434`

As credenciais presentes no `docker-compose.yml` e em `appsettings.Development.json` são fictícias e exclusivas para desenvolvimento local:

- usuário: `korp_dev`
- senha: `korp_dev_password`

Nenhuma senha, token, chave, credencial ou connection string real deve ser versionada.

## Frontend Angular

A aplicação Angular fica em `frontend/korp-web` e usa componentes standalone, `app.config.ts`, `app.routes.ts`, Angular Router, HttpClient e RxJS. O projeto foi criado com Angular CLI `20.3.35`, Angular `20.3.30`, TypeScript `5.9.3`, Node `24.12.0` no ambiente do pnpm e pnpm `11.15.1`.

Estrutura principal:

```text
frontend/korp-web/src/app/
├── core/
│   ├── config/
│   ├── interceptors/
│   ├── models/
│   └── services/
├── components/shared/
└── screens/
```

Rotas iniciais:

```text
/                   Dashboard
/produtos           Produtos
/produtos/novo      Cadastro de produto
/notas-fiscais      Notas fiscais
/notas-fiscais/nova Nova nota fiscal
/notas-fiscais/:id  Detalhes da nota fiscal
/**                 Página não encontrada
```

As screens são carregadas com `loadComponent`, mantendo lazy loading desde a fundação. As páginas ainda são estruturais; os fluxos completos de produtos e notas fiscais ficam para as próximas specs.

A gestão de produtos já está implementada:

- listagem em `/produtos`;
- cadastro em `/produtos/novo`;
- validações de código, descrição e saldo inicial;
- loading, empty state, erro amigável e feedback de sucesso;
- consumo real do InventoryService por `ProdutoService`.

Configuração das APIs:

```ts
inventory: 'http://localhost:5001'
billing: 'http://localhost:5002'
```

Essas URLs ficam em `src/environments/environment.ts` e são expostas aos services por `API_CONFIG`. O frontend usa CORS direto para as APIs, sem proxy Angular nesta etapa, porque os backends já permitem `http://localhost:4200` em desenvolvimento.

Comandos:

```bash
cd frontend/korp-web
pnpm install
pnpm start
pnpm build
pnpm test --watch=false --source-map=false
```

O projeto não possui lint configurado. Prettier foi criado pelo Angular CLI no `package.json`, mas não há script dedicado de formatação.

## Como iniciar os bancos

```bash
docker compose up -d postgres-inventory postgres-billing
```

Para validar a configuração do Compose:

```bash
docker compose config
```

## Como iniciar os serviços

InventoryService:

```bash
dotnet run --project backend/InventoryService/InventoryService.csproj --launch-profile http
```

BillingService:

```bash
dotnet run --project backend/BillingService/BillingService.csproj --launch-profile http
```

## Portas locais

- InventoryService HTTP: `http://localhost:5001`
- InventoryService HTTPS: `https://localhost:7001`
- BillingService HTTP: `http://localhost:5002`
- BillingService HTTPS: `https://localhost:7002`
- Frontend Angular planejado: `http://localhost:4200`

Em ambiente de desenvolvimento, os serviços estão preparados para aceitar CORS a partir de `http://localhost:4200`.

## Health checks

Cada serviço expõe `GET /health` e valida a conectividade com seu banco PostgreSQL correspondente.

InventoryService:

```bash
curl http://localhost:5001/health
```

BillingService:

```bash
curl http://localhost:5002/health
```

Resposta saudável esperada:

```json
{
  "status": "Saudavel",
  "verificacoes": [
    {
      "nome": "postgresql",
      "status": "Saudavel",
      "mensagem": "Banco PostgreSQL acessível."
    }
  ]
}
```

## Swagger UI

Durante o ambiente de desenvolvimento:

- InventoryService: `http://localhost:5001/swagger/index.html`
- BillingService: `http://localhost:5002/swagger/index.html`

## Testes

Restaurar pacotes:

```bash
dotnet restore backend/KorpTeste.sln
```

Compilar:

```bash
dotnet build backend/KorpTeste.sln
```

Executar testes:

```bash
dotnet test backend/KorpTeste.sln
```

## Fluxo de emissão da nota

Implementado:

1. O usuário cria uma nota fiscal.
2. A nota fiscal inicia com status `Aberta`.
3. A nota contém um ou mais produtos e quantidades.
4. O BillingService persiste a nota e permite consulta/listagem.
5. O usuário solicita o processamento.
6. O BillingService chama o InventoryService para consumir o estoque.
7. O arquivo físico é finalizado, a nota é fechada e o download é disponibilizado.

Tratamento implementado:

- nota fechada não pode ser processada novamente;
- saldo insuficiente mantém a nota aberta;
- indisponibilidade do InventoryService retorna HTTP 503;
- falha posterior ao consumo dispara tentativa de compensação.

Planejado para specs futuras:

1. A interface Angular exibe um indicador de processamento.
2. O Angular atualiza a interface após sucesso ou falha.
3. As telas de notas fiscais consomem o BillingService.

## Planejado

- Criação funcional de nota fiscal no Angular.
- Listagem e detalhes de notas fiscais no Angular.
- Processamento e download de nota fiscal pela interface.
