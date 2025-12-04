-- Adicionar campo 'usarpadrao' na tabela cargosconfianca
-- Este campo permite que os cargos de confiança sejam configurados com padrões de texto
-- Exemplo: "Gerente" com usarpadrao=true vai pegar "Gerente I", "Gerente II", etc.

-- Verificar se a coluna já existe antes de adicionar
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'cargosconfianca' 
        AND column_name = 'usarpadrao'
    ) THEN
        ALTER TABLE cargosconfianca 
        ADD COLUMN usarpadrao BOOLEAN DEFAULT false;
        
        COMMENT ON COLUMN cargosconfianca.usarpadrao IS 'Indica se deve usar match por padrão (LIKE) ao invés de match exato';
        
        RAISE NOTICE 'Coluna usarpadrao adicionada com sucesso!';
    ELSE
        RAISE NOTICE 'Coluna usarpadrao já existe!';
    END IF;
END
$$;

-- Atualizar cargos existentes para usarem match exato (comportamento anterior)
UPDATE cargosconfianca 
SET usarpadrao = false 
WHERE usarpadrao IS NULL;

SELECT 
    id, 
    cargo, 
    usarpadrao,
    nivelcriticidade,
    ativo
FROM cargosconfianca
ORDER BY ativo DESC, cargo;

