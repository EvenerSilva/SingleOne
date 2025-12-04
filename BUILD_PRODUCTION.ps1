# 🚀 Script de Build para Produção
# SingleOne - Versão 1.1.0
# Data: 31/10/2024

Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   🚀 SingleOne - Build de Produção v1.1.0" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# 1️⃣ Limpar processos anteriores
Write-Host "1️⃣  Finalizando processos anteriores..." -ForegroundColor Yellow
taskkill /F /IM dotnet.exe 2>$null | Out-Null
taskkill /F /IM node.exe 2>$null | Out-Null
Write-Host "   ✅ Processos finalizados" -ForegroundColor Green
Write-Host ""

# 2️⃣ Build do Backend
Write-Host "2️⃣  Compilando Backend (.NET 6.0)..." -ForegroundColor Yellow
cd C:\SingleOne\SingleOne_Backend\SingleOneAPI
dotnet clean --configuration Release | Out-Null
$buildResult = dotnet build --configuration Release 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Backend compilado com sucesso" -ForegroundColor Green
} else {
    Write-Host "   ❌ Erro ao compilar backend" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}
Write-Host ""

# 3️⃣ Build do Frontend
Write-Host "3️⃣  Compilando Frontend (Angular)..." -ForegroundColor Yellow
cd C:\SingleOne\SingleOne_Frontend

# Limpar cache e node_modules antigos (opcional)
# Remove-Item -Path "node_modules" -Recurse -Force -ErrorAction SilentlyContinue
# Remove-Item -Path "dist" -Recurse -Force -ErrorAction SilentlyContinue

# Build de produção
$ngBuild = ng build --configuration production 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Frontend compilado com sucesso" -ForegroundColor Green
} else {
    Write-Host "   ❌ Erro ao compilar frontend" -ForegroundColor Red
    Write-Host $ngBuild
    exit 1
}
Write-Host ""

# 4️⃣ Criar pasta de distribuição
Write-Host "4️⃣  Criando pacote de distribuição..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$distPath = "C:\SingleOne\SingleOne_DIST_v1.1.0_$timestamp"

New-Item -ItemType Directory -Path $distPath -Force | Out-Null
New-Item -ItemType Directory -Path "$distPath\Backend" -Force | Out-Null
New-Item -ItemType Directory -Path "$distPath\Frontend" -Force | Out-Null
New-Item -ItemType Directory -Path "$distPath\Docs" -Force | Out-Null

# Copiar Backend compilado
Write-Host "   📦 Copiando Backend..." -ForegroundColor Cyan
Copy-Item -Path "C:\SingleOne\SingleOne_Backend\SingleOneAPI\bin\Release\net6.0\*" `
          -Destination "$distPath\Backend" `
          -Recurse -Force

# Copiar Frontend compilado
Write-Host "   📦 Copiando Frontend..." -ForegroundColor Cyan
Copy-Item -Path "C:\SingleOne\SingleOne_Frontend\dist\*" `
          -Destination "$distPath\Frontend" `
          -Recurse -Force

# Copiar Changelog
Write-Host "   📦 Copiando Documentação..." -ForegroundColor Cyan
Copy-Item -Path "C:\SingleOne\CHANGELOG_INVENTARIO_FORCADO.md" `
          -Destination "$distPath\Docs\" `
          -Force

Write-Host "   ✅ Pacote criado em: $distPath" -ForegroundColor Green
Write-Host ""

# 5️⃣ Criar arquivo de instruções
Write-Host "5️⃣  Gerando instruções de instalação..." -ForegroundColor Yellow

$instructions = @"
# 📦 SingleOne - Instruções de Instalação
**Versão:** 1.1.0
**Data:** $(Get-Date -Format "dd/MM/yyyy HH:mm")

## 🔧 Requisitos

### Backend:
- .NET 6.0 Runtime ou SDK
- PostgreSQL 12+
- Porta 5000 disponível

### Frontend:
- Servidor web (IIS, Nginx, Apache, etc.)
- Porta 4200 (desenvolvimento) ou 80/443 (produção)

