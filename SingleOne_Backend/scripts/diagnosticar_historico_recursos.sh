#!/bin/bash

echo "=========================================="
echo "🔍 DIAGNÓSTICO: Histórico de Recursos"
echo "=========================================="
echo ""

# 1. Verificar se a view equipamentohistoricovm existe
echo "📋 [1/5] Verificando view equipamentohistoricovm..."
sudo -u postgres psql -d singleone -c "\d+ vwequipamentohistorico" 2>&1 | head -20
echo ""

# 2. Verificar tabelas relacionadas a histórico
echo "📋 [2/5] Listando tabelas/views com 'historico'..."
sudo -u postgres psql -d singleone -c "\dt *historico*" 2>&1
sudo -u postgres psql -d singleone -c "\dv *historico*" 2>&1
echo ""

# 3. Testar query direta: buscar histórico de um equipamento de teste
echo "📋 [3/5] Testando query de histórico (sample)..."
echo "SELECT * FROM vwequipamentohistorico LIMIT 5;" | sudo -u postgres psql -d singleone 2>&1
echo ""

# 4. Verificar se equipamentos de NF têm histórico
echo "📋 [4/5] Verificando equipamentos de notas fiscais..."
echo "
SELECT 
    e.id as equipamento_id,
    e.numeroserie,
    e.patrimonio,
    e.tipoequipamento as tipo_id,
    e.notafiscal,
    COUNT(eh.id) as qtd_historico
FROM equipamentos e
LEFT JOIN equipamentohistorico eh ON e.id = eh.equipamentoid
WHERE e.notafiscal IS NOT NULL
GROUP BY e.id, e.numeroserie, e.patrimonio, e.tipoequipamento, e.notafiscal
ORDER BY qtd_historico ASC
LIMIT 10;
" | sudo -u postgres psql -d singleone 2>&1
echo ""

# 5. Verificar estrutura da view
echo "📋 [5/5] Verificando definição da view..."
echo "SELECT pg_get_viewdef('vwequipamentohistorico'::regclass, true);" | sudo -u postgres psql -d singleone 2>&1
echo ""

echo "=========================================="
echo "📊 ANÁLISE E RECOMENDAÇÕES"
echo "=========================================="
echo ""
echo "✅ Próximos passos:"
echo "   1. Verificar se a view vwequipamentohistorico existe"
echo "   2. Se não existir, precisa ser criada"
echo "   3. Verificar se equipamentos de NF têm registros na tabela equipamentohistorico"
echo "   4. Se não tiverem, precisa criar registros iniciais ao cadastrar NF"
echo ""

