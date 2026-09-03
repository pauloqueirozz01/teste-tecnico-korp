# Arquitetura

## Arquitetura Angular

O frontend foi iniciado na SPEC-006 em `frontend/korp-web` com Angular `20.3.30`, Angular CLI `20.3.35`, TypeScript `5.9.3`, RxJS `7.8.2` e pnpm `11.15.1`. A aplicação usa componentes standalone, sem `AppModule`, com bootstrap por `app.config.ts`.

As telas de rota ficam em `src/app/screens`, enquanto componentes reutilizáveis de apresentação ficam em `src/app/components`.

Estrutura planejada:

- `src/app/core/models`: interfaces tipadas para produtos, notas fiscais, itens de nota fiscal, respostas da API e erros.
- `src/app/core/services`: clientes de API para InventoryService e BillingService.
- `src/app/core/interceptors`: comportamento transversal de HTTP, como mapeamento de erros e indicadores de requisição.
- `src/app/components/shared`: controles reutilizáveis, indicadores de carregamento, estados vazios e mensagens de validação.
- `src/app/components/products`: UI reutilizável de produtos.
- `src/app/components/invoices`: UI reutilizável de notas fiscais.
- `src/app/screens`: telas de rota para dashboard, produtos, criação de produto, notas fiscais, criação de nota e detalhes da nota.

Decisão de engenharia: o frontend evitará bibliotecas de gerenciamento global de estado enquanto serviços Angular e fluxos RxJS simples forem suficientes. Isso mantém a avaliação objetiva e evita complexidade desnecessária.

## Fundação Angular da SPEC-006

Rotas configuradas em `app.routes.ts`:

- `/`
- `/produtos`
- `/produtos/novo`
- `/notas-fiscais`
- `/notas-fiscais/nova`
- `/notas-fiscais/:id`
- `/**`

As screens são carregadas com `loadComponent`, aplicando lazy loading desde a fundação. O `App` contém apenas o layout principal, cabeçalho, navegação e `<router-outlet>`.

Componentes compartilhados iniciais:

- `PageHeaderComponent`: título, descrição e ação principal.
- `LoadingComponent`: indicador acessível de carregamento.
- `ErrorMessageComponent`: apresentação visual de erros.
- `EmptyStateComponent`: estado vazio para listas e placeholders.

Services HTTP:

- `ProdutoService`: `listarProdutos`, `buscarProduto`, `criarProduto`.
- `NotaFiscalService`: `listarNotas`, `buscarNota`, `criarNota`, `processarNota`, `baixarArquivo`.

Os services retornam `Observable<T>` e usam `HttpClient` por injeção. O padrão previsto para loading nas próximas screens é manter estado local simples e usar `finalize` em operações com subscribe manual quando necessário. Subscriptions manuais devem ser evitadas quando `async` pipe ou fluxo declarativo resolverem.

Models TypeScript refletem os DTOs reais serializados em camelCase. GUIDs são `string` e datas ISO são `string`, sem conversão automática para `Date`.

A configuração das APIs fica em `src/environments` e `API_CONFIG`:

- InventoryService: `http://localhost:5001`
- BillingService: `http://localhost:5002`

Foi escolhida comunicação direta com as APIs usando CORS já configurado nos backends para `http://localhost:4200`; não há proxy Angular nesta etapa.

Tratamento de erros:

- `ErroApi` representa o contrato `{ codigo, mensagem, status }`.
- `ApiErrorService` prioriza a mensagem segura retornada pelo backend.
- indisponibilidade recebe fallback em pt-BR.
- o interceptor global foi criado apenas como ponto técnico comum, sem interpretar regras de negócio.

Signals são usados apenas para estado local simples do layout, como menu mobile. RxJS permanece responsável pelos fluxos HTTP. Lifecycle hooks não foram adicionados artificialmente; serão usados nas próximas screens quando houver carregamento inicial real.

Não foram adicionadas bibliotecas visuais externas, NgRx, proxy, SSR, PWA ou autenticação.

## Limites dos microsserviços

InventoryService é responsável por produtos, estoque, validação de estoque e consumo de estoque.

