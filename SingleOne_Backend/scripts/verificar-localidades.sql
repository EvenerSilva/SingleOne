-- Script para verificar localidades existentes
-- Execute este script para identificar o problema

-- Verificar todas as localidades
SELECT 
    id,
    descricao,
    cliente,
    ativo,
    estado,
    cidade
FROM localidades 
WHERE cliente = 1 
ORDER BY id;

-- Verificar se existe localidade com ID 1
SELECT 
    id,
    descricao,
    cliente,
    ativo,
    estado,
    cidade
FROM localidades 
WHERE id = 1;

-- Verificar filiais e suas localidades
SELECT 
    f.id as filial_id,
    f.nome as filial_nome,
    f.localidadeId,
    l.id as localidade_id,
    l.descricao as localidade_descricao,
    l.ativo as localidade_ativo
FROM filiais f
LEFT JOIN localidades l ON f.localidadeId = l.id
WHERE f.cliente = 1;

-- Contar total de localidades
SELECT COUNT(*) as total_localidades FROM localidades WHERE cliente = 1;

-- Verificar IDs mínimos e máximos
SELECT 
    MIN(id) as menor_id,
    MAX(id) as maior_id
FROM localidades 
WHERE cliente = 1;