---

## 📁 Estrutura dos Arquivos

\`\`\`
SingleOne_DIST_v1.1.0_$timestamp/
├── Backend/           (APIs .NET)
├── Frontend/          (Angular compilado)
└── Docs/              (Changelog e documentação)
\`\`\`

---

## 🚀 Instalação

### **1. Backend**

\`\`\`powershell
cd Backend
dotnet SingleOneAPI.dll
\`\`\`

**Ou configure como serviço Windows:**
\`\`\`powershell
sc.exe create SingleOneAPI binPath="C:\path\to\Backend\SingleOneAPI.exe"
sc.exe start SingleOneAPI
\`\`\`

### **2. Frontend**

**Opção A: Servidor de Desenvolvimento**
\`\`\`powershell
cd Frontend
npx http-server -p 4200
\`\`\`

**Opção B: IIS (Produção)**
1. Abra IIS Manager
2. Crie novo site
3. Aponte para pasta \`Frontend\`
4. Configure binding (porta 80/443)
5. Instale URL Rewrite Module
6. Configure web.config para SPA

### **3. Verificação**

Após iniciar ambos os servidores:
- Backend: http://localhost:5000/api
- Frontend: http://localhost:4200

---

## 🔐 Configurações

### **Backend (.env)**

\`\`\`env
DB_HOST=127.0.0.1
DB_PORT=5432
DB_NAME=singleone
DB_USER=postgres
DB_PASSWORD=sua_senha
SITE_URL=http://localhost:4200
\`\`\`

### **Frontend (environment.prod.ts)**

\`\`\`typescript
export const environment = {
  production: true,
  apiUrl: 'http://seu-servidor:5000/api'
};
\`\`\`

---

## ✅ Novas Funcionalidades (v1.1.0)

- ✅ Validação de duplicidade de inventário forçado
- ✅ Feedback visual com botões coloridos
- ✅ Navegação inteligente para inventários pendentes
- ✅ Busca por nome do colaborador no backend
- ✅ Validação em massa de colaboradores
- ✅ Toast/mensagens sempre visíveis
- ✅ Melhorias de UX na distribuição de botões

**Veja \`Docs/CHANGELOG_INVENTARIO_FORCADO.md\` para detalhes completos.**

---

## 🆘 Suporte

Em caso de problemas:
1. Verifique logs do backend: \`Backend/logs/\`
2. Verifique console do navegador (F12)
3. Confirme conexão com PostgreSQL
4. Verifique permissões de firewall

---

**Desenvolvido por:** Claude AI Assistant
**Data de Build:** $(Get-Date -Format "dd/MM/yyyy HH:mm")
"@

$instructions | Out-File -FilePath "$distPath\INSTALL.md" -Encoding UTF8
Write-Host "   ✅ Instruções geradas: $distPath\INSTALL.md" -ForegroundColor Green
Write-Host ""

# 6️⃣ Resumo Final
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   ✅ Build de Produção Concluído!" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "📦 Pacote de Distribuição:" -ForegroundColor Yellow
Write-Host "   $distPath" -ForegroundColor White
Write-Host ""
Write-Host "📄 Arquivos Incluídos:" -ForegroundColor Yellow
Write-Host "   ✅ Backend compilado (Release)" -ForegroundColor Green
Write-Host "   ✅ Frontend compilado (Production)" -ForegroundColor Green
Write-Host "   ✅ Changelog detalhado" -ForegroundColor Green
Write-Host "   ✅ Instruções de instalação" -ForegroundColor Green
Write-Host ""
Write-Host "🚀 Próximos Passos:" -ForegroundColor Yellow
Write-Host "   1. Revise o arquivo INSTALL.md" -ForegroundColor White
Write-Host "   2. Configure variáveis de ambiente" -ForegroundColor White
Write-Host "   3. Teste em ambiente de homologação" -ForegroundColor White
Write-Host "   4. Deploy em produção" -ForegroundColor White
Write-Host ""
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan

# Abrir pasta de distribuição
explorer $distPath

Write-Host ""
Write-Host "Pressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