BillingService é responsável por notas fiscais, itens da nota, numeração, ciclo de vida, processamento, impressão e arquivos gerados. A SPEC-004 implementou o processamento HTTP, consumo de estoque, geração de arquivo e download.

Nenhum serviço escreverá diretamente no banco de dados de outro serviço. O BillingService se comunicará com o InventoryService via HTTP.

## Comunicação entre serviços

O BillingService chama o InventoryService durante o processamento da nota fiscal usando `IHttpClientFactory` e `IInventoryClient`. O contrato de consumo recebe somente IDs de produtos e quantidades. A URL e o timeout são configuráveis; o valor local atual é `http://localhost:5001` e 5 segundos.

O InventoryService também expõe `POST /api/estoque/repor`, usado exclusivamente para compensar um consumo quando uma etapa posterior do processamento falha. Ele não representa uma edição arbitrária de estoque.

Decisão de engenharia: o endpoint de consumo de estoque deve executar validação e dedução em uma única operação backend para evitar uma condição de corrida entre chamadas separadas de validação e consumo.

A SPEC-005 adicionou chaves idempotentes persistidas às operações mutáveis usadas pelo fluxo. O BillingService envia `Idempotency-Key` com valores determinísticos por nota:

- `consumo-nota-<NotaFiscalId>` para consumo.
- `compensacao-nota-<NotaFiscalId>` para reposição.

O InventoryService grava a chave, o tipo da operação e a resposta JSON em `operacoes_estoque_idempotentes`. Uma repetição com a mesma chave retorna a resposta registrada sem alterar novamente o saldo.

## Bancos de dados

Cada serviço usará seu próprio banco PostgreSQL:

- banco do InventoryService: produtos e quantidades em estoque.
- banco do BillingService: notas fiscais, itens da nota, status da nota, metadados do arquivo gerado e controle de numeração.

Entity Framework Core é usado pelos dois serviços. Os bancos de dados não são compartilhados.

## Ciclo de vida da nota fiscal

Notas fiscais iniciam como `Aberta`. Uma nota pode conter um ou mais itens. Na SPEC-003 não existe endpoint público para fechar, editar status ou excluir uma nota fiscal.

No processamento implementado na SPEC-004, o BillingService valida a nota, cria arquivo temporário, solicita ao InventoryService o consumo de estoque, finaliza o arquivo, salva os metadados e marca a nota como `Fechada`.

Notas fiscais fechadas não podem ser impressas novamente.

O BillingService não deve marcar uma nota como `Fechada` antes de todas as etapas de processamento serem concluídas com sucesso.

## Fluxo de impressão da nota fiscal

1. O cliente envia `POST /api/notas-fiscais/{id}/processar`.
2. O BillingService valida o status e os itens da nota.
3. O BillingService cria um `.tmp` usando `System.IO`.
4. O BillingService chama o InventoryService via HTTP.
5. O InventoryService valida e consome o estoque atomicamente.
6. O BillingService move o temporário para o nome definitivo.
7. O BillingService salva os metadados e marca a nota como `Fechada`.
8. O arquivo é retornado por `GET /api/notas-fiscais/{id}/arquivo`.

Se uma etapa posterior ao consumo falhar, o BillingService remove os arquivos e solicita `repor` ao InventoryService. Essa compensação não elimina a janela de inconsistência de uma comunicação distribuída.

## Geração de arquivo

O BillingService usa a abstração `IGeradorArquivoNotaFiscal`. A implementação `GeradorArquivoNotaFiscal` gera arquivos `.txt` em `storage/notas-fiscais`, primeiro com extensão temporária e depois com nome baseado no número, como `NF-000001.txt`.

O banco armazenará metadados suficientes para recuperar ou baixar o arquivo posteriormente. O arquivo gerado pode ser texto simples ou outro formato físico simples escolhido durante a implementação. Essa é uma decisão de engenharia para atender à exigência de impressão sem adicionar bibliotecas de PDF, a menos que isso se torne explicitamente necessário.

## Tratamento de erros

As APIs retornarão erros HTTP significativos:

