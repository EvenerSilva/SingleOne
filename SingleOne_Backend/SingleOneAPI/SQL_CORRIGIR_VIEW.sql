-- ========================================
-- INVESTIGAR E CORRIGIR A VIEW PLANOSVM
-- ========================================

-- 1. VER A DEFINIÇÃO ATUAL DA VIEW
SELECT pg_get_viewdef('planosvm'::regclass, true);

-- 2. CRIAR BACKUP DA VIEW ANTIGA
DROP VIEW IF EXISTS planosvm_backup CASCADE;
CREATE VIEW planosvm_backup AS SELECT * FROM planosvm;

-- 3. RECRIAR A VIEW COM A QUERY CORRETA
DROP VIEW IF EXISTS planosvm CASCADE;

CREATE OR REPLACE VIEW planosvm AS
SELECT 
    tp.id,
    tp.nome AS plano,
    tp.ativo,
    tp.valor,
    tc.nome AS contrato,
    tc.id AS contratoid,
    tope.nome AS operadora,
    tope.id AS operadoraid,
    
    -- ✅ TOTAL DE LINHAS (independente de emuso)
    COUNT(tl.id) AS contlinhas,
    
    -- ✅ LINHAS EM USO (emuso = true)
    COUNT(CASE WHEN tl.emuso = true THEN 1 END) AS contlinhasemuso,
    
    -- ✅ LINHAS LIVRES (emuso = false)
    COUNT(CASE WHEN tl.emuso = false THEN 1 END) AS contlinhaslivres
    
FROM telefoniaplanos tp
INNER JOIN telefoniacontratos tc ON tp.contrato = tc.id
INNER JOIN telefoniaoperadoras tope ON tc.operadora = tope.id
LEFT JOIN telefonialinhas tl ON tl.plano = tp.id AND tl.ativo = true
GROUP BY 
    tp.id, 
    tp.nome, 
    tp.ativo, 
    tp.valor, 
    tc.nome, 
    tc.id, 
    tope.nome, 
    tope.id;

-- 4. VERIFICAR SE A VIEW FOI CRIADA
SELECT * FROM planosvm LIMIT 5;

-- 5. VERIFICAR CONTAGENS
SELECT 
    plano,
    contlinhas,
    contlinhasemuso,
    contlinhaslivres
FROM planosvm
ORDER BY contlinhas DESC;

