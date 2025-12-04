# 🚀 GUIA COMPLETO: Git + Deploy para Contabo

## 📋 Índice
1. [Instalação do Git](#1-instalação-do-git)
2. [Configuração Inicial](#2-configuração-inicial)
3. [Estrutura de Branches](#3-estrutura-de-branches)
4. [Workflow de Desenvolvimento](#4-workflow-de-desenvolvimento)
5. [Deploy para Contabo](#5-deploy-para-contabo)
6. [Scripts de Automação](#6-scripts-de-automação)
7. [Troubleshooting](#7-troubleshooting)

---

## 1. Instalação do Git

### Windows:
1. Baixe o Git: https://git-scm.com/download/win
2. Execute o instalador
3. Mantenha as opções padrão
4. Reinicie o PowerShell/CMD após a instalação

### Verificar instalação:
```bash
git --version
```

---

## 2. Configuração Inicial

### 2.1. Configurar usuário:
```bash
git config --global user.name "Seu Nome"
git config --global user.email "seu.email@exemplo.com"
```

### 2.2. Inicializar repositório:
```bash
cd C:\SingleOne
git init
```

### 2.3. Criar `.gitignore`:
Vou criar um arquivo `.gitignore` completo para você!

---

## 3. Estrutura de Branches

```
main (produção) ←─── release/vX.X.X ←─── develop ←─── feature/nome-da-funcionalidade
                                            ↑
                                            └─── hotfix/correcao-urgente
```

### Branches principais:

#### `main` (produção)
- Código em **PRODUÇÃO** no Contabo
- **NUNCA** fazer commit direto aqui
- Apenas recebe merges de `release/*` ou `hotfix/*`

#### `develop` (desenvolvimento)
- Código em **DESENVOLVIMENTO**
- Base para novas features
- Recebe merges de `feature/*`

#### `feature/*` (funcionalidades)
- Novas funcionalidades
- Exemplo: `feature/oni-sugestoes`, `feature/relatorio-custos`

#### `hotfix/*` (correções urgentes)
- Correções em **PRODUÇÃO**
- Exemplo: `hotfix/paginacao-quebrada`

#### `release/*` (preparação para produção)
- Preparação para deploy
- Exemplo: `release/v2.5.19`

---

## 4. Workflow de Desenvolvimento

### 4.1. Começando uma nova funcionalidade:

```bash
# 1. Ir para develop
git checkout develop
git pull origin develop

# 2. Criar branch de feature
git checkout -b feature/nome-da-funcionalidade

# 3. Fazer alterações...
# (desenvolver código)

# 4. Adicionar arquivos ao staging
git add .

# 5. Commitar com mensagem descritiva
git commit -m "feat: adiciona sugestões contextuais do Oni"

# 6. Enviar para o repositório remoto
git push origin feature/nome-da-funcionalidade
```

### 4.2. Finalizando a funcionalidade:

```bash
# 1. Atualizar develop
git checkout develop
git pull origin develop

# 2. Fazer merge da feature
git merge feature/nome-da-funcionalidade

# 3. Enviar develop atualizado
git push origin develop

# 4. (Opcional) Deletar branch da feature
git branch -d feature/nome-da-funcionalidade
git push origin --delete feature/nome-da-funcionalidade
```

### 4.3. Correção urgente em produção (hotfix):

```bash
# 1. Criar hotfix a partir da main
git checkout main
git pull origin main
git checkout -b hotfix/correcao-urgente

# 2. Fazer correção...

# 3. Commitar
git add .
git commit -m "fix: corrige paginação quebrada"

# 4. Merge para main
git checkout main
git merge hotfix/correcao-urgente
git push origin main

# 5. Merge para develop também
git checkout develop
git merge hotfix/correcao-urgente
git push origin develop

# 6. Deletar branch
git branch -d hotfix/correcao-urgente
```

---

## 5. Deploy para Contabo

### 5.1. Configurar servidor Contabo:

#### No seu PC (uma vez):
```bash
# Adicionar repositório remoto
git remote add origin https://github.com/seu-usuario/singleone.git

# OU se usar SSH:
git remote add origin git@github.com:seu-usuario/singleone.git
```

#### No servidor Contabo (SSH):
```bash
# 1. Conectar via SSH
ssh usuario@seu-servidor-contabo.com

# 2. Ir para o diretório da aplicação
cd /var/www/singleone

# 3. Clonar o repositório (primeira vez)
git clone https://github.com/seu-usuario/singleone.git .

# 4. Checkout para branch main
git checkout main
```

### 5.2. Deploy manual (Contabo):

```bash
# No servidor Contabo:

# 1. Ir para o diretório
cd /var/www/singleone

# 2. Atualizar código
git pull origin main

# 3. Backend (.NET):
cd SingleOne_Backend/SingleOneAPI
dotnet restore
dotnet build --configuration Release
sudo systemctl restart singleone-backend

# 4. Frontend (Angular):
cd ../../SingleOne_Frontend
npm install
npm run build --prod
sudo systemctl restart nginx
```

### 5.3. Deploy automatizado (recomendado):

Criar script `deploy-contabo.sh` no servidor:

```bash
#!/bin/bash
# deploy-contabo.sh

set -e

echo "🚀 Iniciando deploy..."

# 1. Atualizar código
echo "📥 Atualizando código..."
git pull origin main

# 2. Verificar mudanças no backend
if git diff-tree --no-commit-id --name-only -r HEAD | grep -q "SingleOne_Backend"; then
    echo "🔧 Atualizando Backend..."
    cd SingleOne_Backend/SingleOneAPI
    dotnet restore
    dotnet build --configuration Release
    sudo systemctl restart singleone-backend
    cd ../..
fi

# 3. Verificar mudanças no frontend
if git diff-tree --no-commit-id --name-only -r HEAD | grep -q "SingleOne_Frontend"; then
    echo "🎨 Atualizando Frontend..."
    cd SingleOne_Frontend
    npm install --production
    npm run build --prod
    sudo systemctl restart nginx
    cd ..
fi

echo "✅ Deploy concluído!"
```

Tornar executável:
```bash
chmod +x deploy-contabo.sh
```

Executar:
```bash
./deploy-contabo.sh
```

---

## 6. Scripts de Automação

### 6.1. Script de commit rápido (Windows):

Criar `commit-push.ps1`:

```powershell
# commit-push.ps1
param(
    [string]$mensagem = "feat: atualização"
)

Write-Host "📝 Preparando commit..." -ForegroundColor Green

# Adicionar arquivos
git add .

# Commitar
git commit -m $mensagem

# Push
$branch = git rev-parse --abbrev-ref HEAD
git push origin $branch

Write-Host "✅ Commit e push concluídos na branch $branch!" -ForegroundColor Green
```

Usar:
```powershell
.\commit-push.ps1 "fix: corrige paginação"
```

### 6.2. Script de release (Windows):

Criar `criar-release.ps1`:

```powershell
# criar-release.ps1
param(
    [string]$versao = "v2.5.19"
)

Write-Host "🚀 Criando release $versao..." -ForegroundColor Green

# 1. Atualizar develop
git checkout develop
git pull origin develop

# 2. Criar branch de release
git checkout -b "release/$versao"

# 3. (Aqui você pode atualizar version.txt, package.json, etc.)

# 4. Commitar mudanças de versão
git add .
git commit -m "chore: prepare release $versao"

# 5. Merge para main
git checkout main
git pull origin main
git merge "release/$versao"

# 6. Criar tag
git tag -a $versao -m "Release $versao"

# 7. Push
git push origin main
git push origin $versao

# 8. Merge de volta para develop
git checkout develop
git merge "release/$versao"
git push origin develop

# 9. Deletar branch de release
git branch -d "release/$versao"

Write-Host "✅ Release $versao criada!" -ForegroundColor Green
Write-Host "🚀 Agora faça o deploy no Contabo!" -ForegroundColor Yellow
```

Usar:
```powershell
.\criar-release.ps1 "v2.6.0"
```

---

## 7. Troubleshooting

### Problema: "Changes would be overwritten by merge"
```bash
# Solução 1: Fazer stash das mudanças
git stash
git pull
git stash pop

# Solução 2: Descartar mudanças locais
git reset --hard HEAD
git pull
```

### Problema: "Merge conflict"
```bash
# 1. Ver arquivos em conflito
git status

# 2. Editar arquivos manualmente (remover marcadores <<<<, ====, >>>>)

# 3. Adicionar arquivos resolvidos
git add arquivo-resolvido.cs

# 4. Commitar
git commit -m "fix: resolve conflitos de merge"
```

### Problema: "Repository not found"
```bash
# Verificar remote configurado
git remote -v

# Reconfigurar se necessário
git remote set-url origin https://github.com/seu-usuario/singleone.git
```

### Problema: "Permission denied (publickey)"
```bash
# Gerar chave SSH
ssh-keygen -t ed25519 -C "seu.email@exemplo.com"

# Copiar chave pública
cat ~/.ssh/id_ed25519.pub

# Adicionar no GitHub: Settings → SSH and GPG keys → New SSH key
```

---

## 📚 Convenções de Commit

Use mensagens descritivas seguindo o padrão:

```
tipo(escopo): descrição curta

Descrição detalhada (opcional)
```

### Tipos:
- `feat`: Nova funcionalidade
- `fix`: Correção de bug
- `docs`: Documentação
- `style`: Formatação (não afeta código)
- `refactor`: Refatoração
- `test`: Testes
- `chore`: Tarefas de manutenção

### Exemplos:
```bash
git commit -m "feat(oni): adiciona sugestões contextuais"
git commit -m "fix(paginacao): corrige paginador de custos"
git commit -m "docs(readme): atualiza guia de instalação"
```

---

## 🎯 Checklist de Deploy

Antes de fazer deploy para produção:

- [ ] Código testado localmente
- [ ] Todos os testes passando
- [ ] Sem console.log/debug
- [ ] Variáveis de ambiente configuradas
- [ ] Banco de dados migrado
- [ ] Backup do banco antes do deploy
- [ ] Documentação atualizada
- [ ] Changelog atualizado
- [ ] Tag de versão criada

---

## 🔒 Segurança

### Nunca committar:
- ❌ Senhas
- ❌ Chaves de API
- ❌ Tokens
- ❌ Certificados
- ❌ Arquivos `.env`
- ❌ `appsettings.Production.json` com dados sensíveis

### Sempre usar:
- ✅ Variáveis de ambiente
- ✅ `.gitignore` configurado
- ✅ Secrets do GitHub/GitLab
- ✅ Arquivos `.env.example` (sem dados reais)

---

## 📞 Suporte

Para dúvidas ou problemas:
1. Consulte a documentação do Git: https://git-scm.com/doc
2. Veja o guia do GitHub: https://guides.github.com
3. Stack Overflow: https://stackoverflow.com/questions/tagged/git

---

**Criado em:** Janeiro 2025  
**Versão:** 1.0  
**Projeto:** SingleOne - Plataforma de Governança de Recursos Corporativos