- `400 Bad Request` para entrada inválida.
- `404 Not Found` para produtos ou notas fiscais inexistentes.
- `409 Conflict` para conflitos de regra de negócio, como estoque insuficiente ou tentativa de imprimir uma nota fechada.
- `500 Internal Server Error` para falhas locais ou inconsistência operacional, como falha definitiva de compensação.
- `503 Service Unavailable` quando o InventoryService não puder ser consultado ou quando o circuit breaker estiver aberto.

O Angular exibirá falhas próximas ao fluxo relacionado e removerá indicadores de processamento após sucesso ou falha.

Todas as mensagens apresentadas ao usuário deverão estar em Português Brasileiro. Exemplos esperados: "Estoque insuficiente.", "Não foi possível consultar o serviço de estoque." e "Tente novamente em alguns instantes."

## Resiliência

A comunicação do BillingService com o InventoryService usa `Microsoft.Extensions.Http.Resilience` `10.9.0`, escolhido por ser a integração oficial de resiliência para `HttpClient` no runtime atual do projeto (`net10.0`). Não há uso simultâneo de outra biblioteca de resiliência.

Políticas configuradas:

- timeout por tentativa: `InventoryService:TimeoutSeconds`, padrão 5 segundos.
- retry: 2 tentativas adicionais, delay base de 200 ms, backoff exponencial e jitter.
- circuit breaker: razão de falha 0,5, mínimo de 4 eventos, janela de 10 segundos e abertura por 15 segundos.

A ordem configurada na pipeline é retry, circuit breaker e timeout, seguindo a composição da biblioteca. O timeout continua finito e centralizado em configuração; `HttpClient.Timeout` fica desabilitado para evitar timeout duplicado fora da policy.

Falhas transitórias consideradas:

- falha de conexão;
- timeout controlado;
- HTTP `500`, `502`, `503` e `504`.

Falhas de negócio excluídas das policies:

- `400 Bad Request`;
- `404 Produto não encontrado`;
- `409 Saldo insuficiente`;
- conflitos de regra de negócio do BillingService, como nota já fechada.

Retry automático só é aplicado porque consumo e reposição possuem idempotência persistida. Sem essa chave, retry em `POST /api/estoque/consumir` ou `POST /api/estoque/repor` poderia duplicar saída ou entrada de estoque em caso de resposta perdida.

Quando o circuit breaker está aberto, o BillingService falha rapidamente sem chamar o InventoryService e traduz a falha para `INVENTORY_SERVICE_INDISPONIVEL` com HTTP `503`, sem expor nomes internos como `BrokenCircuitException`.

O `GET /health` do BillingService continua representando a aplicação e seu banco próprio. A indisponibilidade temporária do InventoryService não torna o BillingService unhealthy; operações independentes continuam disponíveis.

A demonstração manual de falha consiste em parar o InventoryService ou apontar o BillingService para uma URL inválida e tentar processar uma nota fiscal. A nota deve permanecer `Aberta`, nenhum arquivo definitivo deve ficar disponível e endpoints de consulta do BillingService devem continuar respondendo.

### Compensação e consistência

Se o estoque foi consumido e uma etapa posterior falha, o BillingService tenta compensar com `POST /api/estoque/repor`. Essa chamada também usa idempotência e entra nas mesmas policies de resiliência.

Se a compensação falhar definitivamente, o BillingService:

- mantém a nota `Aberta`;
- remove arquivos temporários e definitivos quando possível;
- registra log crítico com o `NotaFiscalId`;
- retorna `FALHA_COMPENSACAO_ESTOQUE`.

Essa abordagem reduz a janela de inconsistência, mas não implementa transação distribuída, saga framework ou mensageria. Uma falha definitiva de compensação é um evento operacional explícito.

### Logging

Os serviços usam `ILogger` com placeholders estruturados. O BillingService registra início de processamento, solicitação de consumo, retries relevantes, circuito aberto/timeout, tentativa de compensação, falha de compensação e preservação da nota aberta. Payloads completos, connection strings e segredos não são registrados.

## Uso de LINQ

LINQ será usado naturalmente para validação de coleções, projeção, agregação, filtragem e mapeamento. Exemplos esperados:

