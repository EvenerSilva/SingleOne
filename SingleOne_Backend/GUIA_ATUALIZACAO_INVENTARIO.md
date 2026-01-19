# 📋 Guia de Atualização - Suporte a Inventário em Notas Fiscais

## ✅ Status: Tudo já está no código!

Todas as alterações já estão commitadas no Git e serão aplicadas automaticamente em **novos clientes**. Para **clientes existentes** e **ambiente de demo**, siga este guia.

---

## 🆕 Para Novos Clientes

### ✅ **JÁ ESTÁ INCLUÍDO AUTOMATICAMENTE!**

Quando você executar o script `install_singleone_full.sh` em um novo servidor:

1. ✅ O script `01. Criar Tabelas.sql` já inclui:
   - Coluna `tipo_lancamento` na tabela `notasfiscais`
   - Coluna `valorunitario` nullable em `notasfiscaisitens`
   - Script de migração automática (linhas 444-488)

2. ✅ Todo o código backend e frontend já está atualizado

3. ✅ **Nenhuma ação manual é necessária!**

**Comando para novo cliente:**
```bash
sudo SITE_DOMAIN="novocliente.singleone.com.br" \
     SITE_IP="IP_DO_SERVIDOR" \
     DB_PASSWORD="SenhaSegura123" \
     bash deploy/linux/install_singleone_full.sh
```

---

## 🔄 Para Ambiente de Demo

### Passo 1: Atualizar Código

```bash
# Conectar no servidor de demo
ssh root@[IP_DO_DEMO]

# Atualizar código
cd /opt/SingleOne/SingleOne_Backend
git pull origin main

cd /opt/SingleOne/SingleOne_Frontend
git pull origin main
```

### Passo 2: Executar Migração SQL

```bash
cd /opt/SingleOne/SingleOne_Backend

# Executar script de migração
sudo -u postgres psql -d singleone -f scripts/migrar_suporte_inventario_notasfiscais.sql
```

**OU** se preferir usar o script completo (já inclui a migração):

```bash
cd /opt/SingleOne
sudo -u postgres psql -d singleone -f "01. Criar Tabelas.sql" 2>&1 | grep -i "tipo_lancamento\|valorunitario" | head -10
```

### Passo 3: Recompilar Backend

```bash
# Parar serviço
systemctl stop singleone-api

# Publicar
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI
dotnet publish -c Release -o /opt/singleone-api-publish

# Reiniciar
systemctl start singleone-api
systemctl status singleone-api
```

### Passo 4: Recompilar Frontend

```bash
cd /opt/SingleOne/SingleOne_Frontend
export NODE_OPTIONS=--openssl-legacy-provider
npm run build -- --configuration production
systemctl reload nginx
```

### Passo 5: Verificar

1. Acessar: `https://demo.singleone.com.br/notas-fiscais`
2. Criar nova nota fiscal
3. Verificar se aparece o campo "Tipo de Lançamento"

---

## 🔄 Para Clientes Existentes (FitBank, etc.)

### Opção 1: Script de Migração (Recomendado)

```bash
# 1. Atualizar código
cd /opt/SingleOne/SingleOne_Backend
git pull origin main

# 2. Executar migração
sudo -u postgres psql -d singleone -f scripts/migrar_suporte_inventario_notasfiscais.sql

# 3. Recompilar backend
systemctl stop singleone-api
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI
dotnet publish -c Release -o /opt/singleone-api-publish
systemctl start singleone-api

# 4. Recompilar frontend
cd /opt/SingleOne/SingleOne_Frontend
git pull origin main
export NODE_OPTIONS=--openssl-legacy-provider
npm run build -- --configuration production
systemctl reload nginx
```

### Opção 2: Script Completo de Tabelas

Se preferir garantir que todas as tabelas estão atualizadas:

```bash
cd /opt/SingleOne
sudo -u postgres psql -d singleone -f "01. Criar Tabelas.sql" 2>&1 | grep -i "error\|notice.*tipo_lancamento\|notice.*valorunitario" | head -20
```

---

## ✅ Checklist de Verificação

