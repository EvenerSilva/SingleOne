# 🚀 Início Rápido - Git + Deploy Contabo

## ⚡ Primeiros Passos (Execute uma vez)

### 1. Instalar Git
```bash
# Windows: Baixe e instale
https://git-scm.com/download/win

# Verificar instalação
git --version
```

### 2. Configurar Git
```bash
git config --global user.name "Seu Nome"
git config --global user.email "seu.email@exemplo.com"
```

### 3. Inicializar Repositório
```bash
cd C:\SingleOne
git init
git add .
git commit -m "feat: commit inicial do SingleOne"
```

### 4. Adicionar Repositório Remoto
```bash
# GitHub
git remote add origin https://github.com/seu-usuario/singleone.git

# GitLab
git remote add origin https://gitlab.com/seu-usuario/singleone.git

# Bitbucket
git remote add origin https://bitbucket.org/seu-usuario/singleone.git
```

### 5. Enviar Código
```bash
git branch -M main
git push -u origin main
```

---

## 💻 Uso Diário

### Fazer mudanças e commitar:
```powershell
# Método 1: Usando o script automatizado
.\commit-push.ps1 "feat: adiciona nova funcionalidade"

# Método 2: Manualmente
git add .
git commit -m "feat: adiciona nova funcionalidade"
git push
```

---

## 🎯 Criar Nova Funcionalidade

```bash
# 1. Criar branch da funcionalidade
git checkout -b feature/nome-funcionalidade

# 2. Fazer mudanças no código...

# 3. Commitar
git add .
git commit -m "feat: adiciona nome-funcionalidade"

# 4. Push
git push origin feature/nome-funcionalidade

# 5. Criar Pull Request no GitHub/GitLab/Bitbucket

# 6. Após aprovação, merge para main
git checkout main
git merge feature/nome-funcionalidade
git push origin main
```

---

## 🚀 Criar Release

```powershell
# Usando o script automatizado
.\criar-release.ps1 "v2.6.0"

# Isso cria:
# - Branch release/v2.6.0
# - Merge para main
# - Tag v2.6.0
# - Push para repositório
```

---

## 📦 Deploy para Contabo

### No servidor Contabo (primeira vez):
```bash
# 1. Conectar via SSH
ssh usuario@seu-servidor.contabo.com

# 2. Ir para diretório da aplicação
cd /var/www/singleone

# 3. Clonar repositório
git clone https://github.com/seu-usuario/singleone.git .

# 4. Tornar script executável
chmod +x deploy-contabo.sh

# 5. Executar deploy inicial
./deploy-contabo.sh
```

### Deploys subsequentes:
```bash
# Apenas executar o script
./deploy-contabo.sh
```

---

## 🔥 Comandos Essenciais

```bash
# Ver status
git status

# Ver histórico
git log --oneline

# Ver branches
git branch -a

# Mudar de branch
git checkout nome-branch

# Criar nova branch
git checkout -b nova-branch

# Atualizar branch atual
git pull

# Ver diferenças
git diff

# Desfazer mudanças locais
git reset --hard HEAD

# Ver remotes configurados
git remote -v
```

---

## 📊 Workflow Recomendado

```
1. Trabalho local → feature/nome
2. Testes locais → passar todos
3. Commit → mensagem descritiva
4. Push → origin feature/nome
5. Pull Request → revisão de código
6. Merge → para main
7. Release → criar tag
8. Deploy → Contabo
```

---

## ⚠️ Nunca Commitar

```bash
# Arquivos que NUNCA devem ir para o Git:
❌ .env
❌ appsettings.Production.json
❌ senhas
❌ tokens
❌ chaves de API
❌ node_modules/
❌ bin/
❌ obj/
❌ dist/

# O .gitignore já cuida disso!
```

---

## 🆘 Problemas Comuns

### "Changes would be overwritten"
```bash
git stash
git pull
git stash pop
```

### "Merge conflict"
```bash
# 1. Abrir arquivo em conflito
# 2. Remover marcadores <<<< ==== >>>>
# 3. Salvar arquivo
git add arquivo-resolvido.cs
git commit -m "fix: resolve conflito"
```

### "Permission denied"
```bash
# Usar HTTPS em vez de SSH
git remote set-url origin https://github.com/seu-usuario/singleone.git
```

---

## 📚 Arquivos Criados

| Arquivo | Descrição |
|---------|-----------|
| `GUIA_GIT_DEPLOY_CONTABO.md` | Guia completo e detalhado |
| `.gitignore` | Ignora arquivos desnecessários |
| `commit-push.ps1` | Script automatizado para commit |
| `criar-release.ps1` | Script para criar releases |
| `deploy-contabo.sh` | Script de deploy no servidor |
| `INICIO_RAPIDO_GIT.md` | Este guia (referência rápida) |

---

## 🎓 Aprender Mais

- **Git Documentation**: https://git-scm.com/doc
- **GitHub Guides**: https://guides.github.com
- **Git Cheat Sheet**: https://education.github.com/git-cheat-sheet-education.pdf
- **Pro Git Book**: https://git-scm.com/book/pt-br/v2

---

## ✅ Checklist de Primeiro Uso

- [ ] Git instalado
- [ ] Usuário e email configurados
- [ ] Repositório inicializado
- [ ] `.gitignore` criado
- [ ] Primeiro commit feito
- [ ] Repositório remoto adicionado
- [ ] Código enviado para repositório
- [ ] Scripts testados localmente
- [ ] Servidor Contabo configurado
- [ ] Deploy testado

---

**Dúvidas?** Consulte o `GUIA_GIT_DEPLOY_CONTABO.md` para informações detalhadas!

**Versão:** 1.0  
**Data:** Janeiro 2025  
**Projeto:** SingleOne