- validar que os itens da nota fiscal possuem quantidades positivas.
- agrupar IDs de produto repetidos, se necessário.
- calcular totais da nota.
- mapear entidades para DTOs de resposta.
- filtrar produtos em consultas.

LINQ não será adicionado onde código direto for mais claro.

## Uso de RxJS

RxJS será usado nos serviços e telas Angular para fluxos de requisições HTTP, estado de carregamento, estado de erro, leitura de parâmetros de rota e fluxos simples de atualização após mutações.

O projeto evitará indireção excessiva com observables para estados estáticos ou puramente locais.

## Lifecycle hooks do Angular

Telas de rota podem usar `OnInit` para carregar dados iniciais e ler parâmetros de rota. Componentes devem usar inputs e outputs quando possível. Lifecycle hooks serão documentados quando forem usados para carregamento, limpeza ou comportamento dependente da view.

## Estratégia de testes

Testes backend usarão xUnit e cobrirão regras de negócio relacionadas a produtos, estoque, notas fiscais, impressão, geração de arquivos, falha do InventoryService e preservação do estado da nota. Os testes devem ser executados ao longo da implementação.

Testes Angular cobrirão as interações de UI mais importantes: criação de produtos, criação de notas fiscais, impressão de notas, indicadores de carregamento, estados de sucesso e feedback de erro.

## Fundação backend

A SPEC-001 definiu a fundação backend com .NET SDK `10.0.301` e target framework `net10.0`, usando a versão estável disponível no ambiente sem instalar SDK adicional.

A solution backend é `backend/KorpTeste.sln` e contém quatro projetos:

- `InventoryService`: ASP.NET Core Web API do serviço de estoque.
- `BillingService`: ASP.NET Core Web API do serviço de faturamento.
- `InventoryService.Tests`: testes xUnit do InventoryService.
- `BillingService.Tests`: testes xUnit do BillingService.

Os projetos de teste referenciam apenas seus respectivos serviços. Não existe referência direta entre InventoryService e BillingService.

Cada serviço possui:

- controllers configurados.
- dependency injection padrão do ASP.NET Core.
- Swagger UI em ambiente de desenvolvimento.
- health check em `GET /health`.
- tratamento global inicial de exceções.
- configuração de CORS para desenvolvimento local.
- configuração por ambiente via `appsettings.json` e `appsettings.Development.json`.

## Decisões da SPEC-001

- PostgreSQL será acessado via `Npgsql.EntityFrameworkCore.PostgreSQL`.
- Cada serviço possui seu próprio `DbContext`: `InventoryDbContext` e `BillingDbContext`.
- Os DbContexts ainda são mínimos e não possuem entidades de negócio.
- O Docker Compose define dois bancos independentes: `korp_inventory` e `korp_billing`.
- Portas locais definidas:
  - InventoryService: `http://localhost:5001` e `https://localhost:7001`.
  - BillingService: `http://localhost:5002` e `https://localhost:7002`.
  - PostgreSQL InventoryService: `localhost:5433`.
  - PostgreSQL BillingService: `localhost:5434`.
- Swashbuckle foi usado para Swagger UI porque atende à exigência de OpenAPI/Swagger com uma interface de desenvolvimento simples.
- A configuração de CORS aceita `http://localhost:4200` em desenvolvimento, preparando o consumo futuro pelo Angular.
- O redirecionamento HTTPS não é aplicado em `Development` para permitir health checks HTTP locais diretos.

## InventoryService

A SPEC-002 implementou o domínio de estoque no InventoryService. O serviço é proprietário exclusivo de produtos, códigos de produto, descrição, saldo, validação de disponibilidade e movimentação de saída de estoque.

Entidade persistida:

- `Produto`
  - `Id`
  - `Codigo`
  - `Descricao`
  - `Saldo`
  - `CriadoEm`
  - `AtualizadoEm`

Configuração de persistência:

- tabela `produtos`.
- chave primária em `id`.
- `codigo` obrigatório, tamanho máximo 50 e índice único.
- `descricao` obrigatória, tamanho máximo 200.
- `saldo` obrigatório.
- check constraint `CK_produtos_saldo_nao_negativo` para impedir saldo negativo no banco.

