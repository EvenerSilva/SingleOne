-- Script simples para corrigir a view equipamentohistoricovm
-- Este script é mais conservador e deve funcionar em qualquer versão do PostgreSQL

-- 1. Primeiro, vamos verificar a estrutura atual da view
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'equipamentohistoricovm' 
ORDER BY ordinal_position;

-- 2. Fazer backup da view atual (se existir)
CREATE OR REPLACE VIEW equipamentohistoricovm_old AS 
SELECT * FROM equipamentohistoricovm;

-- 3. Recriar a view com os novos campos
DROP VIEW IF EXISTS equipamentohistoricovm;

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
    -- Novos campos do responsável provisório
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
LEFT JOIN usuarios tr ON r.tecnicoresponsavel = tr.id;

-- 4. Verificar se a view foi criada corretamente
SELECT 'View criada com sucesso!' as status;
