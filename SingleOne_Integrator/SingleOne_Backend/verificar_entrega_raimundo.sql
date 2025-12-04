-- =====================================================
-- VERIFICAÇÃO DE ENTREGA PARA RAIMUNDO NONATO SILVA
-- =====================================================

-- 1. BUSCAR COLABORADOR RAIMUNDO NONATO SILVA
-- =====================================================
SELECT 
    id, 
    nome, 
    cpf, 
    matricula, 
    cliente,
    empresa
FROM colaboradores 
WHERE nome ILIKE '%raimundo%nonato%silva%' 
   OR nome ILIKE '%raimundo%nonato%'
   OR nome ILIKE '%raimundo%silva%'
   OR nome ILIKE '%nonato%silva%';

-- 2. VERIFICAR REQUISIÇÕES DO COLABORADOR
-- =====================================================
SELECT 
    r.id as requisicao_id,
    r.requisicaostatus,
    r.colaboradorfinal,
    c.nome as colaborador_nome,
    r.dtsolicitacao,
    r.dtprocessamento,
    r.dtenviotermo
FROM requisicoes r
JOIN colaboradores c ON r.colaboradorfinal = c.id
WHERE c.nome ILIKE '%raimundo%nonato%silva%' 
   OR c.nome ILIKE '%raimundo%nonato%'
   OR c.nome ILIKE '%raimundo%silva%'
   OR c.nome ILIKE '%nonato%silva%'
ORDER BY r.dtsolicitacao DESC;

-- 3. VERIFICAR ITENS DE REQUISIÇÃO (EQUIPAMENTOS E LINHAS)
-- =====================================================
SELECT 
    ri.id as item_id,
    ri.requisicao,
    ri.equipamento,
    ri.linhatelefonica,
    ri.dtentrega,
    ri.dtdevolucao,
    ri.observacaoentrega,
    r.requisicaostatus,
    c.nome as colaborador_nome
FROM requisicoesitens ri
JOIN requisicoes r ON ri.requisicao = r.id
JOIN colaboradores c ON r.colaboradorfinal = c.id
WHERE (c.nome ILIKE '%raimundo%nonato%silva%' 
   OR c.nome ILIKE '%raimundo%nonato%'
   OR c.nome ILIKE '%raimundo%silva%'
   OR c.nome ILIKE '%nonato%silva%')
  AND ri.dtentrega IS NOT NULL
  AND ri.dtdevolucao IS NULL
ORDER BY ri.dtentrega DESC;

-- 4. VERIFICAR ESPECIFICAMENTE LINHAS TELEFÔNICAS
-- =====================================================
SELECT 
    ri.id as item_id,
    ri.requisicao,
    ri.linhatelefonica,
    ri.dtentrega,
    ri.dtdevolucao,
    tl.numero as numero_linha,
    tl.iccid,
    r.requisicaostatus,
    c.nome as colaborador_nome
FROM requisicoesitens ri
JOIN requisicoes r ON ri.requisicao = r.id
JOIN colaboradores c ON r.colaboradorfinal = c.id
LEFT JOIN telefonialinhas tl ON ri.linhatelefonica = tl.id
WHERE (c.nome ILIKE '%raimundo%nonato%silva%' 
   OR c.nome ILIKE '%raimundo%nonato%'
   OR c.nome ILIKE '%raimundo%silva%'
   OR c.nome ILIKE '%nonato%silva%')
  AND ri.linhatelefonica IS NOT NULL
  AND ri.linhatelefonica > 0
  AND ri.dtentrega IS NOT NULL
  AND ri.dtdevolucao IS NULL
ORDER BY ri.dtentrega DESC;

-- 5. VERIFICAR HISTÓRICO DE EQUIPAMENTOS (INCLUINDO LINHAS)
-- =====================================================
SELECT 
    eh.id,
    eh.equipamento,
    eh.linhatelefonica,
    eh.equipamentostatus,
    eh.colaborador,
    eh.requisicao,
    eh.dtregistro,
    c.nome as colaborador_nome,
    tl.numero as numero_linha
FROM equipamentohistorico eh
JOIN colaboradores c ON eh.colaborador = c.id
LEFT JOIN telefonialinhas tl ON eh.linhatelefonica = tl.id
WHERE (c.nome ILIKE '%raimundo%nonato%silva%' 
   OR c.nome ILIKE '%raimundo%nonato%'
   OR c.nome ILIKE '%raimundo%silva%'
   OR c.nome ILIKE '%nonato%silva%')
  AND eh.equipamentostatus = 4 -- Entregue
ORDER BY eh.dtregistro DESC;

-- 6. VERIFICAR STATUS DAS LINHAS TELEFÔNICAS
-- =====================================================
SELECT 
    tl.id,
    tl.numero,
    tl.iccid,
    tl.emuso,
    tl.ativo,
    tp.nome as plano_nome,
    tc.nome as contrato_nome,
    toper.nome as operadora_nome
FROM telefonialinhas tl
LEFT JOIN telefoniaplanos tp ON tl.plano = tp.id
LEFT JOIN telefoniacontratos tc ON tp.contrato = tc.id
LEFT JOIN telefoniaoperadoras toper ON tc.operadora = toper.id
WHERE tl.id IN (
    SELECT DISTINCT ri.linhatelefonica
    FROM requisicoesitens ri
    JOIN requisicoes r ON ri.requisicao = r.id
    JOIN colaboradores c ON r.colaboradorfinal = c.id
    WHERE (c.nome ILIKE '%raimundo%nonato%silva%' 
       OR c.nome ILIKE '%raimundo%nonato%'
       OR c.nome ILIKE '%raimundo%silva%'
       OR c.nome ILIKE '%nonato%silva%')
      AND ri.linhatelefonica IS NOT NULL
      AND ri.linhatelefonica > 0
)
ORDER BY tl.numero;

-- 7. VERIFICAR VIEW vwUltimasRequisicaoNaoBYOD
-- =====================================================
SELECT 
    requisicaoid,
    cliente,
    colaboradorfinal,
    nomecolaboradorfinal,
    requisicaostatus,
    equipamentostatus,
    linhatelefonica,
    numero,
    dtentrega,
    dtdevolucao
FROM vwUltimasRequisicaoNaoBYOD
WHERE nomecolaboradorfinal ILIKE '%raimundo%nonato%silva%' 
   OR nomecolaboradorfinal ILIKE '%raimundo%nonato%'
   OR nomecolaboradorfinal ILIKE '%raimundo%silva%'
   OR nomecolaboradorfinal ILIKE '%nonato%silva%'
ORDER BY dtentrega DESC;
