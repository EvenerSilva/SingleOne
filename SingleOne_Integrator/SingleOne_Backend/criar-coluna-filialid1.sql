-- Script para criar coluna FilialId1 na tabela equipamentos
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "FilialId1" INTEGER;
