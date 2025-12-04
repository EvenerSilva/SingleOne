-- Verificar se os campos cidade e estado existem na tabela localidades
SELECT 
    column_name as "Campo",
    data_type as "Tipo",
    is_nullable as "Permite_Nulo",
    character_maximum_length as "Tamanho_Max"
FROM information_schema.columns 
WHERE table_name = 'localidades'
ORDER BY ordinal_position;
