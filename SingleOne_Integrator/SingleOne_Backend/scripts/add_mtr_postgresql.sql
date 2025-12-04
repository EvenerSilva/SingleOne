-- =====================================================
-- Script: Adicionar campos MTR ao protocolo de descarte (PostgreSQL)
-- Data: 08/10/2025
-- Descrição: Adiciona campos para Manifesto de Transporte de Resíduos (MTR)
-- =====================================================

\echo 'Iniciando adição de campos MTR...'

-- Verificar se a tabela existe
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'protocolos_descarte') THEN
        RAISE EXCEPTION 'ERRO: Tabela protocolos_descarte não encontrada!';
    END IF;
END $$;

-- Adicionar campos MTR
\echo 'Adicionando campos MTR...'

-- MTR Obrigatório
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_obrigatorio') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_obrigatorio BOOLEAN NOT NULL DEFAULT FALSE;
        RAISE NOTICE 'Campo mtr_obrigatorio adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_obrigatorio já existe';
    END IF;
END $$;

-- Número do MTR
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_numero') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_numero VARCHAR(50);
        RAISE NOTICE 'Campo mtr_numero adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_numero já existe';
    END IF;
END $$;

-- Quem emitiu o MTR
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_emitido_por') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_emitido_por VARCHAR(20);
        RAISE NOTICE 'Campo mtr_emitido_por adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_emitido_por já existe';
    END IF;
END $$;

-- Data de emissão do MTR
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_data_emissao') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_data_emissao TIMESTAMP;
        RAISE NOTICE 'Campo mtr_data_emissao adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_data_emissao já existe';
    END IF;
END $$;

-- Data de validade do MTR
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_validade') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_validade TIMESTAMP;
        RAISE NOTICE 'Campo mtr_validade adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_validade já existe';
    END IF;
END $$;

-- Arquivo MTR
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_arquivo') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_arquivo VARCHAR(500);
        RAISE NOTICE 'Campo mtr_arquivo adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_arquivo já existe';
    END IF;
END $$;

-- Empresa transportadora
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_empresa_transportadora') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_empresa_transportadora VARCHAR(200);
        RAISE NOTICE 'Campo mtr_empresa_transportadora adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_empresa_transportadora já existe';
    END IF;
END $$;

-- CNPJ da transportadora
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_cnpj_transportadora') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_cnpj_transportadora VARCHAR(20);
        RAISE NOTICE 'Campo mtr_cnpj_transportadora adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_cnpj_transportadora já existe';
    END IF;
END $$;

-- Placa do veículo
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_placa_veiculo') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_placa_veiculo VARCHAR(10);
        RAISE NOTICE 'Campo mtr_placa_veiculo adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_placa_veiculo já existe';
    END IF;
END $$;

-- Nome do motorista
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_motorista') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_motorista VARCHAR(100);
        RAISE NOTICE 'Campo mtr_motorista adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_motorista já existe';
    END IF;
END $$;

-- CPF do motorista
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'protocolos_descarte' AND column_name = 'mtr_cpf_motorista') THEN
        ALTER TABLE protocolos_descarte ADD COLUMN mtr_cpf_motorista VARCHAR(14);
        RAISE NOTICE 'Campo mtr_cpf_motorista adicionado';
    ELSE
        RAISE NOTICE 'Campo mtr_cpf_motorista já existe';
    END IF;
END $$;

-- Verificar estrutura final
\echo ''
\echo 'Verificando estrutura final da tabela protocolos_descarte:'
SELECT 
    column_name as "Campo",
    data_type as "Tipo",
    is_nullable as "Permite_Null",
    column_default as "Valor_Padrao"
FROM information_schema.columns 
WHERE table_name = 'protocolos_descarte' 
    AND column_name LIKE 'mtr_%'
ORDER BY ordinal_position;

\echo ''
\echo 'Campos MTR adicionados com sucesso!'
\echo 'Próximos passos:'
\echo '   1. Compilar o backend'
\echo '   2. Atualizar interface TypeScript'
\echo '   3. Modificar modal de protocolo'
\echo '   4. Atualizar template PDF'
