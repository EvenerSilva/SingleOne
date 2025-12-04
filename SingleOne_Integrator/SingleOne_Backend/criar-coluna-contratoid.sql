-- Script para criar coluna ContratoId na tabela equipamentos
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "ContratoId" INTEGER;
