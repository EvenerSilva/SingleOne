-- Script para adicionar coluna versao_template_assinado na tabela requisicoes
-- Esta coluna armazena a versão do template no momento da assinatura

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'requisicoes' AND column_name = 'versao_template_assinado') THEN
        ALTER TABLE requisicoes ADD COLUMN versao_template_assinado INTEGER NULL;
        RAISE NOTICE 'Coluna versao_template_assinado adicionada com sucesso!';
    ELSE
        RAISE NOTICE 'Coluna versao_template_assinado já existe.';
    END IF;
END $$;

