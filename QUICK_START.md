# Quick Start - Iniciar Manualmente

Se o Visual Studio não está iniciando o backend automaticamente, siga estas instruções:

## Opção 1: Dois Terminais (Recomendado)

### Terminal 1 - Backend (.NET)
```powershell
cd C:\Lucas\dev\arxflow-new\ArxFlow.Server
dotnet run
```

Aguarde até ver:
```
[INF] ArxFlow API started successfully
[INF] Now listening on: http://localhost:5236
```

### Terminal 2 - Frontend (Vite)
```powershell
cd C:\Lucas\dev\arxflow-new\arxflow.client
npm run dev
```

Aguarde até ver:
```
VITE v7.x.x  ready in xxx ms
➜  Local:   http://localhost:5173/
```

### Acesse
Abra o navegador em: **http://localhost:5236**

---

## Opção 2: Visual Studio (Configuração)

1. **Abra** `ArxFlow.sln` no Visual Studio

2. **Configure projeto de inicialização**:
   - Solution Explorer → Clique direito em **ArxFlow.Server**
   - Selecione **"Set as Startup Project"**
   - O nome do projeto ficará em **negrito**

3. **Pressione F5**

4. Se ainda não funcionar:
   - Feche o Visual Studio
   - Mate processos: Task Manager → Procure por `ArxFlow.Server` ou `node` → Finalizar
   - Reabra o Visual Studio
   - Tente novamente

---

## Verificação Rápida

### Backend está rodando?
Abra: http://localhost:5236/health

Deve retornar:
```json
{
  "status": "healthy",
  "timestamp": "2026-01-06T..."
}
```

### Frontend está rodando?
Abra: http://localhost:5173

Deve carregar a aplicação React

### API está acessível?
Abra: http://localhost:5236/swagger

Deve mostrar a documentação Swagger da API

---

## Páginas Disponíveis

- **Emissores**: http://localhost:5236/emissores
- **Fundos**: http://localhost:5236/fundos
- **Contrapartes**: http://localhost:5236/contrapartes

---

## Problemas Comuns

### Porta 5236 já está em uso
```powershell
# Windows - Encontrar processo
netstat -ano | findstr :5236

# Matar processo (substitua PID pelo número retornado)
taskkill /PID <PID> /F
```

### Porta 5173 já está em uso
```powershell
# Windows - Encontrar processo
netstat -ano | findstr :5173

# Matar processo
taskkill /PID <PID> /F
```

### npm install não foi executado
```powershell
cd C:\Lucas\dev\arxflow-new\arxflow.client
npm install
```

### Banco de dados corrompido
```powershell
# Delete o banco
del C:\Lucas\dev\arxflow-new\ArxFlow.Server\Data\arxflow.db

# Reinicie o backend - será recriado automaticamente
cd C:\Lucas\dev\arxflow-new\ArxFlow.Server
dotnet run
```
