-- Adicionar colunas de controle de processos obrigatórios na tabela protocolo_descarte_itens

-- Verificar se as colunas já existem antes de adicionar
DO $$ 
BEGIN
    -- Processos obrigatórios gerais
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'protocolo_descarte_itens' 
                   AND column_name = 'processos_obrigatorios') THEN
        ALTER TABLE protocolo_descarte_itens 
        ADD COLUMN processos_obrigatorios BOOLEAN DEFAULT FALSE;
    END IF;

    -- Obrigar sanitização
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'protocolo_descarte_itens' 
                   AND column_name = 'obrigar_sanitizacao') THEN
        ALTER TABLE protocolo_descarte_itens 
        ADD COLUMN obrigar_sanitizacao BOOLEAN DEFAULT FALSE;
    END IF;

    -- Obrigar descaracterização
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'protocolo_descarte_itens' 
                   AND column_name = 'obrigar_descaracterizacao') THEN
        ALTER TABLE protocolo_descarte_itens 
        ADD COLUMN obrigar_descaracterizacao BOOLEAN DEFAULT FALSE;
    END IF;

    -- Obrigar perfuração de disco
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'protocolo_descarte_itens' 
                   AND column_name = 'obrigar_perfuracao_disco') THEN
        ALTER TABLE protocolo_descarte_itens 
        ADD COLUMN obrigar_perfuracao_disco BOOLEAN DEFAULT FALSE;
    END IF;

END $$;

-- Comentários nas colunas
COMMENT ON COLUMN protocolo_descarte_itens.processos_obrigatorios IS 'Indica se equipamento passou por cargo de confiança e requer processos especiais';
COMMENT ON COLUMN protocolo_descarte_itens.obrigar_sanitizacao IS 'Indica se é obrigatório executar sanitização de dados';
COMMENT ON COLUMN protocolo_descarte_itens.obrigar_descaracterizacao IS 'Indica se é obrigatório executar descaracterização';
COMMENT ON COLUMN protocolo_descarte_itens.obrigar_perfuracao_disco IS 'Indica se é obrigatório executar perfuração de disco';

