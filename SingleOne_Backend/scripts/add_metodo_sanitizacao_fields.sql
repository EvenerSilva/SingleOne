-- =====================================================
-- Script: Adicionar campos de Método de Sanitização
-- Data: 2025-10-08
-- Objetivo: Conformidade com Política de Sanitização
-- =====================================================

-- 1. Adicionar campos no protocolo_descarte_itens
ALTER TABLE protocolo_descarte_itens 
ADD COLUMN IF NOT EXISTS metodo_sanitizacao VARCHAR(50),
ADD COLUMN IF NOT EXISTS ferramenta_utilizada VARCHAR(200),
ADD COLUMN IF NOT EXISTS observacoes_sanitizacao TEXT;

-- 2. Adicionar campos no protocolos_descarte (destino final)
ALTER TABLE protocolos_descarte
ADD COLUMN IF NOT EXISTS empresa_destino_final VARCHAR(200),
ADD COLUMN IF NOT EXISTS cnpj_destino_final VARCHAR(20),
ADD COLUMN IF NOT EXISTS certificado_descarte VARCHAR(100);

-- 3. Comentários nas colunas
COMMENT ON COLUMN protocolo_descarte_itens.metodo_sanitizacao IS 'Método de sanitização utilizado (Formatação Simples, Sobregravar Mídia, Destruição Física, etc)';
COMMENT ON COLUMN protocolo_descarte_itens.ferramenta_utilizada IS 'Ferramenta ou equipamento utilizado na sanitização (ex: HDDErase v4.0, Perfurador Industrial)';
COMMENT ON COLUMN protocolo_descarte_itens.observacoes_sanitizacao IS 'Observações adicionais sobre o processo de sanitização';

COMMENT ON COLUMN protocolos_descarte.empresa_destino_final IS 'Empresa responsável pelo destino final do equipamento (logística reversa, reciclagem, etc)';
COMMENT ON COLUMN protocolos_descarte.cnpj_destino_final IS 'CNPJ da empresa de destino final';
COMMENT ON COLUMN protocolos_descarte.certificado_descarte IS 'Número do certificado de descarte ambiental';

-- 4. Verificar as alterações
SELECT column_name, data_type, character_maximum_length 
FROM information_schema.columns 
WHERE table_name = 'protocolo_descarte_itens' 
  AND column_name IN ('metodo_sanitizacao', 'ferramenta_utilizada', 'observacoes_sanitizacao')
ORDER BY column_name;

SELECT column_name, data_type, character_maximum_length 
FROM information_schema.columns 
WHERE table_name = 'protocolos_descarte' 
  AND column_name IN ('empresa_destino_final', 'cnpj_destino_final', 'certificado_descarte')
ORDER BY column_name;

-- =====================================================
-- FIM DO SCRIPT
-- =====================================================

