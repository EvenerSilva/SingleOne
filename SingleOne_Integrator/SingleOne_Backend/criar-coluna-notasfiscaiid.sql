-- Script para criar coluna NotasfiscaiId na tabela equipamentos
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS "NotasfiscaiId" INTEGER;
