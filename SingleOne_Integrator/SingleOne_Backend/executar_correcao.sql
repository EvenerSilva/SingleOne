-- Script para corrigir a view equipamentohistoricovm
-- 1. Fazer backup
CREATE VIEW IF NOT EXISTS equipamentohistoricovm_backup AS 
SELECT * FROM equipamentohistoricovm;

-- 2. Remover view atual
DROP VIEW IF EXISTS equipamentohistoricovm CASCADE;

-- 3. Recriar view com campos corretos
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
LEFT JOIN requisicoes r ON eh.requisicao = r.id
LEFT JOIN usuarios tr ON r.tecnicoresponsavel = tr.id
ORDER BY eh.dtregistro DESC;

-- 4. Verificar se funcionou
SELECT 'View criada com sucesso!' as status;

-- 5. Testar com equipamento 1699
SELECT 
    id,
    equipamentoid,
    tipoequipamento,
    fabricante,
    modelo,
    numeroserie,
    equipamentostatus,
    colaborador,
    usuario,
    tecnicoresponsavel,
    dtregistro
FROM equipamentohistoricovm 
WHERE equipamentoid = 1699
ORDER BY dtregistro DESC
LIMIT 3;
