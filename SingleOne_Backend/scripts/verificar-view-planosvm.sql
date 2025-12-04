-- Script para verificar e recriar a view planosvm
-- Esta view é necessária para o funcionamento da API de planos

DO $$
BEGIN
    -- Verificar se a view existe
    IF EXISTS (SELECT 1 FROM information_schema.views WHERE table_name = 'planosvm') THEN
        RAISE NOTICE 'View planosvm já existe. Recriando...';
        DROP VIEW planosvm;
    ELSE
        RAISE NOTICE 'View planosvm não existe. Criando...';
    END IF;
END $$;

-- Criar a view planosvm
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

-- Verificar se a view foi criada com sucesso
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.views WHERE table_name = 'planosvm') THEN
        RAISE NOTICE '✅ View planosvm criada/recriada com sucesso!';
        
        -- Verificar estrutura da view
        RAISE NOTICE 'Estrutura da view:';
        RAISE NOTICE 'SELECT * FROM planosvm LIMIT 1;';
    ELSE
        RAISE NOTICE '❌ Erro ao criar a view planosvm';
    END IF;
END $$;
