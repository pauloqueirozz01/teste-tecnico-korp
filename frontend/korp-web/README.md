# Korp Web

Aplicação Angular do teste técnico Korp para o Sistema de Notas Fiscais.

## Stack

- Angular `20.3.30`
- Angular CLI `20.3.35`
- TypeScript `5.9.3`
- RxJS `7.8.2`
- pnpm `11.15.1`

## Execução local

```bash
pnpm install
pnpm start
```

A aplicação roda em `http://localhost:4200`.

APIs esperadas em desenvolvimento:

- InventoryService: `http://localhost:5001`
- BillingService: `http://localhost:5002`

## Build

```bash
pnpm build
```

## Testes

```bash
pnpm test --watch=false --source-map=false
```

Os testes usam Karma com Chrome Headless configurado em `karma.conf.js`.

## Estrutura

```text
src/app/
├── core/
│   ├── config/
│   ├── interceptors/
│   ├── models/
│   └── services/
├── components/shared/
├── screens/
├── app.config.ts
├── app.routes.ts
├── app.ts
├── app.html
└── app.css
```

## Rotas

- `/`
- `/produtos`
- `/produtos/novo`
- `/notas-fiscais`
- `/notas-fiscais/nova`
- `/notas-fiscais/:id`
- `/**`

## Decisões

- Componentes standalone.
- Screens carregadas com `loadComponent`.
- Services HTTP retornam `Observable<T>`.
- Estado local simples usa Signals quando apropriado.
- URLs das APIs ficam centralizadas em `environment` e `API_CONFIG`.
- Sem bibliotecas visuais externas nesta fundação.
