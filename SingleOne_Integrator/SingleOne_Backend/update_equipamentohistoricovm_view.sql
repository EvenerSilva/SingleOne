-- Script para atualizar a view equipamentohistoricovm
-- Adiciona campos do responsável provisório (tecnicoresponsavel)

-- Primeiro, vamos verificar se a view existe e fazer backup
-- DROP VIEW IF EXISTS equipamentohistoricovm_backup;
-- CREATE VIEW equipamentohistoricovm_backup AS SELECT * FROM equipamentohistoricovm;

-- Recriar a view com os novos campos
DROP VIEW IF EXISTS equipamentohistoricovm;

CREATE VIEW equipamentohistoricovm AS
SELECT 
    eh.id,
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
LEFT JOIN equipamento e ON eh.equipamento = e.id
LEFT JOIN tipoequipamento te ON e.tipoequipamento = te.id
LEFT JOIN fabricante f ON e.fabricante = f.id
LEFT JOIN modelo m ON e.modelo = m.id
LEFT JOIN equipamentosstatus es ON eh.equipamentostatus = es.id
LEFT JOIN colaborador c ON eh.colaborador = c.id
LEFT JOIN usuario u ON eh.usuario = u.id
-- ✅ NOVO JOIN: Buscar dados da requisição para obter o responsável provisório
LEFT JOIN requisicao r ON eh.requisicao = r.id
LEFT JOIN usuario tr ON r.tecnicoresponsavel = tr.id
ORDER BY eh.dtregistro DESC;

-- Comentários sobre a modificação:
-- 1. Adicionado JOIN com a tabela 'requisicao' para acessar o campo 'tecnicoresponsavel'
-- 2. Adicionado JOIN com a tabela 'usuario' (alias 'tr') para obter o nome do responsável provisório
-- 3. Adicionados campos 'tecnicoresponsavelid' e 'tecnicoresponsavel' na view
-- 4. Mantida compatibilidade com campos existentes
