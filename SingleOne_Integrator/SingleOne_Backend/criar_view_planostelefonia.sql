-- Script para criar a view vwplanostelefonia
BEGIN;

-- 1. Criar a view para planos de telefonia
CREATE OR REPLACE VIEW vwplanostelefonia AS
SELECT 
    p.id,
    p.nome as plano,
    p.ativo,
    p.valor,
    c.nome as contrato,
    c.id as contratoid,
    o.nome as operadora,
    o.id as operadoraid,
    COUNT(l.id) as contlinhas,
    COUNT(CASE WHEN l.emuso = true THEN 1 END) as contlinhasemuso,
    COUNT(CASE WHEN l.emuso = false THEN 1 END) as contlinhaslivres
FROM telefoniaplanos p
INNER JOIN telefoniacontratos c ON p.contrato = c.id
INNER JOIN telefoniaoperadoras o ON c.operadora = o.id
LEFT JOIN telefonialinhas l ON l.plano = p.id
WHERE p.ativo = true
GROUP BY p.id, p.nome, p.ativo, p.valor, c.nome, c.id, o.nome, o.id;

-- 2. Verificar se a view foi criada
SELECT 'VIEW vwplanostelefonia criada com sucesso!' as status;

-- 3. Testar a view com dados
SELECT COUNT(*) as total_planos FROM vwplanostelefonia;

COMMIT;
