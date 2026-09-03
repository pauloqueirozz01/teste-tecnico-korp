# Roteiro da Apresentação

Este arquivo é intencionalmente apenas uma estrutura inicial. Ele será completado progressivamente durante o desenvolvimento.

## 1. Resumo do projeto

- Objetivo da avaliação
- Principal caso de uso
- Stack tecnológica
- Idioma oficial do projeto: Português Brasileiro

## 2. Visão geral da arquitetura

- Frontend Angular
- InventoryService
- BillingService
- Bancos PostgreSQL separados
- Comunicação HTTP entre serviços

## 3. InventoryService

- Responsabilidade por produtos
- Responsabilidade por estoque
- Validação de estoque
- Consumo de estoque

## 4. BillingService

- Responsabilidade por notas fiscais
- Numeração de notas fiscais
- Ciclo de vida da nota fiscal
- Impressão da nota
- Geração de arquivo da nota fiscal

## 5. Fluxo de emissão da nota fiscal

- Criação de nota aberta
- Solicitação de impressão
- Validação e consumo de estoque
- Geração de arquivo
- Transição para status fechado
- Tratamento de falhas

## 6. Demonstração de resiliência

- Mostrar que o BillingService usa `IHttpClientFactory` com `Microsoft.Extensions.Http.Resilience`.
- Explicar timeout configurável de 5 segundos.
- Explicar retry pequeno somente para falhas transitórias: conexão, timeout, `500`, `502`, `503`, `504`.
- Explicar que `400`, `404` e `409` não recebem retry porque são falhas de negócio.
- Explicar idempotência persistida com `Idempotency-Key` para consumo e compensação de estoque.
- Desligar o InventoryService.
- Tentar processar uma nota `Aberta`.
- Mostrar resposta amigável `INVENTORY_SERVICE_INDISPONIVEL` com HTTP `503`.
- Mostrar que a nota continua `Aberta` e sem arquivo definitivo.
- Mostrar que `GET /health`, `GET /api/notas-fiscais` e `GET /api/notas-fiscais/{id}` continuam respondendo no BillingService.
- Provocar falhas suficientes para abrir o circuit breaker e mostrar falha rápida.
- Subir novamente o InventoryService.
- Aguardar a janela de recuperação configurada.
- Processar novamente a nota aberta.
- Mostrar consumo de estoque, arquivo `.txt` gerado e nota `Fechada`.
- Comentar que falha de compensação retorna `FALHA_COMPENSACAO_ESTOQUE`, gera log crítico e não fecha a nota falsamente.

## 7. Implementação frontend

- Aplicação em `frontend/korp-web` com Angular 20 e componentes standalone.
- `app.config.ts` concentra providers modernos, incluindo Router e HttpClient.
- `app.routes.ts` define as páginas e usa `loadComponent` para lazy loading.
- `screens` representam páginas de rota; `components/shared` concentra UI reutilizável.
- `ProdutoService` e `NotaFiscalService` encapsulam chamadas HTTP e retornam `Observable<T>`.
- RxJS aparece naturalmente nos services HTTP e será usado com `finalize`, `catchError` e `switchMap` quando os fluxos completos forem implementados.
- Signals são usados para estado local simples, como menu mobile.
- Lifecycle hooks não foram adicionados sem necessidade; `ngOnInit` será usado quando uma screen precisar carregar dados reais ao iniciar.
- Interface obrigatoriamente em Português Brasileiro.

## 8. Uso de LINQ

- Filtragem
- Projeção
- Agregação
- Validação
- Mapeamento

## 9. Testes

- Testes backend
- Testes Angular
- Verificação manual

## 10. Governança do repositório

- Artefatos internos de especificação ignorados no Git
- Documentação pública preservada
- Segredos e arquivos de ambiente reais fora do versionamento

## 11. Tradeoffs e restrições

- Simplicidade adequada para avaliação técnica
- Evitar overengineering
- Limitações conhecidas
