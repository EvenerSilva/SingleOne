-- Adicionar campos para armazenar arquivo da nota fiscal
-- Tabela: notasfiscais

ALTER TABLE notasfiscais 
ADD COLUMN arquivonotafiscal VARCHAR(500) NULL,
ADD COLUMN nomearquivooriginal VARCHAR(255) NULL,
ADD COLUMN datauploadarquivo TIMESTAMP NULL,
ADD COLUMN usuariouploadarquivo INT NULL,
ADD COLUMN usuarioremocaoarquivo INT NULL,
ADD COLUMN dataremocaoarquivo TIMESTAMP NULL;

-- Adicionar foreign keys para rastreamento
ALTER TABLE notasfiscais 
ADD CONSTRAINT fk_notasfiscais_usuarioupload 
FOREIGN KEY (usuariouploadarquivo) 
REFERENCES usuarios(id)
ON DELETE SET NULL;

ALTER TABLE notasfiscais 
ADD CONSTRAINT fk_notasfiscais_usuarioremocao 
FOREIGN KEY (usuarioremocaoarquivo) 
REFERENCES usuarios(id)
ON DELETE SET NULL;

-- Comentários para documentação
COMMENT ON COLUMN notasfiscais.arquivonotafiscal IS 'Nome do arquivo físico armazenado no servidor';
COMMENT ON COLUMN notasfiscais.nomearquivooriginal IS 'Nome original do arquivo enviado pelo usuário';
COMMENT ON COLUMN notasfiscais.datauploadarquivo IS 'Data e hora do upload do arquivo';
COMMENT ON COLUMN notasfiscais.usuariouploadarquivo IS 'ID do usuário que fez o upload do arquivo';
COMMENT ON COLUMN notasfiscais.usuarioremocaoarquivo IS 'ID do usuário que removeu o arquivo';
COMMENT ON COLUMN notasfiscais.dataremocaoarquivo IS 'Data e hora da remoção do arquivo';

-- Verificar campos criados
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'notasfiscais' 
  AND column_name IN ('arquivonotafiscal', 'nomearquivooriginal', 'datauploadarquivo', 'usuariouploadarquivo', 'usuarioremocaoarquivo', 'dataremocaoarquivo')
ORDER BY column_name;

