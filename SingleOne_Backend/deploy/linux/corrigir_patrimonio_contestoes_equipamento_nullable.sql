-- Permite NULL em equipamento_id na tabela patrimonio_contestoes.
-- Necessário para Auto Inventário: o colaborador declara o patrimônio pelo número de série
-- e o equipamento é vinculado posteriormente pela equipe.
--
-- Erro que corrige: "null value in column equipamento_id of relation patrimonio_contestoes violates not-null constraint"
--
-- Execução: sudo -u postgres psql -d singleone -f deploy/linux/corrigir_patrimonio_contestoes_equipamento_nullable.sql

ALTER TABLE patrimonio_contestoes ALTER COLUMN equipamento_id DROP NOT NULL;

-- Verificar
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'patrimonio_contestoes'
  AND column_name = 'equipamento_id';