Endpoints implementados:

- `POST /api/produtos`
- `GET /api/produtos`
- `GET /api/produtos/{id}`
- `POST /api/estoque/validar`
- `POST /api/estoque/consumir`

O código do produto é normalizado com `Trim()` e `ToUpperInvariant()` no cadastro. Essa decisão evita duplicidade causada por variações simples de caixa ou espaços.

## Regras de estoque

A validação de estoque verifica existência do produto, quantidade maior que zero e saldo disponível. Ela não altera o estoque.

O consumo de estoque valida todos os itens antes de modificar qualquer saldo. Para providers relacionais, a operação usa transação com isolation level `Serializable`. Além disso, a constraint de banco impede saldo negativo como última linha de defesa.

Produtos repetidos em uma mesma solicitação são agrupados antes da validação. Exemplo: duas linhas para o mesmo produto com quantidades 3 e 4 são tratadas como uma solicitação total de 7 unidades.

## Concorrência no estoque

A SPEC-002 implementou uma proteção simples e compatível com múltiplas instâncias: consumo de estoque em transação relacional com isolamento `Serializable`, sem lock global em memória. Isso evita que a consistência dependa de uma única instância do processo.

Retries automáticos para conflitos de serialização não foram adicionados nesta spec, pois retry e circuit breaker pertencem à spec de resiliência.

## Uso de LINQ no InventoryService

Usos relevantes implementados:

- `OrderBy` e `Select` para listagem determinística e projeção para DTOs.
- `AnyAsync` para validar código duplicado.
- `Where` e `Contains` para buscar produtos por coleção de IDs.
- `GroupBy` e `Sum` para agrupar produtos repetidos na solicitação de estoque.
- `ToDictionaryAsync` para validação eficiente por ID.
- `Any` para validar quantidades inválidas, produtos ausentes e saldo insuficiente.

## Tratamento global de exceções

Cada serviço possui middleware inicial de tratamento de exceções. No InventoryService, as exceções de domínio de estoque foram integradas ao middleware. A estrutura de erro HTTP é em Português Brasileiro:

```json
{
  "codigo": "ERRO_INTERNO",
  "mensagem": "Ocorreu um erro interno ao processar a requisição.",
  "status": 500
}
```

Exceções específicas de produto e estoque foram criadas no InventoryService. Na SPEC-003, o BillingService passou a mapear exceções específicas de nota fiscal para o mesmo formato de erro HTTP.

No InventoryService, os códigos específicos implementados são:

- `PRODUTO_NAO_ENCONTRADO`
- `CODIGO_PRODUTO_DUPLICADO`
- `SALDO_INSUFICIENTE`
- `QUANTIDADE_INVALIDA`
- `REQUISICAO_INVALIDA`
- `ERRO_INTERNO`

No BillingService, os códigos específicos implementados são:

- `NOTA_FISCAL_NAO_ENCONTRADA`
- `NOTA_FISCAL_SEM_ITENS`
- `QUANTIDADE_ITEM_INVALIDA`
- `PRODUTO_ITEM_INVALIDO`
- `REQUISICAO_INVALIDA`
- `ERRO_INTERNO`
- `NOTA_FISCAL_JA_FECHADA`
- `NOTA_FISCAL_EM_PROCESSAMENTO`
- `NOTA_FISCAL_NAO_PROCESSADA`
- `INVENTORY_SERVICE_INDISPONIVEL`
- `SALDO_INSUFICIENTE`
- `PRODUTO_NAO_ENCONTRADO`
- `FALHA_GERACAO_ARQUIVO`
- `ARQUIVO_NOTA_FISCAL_NAO_ENCONTRADO`

## BillingService

A SPEC-003 implementou o domínio inicial de faturamento sem integração HTTP com o InventoryService. Essa independência é intencional: o BillingService consegue criar e consultar notas fiscais mesmo que o InventoryService não esteja executando.

Entidades persistidas:

- `NotaFiscal`
  - `Id`
  - `Numero`
  - `Status`
  - `CriadaEm`
  - `FechadaEm`
  - `Itens`
