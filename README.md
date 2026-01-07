# ArxFlow - Sistema de Gestão de Boletas

Migração da aplicação Blazor para arquitetura .NET 9 + React TypeScript.

## Tecnologias

### Backend
- .NET 9 Minimal APIs
- Entity Framework Core 9 + SQLite
- Swagger/OpenAPI
- Serilog

### Frontend
- React 19 + TypeScript
- Vite
- Material-UI (MUI)
- TanStack Query (React Query)
- React Router
- Orval (geração automática de hooks)

## Como Rodar no Visual Studio

### Pré-requisitos
- Visual Studio 2022 (versão 17.8 ou superior)
- .NET 9 SDK
- Node.js 18+ e npm

### Passos para Iniciar

1. **Abrir a Solução**
   - Abra `C:\Lucas\dev\arxflow-new\ArxFlow.sln` no Visual Studio

2. **Restaurar Dependências**
   - O Visual Studio vai restaurar automaticamente os pacotes NuGet
   - Para o frontend, execute manualmente no Package Manager Console ou terminal:
     ```powershell
     cd arxflow.client
     npm install
     ```

3. **Configurar Projeto de Inicialização**
   - No Solution Explorer, clique com botão direito em **ArxFlow.Server**
   - Selecione "Set as Startup Project"
   - O projeto ficará em negrito

4. **Iniciar a Aplicação**
   - Pressione **F5** ou clique em "Iniciar"
   - O Visual Studio irá:
     - Compilar o backend .NET
     - Iniciar o servidor ASP.NET Core na porta 5236
     - Automaticamente iniciar o Vite dev server (frontend) na porta 5173
     - Abrir o navegador em `http://localhost:5236`

5. **Acesso**
   - **Aplicação (recomendado)**: http://localhost:5236
   - **Frontend direto (Vite)**: http://localhost:5173
   - **API**: http://localhost:5236/api
   - **Swagger**: http://localhost:5236/swagger

### Verificar se está funcionando

Após iniciar, você deve ver no console:
```
[INF] Iniciando ArxFlow API
[INF] Database migrated and seeded successfully
[INF] ArxFlow API started successfully
[INF] Now listening on: http://localhost:5236
```

E o Vite deve iniciar automaticamente em outra janela mostrando:
```
VITE v7.x.x  ready in xxx ms

➜  Local:   http://localhost:5173/
```

## Estrutura do Projeto

```
arxflow-new/
├── ArxFlow.Server/              # Backend .NET 9
│   ├── Data/                    # DbContext e Migrations
│   ├── Models/                  # Entidades EF Core
│   ├── DTOs/                    # Request/Response DTOs
│   ├── Services/                # Lógica de negócio
│   ├── Utils/                   # Utilitários (BrazilianCalendar, etc)
│   ├── Endpoints/               # Minimal APIs endpoints
│   └── Program.cs               # Configuração da aplicação
│
└── arxflow.client/              # Frontend React
    ├── src/
    │   ├── api/
    │   │   ├── generated/       # Hooks gerados pelo Orval
    │   │   └── client.ts        # Axios client
    │   ├── components/
    │   │   └── Layout/          # TopBar, SideMenu, MainLayout
    │   ├── pages/               # Páginas React
    │   ├── routes/              # React Router config
    │   └── theme/               # MUI Theme
    ├── orval.config.ts          # Configuração Orval
    └── vite.config.ts           # Configuração Vite
```

## Funcionalidades Implementadas

### CRUD Completo
- ✅ Emissores (GET, POST, PUT, DELETE)
- ✅ Fundos (GET, POST, PUT, DELETE)
- ✅ Contrapartes (GET, POST, PUT, DELETE)

### Páginas React
- ✅ EmissoresPage - CRUD com MUI DataGrid
- ✅ FundosPage - CRUD com MUI DataGrid
- ✅ ContrapartesPage - CRUD com MUI DataGrid

### Integração
- ✅ Orval gera automaticamente hooks TanStack Query a partir do OpenAPI
- ✅ Layout responsivo com TopBar e SideMenu
- ✅ Tema MUI customizado em português
- ✅ TanStack Query DevTools

## Regenerar Hooks do Orval

Sempre que modificar endpoints no backend:

```bash
# 1. Certifique-se que o backend está rodando para gerar swagger.json atualizado

# 2. Na pasta arxflow.client, execute:
npm run generate:api
```

Isso irá:
- Buscar o swagger.json do backend
- Gerar hooks TanStack Query em `src/api/generated/`
- Gerar types TypeScript em `src/api/generated/model/`

## Próximas Implementações

Conforme plano de migração:
- [ ] Ativos (com autocomplete)
- [ ] Calculadora de Títulos (5 tipos TPF)
- [ ] Yield Curve (curvas DI1 e DAP)
- [ ] Downloads (B3, ANBIMA, BCB)
- [ ] Boletas (tabela editável inline)
- [ ] Validação de Calculadoras

## Troubleshooting

### Problema: Apenas o frontend inicia (backend não inicia)
**Solução 1**: Verificar projeto de inicialização
- No Solution Explorer, clique direito em **ArxFlow.Server** → "Set as Startup Project"

**Solução 2**: Iniciar manualmente
- Abra um terminal na pasta `ArxFlow.Server`
- Execute: `dotnet run`
- Em outro terminal, na pasta `arxflow.client`: `npm run dev`

**Solução 3**: Verificar porta
- Certifique-se que a porta 5236 não está em uso
- Feche processos `ArxFlow.Server.exe` no Task Manager

### Problema: Frontend não carrega
- Execute `npm install` na pasta `arxflow.client`
- Inicie manualmente: `npm run dev` na pasta `arxflow.client`
- Verifique se a porta 5173 está livre

### Problema: Erro de database/migration
- Delete o arquivo `ArxFlow.Server/Data/arxflow.db` se existir
- Reinicie a aplicação - o banco será recriado automaticamente

### Problema: Erro de CORS
- Verifique se está acessando via `http://localhost:5236` (não 5173 diretamente para API)
- As origens CORS estão em `Program.cs`

### Problema: Hooks do Orval desatualizados após modificar API
1. Certifique-se que o backend está rodando
2. Na pasta `arxflow.client`, execute: `npm run generate:api`
3. Reinicie o frontend

## Banco de Dados

- **Tipo**: SQLite
- **Localização**: `ArxFlow.Server/Data/arxflow.db`
- **Migrations**: Aplicadas automaticamente na inicialização
- **Seed**: Dados iniciais inseridos automaticamente (contrapartes, emissores, fundos)
