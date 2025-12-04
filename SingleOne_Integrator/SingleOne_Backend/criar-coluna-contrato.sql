-- Script para criar coluna Contrato na tabela equipamentos
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "Contrato" INTEGER;
