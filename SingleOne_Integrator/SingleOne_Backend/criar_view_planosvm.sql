-- Script para criar a view planosvm se não existir
-- Esta view é necessária para o funcionamento da API de planos

-- 1. Verificar se a view planosvm existe
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.views WHERE table_name = 'planosvm') THEN
        RAISE NOTICE 'View planosvm já existe. Recriando...';
        DROP VIEW planosvm;
    ELSE
        RAISE NOTICE 'View planosvm não existe. Criando...';
    END IF;
END $$;

-- 2. Criar a view planosvm
CREATE OR REPLACE VIEW planosvm AS
SELECT 
    p.id,
    p.nome as plano,
    p.ativo,
    p.valor,
    c.nome as contrato,
    c.id as contratoid,
    o.nome as operadora,
    o.id as operadoraid,
    COALESCE(COUNT(tl.id), 0) as contlinhas,
    COALESCE(COUNT(CASE WHEN tl.ativo = true THEN tl.id END), 0) as contlinhasemuso,
    COALESCE(COUNT(CASE WHEN tl.ativo = false THEN tl.id END), 0) as contlinhaslivres
FROM telefoniaplanos p
LEFT JOIN telefoniacontratos c ON p.contrato = c.id
LEFT JOIN telefoniaoperadoras o ON c.operadora = o.id
LEFT JOIN telefonialinhas tl ON p.id = tl.plano
GROUP BY p.id, p.nome, p.ativo, p.valor, c.nome, c.id, o.nome, o.id;

-- 3. Comentário na view
COMMENT ON VIEW planosvm IS 'View para listar planos de telefonia com informações agregadas de linhas';

-- 4. Verificar se foi criada
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.views WHERE table_name = 'planosvm') THEN
        RAISE NOTICE '✅ View planosvm criada/recriada com sucesso!';
    ELSE
        RAISE NOTICE '❌ Erro ao criar a view planosvm';
    END IF;
END $$;

-- 5. Testar a view
SELECT 'Testando a view planosvm...' as status;
SELECT COUNT(*) as total_planos FROM planosvm;
SELECT * FROM planosvm LIMIT 3;
