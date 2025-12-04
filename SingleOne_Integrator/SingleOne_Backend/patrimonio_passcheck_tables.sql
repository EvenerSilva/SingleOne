-- =====================================================
-- SCRIPT PARA FUNCIONALIDADES PASSCHECK E MEU PATRIMÔNIO
-- =====================================================
-- Este script cria apenas as tabelas específicas necessárias
-- Aproveita toda a estrutura existente do sistema

-- Habilitar extensão uuid-ossp se não existir
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =====================================================
-- TABELA 1: CONTESTAÇÕES DE PATRIMÔNIO
-- =====================================================
CREATE TABLE IF NOT EXISTS patrimonio_contestoes (
    id SERIAL PRIMARY KEY,
    colaborador_id INTEGER NOT NULL REFERENCES colaboradores(id),
    equipamento_id INTEGER NOT NULL REFERENCES equipamentos(id),
    motivo TEXT NOT NULL,
    descricao TEXT,
    status VARCHAR(20) DEFAULT 'pendente' CHECK (status IN ('pendente', 'aprovada', 'rejeitada')),
    evidencia_url VARCHAR(500), -- URL para arquivo de evidência (opcional)
    data_contestacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    data_resolucao TIMESTAMP NULL,
    usuario_resolucao INTEGER REFERENCES usuarios(id),
    observacao_resolucao TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- TABELA 2: LOGS DE ACESSO AO SISTEMA
-- =====================================================
CREATE TABLE IF NOT EXISTS patrimonio_logs_acesso (
    id SERIAL PRIMARY KEY,
    tipo_acesso VARCHAR(20) NOT NULL CHECK (tipo_acesso IN ('passcheck', 'patrimonio')),
    colaborador_id INTEGER REFERENCES colaboradores(id),
    cpf_consultado VARCHAR(14), -- Para PassCheck (pode ser diferente do colaborador logado)
    ip_address INET,
    user_agent TEXT,
    dados_consultados JSONB, -- Dados que foram consultados (sem informações sensíveis)
    sucesso BOOLEAN DEFAULT true,
    mensagem_erro TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- ÍNDICES PARA PERFORMANCE
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_patrimonio_contestoes_colaborador ON patrimonio_contestoes(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_patrimonio_contestoes_equipamento ON patrimonio_contestoes(equipamento_id);
CREATE INDEX IF NOT EXISTS idx_patrimonio_contestoes_status ON patrimonio_contestoes(status);
CREATE INDEX IF NOT EXISTS idx_patrimonio_contestoes_data ON patrimonio_contestoes(data_contestacao);

CREATE INDEX IF NOT EXISTS idx_logs_acesso_tipo ON patrimonio_logs_acesso(tipo_acesso);
CREATE INDEX IF NOT EXISTS idx_logs_acesso_colaborador ON patrimonio_logs_acesso(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_logs_acesso_cpf ON patrimonio_logs_acesso(cpf_consultado);
CREATE INDEX IF NOT EXISTS idx_logs_acesso_data ON patrimonio_logs_acesso(created_at);

-- =====================================================
-- TRIGGER PARA ATUALIZAR updated_at
-- =====================================================
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_patrimonio_contestoes_updated_at 
    BEFORE UPDATE ON patrimonio_contestoes 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =====================================================
-- COMENTÁRIOS DAS TABELAS
-- =====================================================
COMMENT ON TABLE patrimonio_contestoes IS 'Tabela para contestações de patrimônio pelos colaboradores';
COMMENT ON TABLE patrimonio_logs_acesso IS 'Tabela para logs de acesso ao sistema PassCheck e Meu Patrimônio';

COMMENT ON COLUMN patrimonio_contestoes.colaborador_id IS 'ID do colaborador que está contestando';
COMMENT ON COLUMN patrimonio_contestoes.equipamento_id IS 'ID do equipamento sendo contestado';
COMMENT ON COLUMN patrimonio_contestoes.motivo IS 'Motivo da contestação (ex: não reconheço, não recebi, etc.)';
COMMENT ON COLUMN patrimonio_contestoes.status IS 'Status da contestação: pendente, aprovada, rejeitada';
COMMENT ON COLUMN patrimonio_contestoes.evidencia_url IS 'URL opcional para arquivo de evidência';

COMMENT ON COLUMN patrimonio_logs_acesso.tipo_acesso IS 'Tipo de acesso: passcheck ou patrimonio';
COMMENT ON COLUMN patrimonio_logs_acesso.cpf_consultado IS 'CPF que foi consultado (para PassCheck)';
COMMENT ON COLUMN patrimonio_logs_acesso.dados_consultados IS 'JSON com dados que foram consultados (sem informações sensíveis)';

-- =====================================================
-- DADOS INICIAIS (OPCIONAL)
-- =====================================================
-- Inserir parâmetros do sistema se não existirem
INSERT INTO parametros (chave, valor, descricao) 
VALUES 
    ('PassCheck_Habilitado', 'true', 'Habilita/desabilita o sistema PassCheck'),
    ('Patrimonio_Contestacoes_Habilitado', 'true', 'Habilita/desabilita contestações de patrimônio'),
    ('PassCheck_Rate_Limit', '10', 'Limite de consultas por minuto por IP')
ON CONFLICT (chave) DO NOTHING;

-- =====================================================
-- VERIFICAÇÃO FINAL
-- =====================================================
-- Verificar se as tabelas foram criadas corretamente
SELECT 
    'patrimonio_contestoes' as tabela,
    COUNT(*) as colunas
FROM information_schema.columns 
WHERE table_name = 'patrimonio_contestoes'

UNION ALL

SELECT 
    'patrimonio_logs_acesso' as tabela,
    COUNT(*) as colunas
FROM information_schema.columns 
WHERE table_name = 'patrimonio_logs_acesso';

-- =====================================================
-- INSTRUÇÕES DE USO
-- =====================================================
/*
INSTRUÇÕES:

1. Execute este script no banco de dados SingleOne
2. As tabelas serão criadas aproveitando a estrutura existente
3. Não há necessidade de alterar tabelas existentes
4. O sistema funcionará com os dados já cadastrados

FUNCIONALIDADES IMPLEMENTADAS:

PASSCHECK (Portal da Portaria):
- Consulta pública por CPF na tabela colaboradores
- Busca equipamentos ativos do colaborador
- Log de todos os acessos

MEU PATRIMÔNIO (Portal do Colaborador):
- Autenticação usando CPF/email da tabela colaboradores
- Visualização de patrimônio pessoal
- Sistema de contestações
- Log de todas as ações

PRÓXIMOS PASSOS:
1. Implementar controllers no backend
2. Criar interface no frontend
3. Testar funcionalidades
*/