- `ItemNotaFiscal`
  - `Id`
  - `NotaFiscalId`
  - `ProdutoId`
  - `CodigoProduto`
  - `DescricaoProduto`
  - `Quantidade`

O relacionamento entre `NotaFiscal` e `ItemNotaFiscal` é 1:N. A tabela `itens_nota_fiscal` possui índice único para `nota_fiscal_id` e `produto_id`, reforçando que produtos repetidos são tratados antes da persistência.

## Numeração de notas fiscais

O número da nota fiscal é gerado pelo backend usando a sequence PostgreSQL `nota_fiscal_numero_seq`. A aplicação reserva o próximo número com `nextval` antes de persistir a nota fiscal. A tabela `notas_fiscais` também possui índice único em `numero` e check constraint para `numero > 0`.

Essa decisão evita `Max(Numero) + 1` como estratégia de produção e usa uma garantia nativa do banco contra duplicidade. Lacunas podem ocorrer se uma transação falhar após reservar um número, o que é aceitável neste teste técnico porque os números seguem monotonicamente crescentes e não são duplicados.

## Snapshot do produto na nota

Na SPEC-003, `ItemNotaFiscal` armazena `ProdutoId`, `CodigoProduto` e `DescricaoProduto`. Essa é uma decisão arquitetural transitória para permitir a criação isolada do domínio de faturamento antes da integração HTTP com o InventoryService.

Na spec de integração, a origem desses dados deve ser revisada para que o BillingService não confie cegamente em código e descrição enviados pelo Angular.

## Uso de LINQ no BillingService

Usos relevantes implementados:

- `GroupBy` e `Sum` para agrupar produtos repetidos na criação da nota.
- `Select` para normalizar entradas e projetar entidades para DTOs.
- `OrderBy` para ordenar itens por código.
- `OrderByDescending` para listar notas fiscais por número decrescente.
- `Count` para calcular a quantidade de itens no resumo.
- `Any`, `Distinct` e `Where` para validar snapshots inconsistentes de produtos repetidos.

## Decisões da SPEC-003

- Não foi criado endpoint de consulta por número para manter a API mínima; a consulta por ID atende ao escopo atual.
- Não foram criados endpoints de atualização, fechamento ou exclusão de nota fiscal.
- A criação de nota fiscal depende apenas do banco do BillingService.
- O comportamento transacional padrão do EF Core é suficiente para persistir nota e itens em uma única chamada de `SaveChangesAsync`.
- Testes rápidos usam um numerador fake para isolar regras de aplicação. A implementação real usa sequence PostgreSQL.

## Processamento da SPEC-004

O endpoint escolhido foi `POST /api/notas-fiscais/{id}/processar`, porque a operação reúne consumo de estoque, geração do arquivo e fechamento. A interface futura poderá chamar isso de impressão.

O BillingService não usa transação distribuída. A proteção contra processamento concorrente usa `ProcessamentoEmAndamento` e `Versao` persistidos na nota, com controle otimista de concorrência. O status de negócio continua sendo somente `Aberta` ou `Fechada`.

A criação do arquivo usa `System.IO`, diretório configurável e arquivo temporário. O caminho persistido é relativo à raiz da aplicação; o consumidor nunca informa caminho de arquivo.

O snapshot de código e descrição existente desde a SPEC-003 continua sendo aceito nesta etapa. O consumo HTTP valida `ProdutoId` e quantidade no InventoryService, mas a consulta de dados cadastrais antes do processamento ainda é uma pendência para revisão futura.

Usos relevantes de LINQ nesta etapa incluem `Select` para montar o contrato de consumo e projeções para respostas. Todas as chamadas HTTP, banco e escrita de arquivo usam APIs assíncronas quando aplicável.

## Governança do repositório público

Documentos e diretórios usados exclusivamente para orientar agentes de IA não fazem parte da entrega pública. A pasta `docs/specs/` deve permanecer disponível localmente para orientar a implementação, mas deve ser ignorada pelo Git.

Documentos úteis ao avaliador, como `docs/architecture.md`, `docs/presentation-script.md` e `README.md`, permanecem públicos e devem estar em Português Brasileiro.
