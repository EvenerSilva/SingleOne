-- Script para criar a view vwplanostelefonia
-- Esta view é necessária para o funcionamento da API de planos com totalizadores

-- Verificar se a view já existe
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.views WHERE table_name = 'vwplanostelefonia') THEN
        RAISE NOTICE 'View vwplanostelefonia já existe. Recriando...';
        DROP VIEW vwplanostelefonia;
    ELSE
        RAISE NOTICE 'View vwplanostelefonia não existe. Criando...';
    END IF;
END $$;

-- Criar a view vwplanostelefonia
CREATE OR REPLACE VIEW vwplanostelefonia AS
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
COMMENT ON VIEW vwplanostelefonia IS 'View para listar planos de telefonia com informações agregadas de linhas';

-- Verificar se a view foi criada com sucesso
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.views WHERE table_name = 'vwplanostelefonia') THEN
        RAISE NOTICE '✅ View vwplanostelefonia criada/recriada com sucesso!';
        
        -- Verificar estrutura da view
        RAISE NOTICE 'Estrutura da view:';
        RAISE NOTICE 'SELECT * FROM vwplanostelefonia LIMIT 1;';
    ELSE
        RAISE NOTICE '❌ Erro ao criar a view vwplanostelefonia';
    END IF;
END $$;

-- Testar a view
SELECT 'Testando a view criada...' as status;
SELECT * FROM vwplanostelefonia LIMIT 3;
