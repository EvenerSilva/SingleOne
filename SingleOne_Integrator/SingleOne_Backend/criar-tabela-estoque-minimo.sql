-- Script para criar a tabela EstoqueMinimoEquipamentos
-- Execute este script no banco de dados PostgreSQL

CREATE TABLE IF NOT EXISTS "EstoqueMinimoEquipamentos" (
    "Id" SERIAL PRIMARY KEY,
    "Cliente" INTEGER NOT NULL,
    "Modelo" INTEGER NOT NULL,
    "Localidade" INTEGER NOT NULL,
    "QuantidadeMinima" INTEGER NOT NULL DEFAULT 0,
    "QuantidadeMaxima" INTEGER NOT NULL DEFAULT 0,
    "QuantidadeTotalLancada" INTEGER NOT NULL DEFAULT 0,
    "Ativo" BOOLEAN NOT NULL DEFAULT TRUE,
    "DtCriacao" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UsuarioCriacao" INTEGER NOT NULL,
    "DtAtualizacao" TIMESTAMP,
    "UsuarioAtualizacao" INTEGER,
    "Observacoes" TEXT,
    
    -- Constraints
    CONSTRAINT "FK_EstoqueMinimoEquipamentos_Cliente" 
        FOREIGN KEY ("Cliente") REFERENCES "Clientes"("Id"),
    CONSTRAINT "FK_EstoqueMinimoEquipamentos_Modelo" 
        FOREIGN KEY ("Modelo") REFERENCES "Modelos"("Id"),
    CONSTRAINT "FK_EstoqueMinimoEquipamentos_Localidade" 
        FOREIGN KEY ("Localidade") REFERENCES "Localidades"("Id"),
    CONSTRAINT "FK_EstoqueMinimoEquipamentos_UsuarioCriacao" 
        FOREIGN KEY ("UsuarioCriacao") REFERENCES "Usuarios"("Id"),
    CONSTRAINT "FK_EstoqueMinimoEquipamentos_UsuarioAtualizacao" 
        FOREIGN KEY ("UsuarioAtualizacao") REFERENCES "Usuarios"("Id"),
    
    -- Índices
    CONSTRAINT "UQ_EstoqueMinimoEquipamentos_Cliente_Modelo_Localidade" 
        UNIQUE ("Cliente", "Modelo", "Localidade")
);

-- Criar índices para melhor performance
CREATE INDEX IF NOT EXISTS "IX_EstoqueMinimoEquipamentos_Cliente" 
    ON "EstoqueMinimoEquipamentos" ("Cliente");
    
CREATE INDEX IF NOT EXISTS "IX_EstoqueMinimoEquipamentos_Modelo" 
    ON "EstoqueMinimoEquipamentos" ("Modelo");
    
CREATE INDEX IF NOT EXISTS "IX_EstoqueMinimoEquipamentos_Localidade" 
    ON "EstoqueMinimoEquipamentos" ("Localidade");
    
CREATE INDEX IF NOT EXISTS "IX_EstoqueMinimoEquipamentos_Ativo" 
    ON "EstoqueMinimoEquipamentos" ("Ativo");

-- Comentários na tabela
COMMENT ON TABLE "EstoqueMinimoEquipamentos" IS 'Tabela para controle de estoque mínimo de equipamentos por cliente, modelo e localidade';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."Cliente" IS 'ID do cliente';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."Modelo" IS 'ID do modelo do equipamento';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."Localidade" IS 'ID da localidade';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."QuantidadeMinima" IS 'Quantidade mínima em estoque';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."QuantidadeMaxima" IS 'Quantidade máxima em estoque';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."QuantidadeTotalLancada" IS 'Quantidade total lançada no sistema';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."Ativo" IS 'Indica se o registro está ativo';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."DtCriacao" IS 'Data de criação do registro';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."UsuarioCriacao" IS 'ID do usuário que criou o registro';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."DtAtualizacao" IS 'Data da última atualização';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."UsuarioAtualizacao" IS 'ID do usuário que fez a última atualização';
COMMENT ON COLUMN "EstoqueMinimoEquipamentos"."Observacoes" IS 'Observações adicionais';

-- Inserir alguns dados de exemplo (opcional)
-- INSERT INTO "EstoqueMinimoEquipamentos" ("Cliente", "Modelo", "Localidade", "QuantidadeMinima", "QuantidadeMaxima", "QuantidadeTotalLancada", "UsuarioCriacao", "Observacoes")
-- VALUES 
--     (1, 1, 1, 10, 50, 25, 1, 'Exemplo de registro de estoque mínimo'),
--     (1, 2, 1, 5, 30, 15, 1, 'Outro exemplo de registro');

-- Verificar se a tabela foi criada
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'EstoqueMinimoEquipamentos'
ORDER BY ordinal_position;
