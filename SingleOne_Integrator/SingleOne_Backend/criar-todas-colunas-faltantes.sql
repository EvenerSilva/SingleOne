-- Script para criar todas as colunas faltantes na tabela equipamentos
-- Baseado nos erros do Entity Framework

-- Colunas já criadas:
-- ClienteId, Contrato, ContratoId

-- Próximas colunas necessárias:
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Empresa" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "EmpresaId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Centrocusto" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "CentrocustoId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Filial" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "FilialId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Localidade" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "LocalidadeId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Fornecedor" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "FornecedorId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Usuario" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "UsuarioId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Equipamentostatus" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "EquipamentostatusId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Tipoequipamento" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "TipoequipamentoId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Fabricante" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "FabricanteId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Modelo" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "ModeloId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Notafiscal" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "NotafiscalId" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Tipoaquisicao" INTEGER;
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "TipoaquisicaoId" INTEGER;

-- Verificar colunas criadas
SELECT column_name FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
AND column_name LIKE '%Id' 
ORDER BY column_name;
