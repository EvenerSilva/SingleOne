-- Script para criar coluna ClienteId na tabela equipamentos
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "ClienteId" INTEGER;
