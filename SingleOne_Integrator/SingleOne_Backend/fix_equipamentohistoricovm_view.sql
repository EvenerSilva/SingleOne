-- Script para corrigir a view equipamentohistoricovm
-- Adiciona campos do responsável provisório (tecnicoresponsavel)

-- Primeiro, vamos verificar se a view existe e fazer backup
DO $$
BEGIN
    -- Verificar se a view existe
    IF EXISTS (SELECT 1 FROM information_schema.views WHERE table_name = 'equipamentohistoricovm') THEN
        -- Fazer backup da view atual
        EXECUTE 'CREATE OR REPLACE VIEW equipamentohistoricovm_backup AS SELECT * FROM equipamentohistoricovm';
        RAISE NOTICE 'Backup da view criado: equipamentohistoricovm_backup';
    END IF;
END $$;

-- Recriar a view com os novos campos
DROP VIEW IF EXISTS equipamentohistoricovm CASCADE;

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

-- Comentários sobre a modificação:
-- 1. Adicionado JOIN com a tabela 'requisicao' para acessar o campo 'tecnicoresponsavel'
-- 2. Adicionado JOIN com a tabela 'usuario' (alias 'tr') para obter o nome do responsável provisório
-- 3. Adicionados campos 'tecnicoresponsavelid' e 'tecnicoresponsavel' na view
-- 4. Mantida compatibilidade com campos existentes
-- 5. Usado CASCADE no DROP para remover dependências

-- Verificar se a view foi criada corretamente
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.views WHERE table_name = 'equipamentohistoricovm') THEN
        RAISE NOTICE 'View equipamentohistoricovm criada com sucesso!';
    ELSE
        RAISE EXCEPTION 'Erro ao criar a view equipamentohistoricovm';
    END IF;
END $$;
