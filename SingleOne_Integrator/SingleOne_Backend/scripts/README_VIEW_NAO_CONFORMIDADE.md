# 📊 View: vw_nao_conformidade_elegibilidade

## 🎯 Propósito

Esta view identifica **não conformidades de elegibilidade**, ou seja, casos onde colaboradores possuem equipamentos mas não são elegíveis conforme as políticas definidas no sistema.

## 🔍 O que a View Faz

A view analisa três aspectos principais:

1. **Equipamentos Alocados**: Busca todos os equipamentos atualmente em posse de colaboradores
2. **Políticas de Elegibilidade**: Verifica se existe política definida para aquela combinação (Tipo Colaborador + Tipo Equipamento)
3. **Não Conformidades**: Identifica casos onde:
   - ❌ Política **NEGA** acesso ao equipamento
   - ❌ Quantidade de equipamentos **EXCEDE** o máximo permitido

## 📋 Estrutura da View

### Campos Retornados:

- **Colaborador**: id, nome, cpf, email, cargo, tipo
- **Empresa/Localização**: empresa, centro de custo, localidade
- **Equipamento**: id, patrimônio, série, tipo, categoria, fabricante, modelo, status
- **Política**: id da política, permite acesso, quantidade máxima, observações
- **Contagem**: quantidade atual de equipamentos do tipo
- **Metadata**: data/hora de geração do relatório

### Lógica de Não Conformidade:

```sql
WHERE 
    -- Política que nega acesso
    (politica_id IS NOT NULL AND permite_acesso = false)
    -- OU
    -- Política permite mas excedeu quantidade
    OR (politica_id IS NOT NULL 
        AND permite_acesso = true 
        AND quantidade_maxima IS NOT NULL 
        AND quantidade_atual > quantidade_maxima)
```

## 🚀 Como Executar

### Opção 1: Via psql (Terminal)
```bash
psql -h localhost -U seu_usuario -d singleone -f vw_nao_conformidade_elegibilidade.sql
```

### Opção 2: Via pgAdmin
1. Abra o pgAdmin
2. Conecte ao banco de dados `singleone`
3. Abra o Query Tool (Tools > Query Tool)
4. Abra o arquivo `vw_nao_conformidade_elegibilidade.sql`
5. Execute (F5 ou clique em Execute)

### Opção 3: Via DBeaver / Outro Cliente SQL
1. Conecte ao banco de dados
2. Abra o arquivo SQL
3. Execute o script

## ✅ Verificação

Após criar a view, teste se está funcionando:

```sql
-- Ver estrutura da view
\d+ vw_nao_conformidade_elegibilidade

-- Testar consulta simples
SELECT COUNT(*) FROM vw_nao_conformidade_elegibilidade;

-- Ver primeiros resultados
SELECT * FROM vw_nao_conformidade_elegibilidade LIMIT 10;
```

## 📊 Exemplos de Uso

### Buscar todas as não conformidades de um cliente:
```sql
SELECT * FROM vw_nao_conformidade_elegibilidade
WHERE cliente = 1;
```

### Contar não conformidades por tipo de colaborador:
```sql
SELECT 
    tipo_colaborador_descricao,
    COUNT(*) as total
FROM vw_nao_conformidade_elegibilidade
GROUP BY tipo_colaborador_descricao;
```

### Buscar não conformidades de um colaborador específico:
```sql
SELECT * FROM vw_nao_conformidade_elegibilidade
WHERE LOWER(colaborador_nome) LIKE '%nome%';
```

## 🔄 Atualização

A view é atualizada **automaticamente** toda vez que é consultada, refletindo o estado atual:
- Equipamentos alocados (sem devolução)
- Políticas ativas
- Colaboradores ativos (não demitidos)

## ⚠️ Performance

- A view faz JOINs em várias tabelas
- Para bases grandes (>100k equipamentos), considere criar índices:

```sql
-- Índices recomendados (se ainda não existirem)
CREATE INDEX IF NOT EXISTS idx_requisicoesitens_colaborador ON requisicoesitens(colaborador) WHERE dtdevolucao IS NULL;
CREATE INDEX IF NOT EXISTS idx_requisicoesitens_equipamento ON requisicoesitens(equipamento) WHERE dtdevolucao IS NULL;
CREATE INDEX IF NOT EXISTS idx_politicas_tipo_equip ON politicaselegibilidade(tipocolaborador, tipoequipamentoid) WHERE ativo = true;
```

## 🐛 Troubleshooting

### Erro: "relação vw_nao_conformidade_elegibilidade não existe"
✅ Execute o script SQL para criar a view

### Erro: "permissão negada"
✅ Execute com um usuário que tenha permissão CREATE VIEW no banco

### View retorna vazio mas existem equipamentos alocados
✅ Verifique se há políticas de elegibilidade cadastradas que NEGAM acesso ou limitam quantidade

## 📚 Dependências

A view depende das seguintes tabelas:
- `colaboradores`
- `requisicoesitens`
- `equipamentos`
- `tiposequipamento`
- `politicaselegibilidade`
- `empresas`
- `centroscusto`
- `localidades`

## 📝 Changelog

- **v1.0** (2025-10-17): Versão inicial com suporte a:
  - Detecção de acesso negado
  - Detecção de quantidade excedida
  - Filtro por cargo (exato e padrão)
  - Apenas colaboradores ativos
  - Apenas equipamentos não devolvidos

