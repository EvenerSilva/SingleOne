-- ========================================
-- TABELA: descarteevidencias
-- Descrição: Armazena evidências (fotos/arquivos) dos processos de descarte
-- Data: 03/10/2025
-- ========================================

-- Criar tabela se não existir
CREATE TABLE IF NOT EXISTS descarteevidencias (
    id SERIAL PRIMARY KEY,
    equipamento INTEGER NOT NULL,
    descricao VARCHAR(500),
    tipoprocesso VARCHAR(50) NOT NULL, -- 'SANITIZACAO', 'DESCARACTERIZACAO', 'PERFURACAO_DISCO', 'EVIDENCIAS_GERAIS'
    nomearquivo VARCHAR(255) NOT NULL,
    caminhoarquivo VARCHAR(500) NOT NULL,
    tipoarquivo VARCHAR(100),
    tamanhoarquivo BIGINT,
    usuarioupload INTEGER NOT NULL,
    dataupload TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ativo BOOLEAN DEFAULT true,
    
    CONSTRAINT fk_descarteevidencias_equipamento FOREIGN KEY (equipamento) 
        REFERENCES equipamentos(id) ON DELETE CASCADE,
    CONSTRAINT fk_descarteevidencias_usuario FOREIGN KEY (usuarioupload) 
        REFERENCES usuarios(id) ON DELETE RESTRICT
);

-- Criar índices para melhor performance
CREATE INDEX IF NOT EXISTS idx_descarteevidencias_equipamento ON descarteevidencias(equipamento);
CREATE INDEX IF NOT EXISTS idx_descarteevidencias_tipoprocesso ON descarteevidencias(tipoprocesso);
CREATE INDEX IF NOT EXISTS idx_descarteevidencias_dataupload ON descarteevidencias(dataupload DESC);

-- Comentários
COMMENT ON TABLE descarteevidencias IS 'Armazena evidências fotográficas e arquivos dos processos de descarte de equipamentos';
COMMENT ON COLUMN descarteevidencias.tipoprocesso IS 'Tipo do processo: SANITIZACAO, DESCARACTERIZACAO, PERFURACAO_DISCO, EVIDENCIAS_GERAIS';
COMMENT ON COLUMN descarteevidencias.nomearquivo IS 'Nome original do arquivo enviado';
COMMENT ON COLUMN descarteevidencias.caminhoarquivo IS 'Caminho relativo onde o arquivo foi salvo no servidor';

-- Verificar se foi criado
SELECT 'Tabela descarteevidencias criada com sucesso!' as resultado;
SELECT column_name, data_type, character_maximum_length, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'descarteevidencias' 
ORDER BY ordinal_position;

