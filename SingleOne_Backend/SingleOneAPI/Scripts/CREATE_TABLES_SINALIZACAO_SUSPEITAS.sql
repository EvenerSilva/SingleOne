-- =====================================================
-- Script de Criação das Tabelas de Sinalização de Suspeitas
-- =====================================================
-- Criado em: 2025-12-12
-- Descrição: Tabelas para gerenciar sinalizações de atividades suspeitas
-- =====================================================

-- 1. Tabela de Motivos de Suspeita
CREATE TABLE IF NOT EXISTS motivos_suspeita (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(20) NOT NULL UNIQUE,
    descricao VARCHAR(100) NOT NULL,
    descricao_detalhada TEXT,
    prioridade_padrao VARCHAR(10) NOT NULL DEFAULT 'media',
    ativo BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 2. Tabela de Sinalizações de Suspeitas
CREATE TABLE IF NOT EXISTS sinalizacoes_suspeitas (
    id SERIAL PRIMARY KEY,
    colaborador_id INTEGER NOT NULL,
    vigilante_id INTEGER,
    investigador_id INTEGER,
    cpf_consultado VARCHAR(20) NOT NULL,
    motivo_suspeita VARCHAR(100) NOT NULL,
    descricao_detalhada TEXT,
    observacoes_vigilante TEXT,
    nome_vigilante VARCHAR(100),
    numero_protocolo VARCHAR(20),
    status VARCHAR(20) NOT NULL DEFAULT 'pendente',
    prioridade VARCHAR(10) NOT NULL DEFAULT 'media',
    dados_consulta JSONB,
    ip_address VARCHAR(45), -- Tipo VARCHAR para compatibilidade com Entity Framework
    user_agent TEXT,
    data_sinalizacao TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_investigacao TIMESTAMP WITHOUT TIME ZONE,
    data_resolucao TIMESTAMP WITHOUT TIME ZONE,
    resultado_investigacao TEXT,
    acoes_tomadas TEXT,
    observacoes_finais TEXT,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_sinalizacao_colaborador FOREIGN KEY (colaborador_id) 
        REFERENCES colaboradores(id) ON DELETE RESTRICT,
    CONSTRAINT fk_sinalizacao_vigilante FOREIGN KEY (vigilante_id) 
        REFERENCES usuarios(id) ON DELETE RESTRICT,
    CONSTRAINT fk_sinalizacao_investigador FOREIGN KEY (investigador_id) 
        REFERENCES usuarios(id) ON DELETE RESTRICT
);

-- 3. Tabela de Histórico de Investigações
CREATE TABLE IF NOT EXISTS historico_investigacoes (
    id SERIAL PRIMARY KEY,
    sinalizacao_id INTEGER NOT NULL,
    usuario_id INTEGER NOT NULL,
    acao VARCHAR(50) NOT NULL,
    descricao TEXT,
    dados_antes JSONB,
    dados_depois JSONB,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_historico_sinalizacao FOREIGN KEY (sinalizacao_id) 
        REFERENCES sinalizacoes_suspeitas(id) ON DELETE CASCADE,
    CONSTRAINT fk_historico_usuario FOREIGN KEY (usuario_id) 
        REFERENCES usuarios(id) ON DELETE RESTRICT
);

-- 4. Tabela de Geolocalização de Assinaturas
CREATE TABLE IF NOT EXISTS geolocalizacao_assinatura (
    id SERIAL PRIMARY KEY,
    colaborador_id INTEGER NOT NULL,
    colaborador_nome VARCHAR(255) NOT NULL,
    usuario_logado_id INTEGER NOT NULL,
    ip_address VARCHAR(45) NOT NULL, -- Tipo VARCHAR para compatibilidade com Entity Framework
    country VARCHAR(100),
    city VARCHAR(100),
    region VARCHAR(100),
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    accuracy_meters DECIMAL(10,2),
    timestamp_captura TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    acao VARCHAR(50) NOT NULL,
    data_criacao TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 5. Tabela de Logs de Acesso ao Patrimônio
CREATE TABLE IF NOT EXISTS patrimonio_logs_acesso (
    id SERIAL PRIMARY KEY,
    tipo_acesso VARCHAR(50) NOT NULL, -- 'passcheck' ou 'patrimonio'
    colaborador_id INTEGER,
    cpf_consultado VARCHAR(20) NOT NULL,
    ip_address VARCHAR(45) NOT NULL, -- Tipo VARCHAR para compatibilidade com Entity Framework
    user_agent TEXT,
    dados_consultados TEXT, -- JSON
    sucesso BOOLEAN NOT NULL DEFAULT true,
    mensagem_erro TEXT,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Índices para Performance
-- =====================================================

-- Índices para sinalizacoes_suspeitas
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_colaborador ON sinalizacoes_suspeitas(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_status ON sinalizacoes_suspeitas(status);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_prioridade ON sinalizacoes_suspeitas(prioridade);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_data ON sinalizacoes_suspeitas(data_sinalizacao);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_investigador ON sinalizacoes_suspeitas(investigador_id);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_protocolo ON sinalizacoes_suspeitas(numero_protocolo);

-- Índices para historico_investigacoes
CREATE INDEX IF NOT EXISTS idx_historico_sinalizacao ON historico_investigacoes(sinalizacao_id);
CREATE INDEX IF NOT EXISTS idx_historico_usuario ON historico_investigacoes(usuario_id);
CREATE INDEX IF NOT EXISTS idx_historico_data ON historico_investigacoes(created_at);

-- Índices para geolocalizacao_assinatura
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_colaborador ON geolocalizacao_assinatura(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_usuario ON geolocalizacao_assinatura(usuario_logado_id);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_timestamp ON geolocalizacao_assinatura(timestamp_captura);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_acao ON geolocalizacao_assinatura(acao);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_ip ON geolocalizacao_assinatura(ip_address);

-- Índices para patrimonio_logs_acesso
CREATE INDEX IF NOT EXISTS idx_patrimonio_logs_tipo ON patrimonio_logs_acesso(tipo_acesso);
CREATE INDEX IF NOT EXISTS idx_patrimonio_logs_colaborador ON patrimonio_logs_acesso(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_patrimonio_logs_data ON patrimonio_logs_acesso(created_at);
CREATE INDEX IF NOT EXISTS idx_patrimonio_logs_ip ON patrimonio_logs_acesso(ip_address);

-- =====================================================
-- Dados Iniciais - Motivos de Suspeita
-- =====================================================

INSERT INTO motivos_suspeita (codigo, descricao, descricao_detalhada, prioridade_padrao) VALUES
('CONSULTA_MULTIPLA', 'Múltiplas consultas em curto período', 'Colaborador realizou várias consultas de CPF em um curto intervalo de tempo', 'alta'),
('CPF_PROPRIO', 'Consulta do próprio CPF', 'Colaborador consultou o próprio CPF, o que pode indicar tentativa de obter informações pessoais', 'media'),
('CPF_FAMILIAR', 'Consulta de CPF de familiar', 'Possível consulta de CPF de parente ou conhecido', 'media'),
('HORARIO_INCOMUM', 'Acesso em horário incomum', 'Acesso fora do horário normal de trabalho', 'baixa'),
('COMPORTAMENTO_SUSPEITO', 'Comportamento suspeito geral', 'Padrão de uso suspeito identificado pelo sistema', 'alta'),
('DADOS_SENSIVEIS', 'Acesso a dados sensíveis', 'Tentativa de acessar dados sensíveis sem justificativa', 'critica')
ON CONFLICT (codigo) DO NOTHING;

-- =====================================================
-- Comentários nas Tabelas
-- =====================================================

COMMENT ON TABLE sinalizacoes_suspeitas IS 'Registros de atividades suspeitas detectadas no sistema';
COMMENT ON TABLE historico_investigacoes IS 'Histórico de ações realizadas durante investigações';
COMMENT ON TABLE geolocalizacao_assinatura IS 'Registro de geolocalização de assinaturas de termos';
COMMENT ON TABLE patrimonio_logs_acesso IS 'Logs de acesso ao sistema de patrimônio e passcheck';
COMMENT ON TABLE motivos_suspeita IS 'Catálogo de motivos de suspeita predefinidos';

-- =====================================================
-- Observações Importantes
-- =====================================================
-- 1. As colunas ip_address usam VARCHAR(45) em vez de INET para compatibilidade
--    com o Entity Framework Core e Npgsql, que têm problemas com conversão
--    automática de string para inet.
-- 
-- 2. Todas as datas usam TIMESTAMP WITHOUT TIME ZONE para compatibilidade
--    com o Entity Framework Core.
-- 
-- 3. Os índices foram criados para otimizar as consultas mais comuns.
-- =====================================================

