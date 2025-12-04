-- ========================================
-- TABELAS: Sistema de Protocolo de Descarte
-- Descrição: Tabelas para gerenciar protocolos de descarte com múltiplos equipamentos
-- Data: 15/01/2025
-- ========================================

-- ========================================
-- 1. TABELA: protocolos_descarte
-- Descrição: Armazena os protocolos de descarte (1 protocolo = N equipamentos)
-- ========================================

CREATE TABLE IF NOT EXISTS protocolos_descarte (
    id SERIAL PRIMARY KEY,
    protocolo VARCHAR(20) UNIQUE NOT NULL, -- Ex: DESC-2025-001234
    cliente INTEGER NOT NULL,
    tipo_descarte VARCHAR(50) NOT NULL, -- DOACAO, VENDA, DEVOLUCAO, LOGISTICA_REVERSA, DESCARTE_FINAL
    motivo_descarte TEXT,
    destino_final VARCHAR(500), -- Para onde vão os equipamentos
    responsavel_protocolo INTEGER NOT NULL, -- Usuário que criou o protocolo
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    data_conclusao TIMESTAMP,
    status VARCHAR(30) DEFAULT 'EM_ANDAMENTO', -- EM_ANDAMENTO, CONCLUIDO, CANCELADO
    valor_total_estimado DECIMAL(10,2), -- Soma dos valores dos equipamentos (se venda)
    documento_gerado BOOLEAN DEFAULT false,
    caminho_documento VARCHAR(500),
    observacoes TEXT,
    ativo BOOLEAN DEFAULT true,
    
    CONSTRAINT fk_protocolos_descarte_cliente FOREIGN KEY (cliente) REFERENCES clientes(id),
    CONSTRAINT fk_protocolos_descarte_responsavel FOREIGN KEY (responsavel_protocolo) REFERENCES usuarios(id)
);

-- ========================================
-- 2. TABELA: protocolo_descarte_itens
-- Descrição: Armazena os equipamentos de cada protocolo (1 protocolo = N itens)
-- ========================================

CREATE TABLE IF NOT EXISTS protocolo_descarte_itens (
    id SERIAL PRIMARY KEY,
    protocolo_id INTEGER NOT NULL,
    equipamento INTEGER NOT NULL,
    processo_sanitizacao BOOLEAN DEFAULT false,
    processo_descaracterizacao BOOLEAN DEFAULT false,
    processo_perfuracao_disco BOOLEAN DEFAULT false,
    evidencias_obrigatorias BOOLEAN DEFAULT false,
    evidencias_executadas BOOLEAN DEFAULT false,
    valor_estimado DECIMAL(10,2), -- Valor individual do equipamento
    observacoes_item TEXT,
    data_processo_iniciado TIMESTAMP,
    data_processo_concluido TIMESTAMP,
    status_item VARCHAR(30) DEFAULT 'PENDENTE', -- PENDENTE, EM_PROCESSO, CONCLUIDO
    ativo BOOLEAN DEFAULT true,
    
    CONSTRAINT fk_protocolo_itens_protocolo FOREIGN KEY (protocolo_id) REFERENCES protocolos_descarte(id) ON DELETE CASCADE,
    CONSTRAINT fk_protocolo_itens_equipamento FOREIGN KEY (equipamento) REFERENCES equipamentos(id)
);

-- ========================================
-- 3. ATUALIZAR TABELA EXISTENTE: descarteevidencias
-- Descrição: Adicionar coluna opcional para vincular evidências ao protocolo
-- ========================================

-- Verificar se a coluna já existe antes de adicionar
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'descarteevidencias' 
        AND column_name = 'protocolo_id'
    ) THEN
        ALTER TABLE descarteevidencias 
        ADD COLUMN protocolo_id INTEGER NULL,
        ADD CONSTRAINT fk_descarteevidencias_protocolo FOREIGN KEY (protocolo_id) REFERENCES protocolos_descarte(id);
    END IF;
END $$;

-- ========================================
-- 4. CRIAR ÍNDICES PARA PERFORMANCE
-- ========================================

-- Índices para protocolos_descarte
CREATE INDEX IF NOT EXISTS idx_protocolos_descarte_cliente ON protocolos_descarte(cliente);
CREATE INDEX IF NOT EXISTS idx_protocolos_descarte_status ON protocolos_descarte(status);
CREATE INDEX IF NOT EXISTS idx_protocolos_descarte_data_criacao ON protocolos_descarte(data_criacao DESC);
CREATE INDEX IF NOT EXISTS idx_protocolos_descarte_tipo ON protocolos_descarte(tipo_descarte);

-- Índices para protocolo_descarte_itens
CREATE INDEX IF NOT EXISTS idx_protocolo_itens_protocolo ON protocolo_descarte_itens(protocolo_id);
CREATE INDEX IF NOT EXISTS idx_protocolo_itens_equipamento ON protocolo_descarte_itens(equipamento);
CREATE INDEX IF NOT EXISTS idx_protocolo_itens_status ON protocolo_descarte_itens(status_item);

-- ========================================
-- 5. COMENTÁRIOS DAS TABELAS E COLUNAS
-- ========================================

COMMENT ON TABLE protocolos_descarte IS 'Armazena os protocolos de descarte que podem conter múltiplos equipamentos';
COMMENT ON TABLE protocolo_descarte_itens IS 'Armazena os equipamentos individuais dentro de cada protocolo de descarte';

-- Comentários das colunas principais
COMMENT ON COLUMN protocolos_descarte.protocolo IS 'Número único do protocolo (ex: DESC-2025-001234)';
COMMENT ON COLUMN protocolos_descarte.tipo_descarte IS 'Tipo: DOACAO, VENDA, DEVOLUCAO, LOGISTICA_REVERSA, DESCARTE_FINAL';
COMMENT ON COLUMN protocolos_descarte.destino_final IS 'Destino final dos equipamentos (ex: Escola X, Empresa Y)';
COMMENT ON COLUMN protocolos_descarte.status IS 'Status: EM_ANDAMENTO, CONCLUIDO, CANCELADO';

COMMENT ON COLUMN protocolo_descarte_itens.status_item IS 'Status individual: PENDENTE, EM_PROCESSO, CONCLUIDO';
COMMENT ON COLUMN protocolo_descarte_itens.valor_estimado IS 'Valor estimado do equipamento (para vendas)';

-- ========================================
-- 6. VERIFICAÇÃO FINAL
-- ========================================

-- Verificar se as tabelas foram criadas
SELECT 'Tabelas criadas com sucesso!' as resultado;

-- Mostrar estrutura das tabelas criadas
SELECT 'protocolos_descarte' as tabela, column_name, data_type, character_maximum_length, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'protocolos_descarte' 
ORDER BY ordinal_position;

SELECT 'protocolo_descarte_itens' as tabela, column_name, data_type, character_maximum_length, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'protocolo_descarte_itens' 
ORDER BY ordinal_position;

-- Verificar se a coluna protocolo_id foi adicionada à descarteevidencias
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'descarteevidencias' 
AND column_name = 'protocolo_id';

SELECT 'Script executado com sucesso! ✅' as status_final;
