-- ========================================
-- CORREÇÃO: Adicionar coluna equipamentoid na view equipamentohistoricovm
-- ========================================
-- Problema: A view não retorna equipamentoid, causando erro 500 na API
-- Solução: Recriar a view incluindo e.id AS equipamentoid

-- Dropar view existente
DROP VIEW IF EXISTS equipamentohistoricovm CASCADE;

-- Recriar view com equipamentoid
CREATE OR REPLACE VIEW equipamentohistoricovm AS
SELECT 
    eh.id,
    e.id AS equipamentoid,  -- ✅ ADICIONADO: equipamentoid para filtrar por equipamento
    te.id AS tipoequipamentoid,
    te.descricao AS tipoequipamento,
    f.id AS fabricanteid,
    f.descricao AS fabricante,
    m.id AS modeloid,
    m.descricao AS modelo,
    e.numeroserie,
    e.patrimonio,
    es.id AS equipamentostatusid,
    es.descricao AS equipamentostatus,
    c.id AS colaboradorid,
    c.nome AS colaborador,
    eh.dtregistro,
    u.id AS usuarioid,
    u.nome AS usuario,
    eh.requisicao AS requisicaoid,  -- ✅ ADICIONADO: para rastreabilidade
    NULL::integer AS tecnicoresponsavelid,  -- ✅ ADICIONADO: compatibilidade com model C#
    NULL::text AS tecnicoresponsavel  -- ✅ ADICIONADO: compatibilidade com model C#
FROM equipamentohistorico eh
    JOIN equipamentos e ON eh.equipamento = e.id
    JOIN usuarios u ON eh.usuario = u.id
    JOIN equipamentosstatus es ON eh.equipamentostatus = es.id
    JOIN fabricantes f ON e.fabricante = f.id
    JOIN modelos m ON e.modelo = m.id
    JOIN tipoequipamentos te ON e.tipoequipamento = te.id
    LEFT JOIN colaboradores c ON eh.colaborador = c.id;

-- Verificar se a view foi criada corretamente
SELECT COUNT(*) as total_registros FROM equipamentohistoricovm;

-- Testar com um equipamento específico
SELECT * FROM equipamentohistoricovm 
WHERE numeroserie = 'AUT-Y1W28V1QO7' 
LIMIT 5;

-- ========================================
-- RESULTADO ESPERADO:
-- - View recriada com coluna equipamentoid
-- - API deve funcionar sem erro 500
-- ========================================

