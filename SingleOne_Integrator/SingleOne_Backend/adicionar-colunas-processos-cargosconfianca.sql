-- Adicionar todas as colunas de processos se não existirem

-- Obrigarsanitizacao
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS obrigarsanitizacao BOOLEAN DEFAULT false;

-- Obrigardescaracterizacao
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS obrigardescaracterizacao BOOLEAN DEFAULT false;

-- Obrigarperfuracaodisco
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS obrigarperfuracaodisco BOOLEAN DEFAULT false;

-- Obrigarevidencias
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS obrigarevidencias BOOLEAN DEFAULT false;

-- Tornar todas NOT NULL
ALTER TABLE cargosconfianca ALTER COLUMN obrigarsanitizacao SET NOT NULL;
ALTER TABLE cargosconfianca ALTER COLUMN obrigardescaracterizacao SET NOT NULL;
ALTER TABLE cargosconfianca ALTER COLUMN obrigarperfuracaodisco SET NOT NULL;
ALTER TABLE cargosconfianca ALTER COLUMN obrigarevidencias SET NOT NULL;

-- Remover defaults (o Entity Framework controla)
ALTER TABLE cargosconfianca ALTER COLUMN obrigarsanitizacao DROP DEFAULT;
ALTER TABLE cargosconfianca ALTER COLUMN obrigardescaracterizacao DROP DEFAULT;
ALTER TABLE cargosconfianca ALTER COLUMN obrigarperfuracaodisco DROP DEFAULT;
ALTER TABLE cargosconfianca ALTER COLUMN obrigarevidencias DROP DEFAULT;

-- Verificar estrutura final
SELECT 
    column_name, 
    data_type, 
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'cargosconfianca'
ORDER BY ordinal_position;

