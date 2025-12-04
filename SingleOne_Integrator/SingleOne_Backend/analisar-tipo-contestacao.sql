-- Análise de registros por tipo_contestacao
-- Banco: PostgreSQL
-- Tabela mapeada pelo modelo: patrimonio_contestoes

-- 1) Total geral de registros
SELECT COUNT(*) AS total_geral
FROM patrimonio_contestoes;

-- 2) Totais por tipo_contestacao (normalizado)
--    Normaliza valores vazios/nulos como 'contestacao' para refletir a regra da GRID
SELECT
  COALESCE(NULLIF(LOWER(TRIM(tipo_contestacao)), ''), 'contestacao') AS tipo_contestacao_normalizado,
  COUNT(*) AS total
FROM patrimonio_contestoes
GROUP BY 1
ORDER BY total DESC;

-- 3) Total que aparecerá na aba "Contestações" da GRID
--    (considera vazio/nulo como 'contestacao')
SELECT COUNT(*) AS total_contestacoes_grid
FROM patrimonio_contestoes
WHERE COALESCE(NULLIF(LOWER(TRIM(tipo_contestacao)), ''), 'contestacao') = 'contestacao';

-- 4) Amostra de 10 registros que aparecem na aba "Contestações"
SELECT
  id,
  colaborador_id,
  equipamento_id,
  status,
  data_contestacao,
  motivo,
  descricao,
  COALESCE(NULLIF(LOWER(TRIM(tipo_contestacao)), ''), 'contestacao') AS tipo_contestacao_normalizado
FROM patrimonio_contestoes
WHERE COALESCE(NULLIF(LOWER(TRIM(tipo_contestacao)), ''), 'contestacao') = 'contestacao'
ORDER BY data_contestacao DESC
LIMIT 10;

-- 5) (Opcional) Amostra de 10 registros do tipo "auto_inventario"
--    Útil para validar a aba de Auto Inventário
SELECT
  id,
  colaborador_id,
  equipamento_id,
  status,
  data_contestacao,
  motivo,
  descricao,
  COALESCE(NULLIF(LOWER(TRIM(tipo_contestacao)), ''), 'contestacao') AS tipo_contestacao_normalizado
FROM patrimonio_contestoes
WHERE COALESCE(NULLIF(LOWER(TRIM(tipo_contestacao)), ''), 'contestacao') = 'auto_inventario'
ORDER BY data_contestacao DESC
LIMIT 10;


