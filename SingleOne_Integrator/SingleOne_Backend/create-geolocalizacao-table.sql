-- Tabela para armazenar dados de geolocalização das assinaturas
CREATE TABLE IF NOT EXISTS public.geolocalizacao_assinatura (
    id SERIAL PRIMARY KEY,
    colaborador_id INTEGER NOT NULL,
    colaborador_nome VARCHAR(255) NOT NULL,
    usuario_logado_id INTEGER NOT NULL,
    ip_address VARCHAR(45) NOT NULL, -- Suporta IPv4 e IPv6
    country VARCHAR(100),
    city VARCHAR(100),
    region VARCHAR(100),
    latitude DECIMAL(10, 8), -- Precisão de ~1 metro
    longitude DECIMAL(11, 8), -- Precisão de ~1 metro
    accuracy_meters DECIMAL(10, 2), -- Precisão em metros
    timestamp_captura TIMESTAMP WITH TIME ZONE NOT NULL,
    acao VARCHAR(50) NOT NULL, -- 'ENVIO_TERMO_EMAIL', 'ASSINATURA_DIGITAL', etc.
    data_criacao TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Índices para consultas frequentes
    -- Nota: Foreign keys comentadas para evitar dependências
    -- CONSTRAINT fk_colaborador FOREIGN KEY (colaborador_id) REFERENCES colaboradores(id),
    -- CONSTRAINT fk_usuario FOREIGN KEY (usuario_logado_id) REFERENCES usuario(id)
);

-- Índices para melhorar performance
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_colaborador ON public.geolocalizacao_assinatura(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_usuario ON public.geolocalizacao_assinatura(usuario_logado_id);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_timestamp ON public.geolocalizacao_assinatura(timestamp_captura);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_acao ON public.geolocalizacao_assinatura(acao);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_ip ON public.geolocalizacao_assinatura(ip_address);

-- Comentários para documentação
COMMENT ON TABLE public.geolocalizacao_assinatura IS 'Armazena dados de IP e geolocalização para auditoria de assinaturas eletrônicas';
COMMENT ON COLUMN public.geolocalizacao_assinatura.ip_address IS 'Endereço IP público do usuário no momento da assinatura';
COMMENT ON COLUMN public.geolocalizacao_assinatura.latitude IS 'Latitude com precisão de aproximadamente 1 metro';
COMMENT ON COLUMN public.geolocalizacao_assinatura.longitude IS 'Longitude com precisão de aproximadamente 1 metro';
COMMENT ON COLUMN public.geolocalizacao_assinatura.accuracy_meters IS 'Precisão da geolocalização em metros (quando disponível)';
COMMENT ON COLUMN public.geolocalizacao_assinatura.acao IS 'Tipo de ação realizada (ENVIO_TERMO_EMAIL, ASSINATURA_DIGITAL, etc.)';

-- Exemplo de consulta para verificar se a tabela foi criada
-- SELECT * FROM public.geolocalizacao_assinatura LIMIT 1;
