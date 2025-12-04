-- Script para criar a view planosvm
-- Esta view é necessária para o funcionamento da API de planos

CREATE OR REPLACE VIEW planosvm AS
SELECT 
    p.id,
    p.nome AS plano,
    p.ativo,
    p.valor,
    c.nome AS contrato,
    c.id AS contratoid,
    o.nome AS operadora,
    o.id AS operadoraid,
    COALESCE(COUNT(l.id), 0) AS contlinhas,
    COALESCE(COUNT(CASE WHEN l.emuso = true THEN l.id END), 0) AS contlinhasemuso,
    COALESCE(COUNT(CASE WHEN l.emuso = false THEN l.id END), 0) AS contlinhaslivres
FROM telefoniaplanos p
LEFT JOIN telefoniacontratos c ON p.contrato = c.id
LEFT JOIN telefoniaoperadoras o ON c.operadora = o.id
LEFT JOIN telefonialinhas l ON l.plano = p.id
WHERE p.ativo = true
GROUP BY p.id, p.nome, p.ativo, p.valor, c.nome, c.id, o.nome, o.id;

-- Comentário explicativo
COMMENT ON VIEW planosvm IS 'View para listar planos de telefonia com informações agregadas de linhas';
