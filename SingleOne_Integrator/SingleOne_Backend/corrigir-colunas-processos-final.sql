-- Garantir que as colunas de processos existam e sejam NOT NULL

-- Adicionar colunas se não existirem
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS obrigarsanitizacao BOOLEAN;
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS obrigardescaracterizacao BOOLEAN;
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS obrigarperfuracaodisco BOOLEAN;
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS obrigarevidencias BOOLEAN;

-- Atualizar valores NULL para FALSE nos registros existentes
UPDATE cargosconfianca SET obrigarsanitizacao = false WHERE obrigarsanitizacao IS NULL;
UPDATE cargosconfianca SET obrigardescaracterizacao = false WHERE obrigardescaracterizacao IS NULL;
UPDATE cargosconfianca SET obrigarperfuracaodisco = false WHERE obrigarperfuracaodisco IS NULL;
UPDATE cargosconfianca SET obrigarevidencias = false WHERE obrigarevidencias IS NULL;

-- Tornar NOT NULL
ALTER TABLE cargosconfianca ALTER COLUMN obrigarsanitizacao SET NOT NULL;
ALTER TABLE cargosconfianca ALTER COLUMN obrigardescaracterizacao SET NOT NULL;
ALTER TABLE cargosconfianca ALTER COLUMN obrigarperfuracaodisco SET NOT NULL;
ALTER TABLE cargosconfianca ALTER COLUMN obrigarevidencias SET NOT NULL;

-- IMPORTANTE: Remover defaults para o EF controlar os valores
ALTER TABLE cargosconfianca ALTER COLUMN obrigarsanitizacao DROP DEFAULT;
ALTER TABLE cargosconfianca ALTER COLUMN obrigardescaracterizacao DROP DEFAULT;
ALTER TABLE cargosconfianca ALTER COLUMN obrigarperfuracaodisco DROP DEFAULT;
ALTER TABLE cargosconfianca ALTER COLUMN obrigarevidencias DROP DEFAULT;

-- Verificar resultado
SELECT 
    column_name, 
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'cargosconfianca' 
  AND column_name IN ('obrigarsanitizacao', 'obrigardescaracterizacao', 'obrigarperfuracaodisco', 'obrigarevidencias')
ORDER BY column_name;

