-- Adicionar campos para rastrear remoção de arquivos
-- Tabela: contratos

ALTER TABLE contratos 
ADD COLUMN usuarioremocaoarquivo INT NULL,
ADD COLUMN dataremocaoarquivo TIMESTAMP NULL;

-- Adicionar foreign key para o usuário que removeu
ALTER TABLE contratos 
ADD CONSTRAINT fk_contratos_usuarioremocao 
FOREIGN KEY (usuarioremocaoarquivo) 
REFERENCES usuarios(id)
ON DELETE SET NULL;

-- Comentários para documentação
COMMENT ON COLUMN contratos.usuarioremocaoarquivo IS 'ID do usuário que removeu o arquivo do contrato';
COMMENT ON COLUMN contratos.dataremocaoarquivo IS 'Data e hora da remoção do arquivo';

-- Verificar campos criados
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'contratos' 
  AND column_name IN ('usuarioremocaoarquivo', 'dataremocaoarquivo')
ORDER BY column_name;

