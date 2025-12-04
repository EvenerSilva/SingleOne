-- Script simplificado para criar tabela de geolocalização
-- Sem foreign keys para evitar dependências

-- Criar tabela se não existir
CREATE TABLE IF NOT EXISTS geolocalizacao_assinatura (
    id SERIAL PRIMARY KEY,
    colaborador_id INTEGER NOT NULL,
    colaborador_nome VARCHAR(255) NOT NULL,
    usuario_logado_id INTEGER NOT NULL,
    ip_address VARCHAR(45) NOT NULL,
    country VARCHAR(100),
    city VARCHAR(100),
    region VARCHAR(100),
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    accuracy_meters DECIMAL(10, 2),
    timestamp_captura TIMESTAMP WITH TIME ZONE NOT NULL,
    acao VARCHAR(50) NOT NULL,
    data_criacao TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Criar índices se não existirem
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_colaborador ON geolocalizacao_assinatura(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_usuario ON geolocalizacao_assinatura(usuario_logado_id);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_timestamp ON geolocalizacao_assinatura(timestamp_captura);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_acao ON geolocalizacao_assinatura(acao);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_ip ON geolocalizacao_assinatura(ip_address);

-- Comentários para documentação
COMMENT ON TABLE geolocalizacao_assinatura IS 'Armazena dados de IP e geolocalização para auditoria de assinaturas eletrônicas';
COMMENT ON COLUMN geolocalizacao_assinatura.ip_address IS 'Endereço IP público do usuário no momento da assinatura';
COMMENT ON COLUMN geolocalizacao_assinatura.latitude IS 'Latitude com precisão de aproximadamente 1 metro';
COMMENT ON COLUMN geolocalizacao_assinatura.longitude IS 'Longitude com precisão de aproximadamente 1 metro';
COMMENT ON COLUMN geolocalizacao_assinatura.accuracy_meters IS 'Precisão da geolocalização em metros (quando disponível)';
COMMENT ON COLUMN geolocalizacao_assinatura.acao IS 'Tipo de ação realizada (ENVIO_TERMO_EMAIL, ASSINATURA_DIGITAL, etc.)';

-- Verificar se a tabela foi criada com sucesso
SELECT 'Tabela geolocalizacao_assinatura criada com sucesso!' as resultado;






































