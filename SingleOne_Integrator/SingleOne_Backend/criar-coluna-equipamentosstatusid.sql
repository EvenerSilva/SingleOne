-- Script para criar coluna EquipamentosstatusId na tabela equipamentos
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "EquipamentosstatusId" INTEGER;
