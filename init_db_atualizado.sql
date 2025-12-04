-- Script para criar as tabelas do SingleOne
-- Execute este script após criar o banco de dados 'singleone'

-- Habilitar extensão uuid-ossp
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Tabela de Usuários
CREATE TABLE IF NOT EXISTS "Usuarios" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(255) NOT NULL,
    "Email" VARCHAR(255) UNIQUE NOT NULL,
    "Senha" VARCHAR(255) NOT NULL,
    "Ativo" BOOLEAN DEFAULT true,
    "DataCriacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Empresas
CREATE TABLE IF NOT EXISTS "Empresas" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(255) NOT NULL,
    "Cnpj" VARCHAR(18) UNIQUE,
    "Ativo" BOOLEAN DEFAULT true,
    "DataCriacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Clientes
CREATE TABLE IF NOT EXISTS "Clientes" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(255) NOT NULL,
    "Cnpj" VARCHAR(18) UNIQUE,
    "Ativo" BOOLEAN DEFAULT true,
    "DataCriacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Colaboradores
CREATE TABLE IF NOT EXISTS "Colaboradores" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(255) NOT NULL,
    "Email" VARCHAR(255) UNIQUE NOT NULL,
    "Cpf" VARCHAR(14) UNIQUE,
    "EmpresaId" INTEGER REFERENCES "Empresas"("Id"),
    "Ativo" BOOLEAN DEFAULT true,
    "DataCriacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Equipamentos
CREATE TABLE IF NOT EXISTS "Equipamentos" (
    "Id" SERIAL PRIMARY KEY,
    "SerialNumber" VARCHAR(255) UNIQUE NOT NULL,
    "Tipo" VARCHAR(100) NOT NULL,
    "Modelo" VARCHAR(255),
    "Fabricante" VARCHAR(255),
    "ColaboradorId" INTEGER REFERENCES "Colaboradores"("Id"),
    "Status" VARCHAR(50) DEFAULT 'Ativo',
    "DataCriacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Contratos
CREATE TABLE IF NOT EXISTS "Contratos" (
    "Id" SERIAL PRIMARY KEY,
    "Numero" VARCHAR(255) UNIQUE NOT NULL,
    "ClienteId" INTEGER REFERENCES "Clientes"("Id"),
    "Valor" DECIMAL(10,2),
    "Status" VARCHAR(50) DEFAULT 'Ativo',
    "DataInicio" DATE,
    "DataFim" DATE,
    "DataCriacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Requisições
CREATE TABLE IF NOT EXISTS "Requisicoes" (
    "Id" SERIAL PRIMARY KEY,
    "Numero" VARCHAR(255) UNIQUE NOT NULL,
    "ColaboradorId" INTEGER REFERENCES "Colaboradores"("Id"),
    "Tipo" VARCHAR(100) NOT NULL,
    "Status" VARCHAR(50) DEFAULT 'Pendente',
    "DataCriacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Itens de Requisição
CREATE TABLE IF NOT EXISTS "RequisicoesItens" (
    "Id" SERIAL PRIMARY KEY,
    "RequisicaoId" INTEGER REFERENCES "Requisicoes"("Id"),
    "EquipamentoId" INTEGER REFERENCES "Equipamentos"("Id"),
    "Quantidade" INTEGER DEFAULT 1,
    "Observacao" TEXT,
    "DataCriacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Telefonia
CREATE TABLE IF NOT EXISTS "Telefonia" (
    "Id" SERIAL PRIMARY KEY,
    "Numero" VARCHAR(20) UNIQUE NOT NULL,
    "ColaboradorId" INTEGER REFERENCES "Colaboradores"("Id"),
    "Operadora" VARCHAR(100),
    "Plano" VARCHAR(100),
    "Status" VARCHAR(50) DEFAULT 'Ativo',
    "DataCriacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Parâmetros
CREATE TABLE IF NOT EXISTS "Parametros" (
    "Id" SERIAL PRIMARY KEY,
    "Chave" VARCHAR(255) UNIQUE NOT NULL,
    "Valor" TEXT,
    "Descricao" TEXT,
    "DataCriacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Inserir dados iniciais
INSERT INTO "Parametros" ("Chave", "Valor", "Descricao") VALUES
('SistemaNome', 'SingleOne', 'Nome do sistema'),
('SistemaVersao', '1.0.0', 'Versão do sistema'),
('EmailSistema', 'noreply@singleone.local', 'Email do sistema');

-- Criar índices para melhor performance
CREATE INDEX IF NOT EXISTS idx_colaboradores_empresa ON "Colaboradores"("EmpresaId");
CREATE INDEX IF NOT EXISTS idx_equipamentos_colaborador ON "Equipamentos"("ColaboradorId");
CREATE INDEX IF NOT EXISTS idx_contratos_cliente ON "Contratos"("ClienteId");
CREATE INDEX IF NOT EXISTS idx_requisicoes_colaborador ON "Requisicoes"("ColaboradorId");
CREATE INDEX IF NOT EXISTS idx_telefonia_colaborador ON "Telefonia"("ColaboradorId");

-- Comentários sobre as tabelas
COMMENT ON TABLE "Usuarios" IS 'Tabela de usuários do sistema';
COMMENT ON TABLE "Empresas" IS 'Tabela de empresas';
COMMENT ON TABLE "Clientes" IS 'Tabela de clientes';
COMMENT ON TABLE "Colaboradores" IS 'Tabela de colaboradores';
COMMENT ON TABLE "Equipamentos" IS 'Tabela de equipamentos';
COMMENT ON TABLE "Contratos" IS 'Tabela de contratos';
COMMENT ON TABLE "Requisicoes" IS 'Tabela de requisições';
COMMENT ON TABLE "Telefonia" IS 'Tabela de telefonia';
COMMENT ON TABLE "Parametros" IS 'Tabela de parâmetros do sistema';