Após atualizar qualquer ambiente, verifique:

### 1. Banco de Dados

```bash
# Verificar coluna tipo_lancamento
sudo -u postgres psql -d singleone -c "
SELECT column_name, data_type, column_default 
FROM information_schema.columns 
WHERE table_name = 'notasfiscais' 
AND column_name = 'tipo_lancamento';
"

# Deve retornar: tipo_lancamento | character varying | 'nota_fiscal'::character varying

# Verificar se valorunitario é nullable
sudo -u postgres psql -d singleone -c "
SELECT is_nullable 
FROM information_schema.columns 
WHERE table_name = 'notasfiscaisitens' 
AND column_name = 'valorunitario';
"

# Deve retornar: YES
```

### 2. Backend

```bash
# Verificar se o serviço está rodando
systemctl status singleone-api

# Verificar logs (sem erros)
journalctl -u singleone-api -n 30 --no-pager | grep -i error
```

### 3. Frontend

```bash
# Verificar se o build foi criado
ls -lh /opt/SingleOne/SingleOne_Frontend/dist/SingleOne/index.html

# Verificar se o código foi atualizado
grep -n "tipoLancamento" /opt/SingleOne/SingleOne_Frontend/src/app/pages/cadastros/notasFiscais/nota-fiscal/nota-fiscal.component.ts
```

### 4. Teste no Navegador

1. Acessar: `/notas-fiscais`
2. Clicar em "Nova Nota Fiscal"
3. ✅ Deve aparecer campo "Tipo de Lançamento" no Passo 1
4. ✅ Deve ter opções: "Nota Fiscal" e "Inventário (sem nota fiscal)"
5. ✅ Ao selecionar "Inventário", deve permitir valor zero

---

## 📊 Resumo por Ambiente

| Ambiente | Status | Ação Necessária |
|----------|--------|-----------------|
| **Novos Clientes** | ✅ Automático | Nenhuma - já está no script de instalação |
| **Demo** | ⚠️ Manual | Executar migração SQL + recompilar |
| **FitBank** | ✅ Atualizado | Já foi feito |
| **Outros Clientes Existentes** | ⚠️ Manual | Executar migração SQL + recompilar |

---

## 🚀 Script Rápido para Atualizar Qualquer Cliente

```bash
#!/bin/bash
# Script para atualizar cliente existente com suporte a inventário

echo "=== Atualizando Suporte a Inventário em Notas Fiscais ==="

# 1. Atualizar código
echo "[1/4] Atualizando código..."
cd /opt/SingleOne/SingleOne_Backend && git pull origin main
cd /opt/SingleOne/SingleOne_Frontend && git pull origin main

# 2. Migração SQL
echo "[2/4] Executando migração SQL..."
cd /opt/SingleOne/SingleOne_Backend
sudo -u postgres psql -d singleone -f scripts/migrar_suporte_inventario_notasfiscais.sql

# 3. Backend
echo "[3/4] Recompilando backend..."
systemctl stop singleone-api
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI
dotnet publish -c Release -o /opt/singleone-api-publish
systemctl start singleone-api

# 4. Frontend
echo "[4/4] Recompilando frontend..."
cd /opt/SingleOne/SingleOne_Frontend
export NODE_OPTIONS=--openssl-legacy-provider
npm run build -- --configuration production
systemctl reload nginx

echo "✅ Atualização concluída!"
```

Salve como `atualizar_inventario.sh` e execute:
```bash
chmod +x atualizar_inventario.sh
sudo ./atualizar_inventario.sh
```

---

## 📝 Notas Importantes

1. **Novos Clientes**: Não precisa fazer nada! O script de instalação já inclui tudo.

2. **Clientes Existentes**: Precisam executar a migração SQL uma vez.

3. **Compatibilidade**: Todas as notas fiscais existentes serão automaticamente marcadas como `tipo_lancamento = 'nota_fiscal'`.

4. **Sem Quebra**: A funcionalidade existente continua funcionando normalmente.

---

**Data**: 2025-01-19  
**Versão**: 1.0  
**Status**: ✅ Pronto para produção
