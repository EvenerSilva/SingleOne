-- Corrigir trigger para não depender de usuario_id
CREATE OR REPLACE FUNCTION criar_historico_sinalizacao()
RETURNS TRIGGER AS $$
BEGIN
    -- Por enquanto, apenas retorna NEW sem inserir no histórico
    -- O histórico pode ser criado manualmente quando necessário
    RETURN NEW;
END;
$$ language 'plpgsql';
