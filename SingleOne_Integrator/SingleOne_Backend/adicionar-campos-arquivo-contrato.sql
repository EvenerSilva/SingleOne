-- Adicionar campos para armazenar arquivo do contrato
-- Tabela: contratos

ALTER TABLE contratos 
ADD COLUMN arquivocontrato VARCHAR(500) NULL,
ADD COLUMN nomearquivooriginal VARCHAR(255) NULL,
ADD COLUMN datauploadarquivo TIMESTAMP NULL,
ADD COLUMN usuariouploadarquivo INT NULL;

-- Adicionar foreign key para o usuário que fez upload
ALTER TABLE contratos 
ADD CONSTRAINT fk_contratos_usuarioupload 
FOREIGN KEY (usuariouploadarquivo) 
REFERENCES usuarios(id);

-- Comentários para documentação
COMMENT ON COLUMN contratos.arquivocontrato IS 'Nome do arquivo físico armazenado no servidor';
COMMENT ON COLUMN contratos.nomearquivooriginal IS 'Nome original do arquivo enviado pelo usuário';
COMMENT ON COLUMN contratos.datauploadarquivo IS 'Data e hora do upload do arquivo';
COMMENT ON COLUMN contratos.usuariouploadarquivo IS 'ID do usuário que fez o upload do arquivo';


