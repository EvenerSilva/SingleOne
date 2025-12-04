-- Script para verificar os perfis dos usuários
-- Este script vai mostrar todos os usuários e seus perfis para entender o problema

SELECT 
    u.id,
    u.nome,
    u.email,
    u.cliente,
    u.su,
    u.adm,
    u.operador,
    u.consulta,
    u.ativo,
    c.razaosocial as nome_cliente
FROM usuario u
LEFT JOIN cliente c ON u.cliente = c.id
ORDER BY u.id;

-- Verificar quantos usuários têm cada perfil
SELECT 
    'Total usuários' as perfil,
    COUNT(*) as quantidade
FROM usuario
UNION ALL
SELECT 
    'su = true' as perfil,
    COUNT(*) as quantidade
FROM usuario
WHERE su = true
UNION ALL
SELECT 
    'su = false' as perfil,
    COUNT(*) as quantidade
FROM usuario
WHERE su = false
UNION ALL
SELECT 
    'adm = true' as perfil,
    COUNT(*) as quantidade
FROM usuario
WHERE adm = true
UNION ALL
SELECT 
    'operador = true' as perfil,
    COUNT(*) as quantidade
FROM usuario
WHERE operador = true
UNION ALL
SELECT 
    'consulta = true' as perfil,
    COUNT(*) as quantidade
FROM usuario
WHERE consulta = true;

-- Verificar usuários por cliente
SELECT 
    u.cliente,
    c.razaosocial as nome_cliente,
    COUNT(*) as total_usuarios,
    SUM(CASE WHEN u.su = true THEN 1 ELSE 0 END) as usuarios_su,
    SUM(CASE WHEN u.su = false THEN 1 ELSE 0 END) as usuarios_nao_su
FROM usuario u
LEFT JOIN cliente c ON u.cliente = c.id
GROUP BY u.cliente, c.razaosocial
ORDER BY u.cliente;
