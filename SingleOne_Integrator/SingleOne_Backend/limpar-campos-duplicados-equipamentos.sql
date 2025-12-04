-- Script para limpar campos duplicados da tabela equipamentos
-- Baseado na análise, vamos remover campos que não têm dados e estão duplicados

-- ⚠️ ATENÇÃO: Execute este script com cuidado!
-- Faça backup da tabela antes de executar:
-- CREATE TABLE equipamentos_backup AS SELECT * FROM equipamentos;

-- 1. Remover campos com maiúsculas que não têm dados (todos estão vazios)
-- Estes campos foram criados por erro e não têm nenhum dado

-- Campos de Cliente duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "ClienteId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS clienteid;

-- Campos de Empresa duplicados  
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "EmpresaId";

-- Campos de Centro de Custo duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "CentrocustoId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Centrocusto";

-- Campos de Filial duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "FilialId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Filial";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "FilialId1";

-- Campos de Localidade duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "LocalidadeId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Localidade";

-- Campos de Fornecedor duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "FornecedorId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Fornecedor";

-- Campos de Usuario duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "UsuarioId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Usuario";

-- Campos de Status duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "EquipamentostatusId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Equipamentostatus";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "EquipamentosstatusId";

-- Campos de Tipo Equipamento duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "TipoequipamentoId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Tipoequipamento";

-- Campos de Fabricante duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "FabricanteId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Fabricante";

-- Campos de Modelo duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "ModeloId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Modelo";

-- Campos de Nota Fiscal duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "NotafiscalId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Notafiscal";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "NotasfiscaiId";

-- Campos de Contrato duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "ContratoId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Contrato";

-- Campos de Tipo Aquisição duplicados
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "TipoaquisicaoId";
ALTER TABLE equipamentos DROP COLUMN IF EXISTS "Tipoaquisicao";

-- 2. Verificar estrutura final da tabela
SELECT 
    'Estrutura final da tabela equipamentos' as info,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
ORDER BY ordinal_position;

-- 3. Verificar se ainda há campos duplicados
SELECT 
    'Verificação de campos duplicados restantes' as info,
    column_name,
    COUNT(*) as total
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
GROUP BY column_name
HAVING COUNT(*) > 1;

-- 4. Verificar integridade das foreign keys
SELECT 
    'Foreign keys da tabela equipamentos' as info,
    tc.constraint_name,
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND tc.table_name = 'equipamentos'
ORDER BY tc.constraint_name;

-- 5. Verificar se a tabela ainda funciona corretamente
SELECT 
    'Verificação de dados após limpeza' as info,
    COUNT(*) as total_registros,
    COUNT(cliente) as cliente_preenchido,
    COUNT(empresa) as empresa_preenchido,
    COUNT(centrocusto) as centrocusto_preenchido,
    COUNT(filial_id) as filial_preenchido,
    COUNT(localidade_id) as localidade_preenchido,
    COUNT(fornecedor) as fornecedor_preenchido,
    COUNT(usuario) as usuario_preenchido,
    COUNT(equipamentostatus) as status_preenchido,
    COUNT(tipoequipamento) as tipo_preenchido,
    COUNT(fabricante) as fabricante_preenchido,
    COUNT(modelo) as modelo_preenchido,
    COUNT(notafiscal) as notafiscal_preenchido,
    COUNT(contrato) as contrato_preenchido,
    COUNT(tipoaquisicao) as tipoaquisicao_preenchido
FROM equipamentos;

-- 6. Mostrar resumo da limpeza
SELECT 
    'Resumo da limpeza' as info,
    'Campos removidos: 30+ campos duplicados com maiúsculas' as acao,
    'Campos mantidos: Todos os campos em minúsculas que têm dados' as resultado;
