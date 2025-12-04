-- Script para criar tabela de Cargos de Confiança
-- Verifica se a tabela existe antes de criar

DO $$
BEGIN
    -- Verifica se a tabela cargosconfianca existe
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'cargosconfianca'
    ) THEN
        -- Cria a tabela
        CREATE TABLE public.cargosconfianca (
            id SERIAL PRIMARY KEY,
            cliente INTEGER NOT NULL,
            cargo VARCHAR(200) NOT NULL,
            nivelcriticidade VARCHAR(20) NOT NULL DEFAULT 'ALTO',
            obrigarsanitizacao BOOLEAN NOT NULL DEFAULT false,
            obrigardescaracterizacao BOOLEAN NOT NULL DEFAULT false,
            obrigarperfuracaodisco BOOLEAN NOT NULL DEFAULT false,
            obrigarevidencias BOOLEAN NOT NULL DEFAULT false,
            ativo BOOLEAN NOT NULL DEFAULT true,
            usuariocriacao INTEGER NOT NULL,
            datacriacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            usuarioalteracao INTEGER,
            dataalteracao TIMESTAMP,
            
            -- Foreign Keys
            CONSTRAINT fk_cargoconfianca_cliente 
                FOREIGN KEY (cliente) 
                REFERENCES public.clientes(id),
            
            CONSTRAINT fk_cargoconfianca_usuario_criacao 
                FOREIGN KEY (usuariocriacao) 
                REFERENCES public.usuarios(id),
            
            CONSTRAINT fk_cargoconfianca_usuario_alteracao 
                FOREIGN KEY (usuarioalteracao) 
                REFERENCES public.usuarios(id)
        );

        -- Cria índices para melhor performance
        CREATE INDEX idx_cargosconfianca_cliente ON public.cargosconfianca(cliente);
        CREATE INDEX idx_cargosconfianca_cargo ON public.cargosconfianca(cargo);
        CREATE INDEX idx_cargosconfianca_ativo ON public.cargosconfianca(ativo);

        -- Adiciona comentários
        COMMENT ON TABLE public.cargosconfianca IS 'Cargos que requerem processos especiais de segurança no descarte';
        COMMENT ON COLUMN public.cargosconfianca.nivelcriticidade IS 'Nível de criticidade: ALTO, MEDIO, BAIXO';
        COMMENT ON COLUMN public.cargosconfianca.obrigarsanitizacao IS 'Indica se é obrigatório realizar sanitização';
        COMMENT ON COLUMN public.cargosconfianca.obrigardescaracterizacao IS 'Indica se é obrigatório realizar descaracterização';
        COMMENT ON COLUMN public.cargosconfianca.obrigarperfuracaodisco IS 'Indica se é obrigatório realizar perfuração de disco';
        COMMENT ON COLUMN public.cargosconfianca.obrigarevidencias IS 'Indica se é obrigatório anexar evidências fotográficas';

        RAISE NOTICE 'Tabela cargosconfianca criada com sucesso!';
    ELSE
        RAISE NOTICE 'Tabela cargosconfianca já existe!';
    END IF;
END $$;

-- Verifica a estrutura da tabela
SELECT 
    column_name, 
    data_type, 
    is_nullable, 
    column_default
FROM information_schema.columns
WHERE table_name = 'cargosconfianca'
ORDER BY ordinal_position;

