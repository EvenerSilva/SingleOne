# 📋 Instruções de Deploy - Suporte a Inventário em Notas Fiscais

## 🎯 O que foi implementado

Adicionado suporte para cadastrar notas fiscais como "Inventário" (sem valor obrigatório), permitindo registrar equipamentos quando o cliente não possui mais a nota fiscal original.

---

## 📦 Passo 1: Atualizar Código no Servidor

### 1.1. Conectar no servidor do FitBank
```bash
ssh root@vmi2972093  # ou o IP do servidor
```

### 1.2. Atualizar código do repositório
```bash
cd /opt/SingleOne/SingleOne_Backend
git pull origin main
```

### 1.3. Atualizar código do frontend
```bash
cd /opt/SingleOne/SingleOne_Frontend
git pull origin main
```

---

## 🗄️ Passo 2: Executar Migração do Banco de Dados

### 2.1. Executar script de migração SQL

```bash
cd /opt/SingleOne/SingleOne_Backend
sudo -u postgres psql -d singleone -f scripts/migrar_suporte_inventario_notasfiscais.sql
```

**OU** se preferir executar manualmente:

```bash
sudo -u postgres psql -d singleone << 'EOF'
-- Adicionar coluna tipo_lancamento
ALTER TABLE notasfiscais ADD COLUMN IF NOT EXISTS tipo_lancamento VARCHAR(20) DEFAULT 'nota_fiscal';

-- Atualizar registros existentes
UPDATE notasfiscais SET tipo_lancamento = 'nota_fiscal' WHERE tipo_lancamento IS NULL OR tipo_lancamento = '';

-- Tornar valorunitario nullable
ALTER TABLE notasfiscaisitens ALTER COLUMN valorunitario DROP NOT NULL;

-- Adicionar constraint de validação
ALTER TABLE notasfiscais ADD CONSTRAINT ck_notasfiscais_tipo_lancamento 
CHECK (tipo_lancamento IN ('nota_fiscal', 'inventario'));
EOF
```

### 2.2. Verificar se a migração foi aplicada

```bash
sudo -u postgres psql -d singleone -c "
SELECT 
    'notasfiscais' AS tabela,
    'tipo_lancamento' AS coluna,
    EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'notasfiscais' 
        AND column_name = 'tipo_lancamento'
    ) AS existe;
"
```

Deve retornar: `existe = true`

```bash
sudo -u postgres psql -d singleone -c "
SELECT 
    'notasfiscaisitens' AS tabela,
    'valorunitario' AS coluna,
    EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'notasfiscaisitens' 
        AND column_name = 'valorunitario'
        AND is_nullable = 'YES'
    ) AS nullable;
"
```

Deve retornar: `nullable = true`

---

## 🔧 Passo 3: Recompilar e Publicar Backend

### 3.1. Publicar API
```bash
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI
dotnet publish -c Release -o /opt/singleone-api-publish
```

### 3.2. Reiniciar serviço da API
```bash
systemctl restart singleone-api
```

### 3.3. Verificar se o serviço está rodando
```bash
systemctl status singleone-api
```

### 3.4. Verificar logs (opcional)
```bash
journalctl -u singleone-api -n 50 --no-pager
```

---

## 🎨 Passo 4: Recompilar Frontend

### 4.1. Instalar dependências (se necessário)
```bash
cd /opt/SingleOne/SingleOne_Frontend
npm install --legacy-peer-deps
```

### 4.2. Fazer build do frontend
```bash
export NODE_OPTIONS=--openssl-legacy-provider
npm run build -- --configuration production
```

### 4.3. Verificar se o build foi bem-sucedido
```bash
ls -lh dist/SingleOne/
```

### 4.4. Reiniciar Nginx (se necessário)
```bash
systemctl reload nginx
```

---

## ✅ Passo 5: Verificação e Testes

### 5.1. Testar cadastro de Nota Fiscal normal
1. Acessar: `https://fitbank.singleone.com.br/notas-fiscais`
2. Clicar em "Nova Nota Fiscal"
3. Preencher:
   - Fornecedor: (qualquer)
   - Número: (qualquer)
   - Data de Emissão: (qualquer)
   - **Tipo de Lançamento: "Nota Fiscal"** (padrão)
4. Adicionar item:
   - Tipo de Recurso, Fabricante, Modelo, Quantidade
   - **Valor Unitário: R$ 100,00** (obrigatório e > 0)
5. Salvar
6. ✅ **Esperado**: Deve salvar normalmente

### 5.2. Testar cadastro de Inventário
1. Acessar: `https://fitbank.singleone.com.br/notas-fiscais`
2. Clicar em "Nova Nota Fiscal"
3. Preencher:
   - Fornecedor: (qualquer)
   - Número: (qualquer - pode ser "INV-001" ou similar)
   - Data de Emissão: (qualquer)
   - **Tipo de Lançamento: "Inventário (sem nota fiscal)"**
4. Adicionar item:
   - Tipo de Recurso, Fabricante, Modelo, Quantidade
   - **Valor Unitário: R$ 0,00** (ou deixar vazio)
5. Salvar
6. ✅ **Esperado**: Deve salvar normalmente (sem erro de valor zero)

### 5.3. Verificar no banco de dados
```bash
sudo -u postgres psql -d singleone -c "
SELECT 
    id,
    numero,
    tipo_lancamento,
    (SELECT COUNT(*) FROM notasfiscaisitens WHERE notafiscal = nf.id) AS qtd_itens
FROM notasfiscais nf
ORDER BY id DESC
LIMIT 5;
"
```

---

## 🐛 Troubleshooting

### Erro: "Coluna tipo_lancamento não existe"
**Solução**: Executar novamente o script de migração SQL (Passo 2.1)

### Erro: "Valor unitário deve ser maior que zero" ao salvar inventário
**Solução**: Verificar se o frontend foi recompilado corretamente (Passo 4)

### Erro: API não inicia após restart
**Solução**: Verificar logs:
```bash
journalctl -u singleone-api -n 100 --no-pager
```

### Erro: Frontend não carrega
**Solução**: Verificar se o build foi bem-sucedido e se o Nginx está servindo os arquivos corretos:
```bash
ls -lh /opt/SingleOne/SingleOne_Frontend/dist/SingleOne/
nginx -t
systemctl status nginx
```

---

## 📝 Resumo dos Comandos (Copy & Paste)

```bash
# 1. Atualizar código
cd /opt/SingleOne/SingleOne_Backend && git pull origin main
cd /opt/SingleOne/SingleOne_Frontend && git pull origin main

# 2. Executar migração SQL
cd /opt/SingleOne/SingleOne_Backend
sudo -u postgres psql -d singleone -f scripts/migrar_suporte_inventario_notasfiscais.sql

# 3. Publicar e reiniciar backend
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI
dotnet publish -c Release -o /opt/singleone-api-publish
systemctl restart singleone-api
systemctl status singleone-api

# 4. Build e reload frontend
cd /opt/SingleOne/SingleOne_Frontend
export NODE_OPTIONS=--openssl-legacy-provider
npm run build -- --configuration production
systemctl reload nginx
```

---

## ✅ Checklist de Deploy

- [ ] Código atualizado no servidor (git pull)
- [ ] Migração SQL executada com sucesso
- [ ] Backend recompilado e serviço reiniciado
- [ ] Frontend recompilado
- [ ] Teste de Nota Fiscal normal funcionando
- [ ] Teste de Inventário funcionando (valor zero permitido)
- [ ] Verificação no banco de dados confirmada

---

**Data da Implementação**: 2025-01-02  
**Versão**: 1.0  
**Status**: ✅ Pronto para deploy
