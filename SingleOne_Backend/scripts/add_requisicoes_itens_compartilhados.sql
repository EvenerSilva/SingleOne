DO $sql=@'\nDO $$\nBEGIN\n    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name='requisicoes_itens_compartilhados') THEN\n        CREATE TABLE requisicoes_itens_compartilhados (\n            id SERIAL PRIMARY KEY,\n            requisicao_item_id INTEGER NOT NULL REFERENCES requisicoesitens(id) ON DELETE RESTRICT,\n            colaborador_id INTEGER NOT NULL REFERENCES colaboradores(id) ON DELETE RESTRICT,\n            tipo_acesso VARCHAR(50) NOT NULL DEFAULT 'usuario_compartilhado',\n            data_inicio TIMESTAMP NOT NULL DEFAULT NOW(),\n            data_fim TIMESTAMP NULL,\n            observacao TEXT NULL,\n            ativo BOOLEAN NOT NULL DEFAULT TRUE,\n            criado_por INTEGER NOT NULL REFERENCES usuarios(id) ON DELETE RESTRICT,\n            criado_em TIMESTAMP NOT NULL DEFAULT NOW()\n        );\n\n        CREATE UNIQUE INDEX ux_reqitem_colab_ativo\n            ON requisicoes_itens_compartilhados(requisicao_item_id, colaborador_id)\n            WHERE ativo = TRUE;\n\n        CREATE INDEX ix_reqitem_ativo\n            ON requisicoes_itens_compartilhados(requisicao_item_id, ativo);\n\n        CREATE INDEX ix_colab_ativo\n            ON requisicoes_itens_compartilhados(colaborador_id, ativo);\n    END IF;\nEND$$;\n'@;\n$env:PGPASSWORD='Admin@2025'; psql -h 127.0.0.1 -U postgres -d singleone -v ON_ERROR_STOP=1 -c $sql; Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name='requisicoes_itens_compartilhados') THEN
        CREATE TABLE requisicoes_itens_compartilhados (
            id SERIAL PRIMARY KEY,
            requisicao_item_id INTEGER NOT NULL REFERENCES requisicoesitens(id) ON DELETE RESTRICT,
            colaborador_id INTEGER NOT NULL REFERENCES colaboradores(id) ON DELETE RESTRICT,
            tipo_acesso VARCHAR(50) NOT NULL DEFAULT 'usuario_compartilhado',
            data_inicio TIMESTAMP NOT NULL DEFAULT NOW(),
            data_fim TIMESTAMP NULL,
            observacao TEXT NULL,
            ativo BOOLEAN NOT NULL DEFAULT TRUE,
            criado_por INTEGER NOT NULL REFERENCES usuarios(id) ON DELETE RESTRICT,
            criado_em TIMESTAMP NOT NULL DEFAULT NOW()
        );

        CREATE UNIQUE INDEX ux_reqitem_colab_ativo
            ON requisicoes_itens_compartilhados(requisicao_item_id, colaborador_id)
            WHERE ativo = TRUE;

        CREATE INDEX ix_reqitem_ativo
            ON requisicoes_itens_compartilhados(requisicao_item_id, ativo);

        CREATE INDEX ix_colab_ativo
            ON requisicoes_itens_compartilhados(colaborador_id, ativo);
    END IF;
END$sql=@'\nDO $$\nBEGIN\n    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name='requisicoes_itens_compartilhados') THEN\n        CREATE TABLE requisicoes_itens_compartilhados (\n            id SERIAL PRIMARY KEY,\n            requisicao_item_id INTEGER NOT NULL REFERENCES requisicoesitens(id) ON DELETE RESTRICT,\n            colaborador_id INTEGER NOT NULL REFERENCES colaboradores(id) ON DELETE RESTRICT,\n            tipo_acesso VARCHAR(50) NOT NULL DEFAULT 'usuario_compartilhado',\n            data_inicio TIMESTAMP NOT NULL DEFAULT NOW(),\n            data_fim TIMESTAMP NULL,\n            observacao TEXT NULL,\n            ativo BOOLEAN NOT NULL DEFAULT TRUE,\n            criado_por INTEGER NOT NULL REFERENCES usuarios(id) ON DELETE RESTRICT,\n            criado_em TIMESTAMP NOT NULL DEFAULT NOW()\n        );\n\n        CREATE UNIQUE INDEX ux_reqitem_colab_ativo\n            ON requisicoes_itens_compartilhados(requisicao_item_id, colaborador_id)\n            WHERE ativo = TRUE;\n\n        CREATE INDEX ix_reqitem_ativo\n            ON requisicoes_itens_compartilhados(requisicao_item_id, ativo);\n\n        CREATE INDEX ix_colab_ativo\n            ON requisicoes_itens_compartilhados(colaborador_id, ativo);\n    END IF;\nEND$$;\n'@;\n$env:PGPASSWORD='Admin@2025'; psql -h 127.0.0.1 -U postgres -d singleone -v ON_ERROR_STOP=1 -c $sql; Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue;
