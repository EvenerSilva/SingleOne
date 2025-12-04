-- =====================================================
-- CORREÇÃO DO SISTEMA DE LOCALIDADES
-- =====================================================

-- 1. VERIFICAR ESTADO DAS CIDADES DO CEARÁ
-- =====================================================

SELECT 
    'CIDADES DO CEARÁ' as info,
    id,
    nome,
    estado_id
FROM cidades 
WHERE estado_id = 6 
ORDER BY nome;

-- 2. CORRIGIR LOCALIDADES INCORRETAS
-- =====================================================

-- Corrigir Aquiraz (deve ser estado 6, cidade 151)
UPDATE localidades 
SET estado = '6', cidade = '151' 
WHERE id = 5 AND descricao = 'Aquiraz';

-- Corrigir Caucaia (deve ser estado 6, cidade 151) 
UPDATE localidades 
SET estado = '6', cidade = '151' 
WHERE id = 6 AND descricao = 'Caucaia';

-- Corrigir Fortaleza (deve ser estado 6, cidade 150)
UPDATE localidades 
SET estado = '6', cidade = '150' 
WHERE id = 2 AND descricao = 'Fortaleza';

-- 3. VERIFICAR SE AS CORREÇÕES FUNCIONARAM
-- =====================================================

SELECT 
    'VERIFICAÇÃO PÓS-CORREÇÃO' as status,
    l.id,
    l.descricao,
    l.cidade,
    l.estado,
    c.nome as nome_cidade,
    e.nome as nome_estado,
    CASE 
        WHEN c.id IS NOT NULL THEN '✅ CIDADE VÁLIDA'
        ELSE '❌ CIDADE INVÁLIDA'
    END as status_cidade,
    CASE 
        WHEN e.id IS NOT NULL THEN '✅ ESTADO VÁLIDO'
        ELSE '❌ ESTADO INVÁLIDO'
    END as status_estado
FROM localidades l
LEFT JOIN cidades c ON l.cidade::integer = c.id
LEFT JOIN estados e ON l.estado::integer = e.id
WHERE l.cliente = 1 
ORDER BY l.descricao;

-- 4. VERIFICAR SE AGORA TODAS AS CIDADES DO CEARÁ APARECEM
-- =====================================================

SELECT 
    'CIDADES DO CEARÁ DISPONÍVEIS' as info,
    c.id as cidade_id,
    c.nome as nome_cidade,
    e.sigla as estado_sigla,
    CASE 
        WHEN l.id IS NOT NULL THEN '✅ JÁ CADASTRADA'
        ELSE '❌ NÃO CADASTRADA'
    END as status
FROM cidades c
INNER JOIN estados e ON c.estado_id = e.id
LEFT JOIN localidades l ON l.cidade = c.id::text AND l.cliente = 1
WHERE c.estado_id = 6 
ORDER BY c.nome;
