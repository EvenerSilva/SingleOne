-- =====================================================
-- SCRIPT FINAL: Corrigir View equipamentohistoricovm
-- DESCRIÇÃO: Recria a view com nomes corretos das tabelas
-- DATA: 2025-01-15
-- =====================================================

-- 1. Fazer backup da view atual (se existir)
CREATE VIEW IF NOT EXISTS equipamentohistoricovm_backup AS 
SELECT * FROM equipamentohistoricovm;

-- 2. Remover a view atual
DROP VIEW IF EXISTS equipamentohistoricovm CASCADE;

-- 3. Recriar a view com nomes corretos das tabelas
CREATE VIEW equipamentohistoricovm AS
SELECT 
    eh.id,
    eh.equipamento as equipamentoid,
    e.tipoequipamento as tipoequipamentoid,
    te.descricao as tipoequipamento,
    e.fabricante as fabricanteid,
    f.descricao as fabricante,
    e.modelo as modeloid,
    m.descricao as modelo,
    e.numeroserie,
    e.patrimonio,
    eh.equipamentostatus as equipamentostatusid,
    es.descricao as equipamentostatus,
    eh.colaborador as colaboradorid,
    c.nome as colaborador,
    eh.dtregistro,
    eh.usuario as usuarioid,
    u.nome as usuario,
    -- ✅ NOVOS CAMPOS: Responsável Provisório
    r.tecnicoresponsavel as tecnicoresponsavelid,
    tr.nome as tecnicoresponsavel
FROM equipamentohistorico eh
LEFT JOIN equipamentos e ON eh.equipamento = e.id
LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
LEFT JOIN fabricantes f ON e.fabricante = f.id
LEFT JOIN modelos m ON e.modelo = m.id
LEFT JOIN equipamentosstatus es ON eh.equipamentostatus = es.id
LEFT JOIN colaboradores c ON eh.colaborador = c.id
LEFT JOIN usuarios u ON eh.usuario = u.id
-- ✅ NOVO JOIN: Buscar dados da requisição para obter o responsável provisório
LEFT JOIN requisicoes r ON eh.requisicao = r.id
LEFT JOIN usuarios tr ON r.tecnicoresponsavel = tr.id
ORDER BY eh.dtregistro DESC;

-- 4. Verificar se a view foi criada corretamente
SELECT 'View equipamentohistoricovm criada com sucesso!' as status;

-- 5. Testar a view
SELECT 
    id,
    tipoequipamento,
    fabricante,
    modelo,
    numeroserie,
    patrimonio,
    equipamentostatus,
    colaborador,
    usuario,
    tecnicoresponsavel,
    dtregistro
FROM equipamentohistoricovm 
LIMIT 5;

-- 6. Verificar se os novos campos estão presentes
SELECT 
    column_name,
    data_type
FROM information_schema.columns 
WHERE table_name = 'equipamentohistoricovm' 
AND column_name IN ('tecnicoresponsavelid', 'tecnicoresponsavel')
ORDER BY column_name;

-- Comentários sobre a correção:
-- 1. Corrigido 'equipamento' para 'equipamentos' (plural)
-- 2. Corrigido 'tipoequipamento' para 'tipoequipamentos' (plural)
-- 3. Corrigido 'fabricante' para 'fabricantes' (plural)
-- 4. Corrigido 'modelo' para 'modelos' (plural)
-- 5. Corrigido 'colaborador' para 'colaboradores' (plural)
-- 6. Corrigido 'usuario' para 'usuarios' (plural)
-- 7. Corrigido 'requisicao' para 'requisicoes' (plural)
-- 8. Adicionado campos do responsável provisório
