-- =====================================================
-- MIGRAÇÃO: Adicionar suporte a Inventário em Notas Fiscais
-- Data: 2025-01-02
-- Versão: 1.0
-- 
-- Descrição: 
--   Permite cadastrar notas fiscais como "inventário" (sem valor obrigatório)
--   Útil para casos onde o cliente não possui mais a nota fiscal original
--   mas precisa registrar os equipamentos no sistema
--
-- Uso:
--   sudo -u postgres psql -d singleone -f migrar_suporte_inventario_notasfiscais.sql
-- =====================================================

\echo '====================================================='
\echo 'Migração: Suporte a Inventário em Notas Fiscais'
\echo '====================================================='
\echo ''

DO $$
BEGIN
	\echo '[1/4] Adicionando coluna tipo_lancamento na tabela notasfiscais...'
	
	-- Adicionar coluna tipo_lancamento na tabela notasfiscais se não existir
	IF NOT EXISTS (
		SELECT 1 FROM information_schema.columns 
		WHERE table_schema = 'public' 
		AND table_name = 'notasfiscais' 
		AND column_name = 'tipo_lancamento'
	) THEN
		ALTER TABLE notasfiscais ADD COLUMN tipo_lancamento VARCHAR(20) DEFAULT 'nota_fiscal';
		RAISE NOTICE '✅ Coluna tipo_lancamento adicionada à tabela notasfiscais';
	ELSE
		RAISE NOTICE 'ℹ️  Coluna tipo_lancamento já existe na tabela notasfiscais';
	END IF;
	
	\echo '[2/4] Atualizando registros existentes...'
	
	-- Atualizar registros existentes para garantir que todos tenham tipo_lancamento
	UPDATE notasfiscais 
	SET tipo_lancamento = 'nota_fiscal' 
	WHERE tipo_lancamento IS NULL OR tipo_lancamento = '';
	
	RAISE NOTICE '✅ Registros existentes atualizados: % registros', (SELECT COUNT(*) FROM notasfiscais);
	
	\echo '[3/4] Tornando valorunitario nullable na tabela notasfiscaisitens...'
	
	-- Tornar valorunitario nullable na tabela notasfiscaisitens (para permitir inventário sem valor)
	IF EXISTS (
		SELECT 1 FROM information_schema.columns 
		WHERE table_schema = 'public' 
		AND table_name = 'notasfiscaisitens' 
		AND column_name = 'valorunitario'
		AND is_nullable = 'NO'
	) THEN
		ALTER TABLE notasfiscaisitens ALTER COLUMN valorunitario DROP NOT NULL;
		RAISE NOTICE '✅ Coluna valorunitario tornada nullable na tabela notasfiscaisitens';
	ELSE
		RAISE NOTICE 'ℹ️  Coluna valorunitario já é nullable na tabela notasfiscaisitens';
	END IF;
	
	\echo '[4/4] Adicionando constraint de validação...'
	
	-- Adicionar constraint CHECK para validar valores de tipo_lancamento
	IF NOT EXISTS (
		SELECT 1 FROM information_schema.table_constraints 
		WHERE table_schema = 'public' 
		AND table_name = 'notasfiscais' 
		AND constraint_name = 'ck_notasfiscais_tipo_lancamento'
	) THEN
		ALTER TABLE notasfiscais 
		ADD CONSTRAINT ck_notasfiscais_tipo_lancamento 
		CHECK (tipo_lancamento IN ('nota_fiscal', 'inventario'));
		RAISE NOTICE '✅ Constraint de validação adicionada para tipo_lancamento';
	ELSE
		RAISE NOTICE 'ℹ️  Constraint de validação já existe';
	END IF;
	
	\echo ''
	\echo '====================================================='
	\echo '✅ Migração concluída com sucesso!'
	\echo '====================================================='
	\echo ''
	\echo 'Resumo:'
	\echo '  - Coluna tipo_lancamento adicionada/verificada'
	\echo '  - Coluna valorunitario tornada nullable'
	\echo '  - Constraint de validação adicionada'
	\echo '  - Registros existentes atualizados'
	\echo ''
	\echo 'Próximos passos:'
	\echo '  1. Atualizar modelos C# (Notasfiscai.cs e Notasfiscaisiten.cs)'
	\echo '  2. Atualizar mapeamentos EF Core'
	\echo '  3. Modificar validações no backend'
	\echo '  4. Adicionar campo no formulário frontend'
	\echo ''
	
EXCEPTION
	WHEN OTHERS THEN
		RAISE NOTICE '❌ Erro na migração: %', SQLERRM;
		RAISE;
END $$;

-- Verificar resultado
\echo 'Verificando resultado da migração...'
\echo ''

SELECT 
	'notasfiscais' AS tabela,
	'tipo_lancamento' AS coluna,
	EXISTS (
		SELECT 1 FROM information_schema.columns 
		WHERE table_schema = 'public' 
		AND table_name = 'notasfiscais' 
		AND column_name = 'tipo_lancamento'
	) AS existe,
	(SELECT COUNT(*) FROM notasfiscais WHERE tipo_lancamento = 'nota_fiscal') AS registros_nota_fiscal,
	(SELECT COUNT(*) FROM notasfiscais WHERE tipo_lancamento = 'inventario') AS registros_inventario
UNION ALL
SELECT 
	'notasfiscaisitens' AS tabela,
	'valorunitario' AS coluna,
	EXISTS (
		SELECT 1 FROM information_schema.columns 
		WHERE table_schema = 'public' 
		AND table_name = 'notasfiscaisitens' 
		AND column_name = 'valorunitario'
		AND is_nullable = 'YES'
	) AS existe,
	(SELECT COUNT(*) FROM notasfiscaisitens WHERE valorunitario IS NULL) AS registros_null,
	(SELECT COUNT(*) FROM notasfiscaisitens WHERE valorunitario = 0) AS registros_zero;

\echo ''
\echo 'Migração finalizada!'
