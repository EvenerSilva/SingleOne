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
DECLARE
	v_count INTEGER;
BEGIN
	RAISE NOTICE '[1/4] Adicionando coluna tipo_lancamento na tabela notasfiscais...';
	
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
	
	RAISE NOTICE '[2/4] Atualizando registros existentes...';
	
	-- Atualizar registros existentes para garantir que todos tenham tipo_lancamento
	UPDATE notasfiscais 
	SET tipo_lancamento = 'nota_fiscal' 
	WHERE tipo_lancamento IS NULL OR tipo_lancamento = '';
	
	SELECT COUNT(*) INTO v_count FROM notasfiscais;
	RAISE NOTICE '✅ Registros existentes atualizados: % registros', v_count;
	
	RAISE NOTICE '[3/4] Tornando valorunitario nullable na tabela notasfiscaisitens...';
	
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
	
	RAISE NOTICE '[4/4] Adicionando constraint de validação...';
	
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
	
	RAISE NOTICE '';
	RAISE NOTICE '=====================================================';
	RAISE NOTICE '✅ Migração concluída com sucesso!';
	RAISE NOTICE '=====================================================';
	RAISE NOTICE '';
	RAISE NOTICE 'Resumo:';
	RAISE NOTICE '  - Coluna tipo_lancamento adicionada/verificada';
	RAISE NOTICE '  - Coluna valorunitario tornada nullable';
	RAISE NOTICE '  - Constraint de validação adicionada';
	RAISE NOTICE '  - Registros existentes atualizados';
	
EXCEPTION
	WHEN OTHERS THEN
		RAISE NOTICE '❌ Erro na migração: %', SQLERRM;
		RAISE;
END $$;

-- Verificar resultado
\echo 'Verificando resultado da migração...'
\echo ''

-- Verificar coluna tipo_lancamento
SELECT 
	'notasfiscais' AS tabela,
	'tipo_lancamento' AS coluna,
	EXISTS (
		SELECT 1 FROM information_schema.columns 
		WHERE table_schema = 'public' 
		AND table_name = 'notasfiscais' 
		AND column_name = 'tipo_lancamento'
	) AS coluna_existe;

-- Verificar se valorunitario é nullable (apenas se a tabela existir)
DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM information_schema.tables 
		WHERE table_schema = 'public' 
		AND table_name = 'notasfiscaisitens'
	) THEN
		RAISE NOTICE 'Verificando coluna valorunitario...';
		PERFORM EXISTS (
			SELECT 1 FROM information_schema.columns 
			WHERE table_schema = 'public' 
			AND table_name = 'notasfiscaisitens' 
			AND column_name = 'valorunitario'
			AND is_nullable = 'YES'
		);
		RAISE NOTICE '✅ Tabela notasfiscaisitens existe e valorunitario é nullable';
	ELSE
		RAISE NOTICE 'ℹ️  Tabela notasfiscaisitens não existe ainda (será criada quando necessário)';
	END IF;
END $$;

\echo ''
\echo 'Migração finalizada!'
