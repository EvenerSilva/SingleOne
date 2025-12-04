-- ===============================================
-- Script de Criação da Tabela de Staging
-- Importação de Colaboradores
-- ===============================================

-- Criar tabela de staging para importação de colaboradores
CREATE TABLE IF NOT EXISTS importacao_colaborador_staging (
    id SERIAL PRIMARY KEY,
    lote_id UUID NOT NULL,
    cliente INT NOT NULL,
    usuario_importacao INT NOT NULL,
    data_importacao TIMESTAMP NOT NULL DEFAULT NOW(),
    
    -- Dados do colaborador vindos do arquivo
    nome_colaborador VARCHAR(255),
    cpf VARCHAR(14),
    matricula VARCHAR(50),
    email VARCHAR(255),
    cargo VARCHAR(100),
    setor VARCHAR(100),
    data_admissao DATE,
    tipo_colaborador VARCHAR(1),  -- F, T ou C
    data_demissao DATE,
    matricula_superior VARCHAR(50),
    
    -- Dados relacionados (do arquivo)
    empresa_nome VARCHAR(255),
    empresa_cnpj VARCHAR(18),
    localidade_descricao VARCHAR(255),
    localidade_cidade VARCHAR(100),
    localidade_estado VARCHAR(2),
    centro_custo_codigo VARCHAR(50),
    centro_custo_nome VARCHAR(255),
    filial_nome VARCHAR(255),
    filial_cnpj VARCHAR(18),
    
    -- Validação e Status
    status CHAR(1) NOT NULL DEFAULT 'P',  -- P=Pendente, V=Validado, E=Erro, I=Importado
    mensagens_validacao TEXT,  -- JSON com erros/avisos
    linha_arquivo INT,
    
    -- IDs resolvidos após validação
    empresa_id INT,
    localidade_id INT,
    centro_custo_id INT,
    filial_id INT,
    
    -- Flags de ação
    criar_empresa BOOLEAN DEFAULT FALSE,
    criar_localidade BOOLEAN DEFAULT FALSE,
    criar_centro_custo BOOLEAN DEFAULT FALSE,
    criar_filial BOOLEAN DEFAULT FALSE,
    
    -- Foreign Keys (usando nomes corretos das tabelas)
    CONSTRAINT fk_colaborador_staging_usuario FOREIGN KEY (usuario_importacao) 
        REFERENCES Usuarios(Id) ON DELETE RESTRICT,
    CONSTRAINT fk_colaborador_staging_cliente FOREIGN KEY (cliente) 
        REFERENCES Clientes(Id) ON DELETE RESTRICT
);

-- Criar índices para performance
CREATE INDEX IF NOT EXISTS idx_colaborador_staging_lote ON importacao_colaborador_staging(lote_id);
CREATE INDEX IF NOT EXISTS idx_colaborador_staging_status ON importacao_colaborador_staging(status);
CREATE INDEX IF NOT EXISTS idx_colaborador_staging_cliente ON importacao_colaborador_staging(cliente);
CREATE INDEX IF NOT EXISTS idx_colaborador_staging_data ON importacao_colaborador_staging(data_importacao);

-- Comentários nas colunas
COMMENT ON TABLE importacao_colaborador_staging IS 'Tabela de staging para importação em massa de colaboradores';
COMMENT ON COLUMN importacao_colaborador_staging.lote_id IS 'GUID único do lote de importação';
COMMENT ON COLUMN importacao_colaborador_staging.status IS 'P=Pendente, V=Validado, E=Erro, I=Importado';
COMMENT ON COLUMN importacao_colaborador_staging.tipo_colaborador IS 'F=Funcionário, T=Terceiro, C=Consultor';
COMMENT ON COLUMN importacao_colaborador_staging.mensagens_validacao IS 'JSON contendo arrays de erros e avisos';

-- ===============================================
-- Verificar tabela importacao_log
-- ===============================================
-- A tabela importacao_log já existe e suporta tipo_importacao = 'COLABORADORES'
-- Não é necessário criar nada adicional

DO $$
BEGIN
    -- Verificar se a tabela importacao_log existe
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'importacao_log') THEN
        RAISE NOTICE '✅ Tabela importacao_log já existe. Tipo COLABORADORES pode ser usado.';
    ELSE
        RAISE NOTICE '⚠️  ATENÇÃO: Tabela importacao_log não encontrada. Execute o script init_db_atualizado.sql primeiro!';
    END IF;
END $$;

-- ===============================================
-- Script concluído com sucesso
-- ===============================================
SELECT 
    '✅ Tabela importacao_colaborador_staging criada com sucesso!' as mensagem,
    COUNT(*) as total_indices
FROM pg_indexes 
WHERE tablename = 'importacao_colaborador_staging';
