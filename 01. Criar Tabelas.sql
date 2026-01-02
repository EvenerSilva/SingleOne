-- =====================================================
-- SCRIPT DE INICIALIZAÇÃO DO BANCO DE DADOS - SINGLEONE
-- Descrição: Script completo para criar banco limpo com dados básicos
-- Versão: 2.4 (Atualizado em 08/11/2025)
-- v2.4: Adicionado tratamento de erros e criação automática do banco
-- v2.3: View colaboradoresvm estendida + staging de importação de colaboradores
-- v2.2: Adicionadas 9 tabelas faltantes + colunas 2FA + coluna logo
-- v2.1: Adicionadas 8 novas tabelas (campanhas, estoque mínimo, importação, etc)
-- v2.0: Script base com estrutura principal
-- =====================================================
-- NOTA: Este script é executado automaticamente quando o volume PostgreSQL é criado
--       pela primeira vez (via /docker-entrypoint-initdb.d/)
--       Se o banco sumir, execute manualmente: cat init_db_atualizado.sql | docker exec -i singleone-postgres psql -U postgres -d singleone
--
-- IMPORTANTE: Este script assume que está sendo executado no banco 'singleone'
--             Se executado no banco 'postgres', ele criará o banco 'singleone' automaticamente
--             e então você precisará executar novamente no banco 'singleone'

-- =====================================================
-- CONFIGURAÇÃO INICIAL - CONTINUAR MESMO COM ERROS
-- =====================================================
-- Este script foi projetado para ser idempotente e continuar mesmo com erros
-- para garantir que todas as tabelas, views e dados sejam criados

-- Verificar se o banco singleone existe e avisar se necessário
-- NOTA: Este script DEVE ser executado no banco 'singleone'
--       Se o banco não existir, crie-o ANTES: CREATE DATABASE singleone;
DO $$
BEGIN
    IF current_database() = 'postgres' THEN
        IF EXISTS (SELECT 1 FROM pg_database WHERE datname = 'singleone') THEN
            RAISE NOTICE 'Banco "singleone" existe. Conecte-se a ele antes de executar este script:';
            RAISE NOTICE '  \c singleone';
            RAISE NOTICE '  OU: psql -U postgres -d singleone -f init_db_atualizado.sql';
        ELSE
            RAISE NOTICE '⚠️  Banco "singleone" NÃO existe!';
            RAISE NOTICE '   Crie o banco antes de executar este script:';
            RAISE NOTICE '   CREATE DATABASE singleone;';
            RAISE EXCEPTION 'Banco singleone não existe. Crie-o primeiro.';
        END IF;
    ELSIF current_database() = 'singleone' THEN
        RAISE NOTICE '✅ Executando script no banco correto: singleone';
    ELSE
        RAISE NOTICE '⚠️  Executando no banco: %. Esperado: singleone', current_database();
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        -- Continuar mesmo se houver erro na verificação
        RAISE NOTICE 'Continuando execução no banco: %', current_database();
END $$;

-- Habilitar extensão UUID (com tratamento de erro)
DO $$
BEGIN
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Extensão uuid-ossp já existe ou erro ao criar: %', SQLERRM;
END $$;

-- =====================================================
-- HANGFIRE (TAREFAS EM BACKGROUND)
-- =====================================================
-- IMPORTANTE:
--  - As tabelas do Hangfire NÃO são mais criadas manualmente aqui.
--  - O próprio Hangfire (via PostgreSqlStorage, com PrepareSchemaIfNecessary = true)
--    é responsável por criar e versionar o schema/tabelas `hangfire.*`.
--  - Manter a criação manual aqui causava conflitos como:
--        42701: column \"acquired\" of relation \"lock\" already exists
--  - Para um ambiente novo:
--        1. Executar este script para criar o banco `singleone`.
--        2. Subir o backend (SingleOneAPI).
--        3. O Hangfire criará automaticamente o schema e todas as tabelas necessárias.

-- Variável para contar erros (será usado no final)
DO $$
BEGIN
    -- Criar tabela temporária para log de erros se não existir
    CREATE TEMP TABLE IF NOT EXISTS script_errors (
        id SERIAL PRIMARY KEY,
        erro TEXT,
        timestamp TIMESTAMP DEFAULT NOW()
    );
EXCEPTION
    WHEN OTHERS THEN
        NULL; -- Ignorar erro se não puder criar
END $$;

-- =====================================================
-- TABELAS PRINCIPAIS DO SISTEMA
-- =====================================================

-- Tabela: Clientes
CREATE TABLE IF NOT EXISTS clientes
(
	id serial not null primary key,
	razaosocial varchar(200) not null,
	cnpj varchar(20) not null,
	ativo boolean not null,
	logo TEXT,
	logo_bytes bytea,
	logo_content_type varchar(100),
	site_url VARCHAR(500)
);

-- Migração: Renomear tabela e colunas de Clientes para snake_case se necessário
DO $$
BEGIN
	-- Renomear tabela se existir com nome antigo
	IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Clientes') THEN
		IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'clientes') THEN
			ALTER TABLE "Clientes" RENAME TO clientes;
		END IF;
	END IF;
	
	-- Renomear colunas se necessário
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'clientes' AND column_name = 'Id') THEN
		ALTER TABLE clientes RENAME COLUMN "Id" TO id;
	END IF;
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'clientes' AND column_name = 'RazaoSocial') THEN
		ALTER TABLE clientes RENAME COLUMN "RazaoSocial" TO razaosocial;
	END IF;
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'clientes' AND column_name = 'Cnpj') THEN
		ALTER TABLE clientes RENAME COLUMN "Cnpj" TO cnpj;
	END IF;
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'clientes' AND column_name = 'Ativo') THEN
		ALTER TABLE clientes RENAME COLUMN "Ativo" TO ativo;
	END IF;
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'clientes' AND column_name = 'Logo') THEN
		ALTER TABLE clientes RENAME COLUMN "Logo" TO logo;
	END IF;
END $$;

-- Tabela: Usuarios
CREATE TABLE IF NOT EXISTS Usuarios
(
	Id serial not null primary key,
	Cliente int not null,
	Nome varchar(200) not null,
	Email varchar(200) not null unique,
	Senha varchar(200) not null,
	PalavraCriptografada varchar(200) not null,
	SU boolean not null,
	Adm boolean not null,
	Operador boolean not null,
	Consulta boolean not null,
	Ativo boolean not null,
	MigrateID int,
	ultimologin TIMESTAMP,
	two_factor_enabled BOOLEAN DEFAULT FALSE,
	two_factor_secret TEXT,
	two_factor_backup_codes TEXT,
	two_factor_last_used TIMESTAMP,
	constraint fkusuariocliente foreign key (cliente) references clientes(id)
);

-- =====================================================
-- TABELAS DE EQUIPAMENTOS
-- =====================================================

-- Tabela: TipoEquipamentos
CREATE TABLE IF NOT EXISTS TipoEquipamentos
(
	Id serial not null primary key,
	Descricao varchar(200) not null,
	Ativo BOOLEAN not null,
	TransitoLivre BOOLEAN not null default false,
	categoria_id INTEGER
);

-- Tabela: TipoEquipamentosClientes
CREATE TABLE IF NOT EXISTS TipoEquipamentosClientes
(
	Id serial not null primary key,
	Cliente int not null,
	Tipo int not null,
	constraint fktipoeqpcliente foreign key (cliente) references clientes(id),
	constraint fktipoeqpclientetipo foreign key (tipo) references tipoequipamentos(id),
	constraint uk_tipoequipamentocliente unique (Cliente, Tipo)
);

-- Tabela: Fabricantes
CREATE TABLE IF NOT EXISTS Fabricantes
(
	Id serial not null primary key,
	TipoEquipamento int not null,
	Cliente int not null,
	Descricao varchar(200) not null,
	Ativo boolean not null,
	MigrateID int,
	constraint fkfabricantecliente foreign key (cliente) references clientes(id),
	constraint fkFabricanteTipoEqp foreign key (TipoEquipamento) references TipoEquipamentos(Id)
);

-- Tabela: Modelos
CREATE TABLE IF NOT EXISTS Modelos
(
	Id serial not null primary key,
	Fabricante int not null,
	Cliente int not null,
	Descricao varchar(200) not null,
	QuantidadeEstoqueMinimo numeric,
	SetEstoqueMinimo boolean,
	Ativo boolean not null,
	MigrateID int,
	constraint fkmodeloscliente foreign key (cliente) references clientes(id),
	constraint fkModelosFabricantes foreign key (Fabricante) references Fabricantes(Id)
);

-- Tabela: EquipamentosStatus
CREATE TABLE IF NOT EXISTS EquipamentosStatus
(
	Id serial not null primary key,
	Descricao varchar(100) not null,
	Ativo boolean not null
);

-- =====================================================
-- TABELAS DE FORNECEDORES E CONTRATOS
-- =====================================================

-- Tabela: Fornecedores
CREATE TABLE IF NOT EXISTS fornecedores
(
	id serial not null primary key,
	cliente int not null,
	nome varchar(200) not null,
	cnpj varchar(20),
	ativo boolean not null,
	destinador_residuos boolean not null default false,
	migrateid int,
	constraint fkfornecedorcliente foreign key (cliente) references clientes(id)
);

-- Migração: Renomear tabela e colunas de Fornecedores para snake_case se necessário
DO $$
BEGIN
	-- Renomear tabela se existir com nome antigo
	IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Fornecedores') THEN
		IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'fornecedores') THEN
			ALTER TABLE "Fornecedores" RENAME TO fornecedores;
		END IF;
	END IF;
	
	-- Renomear colunas se necessário
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'fornecedores' AND column_name = 'Id') THEN
		ALTER TABLE fornecedores RENAME COLUMN "Id" TO id;
	END IF;
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'fornecedores' AND column_name = 'Cliente') THEN
		ALTER TABLE fornecedores RENAME COLUMN "Cliente" TO cliente;
	END IF;
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'fornecedores' AND column_name = 'Nome') THEN
		ALTER TABLE fornecedores RENAME COLUMN "Nome" TO nome;
	END IF;
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'fornecedores' AND column_name = 'CNPJ') THEN
		ALTER TABLE fornecedores RENAME COLUMN "CNPJ" TO cnpj;
	END IF;
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'fornecedores' AND column_name = 'Ativo') THEN
		ALTER TABLE fornecedores RENAME COLUMN "Ativo" TO ativo;
	END IF;
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'fornecedores' AND column_name = 'MigrateID') THEN
		ALTER TABLE fornecedores RENAME COLUMN "MigrateID" TO migrateid;
	END IF;
	
	-- Renomear constraint se necessário
	IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'fkFornecedorCliente' AND table_name = 'fornecedores') THEN
		ALTER TABLE fornecedores RENAME CONSTRAINT "fkFornecedorCliente" TO fkfornecedorcliente;
	END IF;
END $$;

-- Tabela: ContratoStatus
CREATE TABLE IF NOT EXISTS ContratoStatus (
	Id INT NOT NULL,
	Nome VARCHAR(100),
	CONSTRAINT PK_StatusContrato PRIMARY KEY(Id)
);

-- Tabela: Contratos
CREATE TABLE IF NOT EXISTS Contratos (
    Id SERIAL NOT NULL,
	Cliente INT not null,
	Fornecedor INT NOT NULL,
	Numero INT,
	Aditivo INT,
	Descricao VARCHAR(100),
	DTInicioVigencia TIMESTAMP NOT NULL,
	DTFinalVigencia TIMESTAMP,
	Valor MONEY,
	Status INT NOT NULL,
	GeraNF BOOLEAN NOT NULL,
	DTCriacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	UsuarioCriacao INT NOT NULL,
	DTExclusao TIMESTAMP,
	UsuarioExclusao INT,
	ContratoPai INT,
	Renovavel BOOLEAN,
	ArquivoNome VARCHAR(255),
	ArquivoCaminho VARCHAR(500),
	ArquivoTamanho BIGINT,
	ArquivoTipo VARCHAR(100),
	ArquivoDataUpload TIMESTAMP,
	ArquivoUsuarioUpload INT,
	ArquivoDataRemocao TIMESTAMP,
	ArquivoUsuarioRemocao INT,
	arquivocontrato VARCHAR(500),
	nomearquivooriginal VARCHAR(255),
	datauploadarquivo TIMESTAMP,
	usuariouploadarquivo INT,
	usuarioremocaoarquivo INT,
	dataremocaoarquivo TIMESTAMP,
	CONSTRAINT PK_Contrato PRIMARY KEY(Id),
	CONSTRAINT FK_Contrato_Clientes FOREIGN KEY (Cliente) references clientes(id),
	CONSTRAINT FK_Contrato_Fornecedor FOREIGN KEY (Fornecedor) REFERENCES Fornecedores(Id),
	CONSTRAINT FK_Contrato_Status FOREIGN KEY (Status) REFERENCES ContratoStatus(Id),
	CONSTRAINT FK_Contrato_ContratoPai FOREIGN KEY (ContratoPai) REFERENCES Contratos(Id),
	CONSTRAINT FK_Contrato_UsuarioCriacao FOREIGN KEY (UsuarioCriacao) REFERENCES Usuarios(Id),
	CONSTRAINT FK_Contrato_UsuarioExclusao FOREIGN KEY (UsuarioExclusao) REFERENCES Usuarios(Id),
	CONSTRAINT FK_Contrato_ArquivoUsuarioUpload FOREIGN KEY (ArquivoUsuarioUpload) REFERENCES Usuarios(Id),
	CONSTRAINT FK_Contrato_ArquivoUsuarioRemocao FOREIGN KEY (ArquivoUsuarioRemocao) REFERENCES Usuarios(Id)
);

-- =====================================================
-- TABELAS DE NOTAS FISCAIS
-- =====================================================

-- Tabela: TipoAquisicao
CREATE TABLE IF NOT EXISTS TipoAquisicao (
    Id INT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL
);

-- Tabela: NotasFiscais (notasfiscais em snake_case)
CREATE TABLE IF NOT EXISTS notasfiscais 
(
	id serial not null primary key,
	cliente int not null,
	fornecedor int not null,
	numero int not null,
	dtemissao TIMESTAMP not null,
	descricao varchar(500),
	valor money,
	contrato int,
	virtual boolean not null,
	gerouequipamento boolean not null,
	migrateid int,
	arquivonome VARCHAR(255),
	arquivocaminho VARCHAR(500),
	arquivotamanho BIGINT,
	arquivotipo VARCHAR(100),
	arquivodataupload TIMESTAMP,
	arquivousuarioupload INT,
	arquivonotafiscal VARCHAR(500),
	nomearquivooriginal VARCHAR(255),
	datauploadarquivo TIMESTAMP,
	usuariouploadarquivo INT,
	usuarioremocaoarquivo INT,
	dataremocaoarquivo TIMESTAMP,
	constraint fknfcliente foreign key (cliente) references clientes(id),
	constraint fknffornecedor foreign key (fornecedor) references fornecedores(id),
	constraint fknfcontrato foreign key (contrato) references contratos(id),
	CONSTRAINT fk_notasfiscais_arquivousuarioupload FOREIGN KEY (arquivousuarioupload) REFERENCES usuarios(id),
	CONSTRAINT fk_notasfiscais_usuariouploadarquivo FOREIGN KEY (usuariouploadarquivo) REFERENCES usuarios(id),
	CONSTRAINT fk_notasfiscais_usuarioremocaoarquivo FOREIGN KEY (usuarioremocaoarquivo) REFERENCES usuarios(id)
);

-- Migração: Renomear tabela NotasFiscais para notasfiscais se necessário
DO $$
BEGIN
	IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'NotasFiscais') THEN
		IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'notasfiscais') THEN
			ALTER TABLE "NotasFiscais" RENAME TO notasfiscais;
			-- Renomear colunas
			ALTER TABLE notasfiscais RENAME COLUMN "Id" TO id;
			ALTER TABLE notasfiscais RENAME COLUMN "Cliente" TO cliente;
			ALTER TABLE notasfiscais RENAME COLUMN "Fornecedor" TO fornecedor;
			ALTER TABLE notasfiscais RENAME COLUMN "Numero" TO numero;
			ALTER TABLE notasfiscais RENAME COLUMN "DtEmissao" TO dtemissao;
			ALTER TABLE notasfiscais RENAME COLUMN "Descricao" TO descricao;
			ALTER TABLE notasfiscais RENAME COLUMN "Valor" TO valor;
			ALTER TABLE notasfiscais RENAME COLUMN "Contrato" TO contrato;
			ALTER TABLE notasfiscais RENAME COLUMN "Virtual" TO virtual;
			ALTER TABLE notasfiscais RENAME COLUMN "GerouEquipamento" TO gerouequipamento;
			ALTER TABLE notasfiscais RENAME COLUMN "MigrateID" TO migrateid;
		END IF;
	END IF;
EXCEPTION
	WHEN OTHERS THEN
		RAISE NOTICE 'Erro na migração de NotasFiscais: %', SQLERRM;
END $$;

-- Tabela: NotasFiscaisItens (notasfiscaisitens em snake_case)
CREATE TABLE IF NOT EXISTS notasfiscaisitens
(
	id serial not null primary key,
	notafiscal int not null,
	tipoequipamento int not null,
	fabricante int not null,
	modelo int not null,
	quantidade int not null,
	valorunitario money not null,
	tipoaquisicao int not null,
	dtlimitegarantia TIMESTAMP,,
	contrato int,
	constraint fknfinotafiscal foreign key (notafiscal) references notasfiscais(id),
	constraint fknfitipoeqp foreign key (tipoequipamento) references tipoequipamentos(id),
	constraint fknfifabricante foreign key (fabricante) references fabricantes(id),
	constraint fknfimodelo foreign key (modelo) references modelos(id),
	constraint fknfitipoaquisicao foreign key (tipoaquisicao) references tipoaquisicao(id),
	constraint fknficontrato foreign key (contrato) references contratos(id)
);

-- Migração: Renomear tabela NotasFiscaisItens para notasfiscaisitens se necessário
DO $$
BEGIN
	IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'NotasFiscaisItens') THEN
		IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'notasfiscaisitens') THEN
			ALTER TABLE "NotasFiscaisItens" RENAME TO notasfiscaisitens;
			-- Renomear colunas
			ALTER TABLE notasfiscaisitens RENAME COLUMN "Id" TO id;
			ALTER TABLE notasfiscaisitens RENAME COLUMN "NotaFiscal" TO notafiscal;
			ALTER TABLE notasfiscaisitens RENAME COLUMN "TipoEquipamento" TO tipoequipamento;
			ALTER TABLE notasfiscaisitens RENAME COLUMN "Fabricante" TO fabricante;
			ALTER TABLE notasfiscaisitens RENAME COLUMN "Modelo" TO modelo;
			ALTER TABLE notasfiscaisitens RENAME COLUMN "Quantidade" TO quantidade;
			ALTER TABLE notasfiscaisitens RENAME COLUMN "ValorUnitario" TO valorunitario;
			ALTER TABLE notasfiscaisitens RENAME COLUMN "TipoAquisicao" TO tipoaquisicao;
			ALTER TABLE notasfiscaisitens RENAME COLUMN "DtLimiteGarantia" TO dtlimitegarantia;
			ALTER TABLE notasfiscaisitens RENAME COLUMN "Contrato" TO contrato;
		END IF;
	END IF;
EXCEPTION
	WHEN OTHERS THEN
		RAISE NOTICE 'Erro na migração de NotasFiscaisItens: %', SQLERRM;
END $$;

-- =====================================================
-- TABELAS DE LOCALIZAÇÃO
-- =====================================================

-- Tabela: Estados
CREATE TABLE IF NOT EXISTS estados (
    id SERIAL PRIMARY KEY,
    sigla VARCHAR(2) NOT NULL UNIQUE,
    nome VARCHAR(50) NOT NULL,
    ativo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: Cidades
CREATE TABLE IF NOT EXISTS cidades (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    estado_id INTEGER NOT NULL,
    ativo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_cidade_estado FOREIGN KEY (estado_id) REFERENCES estados(id)
);

-- Tabela: Localidades
CREATE TABLE IF NOT EXISTS Localidades
(
	Id serial not null primary key,
	Cliente int not null,
	Descricao varchar(300) not null default '',
	Cidade varchar(100),
	Estado varchar(50),
	Ativo boolean not null,
	MigrateID int,
	constraint fkLocalidadeCliente foreign key (Cliente) references clientes(id)
);

-- =====================================================
-- TABELAS DE EMPRESAS E CENTROS DE CUSTO
-- =====================================================

-- Tabela: Empresas
CREATE TABLE IF NOT EXISTS Empresas
(
	Id serial not null primary key,
	Cliente int not null,
	Nome varchar(250) not null,
	Cnpj varchar(20) not null,
	Localidade_Id int,
	MigrateID int,
	created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	constraint fkEmpresaCliente foreign key (Cliente) references clientes(id),
	constraint fkEmpresaLocalidade foreign key (Localidade_Id) references Localidades(Id)
);

-- Tabela: CentroCusto
CREATE TABLE IF NOT EXISTS CentroCusto 
(
	Id serial not null primary key,	
	Empresa int not null,
	Codigo varchar(10) not null,
	Nome varchar(100) not null,
	Filial_Id int,
	MigrateID int,
	Ativo boolean default true,
	created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	constraint fkCCEmpresa foreign key (Empresa) references Empresas(Id)
	-- NOTA: FK para Filiais será adicionada após criação da tabela Filiais
);

-- =====================================================
-- TABELAS DE COLABORADORES
-- =====================================================

-- Tabela: Colaboradores
CREATE TABLE IF NOT EXISTS Colaboradores
(
	Id serial not null primary key,
	Cliente int,
	Usuario int not null,
	Empresa int not null,
	CentroCusto int not null,
	Localidade int not null,
	Localidade_Id int,
	Filial_Id int,
	Nome varchar(300) not null,
	CPF varchar(50) not null unique,
	Matricula varchar(50) not null,
	Email varchar(300) not null,
	Cargo varchar(100),
	Setor varchar(100),
	DtAdmissao timestamp not null,
	Dtdemissao timestamp,
	TipoColaborador char(1), --F: Funcionario, T: Terceirizado, C: Consultor
	DtCadastro timestamp,
	DtAtualizacao timestamp,
	Situacao char(1), --A:Ativo, D:Desligado, F:Ferias
	AntigaEmpresa int,
	AntigoCentroCusto int,
	AntigaLocalidade int,
	MatriculaSuperior varchar(50),
	DtAtualizacaoLocalidade timestamp,
	DtAtualizacaoEmpresa timestamp,
	DtAtualizacaoCentroCusto timestamp,
	SituacaoAntiga char(1),
	MigrateID int,
	constraint fkColaboradorUsuario foreign key (Usuario) references Usuarios(Id),
	constraint fkColaboradorEmpresa foreign key (Empresa) references Empresas(Id),
	constraint fkColaboradorCentroCusto foreign key (CentroCusto) references CentroCusto(Id),
	constraint fkColaboradorLocalidade foreign key (Localidade) references Localidades(Id),
	constraint fkColaboradorLocalidadeId foreign key (Localidade_Id) references Localidades(Id),
	constraint fkColaboradorFilial foreign key (Filial_Id) references Filiais(Id),
	constraint fkColaboradorCliente foreign key (Cliente) references clientes(id)
);

-- Adicionar colunas faltantes em Colaboradores se a tabela já existir (migração)
DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'colaboradores' AND column_name = 'localidade_id') THEN
		ALTER TABLE colaboradores ADD COLUMN localidade_id INTEGER;
		ALTER TABLE colaboradores ADD CONSTRAINT fkColaboradorLocalidadeId FOREIGN KEY (localidade_id) REFERENCES localidades(id);
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'colaboradores' AND column_name = 'filial_id') THEN
		ALTER TABLE colaboradores ADD COLUMN filial_id INTEGER;
		ALTER TABLE colaboradores ADD CONSTRAINT fkColaboradorFilial FOREIGN KEY (filial_id) REFERENCES filiais(id);
	END IF;

	-- Tornar Cliente nullable se ainda não for
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'colaboradores' AND column_name = 'cliente' AND is_nullable = 'NO') THEN
		ALTER TABLE colaboradores ALTER COLUMN cliente DROP NOT NULL;
	END IF;
END $$;

-- =====================================================
-- TABELAS DE TELEFONIA
-- =====================================================

-- Tabela: TelefoniaOperadoras
CREATE TABLE IF NOT EXISTS TelefoniaOperadoras
(
	Id serial not null primary key,
	Nome varchar(100) not null,
	MigrateID int,
	Ativo boolean not null
);

-- Tabela: TelefoniaContratos
CREATE TABLE IF NOT EXISTS TelefoniaContratos
(
	Id serial not null primary key,
	Cliente int not null,
	Operadora int not null,
	Nome varchar(100) not null,
	Descricao varchar(250),
	MigrateID int,
	Ativo boolean not null,
	constraint fkTelContratoCliente foreign key (Cliente) references clientes(id),
	constraint fktelContratoOperadora foreign key (Operadora) references TelefoniaOperadoras(Id)
);

-- Tabela: TelefoniaPlanos
CREATE TABLE IF NOT EXISTS TelefoniaPlanos
(
	Id serial not null primary key,
	Contrato int not null,
	Nome varchar(150) not null,
	Valor money not null,
	MigrateID int,
	Ativo boolean not null,
	constraint fkTelefoniaPlanosContrato foreign key (Contrato) references TelefoniaContratos(Id)
);

-- Tabela: TelefoniaLinhas
CREATE TABLE IF NOT EXISTS TelefoniaLinhas
(
	Id serial not null primary key,
	Plano int not null,
	Numero numeric not null,
	ICCID varchar(500),
	EmUso boolean not null,
	MigrateID int,
	Ativo boolean not null,
	constraint fkLinhaPlano foreign key (Plano) references TelefoniaPlanos(Id)
);

-- =====================================================
-- TABELAS DE EQUIPAMENTOS (CONTINUAÇÃO)
-- =====================================================

-- Tabela: Equipamentos
CREATE TABLE IF NOT EXISTS Equipamentos
(
	Id serial not null primary key,
	Cliente int,
	TipoEquipamento int not null,
	Fabricante int not null,
	Modelo int not null,
	NotaFiscal int,
	Contrato int,
	EquipamentoStatus int,
	Usuario int,
	Localidade_Id int,
	Filial_Id int,
	TipoAquisicao int not null,
	Fornecedor int,
	PossuiBO boolean not null,
	DescricaoBO TEXT,
	NumeroSerie varchar(100) not null,
	Patrimonio varchar(100),
	DtLimiteGarantia TIMESTAMP,
	DtCadastro TIMESTAMP not null,
	Empresa int,
	CentroCusto int,
	Ativo boolean not null,
	MigrateID int,
	enviouEmailReporte boolean default false,
	compartilhado BOOLEAN DEFAULT FALSE NOT NULL,
	constraint fkEquipamentoCliente foreign key (Cliente) references clientes(id),
	constraint fkEquipamentoTipoEqp foreign key (TipoEquipamento) references TipoEquipamentos(Id),
	constraint fkEquipamentoFabricante foreign key (Fabricante) references Fabricantes(Id),
	constraint fkEquipamentoModelo foreign key (Modelo) references Modelos(Id),
	constraint fkEquipamentoNotaFiscal foreign key (NotaFiscal) references NotasFiscais(Id),
	constraint fkEquipamentoContrato foreign key (Contrato) references Contratos(Id),
	constraint fkEquipamentoStatus foreign key (EquipamentoStatus) references EquipamentosStatus(Id),
	constraint fkEquipamentoUsuario foreign key (Usuario) references Usuarios(Id),
	constraint fkEquipamentoLocalidadeId foreign key (Localidade_Id) references Localidades(Id),
	constraint fkEquipamentoFilial foreign key (Filial_Id) references Filiais(Id),
	constraint fkEquipamentoEmpresa foreign key (Empresa) references Empresas(Id),
	constraint fkEquipamentoCentro foreign key (CentroCusto) references centrocusto(Id),
	constraint fkEquipamentoFornecedor foreign key (Fornecedor) references Fornecedores(Id),
	constraint fkEquipamentoTipoAquisicao foreign key (TipoAquisicao) references TipoAquisicao(Id)
);

-- Adicionar colunas faltantes em Equipamentos se a tabela já existir (migração)
DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'equipamentos' AND column_name = 'localidade_id') THEN
		ALTER TABLE equipamentos ADD COLUMN localidade_id INTEGER;
		ALTER TABLE equipamentos ADD CONSTRAINT fkEquipamentoLocalidadeId FOREIGN KEY (localidade_id) REFERENCES localidades(id);
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'equipamentos' AND column_name = 'filial_id') THEN
		ALTER TABLE equipamentos ADD COLUMN filial_id INTEGER;
		ALTER TABLE equipamentos ADD CONSTRAINT fkEquipamentoFilial FOREIGN KEY (filial_id) REFERENCES filiais(id);
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'equipamentos' AND column_name = 'compartilhado') THEN
		ALTER TABLE equipamentos ADD COLUMN compartilhado BOOLEAN DEFAULT FALSE NOT NULL;
	END IF;

	-- Tornar Cliente nullable se ainda não for
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'equipamentos' AND column_name = 'cliente' AND is_nullable = 'NO') THEN
		ALTER TABLE equipamentos ALTER COLUMN cliente DROP NOT NULL;
	END IF;
END $$;

-- Tabela: Equipamento Usuários Compartilhados
CREATE TABLE IF NOT EXISTS equipamento_usuarios_compartilhados (
    id SERIAL PRIMARY KEY,
    equipamento_id INTEGER NOT NULL,
    colaborador_id INTEGER NOT NULL,
    data_inicio TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    data_fim TIMESTAMP NULL,
    ativo BOOLEAN DEFAULT TRUE NOT NULL,
    tipo_acesso VARCHAR(50) DEFAULT 'usuario_compartilhado' NOT NULL,
    observacao TEXT NULL,
    criado_por INTEGER NOT NULL,
    criado_em TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT fk_equip_usuarios_comp_equipamento 
        FOREIGN KEY (equipamento_id) 
        REFERENCES equipamentos(id) 
        ON DELETE CASCADE,
    
    CONSTRAINT fk_equip_usuarios_comp_colaborador 
        FOREIGN KEY (colaborador_id) 
        REFERENCES colaboradores(id) 
        ON DELETE CASCADE,
    
    CONSTRAINT fk_equip_usuarios_comp_criado_por 
        FOREIGN KEY (criado_por) 
        REFERENCES usuarios(id) 
        ON DELETE RESTRICT,
    
    CONSTRAINT chk_tipo_acesso 
        CHECK (tipo_acesso IN ('usuario_compartilhado', 'temporario', 'turno'))
    
    -- NOTA: Constraint UNIQUE NULLS NOT DISTINCT removida (requer PostgreSQL 15+)
    -- Para PostgreSQL 14, criar índice parcial único manualmente se necessário:
    -- CREATE UNIQUE INDEX uk_equipamento_colaborador_ativo 
    --   ON equipamento_usuarios_compartilhados(equipamento_id, colaborador_id) 
    --   WHERE ativo = TRUE;
);

-- =====================================================
-- TABELAS DE LAUDOS E ANEXOS
-- =====================================================

-- Tabela: Laudos
CREATE TABLE IF NOT EXISTS Laudos 
(
	Id serial not null primary key,
	Cliente int not null,
	Equipamento int not null,
	Usuario int not null,
	Tecnico int not null,
	Descricao text not null,
	Laudo text,
	DtEntrada timestamp not null,
	DtLaudo timestamp,
	TemConserto boolean not null,
	MauUso boolean not null,
	ValorManutencao decimal(10,2),
	Ativo boolean not null,
	constraint fkLaudoCliente foreign key (Cliente) references Clientes(id),
	constraint fkLaudoEquipamento foreign key (Equipamento) references equipamentos(id),
	constraint fkLaudoUsuario foreign key (Usuario) references Usuarios(Id),
	constraint fkLaudoTecnico foreign key (Tecnico) references Usuarios(Id)
);

-- Tabela: EquipamentoAnexos
CREATE TABLE IF NOT EXISTS EquipamentoAnexos
(
	Id serial not null primary key,
	Equipamento int not null,
	Usuario int not null,
	Laudo int,
	Arquivo text,
	Nome varchar(100) not null,
	isBO boolean not null,
	isLaudo boolean not null,
	DtRegistro timestamp not null,
	constraint fkEquipamentoAnexoEquipamento foreign key (Equipamento) references Equipamentos(Id),
	constraint fkEquipamentoAnexoUsuario foreign key (Usuario) references Usuarios(Id),
	constraint fkEquipamentoAnexoLaudo foreign key (Laudo) references Laudos(Id)
);

-- =====================================================
-- TABELAS DE REQUISIÇÕES
-- =====================================================

-- Tabela: RequisicoesStatus
CREATE TABLE IF NOT EXISTS RequisicoesStatus
(
	Id serial not null primary key,
	Descricao varchar(100) not null,
	Ativo boolean not null
);

-- Tabela: Requisicoes
CREATE TABLE IF NOT EXISTS Requisicoes
(
	Id serial not null primary key,
	Cliente int not null,
	UsuarioRequisicao int not null,
	TecnicoResponsavel int not null,
	RequisicaoStatus int not null,
	ColaboradorFinal int,
	DtSolicitacao timestamp,
	DtProcessamento timestamp,
	AssinaturaEletronica boolean not null,
	DtAssinaturaEletronica timestamp,
	conteudo_template_assinado TEXT,
	tipo_termo_assinado int,
	versao_template_assinado int,
	DtEnvioTermo timestamp,
	HashRequisicao varchar(200) not null,
	MigrateID int,
	constraint fkRequisicaoCliente foreign key (Cliente) references clientes(id),
	constraint fkRequisicaoUsuario foreign key (UsuarioRequisicao) references Usuarios(Id),
	constraint fkRequisicaoTecnico foreign key (TecnicoResponsavel) references Usuarios(Id),
	constraint fkRequisicaoStatus foreign key (RequisicaoStatus) references RequisicoesStatus(Id),
	constraint fkRequisicaoColaborador foreign key (ColaboradorFinal) references Colaboradores(Id)
);

-- Adicionar colunas faltantes em Requisicoes se a tabela já existir (migração)
DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'requisicoes' AND column_name = 'conteudo_template_assinado') THEN
		ALTER TABLE requisicoes ADD COLUMN conteudo_template_assinado TEXT;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'requisicoes' AND column_name = 'tipo_termo_assinado') THEN
		ALTER TABLE requisicoes ADD COLUMN tipo_termo_assinado INTEGER;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'requisicoes' AND column_name = 'versao_template_assinado') THEN
		ALTER TABLE requisicoes ADD COLUMN versao_template_assinado INTEGER;
	END IF;

	-- Tornar DtSolicitacao nullable se ainda não for
	IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'requisicoes' AND column_name = 'dtsolicitacao' AND is_nullable = 'NO') THEN
		ALTER TABLE requisicoes ALTER COLUMN dtsolicitacao DROP NOT NULL;
	END IF;
END $$;

-- Tabela: RequisicoesItens
CREATE TABLE IF NOT EXISTS RequisicoesItens
(
	Id serial not null primary key,
	Requisicao int not null,
	Equipamento int,
	LinhaTelefonica int,
	UsuarioEntrega int,
	UsuarioDevolucao int,
	DtEntrega timestamp,
	DtDevolucao timestamp,
	ObservacaoEntrega varchar(500),
	DtProgramadaRetorno timestamp,
	constraint fkRIRequisicao foreign key (Requisicao) references Requisicoes(Id),
	constraint fkRIEquipamento foreign key (Equipamento) references Equipamentos(Id),
	constraint fkRILinhaTelefonica foreign key (LinhaTelefonica) references TelefoniaLinhas(Id),
	constraint fkRIUsuarioEntrega foreign key (UsuarioEntrega) references Usuarios(Id),
	constraint fkRIUsuarioDevolucao foreign key (UsuarioDevolucao) references Usuarios(Id)
);

-- Tabela: EquipamentoHistorico
CREATE TABLE IF NOT EXISTS EquipamentoHistorico
(
	Id serial not null primary key,
	Equipamento int not null,
	EquipamentoStatus int not null,
	Usuario int not null,
	LinhaTelefonica int,
	LinhaEmUso boolean,
	Requisicao int,
	Colaborador int,
	DtRegistro timestamp not null,
	constraint fkEqpHistoricoEquipamento foreign key (Equipamento) references Equipamentos(Id),
	constraint fkEqpHistoricoStatus foreign key (EquipamentoStatus) references EquipamentosStatus(Id),
	constraint fkEqpHistoricoUsuario foreign key (Usuario) references Usuarios(Id),
	constraint fkEqpHistoricoColaborador foreign key (Colaborador) references Colaboradores(Id),
	constraint fkEqpHistoricoTelefonialinha foreign key (LinhaTelefonica) references Telefonialinhas(Id),
	constraint fkEqpHistoricoRequisicao foreign key (Requisicao) references Requisicoes(Id)
);

-- =====================================================
-- TABELAS DE TEMPLATES
-- =====================================================

-- Tabela: TemplateTipos
CREATE TABLE IF NOT EXISTS TemplateTipos
(
	Id int not null primary key,
	Descricao varchar(100) not null
);

-- Tabela: Templates
CREATE TABLE IF NOT EXISTS Templates
(
	Id serial not null primary key,
	Tipo int not null,
	Cliente int not null,
	Titulo varchar(100) not null,
	Conteudo text not null,
	Versao int not null,
	DataCriacao timestamp not null,
	DataAlteracao timestamp,
	Ativo boolean not null,
	constraint fkTemplatesTipo foreign key (Tipo) references TemplateTipos(Id),
	constraint fkTemplateCliente foreign key (Cliente) references clientes(id)
);

-- Tabela: regrasTemplate
CREATE TABLE IF NOT EXISTS regrasTemplate
(
	Id SERIAL PRIMARY KEY NOT NULL,
	TipoTemplate INT NOT NULL,
	TipoAquisicao INT NOT NULL,
	CONSTRAINT fkRegrasTemplateTipoTemplate FOREIGN KEY (TipoTemplate) REFERENCES TemplateTipos(Id),
	CONSTRAINT fkRegrasTemplateTipoAquisicao FOREIGN KEY (TipoAquisicao) REFERENCES TipoAquisicao(Id),
	CONSTRAINT ukRegrasTemplate UNIQUE (TipoTemplate, TipoAquisicao)
);

-- =====================================================
-- TABELAS DE DESCARTE
-- =====================================================

-- Tabela: DescarteCargos
CREATE TABLE IF NOT EXISTS DescarteCargos
(
	Id serial not null primary key,
	Cliente int not null,
	Cargo varchar(500) not null,
	constraint fkCargoCliente foreign key (Cliente) references clientes(id)
);

-- Tabela: CargosConfianca
CREATE TABLE IF NOT EXISTS cargosconfianca (
    id SERIAL PRIMARY KEY,
    cliente INTEGER NOT NULL,
    cargo VARCHAR(200) NOT NULL,
    usarpadrao BOOLEAN NOT NULL DEFAULT true,
    nivelcriticidade VARCHAR(20) NOT NULL DEFAULT 'ALTO',
    obrigarsanitizacao BOOLEAN NOT NULL DEFAULT false,
    obrigardescaracterizacao BOOLEAN NOT NULL DEFAULT false,
    obrigarperfuracaodisco BOOLEAN NOT NULL DEFAULT false,
    obrigarevidencias BOOLEAN NOT NULL DEFAULT false,
    ativo BOOLEAN NOT NULL DEFAULT true,
    usuariocriacao INTEGER NOT NULL,
    datacriacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    usuarioalteracao INTEGER,
    dataalteracao TIMESTAMP,
    
    CONSTRAINT fk_cargoconfianca_cliente FOREIGN KEY (cliente) REFERENCES clientes(id),
    CONSTRAINT fk_cargoconfianca_usuario_criacao FOREIGN KEY (usuariocriacao) REFERENCES usuarios(id),
    CONSTRAINT fk_cargoconfianca_usuario_alteracao FOREIGN KEY (usuarioalteracao) REFERENCES usuarios(id)
);

-- Tabela: Protocolos de Descarte
CREATE TABLE IF NOT EXISTS protocolos_descarte (
    id SERIAL PRIMARY KEY,
    protocolo VARCHAR(20) UNIQUE NOT NULL,
    cliente INTEGER NOT NULL,
    tipo_descarte VARCHAR(50) NOT NULL,
    motivo_descarte TEXT,
    destino_final VARCHAR(500),
    responsavel_protocolo INTEGER NOT NULL,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    data_conclusao TIMESTAMP,
    status VARCHAR(30) DEFAULT 'EM_ANDAMENTO',
    valor_total_estimado DECIMAL(10,2),
    documento_gerado BOOLEAN DEFAULT false,
    caminho_documento VARCHAR(500),
    observacoes TEXT,
    ativo BOOLEAN DEFAULT true,
    
    CONSTRAINT fk_protocolos_descarte_cliente FOREIGN KEY (cliente) REFERENCES clientes(id),
    CONSTRAINT fk_protocolos_descarte_responsavel FOREIGN KEY (responsavel_protocolo) REFERENCES usuarios(id)
);

-- Tabela: Protocolo Descarte Itens
CREATE TABLE IF NOT EXISTS protocolo_descarte_itens (
    id SERIAL PRIMARY KEY,
    protocolo_id INTEGER NOT NULL,
    equipamento INTEGER NOT NULL,
    processo_sanitizacao BOOLEAN DEFAULT false,
    processo_descaracterizacao BOOLEAN DEFAULT false,
    processo_perfuracao_disco BOOLEAN DEFAULT false,
    evidencias_obrigatorias BOOLEAN DEFAULT false,
    evidencias_executadas BOOLEAN DEFAULT false,
    valor_estimado DECIMAL(10,2),
    observacoes_item TEXT,
    data_processo_iniciado TIMESTAMP,
    data_processo_concluido TIMESTAMP,
    status_item VARCHAR(30) DEFAULT 'PENDENTE',
    ativo BOOLEAN DEFAULT true,
    processos_obrigatorios BOOLEAN DEFAULT FALSE,
    obrigar_sanitizacao BOOLEAN DEFAULT FALSE,
    obrigar_descaracterizacao BOOLEAN DEFAULT FALSE,
    obrigar_perfuracao_disco BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT fk_protocolo_itens_protocolo FOREIGN KEY (protocolo_id) REFERENCES protocolos_descarte(id) ON DELETE CASCADE,
    CONSTRAINT fk_protocolo_itens_equipamento FOREIGN KEY (equipamento) REFERENCES equipamentos(id)
);

-- Tabela: Descarte Evidências
CREATE TABLE IF NOT EXISTS descarteevidencias (
    id SERIAL PRIMARY KEY,
    equipamento INTEGER NOT NULL,
    descricao VARCHAR(500),
    tipoprocesso VARCHAR(50) NOT NULL,
    nomearquivo VARCHAR(255) NOT NULL,
    caminhoarquivo VARCHAR(500) NOT NULL,
    tipoarquivo VARCHAR(100),
    tamanhoarquivo BIGINT,
    usuarioupload INTEGER NOT NULL,
    dataupload TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ativo BOOLEAN DEFAULT true,
    protocolo_id INTEGER NULL,
    
    CONSTRAINT fk_descarteevidencias_equipamento FOREIGN KEY (equipamento) 
        REFERENCES equipamentos(id) ON DELETE CASCADE,
    CONSTRAINT fk_descarteevidencias_usuario FOREIGN KEY (usuarioupload) 
        REFERENCES usuarios(id) ON DELETE RESTRICT,
    CONSTRAINT fk_descarteevidencias_protocolo FOREIGN KEY (protocolo_id) 
        REFERENCES protocolos_descarte(id)
);

-- =====================================================
-- TABELAS DE POL´┐¢TICAS E ELEGIBILIDADE
-- =====================================================

-- Tabela: Pol´┐¢ticas de Elegibilidade
CREATE TABLE IF NOT EXISTS politicas_elegibilidade (
    id SERIAL PRIMARY KEY,
    cliente INTEGER NOT NULL,
    tipo_colaborador VARCHAR(50) NOT NULL,
    cargo VARCHAR(100),
    tipo_equipamento_id INTEGER NOT NULL,
    permite_acesso BOOLEAN NOT NULL DEFAULT true,
    quantidade_maxima INTEGER,
    observacoes TEXT,
    usarpadrao BOOLEAN NOT NULL DEFAULT true,
    ativo BOOLEAN NOT NULL DEFAULT true,
    dt_cadastro TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    dt_atualizacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    usuario_cadastro INTEGER,
    
    CONSTRAINT fk_politica_cliente FOREIGN KEY (cliente) REFERENCES clientes(id) ON DELETE CASCADE,
    CONSTRAINT fk_politica_tipo_equipamento FOREIGN KEY (tipo_equipamento_id) REFERENCES tipoequipamentos(id) ON DELETE CASCADE,
    CONSTRAINT fk_politica_usuario FOREIGN KEY (usuario_cadastro) REFERENCES usuarios(id) ON DELETE SET NULL
);

-- Índice único para evitar duplicatas (tratando NULLs corretamente)
-- PostgreSQL trata múltiplos NULLs como valores distintos em UNIQUE, então precisamos de índices parciais
DO $$
BEGIN
    -- Remover constraint antiga se existir
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uk_politica_elegibilidade') THEN
        ALTER TABLE politicas_elegibilidade DROP CONSTRAINT uk_politica_elegibilidade;
    END IF;
    
    -- Criar índices únicos parciais
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'uk_politica_elegibilidade_com_cargo') THEN
        CREATE UNIQUE INDEX uk_politica_elegibilidade_com_cargo 
        ON politicas_elegibilidade (cliente, tipo_colaborador, cargo, tipo_equipamento_id)
        WHERE cargo IS NOT NULL;
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'uk_politica_elegibilidade_sem_cargo') THEN
        CREATE UNIQUE INDEX uk_politica_elegibilidade_sem_cargo 
        ON politicas_elegibilidade (cliente, tipo_colaborador, tipo_equipamento_id)
        WHERE cargo IS NULL;
    END IF;
END $$;

-- =====================================================
-- TABELAS DE PASSCHECK E PATRIM´┐¢NIO
-- =====================================================

-- Tabela: Patrim´┐¢nio Contestaçes
CREATE TABLE IF NOT EXISTS patrimonio_contestoes (
    id SERIAL PRIMARY KEY,
    colaborador_id INTEGER NOT NULL REFERENCES colaboradores(id),
    equipamento_id INTEGER NOT NULL REFERENCES equipamentos(id),
    motivo TEXT NOT NULL,
    descricao TEXT,
    status VARCHAR(20) DEFAULT 'pendente' CHECK (status IN ('pendente', 'aprovada', 'rejeitada', 'negada')),
    tipo_contestacao VARCHAR(50) DEFAULT 'contestacao',
    evidencia_url VARCHAR(500),
    data_contestacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    data_resolucao TIMESTAMP NULL,
    usuario_resolucao INTEGER REFERENCES usuarios(id),
    observacao_resolucao TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: Patrim´┐¢nio Logs de Acesso
CREATE TABLE IF NOT EXISTS patrimonio_logs_acesso (
    id SERIAL PRIMARY KEY,
    tipo_acesso VARCHAR(20) NOT NULL CHECK (tipo_acesso IN ('passcheck', 'patrimonio')),
    colaborador_id INTEGER REFERENCES colaboradores(id),
    cpf_consultado VARCHAR(14),
    ip_address INET,
    user_agent TEXT,
    dados_consultados JSONB,
    sucesso BOOLEAN DEFAULT true,
    mensagem_erro TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- TABELAS DE SINALIZAÇÕES DE SUSPEITAS
-- =====================================================

-- Tabela: Motivos de Suspeita
CREATE TABLE IF NOT EXISTS motivos_suspeita (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) UNIQUE NOT NULL,
    descricao VARCHAR(100) NOT NULL,
    descricao_detalhada TEXT,
    ativo BOOLEAN DEFAULT true,
    prioridade_padrao VARCHAR(10) DEFAULT 'media' CHECK (prioridade_padrao IN ('baixa', 'media', 'alta', 'critica')),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: Sinalizaçes de Suspeitas
CREATE TABLE IF NOT EXISTS sinalizacoes_suspeitas (
    id SERIAL PRIMARY KEY,
    colaborador_id INTEGER NOT NULL REFERENCES colaboradores(id),
    vigilante_id INTEGER REFERENCES usuarios(id),
    cpf_consultado VARCHAR(14) NOT NULL,
    motivo_suspeita VARCHAR(50) NOT NULL,
    descricao_detalhada TEXT,
    observacoes_vigilante TEXT,
    status VARCHAR(20) DEFAULT 'pendente' CHECK (status IN ('pendente', 'em_investigacao', 'resolvida', 'arquivada')),
    prioridade VARCHAR(10) DEFAULT 'media' CHECK (prioridade IN ('baixa', 'media', 'alta', 'critica')),
    dados_consulta JSONB,
    ip_address INET,
    user_agent TEXT,
    evidencia_urls TEXT[],
    data_sinalizacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    data_investigacao TIMESTAMP NULL,
    data_resolucao TIMESTAMP NULL,
    investigador_id INTEGER REFERENCES usuarios(id),
    resultado_investigacao TEXT,
    acoes_tomadas TEXT,
    observacoes_finais TEXT,
    nome_vigilante VARCHAR(100),
    numero_protocolo VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Adicionar colunas faltantes em sinalizacoes_suspeitas se a tabela já existir (migração)
DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sinalizacoes_suspeitas' AND column_name = 'nome_vigilante') THEN
		ALTER TABLE sinalizacoes_suspeitas ADD COLUMN nome_vigilante VARCHAR(100);
	END IF;
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sinalizacoes_suspeitas' AND column_name = 'numero_protocolo') THEN
		ALTER TABLE sinalizacoes_suspeitas ADD COLUMN numero_protocolo VARCHAR(20);
	END IF;
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sinalizacoes_suspeitas' AND column_name = 'created_at') THEN
		ALTER TABLE sinalizacoes_suspeitas ADD COLUMN created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;
	END IF;
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'sinalizacoes_suspeitas' AND column_name = 'updated_at') THEN
		ALTER TABLE sinalizacoes_suspeitas ADD COLUMN updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;
	END IF;
END $$;

-- Tabela: Histórico de Investigações
CREATE TABLE IF NOT EXISTS historico_investigacoes (
    id SERIAL PRIMARY KEY,
    sinalizacao_id INTEGER NOT NULL REFERENCES sinalizacoes_suspeitas(id) ON DELETE CASCADE,
    usuario_id INTEGER NOT NULL REFERENCES usuarios(id),
    acao VARCHAR(50) NOT NULL,
    descricao TEXT,
    dados_antes JSONB,
    dados_depois JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- TABELAS DE PARÂMETROS E PROCESSAMENTOS
-- =====================================================

-- Tabela: Parametros
CREATE TABLE IF NOT EXISTS PARAMETROS 
(
	Id serial primary key not null,
	Cliente int not null,
	EmailReporte varchar(300),
	-- Configuração de E-mail para Descontos
	email_descontos_enabled BOOLEAN DEFAULT false,
	-- Configurações de SMTP
	smtp_enabled BOOLEAN DEFAULT false,
	smtp_host VARCHAR(200),
	smtp_port INTEGER,
	smtp_login VARCHAR(200),
	smtp_password VARCHAR(200),
	smtp_enable_ssl BOOLEAN DEFAULT false,
	smtp_email_from VARCHAR(200),
	-- Configurações de 2FA (Duplo Fator)
	two_factor_enabled BOOLEAN DEFAULT false,
	two_factor_type VARCHAR(50) DEFAULT 'email',
	two_factor_expiration_minutes INTEGER DEFAULT 5,
	two_factor_max_attempts INTEGER DEFAULT 3,
	two_factor_lockout_minutes INTEGER DEFAULT 15,
	two_factor_email_template TEXT,
	constraint fkParametrosCliente foreign key (Cliente) references clientes(id)
);

-- Adicionar colunas faltantes se a tabela já existir (migração)
DO $$
BEGIN
	-- Configuração de E-mail para Descontos
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'email_descontos_enabled') THEN
		ALTER TABLE parametros ADD COLUMN email_descontos_enabled BOOLEAN DEFAULT false;
	END IF;

	-- Configurações de SMTP
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_enabled') THEN
		ALTER TABLE parametros ADD COLUMN smtp_enabled BOOLEAN DEFAULT false;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_host') THEN
		ALTER TABLE parametros ADD COLUMN smtp_host VARCHAR(200);
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_port') THEN
		ALTER TABLE parametros ADD COLUMN smtp_port INTEGER;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_login') THEN
		ALTER TABLE parametros ADD COLUMN smtp_login VARCHAR(200);
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_password') THEN
		ALTER TABLE parametros ADD COLUMN smtp_password VARCHAR(200);
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_enable_ssl') THEN
		ALTER TABLE parametros ADD COLUMN smtp_enable_ssl BOOLEAN DEFAULT false;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'smtp_email_from') THEN
		ALTER TABLE parametros ADD COLUMN smtp_email_from VARCHAR(200);
	END IF;

	-- Configurações de 2FA (Duplo Fator)
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'two_factor_enabled') THEN
		ALTER TABLE parametros ADD COLUMN two_factor_enabled BOOLEAN DEFAULT false;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'two_factor_type') THEN
		ALTER TABLE parametros ADD COLUMN two_factor_type VARCHAR(50) DEFAULT 'email';
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'two_factor_expiration_minutes') THEN
		ALTER TABLE parametros ADD COLUMN two_factor_expiration_minutes INTEGER DEFAULT 5;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'two_factor_max_attempts') THEN
		ALTER TABLE parametros ADD COLUMN two_factor_max_attempts INTEGER DEFAULT 3;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'two_factor_lockout_minutes') THEN
		ALTER TABLE parametros ADD COLUMN two_factor_lockout_minutes INTEGER DEFAULT 15;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parametros' AND column_name = 'two_factor_email_template') THEN
		ALTER TABLE parametros ADD COLUMN two_factor_email_template TEXT;
	END IF;
END $$;

-- Adicionar colunas faltantes em cargosconfianca se a tabela já existir (migração)
DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'cargosconfianca' AND column_name = 'usarpadrao') THEN
		ALTER TABLE cargosconfianca ADD COLUMN usarpadrao BOOLEAN NOT NULL DEFAULT true;
	END IF;
END $$;

-- Adicionar colunas faltantes em politicas_elegibilidade se a tabela já existir (migração)
DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'politicas_elegibilidade' AND column_name = 'usarpadrao') THEN
		ALTER TABLE politicas_elegibilidade ADD COLUMN usarpadrao BOOLEAN NOT NULL DEFAULT true;
	END IF;
END $$;

-- Tabela: ProcessamentosServicos
CREATE TABLE IF NOT EXISTS ProcessamentosServicos
(
	Id serial not null primary key,
	Nome varchar(50) not null,
	DtExecucao timestamp not null,
	Sucesso boolean not null,
	Excecao text
);

-- =====================================================
-- TABELAS ADICIONADAS EM v1.1.0 (31/10/2024)
-- =====================================================

-- Tabela: Campanhas de Assinatura
CREATE TABLE IF NOT EXISTS campanhasassinaturas (
    Id SERIAL PRIMARY KEY,
    Cliente INTEGER NOT NULL,
    UsuarioCriacao INTEGER NOT NULL,
    Nome VARCHAR(200) NOT NULL,
    Descricao TEXT,
    DataCriacao TIMESTAMP NOT NULL DEFAULT NOW(),
    DataInicio TIMESTAMP,
    DataFim TIMESTAMP,
    Status CHAR(1) NOT NULL DEFAULT 'A',
    FiltrosJson TEXT,
    TotalColaboradores INTEGER NOT NULL DEFAULT 0,
    TotalEnviados INTEGER NOT NULL DEFAULT 0,
    TotalAssinados INTEGER NOT NULL DEFAULT 0,
    TotalPendentes INTEGER NOT NULL DEFAULT 0,
    PercentualAdesao DECIMAL(5,2),
    DataUltimoEnvio TIMESTAMP,
    DataConclusao TIMESTAMP,
    CONSTRAINT fk_campanha_cliente FOREIGN KEY (Cliente) references clientes(id) ON DELETE CASCADE,
    CONSTRAINT fk_campanha_usuario FOREIGN KEY (UsuarioCriacao) REFERENCES Usuarios(Id)
);

-- Tabela: Campanhas Colaboradores
CREATE TABLE IF NOT EXISTS campanhascolaboradores (
    Id SERIAL PRIMARY KEY,
    CampanhaId INTEGER NOT NULL,
    ColaboradorId INTEGER NOT NULL,
    DataInclusao TIMESTAMP NOT NULL DEFAULT NOW(),
    StatusAssinatura CHAR(1) NOT NULL DEFAULT 'P',
    DataEnvio TIMESTAMP,
    DataAssinatura TIMESTAMP,
    TotalEnvios INTEGER DEFAULT 0,
    DataUltimoEnvio TIMESTAMP,
    IpEnvio VARCHAR(45),
    LocalizacaoEnvio TEXT,
    CONSTRAINT fk_campanha_colab_campanha FOREIGN KEY (CampanhaId) REFERENCES campanhasassinaturas(Id) ON DELETE CASCADE,
    CONSTRAINT fk_campanha_colab_colaborador FOREIGN KEY (ColaboradorId) REFERENCES Colaboradores(Id) ON DELETE CASCADE
);

-- Tabela: Estoque Mínimo Equipamentos
CREATE TABLE IF NOT EXISTS estoqueminimoequipamentos (
    Id SERIAL PRIMARY KEY,
    cliente INTEGER NOT NULL,
    modelo INTEGER NOT NULL,
    localidade INTEGER NOT NULL,
    quantidademinima INTEGER NOT NULL,
    quantidadetotallancada INTEGER NOT NULL DEFAULT 0,
    quantidademaxima INTEGER NOT NULL DEFAULT 0,
    observacoes TEXT,
    ativo BOOLEAN NOT NULL DEFAULT true,
    dtcriacao TIMESTAMP NOT NULL DEFAULT NOW(),
    usuariocriacao INTEGER NOT NULL,
    dtatualizacao TIMESTAMP,
    usuarioatualizacao INTEGER,
    CONSTRAINT fk_estoquemin_equip_cliente FOREIGN KEY (cliente) references clientes(id) ON DELETE CASCADE,
    CONSTRAINT fk_estoquemin_equip_modelo FOREIGN KEY (modelo) REFERENCES Modelos(Id) ON DELETE CASCADE,
    CONSTRAINT fk_estoquemin_equip_localidade FOREIGN KEY (localidade) REFERENCES Localidades(Id) ON DELETE CASCADE,
    CONSTRAINT fk_estoquemin_equip_usuario_criacao FOREIGN KEY (usuariocriacao) REFERENCES Usuarios(Id),
    CONSTRAINT fk_estoquemin_equip_usuario_atualizacao FOREIGN KEY (usuarioatualizacao) REFERENCES Usuarios(Id)
);

-- Tabela: Estoque Mínimo Linhas
CREATE TABLE IF NOT EXISTS estoqueminimolinhas (
    Id SERIAL PRIMARY KEY,
    cliente INTEGER NOT NULL,
    operadora INTEGER NOT NULL,
    plano INTEGER NOT NULL,
    localidade INTEGER NOT NULL,
    quantidademinima INTEGER NOT NULL,
    quantidadetotallancada INTEGER NOT NULL DEFAULT 0,
    quantidademaxima INTEGER NOT NULL DEFAULT 0,
    perfiluso VARCHAR(100),
    observacoes TEXT,
    ativo BOOLEAN NOT NULL DEFAULT true,
    dtcriacao TIMESTAMP NOT NULL DEFAULT NOW(),
    usuariocriacao INTEGER NOT NULL,
    dtatualizacao TIMESTAMP,
    usuarioatualizacao INTEGER,
    CONSTRAINT fk_estoquemin_linha_cliente FOREIGN KEY (cliente) references clientes(id) ON DELETE CASCADE,
    CONSTRAINT fk_estoquemin_linha_operadora FOREIGN KEY (operadora) REFERENCES TelefoniaOperadoras(Id) ON DELETE CASCADE,
    CONSTRAINT fk_estoquemin_linha_plano FOREIGN KEY (plano) REFERENCES TelefoniaPlanos(Id) ON DELETE CASCADE,
    CONSTRAINT fk_estoquemin_linha_localidade FOREIGN KEY (localidade) REFERENCES Localidades(Id) ON DELETE CASCADE,
    CONSTRAINT fk_estoquemin_linha_usuario_criacao FOREIGN KEY (usuariocriacao) REFERENCES Usuarios(Id),
    CONSTRAINT fk_estoquemin_linha_usuario_atualizacao FOREIGN KEY (usuarioatualizacao) REFERENCES Usuarios(Id)
);

-- Tabela: Importaço Linha Staging
CREATE TABLE IF NOT EXISTS importacao_linha_staging (
    id SERIAL PRIMARY KEY,
    cliente INTEGER NOT NULL,
    lote_id UUID NOT NULL,
    usuario_importacao INTEGER NOT NULL,
    data_importacao TIMESTAMP NOT NULL DEFAULT NOW(),
    operadora_nome VARCHAR(200) NOT NULL,
    contrato_nome VARCHAR(200) NOT NULL,
    plano_nome VARCHAR(200) NOT NULL,
    plano_valor DECIMAL(10,2) NOT NULL,
    numero_linha DECIMAL(15,0) NOT NULL,
    iccid VARCHAR(50) NOT NULL,
    status CHAR(1) NOT NULL DEFAULT 'P',
    mensagens_validacao TEXT,
    linha_arquivo INTEGER NOT NULL,
    operadora_id INTEGER,
    contrato_id INTEGER,
    plano_id INTEGER,
    criar_operadora BOOLEAN NOT NULL DEFAULT false,
    criar_contrato BOOLEAN NOT NULL DEFAULT false,
    criar_plano BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_importacao_staging_cliente FOREIGN KEY (cliente) references clientes(id) ON DELETE CASCADE,
    CONSTRAINT fk_importacao_staging_usuario FOREIGN KEY (usuario_importacao) REFERENCES Usuarios(Id)
);

-- Tabela: Importaço Colaborador Staging
CREATE TABLE IF NOT EXISTS importacao_colaborador_staging (
    id SERIAL PRIMARY KEY,
    lote_id UUID NOT NULL,
    cliente INT NOT NULL,
    usuario_importacao INT NOT NULL,
    data_importacao TIMESTAMP NOT NULL DEFAULT NOW(),
    nome_colaborador VARCHAR(255) NOT NULL,
    cpf VARCHAR(20) NOT NULL,
    matricula VARCHAR(50) NOT NULL,
    email VARCHAR(255) NOT NULL,
    cargo VARCHAR(150),
    setor VARCHAR(150),
    data_admissao TIMESTAMP,
    tipo_colaborador CHAR(1) NOT NULL,
    data_demissao TIMESTAMP,
    matricula_superior VARCHAR(50),
    empresa_nome VARCHAR(255) NOT NULL,
    empresa_cnpj VARCHAR(20),
    localidade_descricao VARCHAR(255) NOT NULL,
    localidade_cidade VARCHAR(100),
    localidade_estado VARCHAR(50),
    centro_custo_codigo VARCHAR(50) NOT NULL,
    centro_custo_nome VARCHAR(255) NOT NULL,
    filial_nome VARCHAR(255),
    filial_cnpj VARCHAR(20),
    status CHAR(1) NOT NULL DEFAULT 'P',
    mensagens_validacao TEXT,
    linha_arquivo INT NOT NULL,
    empresa_id INT,
    localidade_id INT,
    centro_custo_id INT,
    filial_id INT,
    criar_empresa BOOLEAN NOT NULL DEFAULT FALSE,
    criar_localidade BOOLEAN NOT NULL DEFAULT FALSE,
    criar_centro_custo BOOLEAN NOT NULL DEFAULT FALSE,
    criar_filial BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT fk_colab_staging_usuario FOREIGN KEY (usuario_importacao) REFERENCES Usuarios(Id) ON DELETE CASCADE,
    CONSTRAINT fk_colab_staging_cliente FOREIGN KEY (cliente) references clientes(id) ON DELETE CASCADE
);

-- Tabela: Importaço Log
CREATE TABLE IF NOT EXISTS importacao_log (
    id SERIAL PRIMARY KEY,
    lote_id UUID NOT NULL,
    cliente INTEGER NOT NULL,
    usuario INTEGER NOT NULL,
    tipo_importacao VARCHAR(50) NOT NULL,
    data_inicio TIMESTAMP NOT NULL DEFAULT NOW(),
    data_fim TIMESTAMP,
    status VARCHAR(20) NOT NULL DEFAULT 'PROCESSANDO',
    total_registros INTEGER NOT NULL DEFAULT 0,
    total_validados INTEGER NOT NULL DEFAULT 0,
    total_erros INTEGER NOT NULL DEFAULT 0,
    total_importados INTEGER NOT NULL DEFAULT 0,
    nome_arquivo VARCHAR(255) NOT NULL,
    observacoes TEXT,
    CONSTRAINT fk_importacao_log_cliente FOREIGN KEY (cliente) references clientes(id) ON DELETE CASCADE,
    CONSTRAINT fk_importacao_log_usuario FOREIGN KEY (usuario) REFERENCES Usuarios(Id)
);

-- Tabela: Geolocalizaço Assinatura
CREATE TABLE IF NOT EXISTS geolocalizacao_assinatura (
    id SERIAL PRIMARY KEY,
    colaborador_id INTEGER NOT NULL,
    colaborador_nome VARCHAR(255) NOT NULL,
    usuario_logado_id INTEGER NOT NULL,
    ip_address VARCHAR(45) NOT NULL,
    country VARCHAR(100),
    city VARCHAR(100),
    region VARCHAR(100),
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    accuracy_meters DECIMAL(10,2),
    timestamp_captura TIMESTAMP NOT NULL,
    acao VARCHAR(50) NOT NULL,
    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_geolocalizacao_colaborador FOREIGN KEY (colaborador_id) REFERENCES Colaboradores(Id) ON DELETE CASCADE,
    CONSTRAINT fk_geolocalizacao_usuario FOREIGN KEY (usuario_logado_id) REFERENCES Usuarios(Id)
);

-- Tabela: Requisição Item Compartilhado
CREATE TABLE IF NOT EXISTS requisicoes_itens_compartilhados (
    id SERIAL PRIMARY KEY,
    requisicao_item_id INTEGER NOT NULL,
    colaborador_id INTEGER NOT NULL,
    tipo_acesso VARCHAR(50) NOT NULL DEFAULT 'usuario_compartilhado',
    data_inicio TIMESTAMP NOT NULL DEFAULT NOW(),
    data_fim TIMESTAMP,
    observacao TEXT,
    ativo BOOLEAN NOT NULL DEFAULT true,
    criado_por INTEGER NOT NULL,
    criado_em TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_req_comp_item FOREIGN KEY (requisicao_item_id) REFERENCES RequisicoesItens(Id) ON DELETE RESTRICT,
    CONSTRAINT fk_req_comp_colaborador FOREIGN KEY (colaborador_id) REFERENCES Colaboradores(Id) ON DELETE RESTRICT,
    CONSTRAINT fk_req_comp_usuario FOREIGN KEY (criado_por) REFERENCES Usuarios(Id) ON DELETE RESTRICT
);

-- =====================================================
-- TABELAS NECESSÁRIAS PARA AS VIEWS
-- =====================================================

-- Tabela: Filiais (necessária para views de equipamentos e colaboradores)
CREATE TABLE IF NOT EXISTS Filiais
(
    Id serial not null primary key,
    Nome varchar(100) not null,
    Empresa_Id int not null,
    Localidade_Id int not null,
    Cnpj varchar(18),
    Endereco text,
    Telefone varchar(20),
    Email varchar(100),
    Ativo boolean default true,
    created_at timestamp default CURRENT_TIMESTAMP,
    updated_at timestamp default CURRENT_TIMESTAMP,
    constraint fkFilialEmpresa foreign key (Empresa_Id) references Empresas(Id),
    constraint fkFilialLocalidade foreign key (Localidade_Id) references Localidades(Id)
);

-- Tabela: TinOne_Analytics (necessária para view vw_tinone_estatisticas)
CREATE TABLE IF NOT EXISTS TinOne_Analytics
(
    Id serial not null primary key,
    usuario_id int,
    cliente_id int,
    sessao_id varchar(100),
    pagina_url varchar(500),
    pagina_nome varchar(200),
    acao_tipo varchar(100),
    pergunta text,
    resposta text,
    tempo_resposta_ms int,
    foi_util boolean,
    feedback_texto text,
    created_at timestamp default CURRENT_TIMESTAMP,
    updated_at timestamp,
    constraint fkTinOneAnalyticsUsuario foreign key (usuario_id) references Usuarios(Id),
    constraint fkTinOneAnalyticsCliente foreign key (cliente_id) references clientes(id)
);


-- =====================================================
-- TRIGGERS E FUNÇÕES
-- =====================================================

-- Funço: update_updated_at_column
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
	NEW.updated_at = CURRENT_TIMESTAMP;
	RETURN NEW;
END;
$$ language 'plpgsql';

-- Trigger: update_patrimonio_contestoes_updated_at
CREATE TRIGGER update_patrimonio_contestoes_updated_at 
	BEFORE UPDATE ON patrimonio_contestoes 
	FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Trigger: update_sinalizacoes_updated_at
CREATE TRIGGER update_sinalizacoes_updated_at 
	BEFORE UPDATE ON sinalizacoes_suspeitas 
	FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Funço: criar_historico_sinalizacao
CREATE OR REPLACE FUNCTION criar_historico_sinalizacao()
RETURNS TRIGGER AS $$
BEGIN
	IF TG_OP = 'INSERT' THEN
		INSERT INTO historico_investigacoes (sinalizacao_id, usuario_id, acao, descricao)
		VALUES (NEW.id, COALESCE(NEW.vigilante_id, 0), 'criada', 'Sinalizaço de suspeita criada');
	END IF;
	
	IF TG_OP = 'UPDATE' AND OLD.status != NEW.status THEN
		INSERT INTO historico_investigacoes (sinalizacao_id, usuario_id, acao, descricao, dados_antes, dados_depois)
		VALUES (NEW.id, COALESCE(NEW.investigador_id, NEW.vigilante_id, 0), 
				'status_alterado', 
				'Status alterado de ' || OLD.status || ' para ' || NEW.status,
				json_build_object('status', OLD.status),
				json_build_object('status', NEW.status));
	END IF;
	
	RETURN NEW;
END;
$$ language 'plpgsql';

-- Trigger: trigger_historico_sinalizacao
CREATE TRIGGER trigger_historico_sinalizacao
	AFTER INSERT OR UPDATE ON sinalizacoes_suspeitas
	FOR EACH ROW EXECUTE FUNCTION criar_historico_sinalizacao();

-- Funço: fn_validar_equipamento_compartilhado
CREATE OR REPLACE FUNCTION fn_validar_equipamento_compartilhado()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
	IF NEW.tipo_acesso NOT IN ('usuario_compartilhado', 'temporario', 'turno') THEN
		RAISE EXCEPTION 'Tipo de acesso inválido: %', NEW.tipo_acesso;
	END IF;
	
	IF NEW.tipo_acesso = 'temporario' AND NEW.data_fim IS NULL THEN
		RAISE EXCEPTION 'Para tipo de acesso temporário, a data de fim deve ser informada';
	END IF;
	
	IF NEW.data_fim IS NOT NULL AND NEW.data_fim <= NEW.data_inicio THEN
		RAISE EXCEPTION 'Data de fim deve ser maior que data de início';
	END IF;
	
	RETURN NEW;
END;
$$;

-- Trigger: trg_validar_equipamento_compartilhado
DROP TRIGGER IF EXISTS trg_validar_equipamento_compartilhado 
ON equipamento_usuarios_compartilhados;

CREATE TRIGGER trg_validar_equipamento_compartilhado
	BEFORE INSERT OR UPDATE ON equipamento_usuarios_compartilhados
	FOR EACH ROW
	EXECUTE FUNCTION fn_validar_equipamento_compartilhado();

-- =====================================================
-- ÍNDICES PARA PERFORMANCE
-- =====================================================

-- Índices: Estados e Cidades
CREATE INDEX IF NOT EXISTS idx_cidades_estado_id ON cidades(estado_id);
CREATE INDEX IF NOT EXISTS idx_cidades_nome ON cidades(nome);
CREATE INDEX IF NOT EXISTS idx_estados_sigla ON estados(sigla);

-- Índices: Equipamentos
CREATE INDEX IF NOT EXISTS idx_equipamentos_compartilhado 
ON equipamentos(compartilhado) 
WHERE compartilhado = TRUE;

-- Índices: Equipamento Usuários Compartilhados
CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_equipamento 
ON equipamento_usuarios_compartilhados(equipamento_id);

CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_colaborador 
ON equipamento_usuarios_compartilhados(colaborador_id);

CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_ativo 
ON equipamento_usuarios_compartilhados(ativo) 
WHERE ativo = TRUE;

CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_tipo 
ON equipamento_usuarios_compartilhados(tipo_acesso);

CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_data_fim 
ON equipamento_usuarios_compartilhados(data_fim) 
WHERE data_fim IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_eq_ativo 
ON equipamento_usuarios_compartilhados(equipamento_id, ativo);

-- Índices: Protocolos de Descarte
CREATE INDEX IF NOT EXISTS idx_protocolos_descarte_cliente ON protocolos_descarte(cliente);
CREATE INDEX IF NOT EXISTS idx_protocolos_descarte_status ON protocolos_descarte(status);
CREATE INDEX IF NOT EXISTS idx_protocolos_descarte_data_criacao ON protocolos_descarte(data_criacao DESC);
CREATE INDEX IF NOT EXISTS idx_protocolos_descarte_tipo ON protocolos_descarte(tipo_descarte);

-- Índices: Protocolo Descarte Itens
CREATE INDEX IF NOT EXISTS idx_protocolo_itens_protocolo ON protocolo_descarte_itens(protocolo_id);
CREATE INDEX IF NOT EXISTS idx_protocolo_itens_equipamento ON protocolo_descarte_itens(equipamento);
CREATE INDEX IF NOT EXISTS idx_protocolo_itens_status ON protocolo_descarte_itens(status_item);

-- Índices: Descarte Evidências
CREATE INDEX IF NOT EXISTS idx_descarteevidencias_equipamento ON descarteevidencias(equipamento);
CREATE INDEX IF NOT EXISTS idx_descarteevidencias_tipoprocesso ON descarteevidencias(tipoprocesso);
CREATE INDEX IF NOT EXISTS idx_descarteevidencias_dataupload ON descarteevidencias(dataupload DESC);

-- Índices: Cargos de Confiança
CREATE INDEX IF NOT EXISTS idx_cargosconfianca_cliente ON cargosconfianca(cliente);
CREATE INDEX IF NOT EXISTS idx_cargosconfianca_cargo ON cargosconfianca(cargo);
CREATE INDEX IF NOT EXISTS idx_cargosconfianca_ativo ON cargosconfianca(ativo);

-- Índices: Políticas de Elegibilidade
CREATE INDEX IF NOT EXISTS idx_politica_cliente ON politicas_elegibilidade(cliente);
CREATE INDEX IF NOT EXISTS idx_politica_tipo_colaborador ON politicas_elegibilidade(tipo_colaborador);
CREATE INDEX IF NOT EXISTS idx_politica_tipo_equipamento ON politicas_elegibilidade(tipo_equipamento_id);
CREATE INDEX IF NOT EXISTS idx_politica_ativo ON politicas_elegibilidade(ativo);

-- Índices: Patrimônio Contestações
CREATE INDEX IF NOT EXISTS idx_patrimonio_contestoes_colaborador ON patrimonio_contestoes(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_patrimonio_contestoes_equipamento ON patrimonio_contestoes(equipamento_id);
CREATE INDEX IF NOT EXISTS idx_patrimonio_contestoes_status ON patrimonio_contestoes(status);
CREATE INDEX IF NOT EXISTS idx_patrimonio_contestoes_data ON patrimonio_contestoes(data_contestacao);

-- Índices: Patrimônio Logs de Acesso
CREATE INDEX IF NOT EXISTS idx_logs_acesso_tipo ON patrimonio_logs_acesso(tipo_acesso);
CREATE INDEX IF NOT EXISTS idx_logs_acesso_colaborador ON patrimonio_logs_acesso(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_logs_acesso_cpf ON patrimonio_logs_acesso(cpf_consultado);
CREATE INDEX IF NOT EXISTS idx_logs_acesso_data ON patrimonio_logs_acesso(created_at);

-- Índices: Sinalizações de Suspeitas
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_colaborador ON sinalizacoes_suspeitas(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_vigilante ON sinalizacoes_suspeitas(vigilante_id);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_status ON sinalizacoes_suspeitas(status);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_prioridade ON sinalizacoes_suspeitas(prioridade);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_data ON sinalizacoes_suspeitas(data_sinalizacao);
CREATE INDEX IF NOT EXISTS idx_sinalizacoes_investigador ON sinalizacoes_suspeitas(investigador_id);

-- Índices: Histórico de Investigações
CREATE INDEX IF NOT EXISTS idx_historico_sinalizacao ON historico_investigacoes(sinalizacao_id);
CREATE INDEX IF NOT EXISTS idx_historico_usuario ON historico_investigacoes(usuario_id);
CREATE INDEX IF NOT EXISTS idx_historico_data ON historico_investigacoes(created_at);

-- Índices: Motivos de Suspeita
CREATE INDEX IF NOT EXISTS idx_motivos_ativo ON motivos_suspeita(ativo);

-- Índices: Campanhas (ADICIONADOS v1.1.0)
CREATE INDEX IF NOT EXISTS idx_campanhas_cliente ON campanhasassinaturas(Cliente);
CREATE INDEX IF NOT EXISTS idx_campanhas_status ON campanhasassinaturas(Status);
CREATE INDEX IF NOT EXISTS idx_campanhas_data_criacao ON campanhasassinaturas(DataCriacao);
CREATE INDEX IF NOT EXISTS idx_campanhas_colab_campanha ON campanhascolaboradores(CampanhaId);
CREATE INDEX IF NOT EXISTS idx_campanhas_colab_colaborador ON campanhascolaboradores(ColaboradorId);
CREATE INDEX IF NOT EXISTS idx_campanhas_colab_status ON campanhascolaboradores(StatusAssinatura);

-- Índices: Estoque Mínimo (ADICIONADOS v1.1.0)
CREATE INDEX IF NOT EXISTS idx_estoque_equip_cliente ON estoqueminimoequipamentos(cliente);
CREATE INDEX IF NOT EXISTS idx_estoque_equip_modelo ON estoqueminimoequipamentos(modelo);
CREATE INDEX IF NOT EXISTS idx_estoque_equip_localidade ON estoqueminimoequipamentos(localidade);
CREATE INDEX IF NOT EXISTS idx_estoque_equip_ativo ON estoqueminimoequipamentos(ativo);
CREATE INDEX IF NOT EXISTS idx_estoque_linha_cliente ON estoqueminimolinhas(cliente);
CREATE INDEX IF NOT EXISTS idx_estoque_linha_operadora ON estoqueminimolinhas(operadora);
CREATE INDEX IF NOT EXISTS idx_estoque_linha_plano ON estoqueminimolinhas(plano);
CREATE INDEX IF NOT EXISTS idx_estoque_linha_localidade ON estoqueminimolinhas(localidade);
CREATE INDEX IF NOT EXISTS idx_estoque_linha_ativo ON estoqueminimolinhas(ativo);

-- Índices: Importação (ADICIONADOS v1.1.0)
CREATE INDEX IF NOT EXISTS idx_importacao_staging_lote ON importacao_linha_staging(lote_id);
CREATE INDEX IF NOT EXISTS idx_importacao_staging_status ON importacao_linha_staging(status);
CREATE INDEX IF NOT EXISTS idx_importacao_staging_cliente ON importacao_linha_staging(cliente);
CREATE INDEX IF NOT EXISTS idx_importacao_colab_staging_lote ON importacao_colaborador_staging(lote_id);
CREATE INDEX IF NOT EXISTS idx_importacao_colab_staging_status ON importacao_colaborador_staging(status);
CREATE INDEX IF NOT EXISTS idx_importacao_colab_staging_cliente ON importacao_colaborador_staging(cliente);
CREATE INDEX IF NOT EXISTS idx_importacao_log_lote ON importacao_log(lote_id);
CREATE INDEX IF NOT EXISTS idx_importacao_log_status ON importacao_log(status);
CREATE INDEX IF NOT EXISTS idx_importacao_log_cliente ON importacao_log(cliente);

-- Índices: Geolocalização (ADICIONADOS v1.1.0)
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_colaborador ON geolocalizacao_assinatura(colaborador_id);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_usuario ON geolocalizacao_assinatura(usuario_logado_id);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_data ON geolocalizacao_assinatura(data_criacao);
CREATE INDEX IF NOT EXISTS idx_geolocalizacao_acao ON geolocalizacao_assinatura(acao);

-- Índices: Requisição Item Compartilhado (ADICIONADOS v1.1.0)
CREATE INDEX IF NOT EXISTS idx_req_comp_item ON RequisicaoItemCompartilhado(RequisicaoItemId);
CREATE INDEX IF NOT EXISTS idx_req_comp_colaborador ON RequisicaoItemCompartilhado(ColaboradorId);
CREATE INDEX IF NOT EXISTS idx_req_comp_ativo ON RequisicaoItemCompartilhado(Ativo);

-- =====================================================
-- DADOS BÁSICOS DO SISTEMA
-- =====================================================

-- Inserir Cliente SingleOne
INSERT INTO clientes(razaosocial, cnpj, ativo) 
SELECT 'SingleOne', '51908470000199', true
WHERE NOT EXISTS (SELECT 1 FROM clientes WHERE cnpj = '51908470000199');

-- Inserir Status de Requisições
INSERT INTO RequisicoesStatus(Descricao, ativo) 
SELECT 'Ativa', true WHERE NOT EXISTS (SELECT 1 FROM RequisicoesStatus WHERE Descricao = 'Ativa');
INSERT INTO RequisicoesStatus(Descricao, ativo) 
SELECT 'Cancelada', true WHERE NOT EXISTS (SELECT 1 FROM RequisicoesStatus WHERE Descricao = 'Cancelada');
INSERT INTO RequisicoesStatus(Descricao, ativo) 
SELECT 'Processada', true WHERE NOT EXISTS (SELECT 1 FROM RequisicoesStatus WHERE Descricao = 'Processada');

-- Inserir Status de Equipamentos
INSERT INTO EquipamentosStatus(Descricao, ativo) 
SELECT 'Danificado', true WHERE NOT EXISTS (SELECT 1 FROM EquipamentosStatus WHERE Descricao = 'Danificado');
INSERT INTO EquipamentosStatus(Descricao, ativo) 
SELECT 'Devolvido', true WHERE NOT EXISTS (SELECT 1 FROM EquipamentosStatus WHERE Descricao = 'Devolvido');
INSERT INTO EquipamentosStatus(Descricao, ativo) 
SELECT 'Em estoque', true WHERE NOT EXISTS (SELECT 1 FROM EquipamentosStatus WHERE Descricao = 'Em estoque');
INSERT INTO EquipamentosStatus(Descricao, ativo) 
SELECT 'Entregue', true WHERE NOT EXISTS (SELECT 1 FROM EquipamentosStatus WHERE Descricao = 'Entregue');
INSERT INTO EquipamentosStatus(Descricao, ativo) 
SELECT 'Extraviado', true WHERE NOT EXISTS (SELECT 1 FROM EquipamentosStatus WHERE Descricao = 'Extraviado');
INSERT INTO EquipamentosStatus(Descricao, ativo) 
SELECT 'Novo', true WHERE NOT EXISTS (SELECT 1 FROM EquipamentosStatus WHERE Descricao = 'Novo');
INSERT INTO EquipamentosStatus(Descricao, ativo) 
SELECT 'Requisitado', true WHERE NOT EXISTS (SELECT 1 FROM EquipamentosStatus WHERE Descricao = 'Requisitado');
INSERT INTO EquipamentosStatus(Descricao, ativo) 
SELECT 'Roubado', true WHERE NOT EXISTS (SELECT 1 FROM EquipamentosStatus WHERE Descricao = 'Roubado');
INSERT INTO EquipamentosStatus(Descricao, ativo) 
SELECT 'Sinistrado', true WHERE NOT EXISTS (SELECT 1 FROM EquipamentosStatus WHERE Descricao = 'Sinistrado');
INSERT INTO EquipamentosStatus(Descricao, ativo) 
SELECT 'Descartado', true WHERE NOT EXISTS (SELECT 1 FROM EquipamentosStatus WHERE Descricao = 'Descartado');

-- Inserir Usuário Administrador
INSERT INTO Usuarios(Cliente, Nome, Email, Senha, PalavraCriptografada, Su, Adm, Operador, consulta, Ativo) 
VALUES(1, 'Adminstrador', 'administrador@singleone.tech', 'MTQyNTM2QEFkbWlu', '', true, true, false, false, true)
ON CONFLICT (Email) DO NOTHING;

-- Inserir Localidades Padrão
INSERT INTO Localidades(Descricao, Ativo, Cliente) 
SELECT 'Padrão', FALSE, 1 
WHERE NOT EXISTS (SELECT 1 FROM Localidades WHERE Cliente = 1 AND Descricao = 'Padrão');

-- Inserir Tipo de Equipamento para Telefonia (necessário para recursos de telefonia)
INSERT INTO TipoEquipamentos(Descricao, ativo) 
SELECT 'Linha Telefonica', true 
WHERE NOT EXISTS (SELECT 1 FROM TipoEquipamentos WHERE Descricao = 'Linha Telefonica');
-- Associar tipo de equipamento ao cliente (necessário para ListarTiposRecursos funcionar)
-- NOTA: O tipo ID=1 (Linha Telefonica) é excluído na listagem, mas precisa existir na tabela de relacionamento
INSERT INTO TipoEquipamentosClientes(Cliente, Tipo) 
SELECT 1, (SELECT Id FROM TipoEquipamentos WHERE Descricao = 'Linha Telefonica' LIMIT 1)
WHERE NOT EXISTS (SELECT 1 FROM TipoEquipamentosClientes WHERE Cliente = 1 AND Tipo = (SELECT Id FROM TipoEquipamentos WHERE Descricao = 'Linha Telefonica' LIMIT 1));
INSERT INTO Fabricantes(TipoEquipamento, Descricao, Ativo, Cliente) 
SELECT (SELECT Id FROM TipoEquipamentos WHERE Descricao = 'Linha Telefonica' LIMIT 1), 'Linha Telefonica', false, 1 
WHERE NOT EXISTS (SELECT 1 FROM Fabricantes WHERE Cliente = 1 AND Descricao = 'Linha Telefonica');
INSERT INTO Modelos(Fabricante, Descricao, Ativo, Cliente) 
SELECT (SELECT Id FROM Fabricantes WHERE Cliente = 1 AND Descricao = 'Linha Telefonica' LIMIT 1), 'Linha Telefonica', false, 1 
WHERE NOT EXISTS (SELECT 1 FROM Modelos WHERE Cliente = 1 AND Descricao = 'Linha Telefonica');

-- Inserir Tipos de Aquisiço
INSERT INTO TipoAquisicao (Id, Nome) VALUES (1, 'Alugado') ON CONFLICT (Id) DO UPDATE SET Nome = 'Alugado';
INSERT INTO TipoAquisicao (Id, Nome) VALUES (2, 'Próprio') ON CONFLICT (Id) DO UPDATE SET Nome = 'Próprio';
INSERT INTO TipoAquisicao (Id, Nome) VALUES (3, 'Corporativo') ON CONFLICT (Id) DO UPDATE SET Nome = 'Corporativo';

-- Inserir Equipamento Dummy (necessário para telefonia)
INSERT INTO Equipamentos(Cliente, TipoEquipamento, Fabricante, Modelo, EquipamentoStatus, Usuario, Localizacao, PossuiBO, NumeroSerie, DtCadastro, Ativo, TipoAquisicao)
SELECT 1, 
       (SELECT Id FROM TipoEquipamentos WHERE Descricao = 'Linha Telefonica' LIMIT 1),
       (SELECT Id FROM Fabricantes WHERE Cliente = 1 AND Descricao = 'Linha Telefonica' LIMIT 1),
       (SELECT Id FROM Modelos WHERE Cliente = 1 AND Descricao = 'Linha Telefonica' LIMIT 1),
       6, 1, 1, false, 'Não cadastrado', now(), false, 3
WHERE NOT EXISTS (SELECT 1 FROM Equipamentos WHERE Cliente = 1 AND NumeroSerie = 'Não cadastrado');

-- Inserir Tipos de Templates
INSERT INTO TemplateTipos(Id, Descricao) 
SELECT 1, 'Termo de nada consta' WHERE NOT EXISTS (SELECT 1 FROM TemplateTipos WHERE Id = 1);
INSERT INTO TemplateTipos(Id, Descricao) 
SELECT 2, 'Termo de responsabilidade' WHERE NOT EXISTS (SELECT 1 FROM TemplateTipos WHERE Id = 2);
INSERT INTO TemplateTipos(Id, Descricao) 
SELECT 3, 'Termo de responsabilidade - BYOD' WHERE NOT EXISTS (SELECT 1 FROM TemplateTipos WHERE Id = 3);
INSERT INTO TemplateTipos(Id, Descricao) 
SELECT 4, 'Termo de sinistros' WHERE NOT EXISTS (SELECT 1 FROM TemplateTipos WHERE Id = 4);
INSERT INTO TemplateTipos(Id, Descricao) 
SELECT 5, 'Termo de descarte, doação, logística reversa' WHERE NOT EXISTS (SELECT 1 FROM TemplateTipos WHERE Id = 5);
INSERT INTO TemplateTipos(Id, Descricao) 
SELECT 6, 'Levantamento de Recursos - Inventário forçado' WHERE NOT EXISTS (SELECT 1 FROM TemplateTipos WHERE Id = 6);

-- =====================================================
-- TEMPLATES COMPLETOS (OPCIONAL)
-- =====================================================
-- 
-- IMPORTANTE: Os 6 templates do sistema estão disponíveis no arquivo:
--   templates_completos_v2.2.sql (63 KB)
--
-- Para incluir os templates no banco, execute após este script:
--   psql -U postgres -d singleone -f templates_completos_v2.2.sql
--
-- Templates incluídos:
--   1. Nada Consta de Colaboradores
--   2. Termo de Responsabilidade - Colaboradores (16 KB)
--   3. Termo de Responsabilidade - BYOD (14 KB)
--   4. Sinistros (Laudo Técnico) (14 KB)
--   5. Descartes, Doações, Logística Reversa (15 KB)
--   6. Levantamento de Recursos de TI (Inventário Forçado) (2 KB)
--
-- Total: ~63 KB de conteúdo HTML (incluindo imagens base64)
-- 
-- NOTA: O sistema funciona sem os templates - eles são opcionais.
--       Templates podem ser cadastrados manualmente via interface.
-- =====================================================

-- Inserir Regras de Templates
INSERT INTO regrasTemplate (TipoTemplate, TipoAquisicao) 
SELECT 2, 1 WHERE NOT EXISTS (SELECT 1 FROM regrasTemplate WHERE TipoTemplate = 2 AND TipoAquisicao = 1);
INSERT INTO regrasTemplate (TipoTemplate, TipoAquisicao) 
SELECT 2, 3 WHERE NOT EXISTS (SELECT 1 FROM regrasTemplate WHERE TipoTemplate = 2 AND TipoAquisicao = 3);
INSERT INTO regrasTemplate (TipoTemplate, TipoAquisicao) 
SELECT 3, 2 WHERE NOT EXISTS (SELECT 1 FROM regrasTemplate WHERE TipoTemplate = 3 AND TipoAquisicao = 2);

-- Inserir Status de Contratos
INSERT INTO contratostatus(id, nome) 
SELECT 1, 'Aguardando Inicio Vigência' WHERE NOT EXISTS (SELECT 1 FROM contratostatus WHERE id = 1);
INSERT INTO contratostatus(id, nome) 
SELECT 2, 'Vigente' WHERE NOT EXISTS (SELECT 1 FROM contratostatus WHERE id = 2);
INSERT INTO contratostatus(id, nome) 
SELECT 3, 'Vencido' WHERE NOT EXISTS (SELECT 1 FROM contratostatus WHERE id = 3);

-- Inserir Estados Brasileiros
INSERT INTO estados (sigla, nome) VALUES
('AC', 'Acre'), ('AL', 'Alagoas'), ('AP', 'Amapá'), ('AM', 'Amazonas'),
('BA', 'Bahia'), ('CE', 'Ceará'), ('DF', 'Distrito Federal'), ('ES', 'Espírito Santo'),
('GO', 'Goiás'), ('MA', 'Maranhão'), ('MT', 'Mato Grosso'), ('MS', 'Mato Grosso do Sul'),
('MG', 'Minas Gerais'), ('PA', 'Pará'), ('PB', 'Paraíba'), ('PR', 'Paraná'),
('PE', 'Pernambuco'), ('PI', 'Piauí'), ('RJ', 'Rio de Janeiro'), ('RN', 'Rio Grande do Norte'),
('RS', 'Rio Grande do Sul'), ('RO', 'Rondônia'), ('RR', 'Roraima'), ('SC', 'Santa Catarina'),
('SP', 'São Paulo'), ('SE', 'Sergipe'), ('TO', 'Tocantins')
ON CONFLICT (sigla) DO NOTHING;

-- Inserir Motivos de Suspeita
INSERT INTO motivos_suspeita (codigo, descricao, descricao_detalhada, prioridade_padrao) VALUES 
('comportamento_estranho', 'Comportamento Estranho', 'Colaborador apresentou comportamento suspeito ou atípico', 'media'),
('documentos_inconsistentes', 'Documentos Inconsistentes', 'Documentos apresentados não conferem com os dados do sistema', 'alta'),
('equipamentos_nao_reconhecidos', 'Equipamentos Não Reconhecidos', 'Colaborador não reconhece equipamentos listados no sistema', 'alta'),
('tentativa_evasao', 'Tentativa de Evasão', 'Colaborador tentou evitar procedimentos de verificação', 'critica'),
('acompanhante_suspeito', 'Acompanhante Suspeito', 'Pessoa acompanhando o colaborador apresentou comportamento suspeito', 'alta'),
('horario_atipico', 'Horário Atípico', 'Acesso em horário não usual ou fora do expediente', 'baixa'),
('equipamentos_em_excesso', 'Equipamentos em Excesso', 'Quantidade de equipamentos superior ao esperado', 'media'),
('nervosismo_excessivo', 'Nervosismo Excessivo', 'Colaborador demonstrou nervosismo ou ansiedade excessiva', 'media'),
('outros', 'Outros Motivos', 'Outros motivos não listados', 'media')
ON CONFLICT (codigo) DO NOTHING;

-- =====================================================
-- TEMPLATES DE DOCUMENTOS (OPCIONAL)
-- =====================================================
-- NOTA: Templates têm conteúdo HTML extenso (alguns com imagens base64)
--       Por questões de tamanho, não são incluídos neste script base
--       
-- OPÇÕES PARA ADICIONAR TEMPLATES:
-- 1. Sistema funciona SEM templates (gera documentos básicos)
-- 2. Importar de banco existente (ver templates_iniciais_v1.1.0.sql)
-- 3. Criar via interface: Cadastros > Templates
-- 4. Executar scripts individuais:
--    - SingleOne_Backend/SingleOneAPI/Templates/Insert_Template_InventarioForcado.sql
--    - templates_iniciais_v1.1.0.sql (template básico de Nada Consta)
--
-- Tipos de templates disponíveis:
-- 1 - Nada Consta
-- 2 - Termo de Responsabilidade - Colaboradores  
-- 3 - Termo de Responsabilidade - BYOD
-- 4 - Sinistros (Laudo Técnico)
-- 5 - Descartes, Doações, Logística Reversa
-- 6 - Notificação de Inventário Forçado (v1.1.0)

-- =====================================================
-- COMENTÁRIOS DAS TABELAS
-- =====================================================

COMMENT ON TABLE clientes IS 'Tabela de clientes do sistema';
COMMENT ON TABLE usuarios IS 'Tabela de usuários do sistema com perfis de acesso';
COMMENT ON TABLE equipamentos IS 'Tabela principal de equipamentos';
COMMENT ON TABLE colaboradores IS 'Tabela de colaboradores (usuários finais dos equipamentos)';
COMMENT ON TABLE requisicoes IS 'Tabela de requisições de equipamentos';
COMMENT ON TABLE protocolos_descarte IS 'Armazena os protocolos de descarte que podem conter múltiplos equipamentos';
COMMENT ON TABLE protocolo_descarte_itens IS 'Armazena os equipamentos individuais dentro de cada protocolo de descarte';
COMMENT ON TABLE descarteevidencias IS 'Armazena evidências fotográficas e arquivos dos processos de descarte de equipamentos';
COMMENT ON TABLE cargosconfianca IS 'Cargos que requerem processos especiais de segurança no descarte';
COMMENT ON TABLE politicas_elegibilidade IS 'Armazena as políticas de elegibilidade de recursos por perfil de colaborador';
COMMENT ON TABLE patrimonio_contestoes IS 'Tabela para contestações de patrimônio pelos colaboradores';
COMMENT ON TABLE patrimonio_logs_acesso IS 'Tabela para logs de acesso ao sistema PassCheck e Meu Patrimônio';
COMMENT ON TABLE sinalizacoes_suspeitas IS 'Tabela para sinalizações de suspeitas feitas pelos vigilantes da portaria';
COMMENT ON TABLE historico_investigacoes IS 'Histórico de todas as ações realizadas nas investigações';
COMMENT ON TABLE motivos_suspeita IS 'Motivos pré-definidos para sinalizações de suspeitas';
COMMENT ON TABLE equipamento_usuarios_compartilhados IS 'Gerencia múltiplos usuários para equipamentos compartilhados';
COMMENT ON TABLE campanhasassinaturas IS 'Campanhas para coleta de assinaturas eletrônicas de colaboradores (v1.1.0)';
COMMENT ON TABLE campanhascolaboradores IS 'Relacionamento entre campanhas e colaboradores participantes (v1.1.0)';
COMMENT ON TABLE estoqueminimoequipamentos IS 'Controle de estoque mínimo de equipamentos por localidade e modelo (v1.1.0)';
COMMENT ON TABLE estoqueminimolinhas IS 'Controle de estoque mínimo de linhas telefônicas por localidade, operadora e plano (v1.1.0)';
COMMENT ON TABLE importacao_linha_staging IS 'Área de staging para importação em lote de linhas telefônicas via Excel (v1.1.0)';
COMMENT ON TABLE importacao_colaborador_staging IS 'Área de staging para importação em lote de colaboradores via Excel (v2.3.0)';
COMMENT ON TABLE importacao_log IS 'Log de processos de importação em lote de linhas e colaboradores (v1.1.0)';
COMMENT ON TABLE geolocalizacao_assinatura IS 'Geolocalização capturada durante assinatura de termos e políticas (v1.1.0)';
COMMENT ON TABLE RequisicaoItemCompartilhado IS 'Compartilhamento de recursos entre múltiplos colaboradores (v1.1.0)';

COMMENT ON COLUMN equipamentos.compartilhado IS 'Indica se o equipamento permite múltiplos usuários';
COMMENT ON COLUMN equipamentos.enviouEmailReporte IS 'Indica se email de reporte foi enviado';
COMMENT ON COLUMN protocolos_descarte.protocolo IS 'Número único do protocolo (ex: DESC-2025-001234)';
COMMENT ON COLUMN protocolos_descarte.tipo_descarte IS 'Tipo: DOACAO, VENDA, DEVOLUCAO, LOGISTICA_REVERSA, DESCARTE_FINAL';
COMMENT ON COLUMN protocolo_descarte_itens.status_item IS 'Status individual: PENDENTE, EM_PROCESSO, CONCLUIDO';
COMMENT ON COLUMN protocolo_descarte_itens.processos_obrigatorios IS 'Indica se equipamento passou por cargo de confiança e requer processos especiais';
COMMENT ON COLUMN descarteevidencias.tipoprocesso IS 'Tipo do processo: SANITIZACAO, DESCARACTERIZACAO, PERFURACAO_DISCO, EVIDENCIAS_GERAIS';
COMMENT ON COLUMN sinalizacoes_suspeitas.cpf_consultado IS 'CPF que foi consultado no PassCheck';
COMMENT ON COLUMN sinalizacoes_suspeitas.status IS 'Status da investigação: pendente, em_investigacao, resolvida, arquivada';

-- =====================================================
-- TABELAS ADICIONAIS (FALTANTES NO SCRIPT ORIGINAL)
-- =====================================================

-- Tabela: Categorias
CREATE TABLE IF NOT EXISTS categorias
(
    id serial not null primary key,
    nome varchar(100) not null UNIQUE,
    descricao text,
    ativo boolean default true,
    data_criacao timestamp default CURRENT_TIMESTAMP,
    data_atualizacao timestamp default CURRENT_TIMESTAMP
);

-- Comentários e índices para categorias
COMMENT ON TABLE categorias IS 'Categorias de equipamentos para organização hierárquica';
COMMENT ON COLUMN categorias.nome IS 'Nome único da categoria';
COMMENT ON COLUMN categorias.ativo IS 'Indica se a categoria está ativa';

-- Índices para categorias
CREATE INDEX IF NOT EXISTS idx_categorias_ativo ON categorias(ativo);
CREATE INDEX IF NOT EXISTS idx_categorias_nome ON categorias(nome);

-- Adicionar coluna categoria_id em tipoequipamentos se não existir (migração)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'tipoequipamentos' AND column_name = 'categoria_id') THEN
        ALTER TABLE tipoequipamentos ADD COLUMN IF NOT EXISTS categoria_id INTEGER;
        ALTER TABLE tipoequipamentos ADD COLUMN IF NOT EXISTS transitolivre BOOLEAN NOT NULL DEFAULT false;
    END IF;

    -- Adicionar foreign key categoria_id -> categorias(id) se não existir
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint 
        WHERE conname = 'fk_tipoequipamento_categoria'
    ) THEN
        ALTER TABLE tipoequipamentos
        ADD CONSTRAINT fk_tipoequipamento_categoria 
        FOREIGN KEY (categoria_id) REFERENCES categorias(id) ON DELETE SET NULL;
    END IF;

    -- Criar índice para categoria_id se não existir
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE tablename = 'tipoequipamentos' AND indexname = 'idx_tipoequipamento_categoria_id'
    ) THEN
        CREATE INDEX idx_tipoequipamento_categoria_id ON tipoequipamentos(categoria_id);
    END IF;
END $$;

-- Adicionar FKs de Filiais (após criação de Filiais)
DO $$
BEGIN
    -- FK para CentroCusto
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint 
        WHERE conname = 'fk_centrocusto_filial'
    ) THEN
        ALTER TABLE centrocusto
        ADD CONSTRAINT fk_centrocusto_filial 
        FOREIGN KEY (filial_id) REFERENCES filiais(id);
    END IF;

    -- FK para Colaboradores
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint 
        WHERE conname = 'fk_colaboradores_filial'
    ) THEN
        ALTER TABLE colaboradores
        ADD CONSTRAINT fk_colaboradores_filial 
        FOREIGN KEY (filial_id) REFERENCES filiais(id);
    END IF;
END $$;

-- Tabela: LaudoEvidencias
CREATE TABLE IF NOT EXISTS LaudoEvidencias
(
    Id serial not null primary key,
    Laudo int,
    NomeArquivo varchar(255),
    Ordem int default 0,
    constraint fkLaudoEvidencia foreign key (Laudo) references Laudos(Id)
);

-- Tabela duplicada removida - usar requisicoes_itens_compartilhados acima

-- Tabela: TinOne_Config
CREATE TABLE IF NOT EXISTS TinOne_Config
(
    Id serial not null primary key,
    Cliente int,
    Chave varchar(100) not null,
    Valor text,
    Descricao text,
    Ativo boolean default true,
    Created_At timestamp default CURRENT_TIMESTAMP,
    Updated_At timestamp default CURRENT_TIMESTAMP,
    constraint fkTinOneConfigCliente foreign key (Cliente) references clientes(id)
);


-- Tabela: TinOne_Conversas
CREATE TABLE IF NOT EXISTS TinOne_Conversas
(
    Id serial not null primary key,
    usuario_id int,
    sessao_id varchar(100),
    tipo_mensagem varchar(20),
    mensagem text not null,
    pagina_contexto varchar(200),
    metadata jsonb,
    created_at timestamp default CURRENT_TIMESTAMP,
    constraint fkTinOneConversasUsuario foreign key (usuario_id) references Usuarios(Id)
);

-- Tabela: TinOne_Processos_Guiados
CREATE TABLE IF NOT EXISTS TinOne_Processos_Guiados
(
    Id serial not null primary key,
    Usuario_Id int,
    Processo_Id varchar(100),
    Processo_Nome varchar(200),
    Iniciado_Em timestamp default CURRENT_TIMESTAMP,
    Concluido_Em timestamp,
    Abandonado_Em timestamp,
    Status varchar(50),
    Passo_Atual int,
    Total_Passos int,
    Passos_Concluidos jsonb,
    Tempo_Total_Segundos int,
    constraint fkTinOneProcGuiadosUsuario foreign key (Usuario_Id) references Usuarios(Id),
    constraint chk_status check (Status in ('em_andamento', 'concluido', 'abandonado'))
);

-- Tabela: __EFMigrationsHistory (Entity Framework Core)
CREATE TABLE IF NOT EXISTS __EFMigrationsHistory
(
    MigrationId varchar(150) not null primary key,
    ProductVersion varchar(32) not null
);

-- =====================================================
-- VALIDAÇÃO COMPLETA DA ESTRUTURA DO BANCO
-- =====================================================
-- Esta seção valida que TODAS as tabelas e views esperadas foram criadas
-- e reporta quaisquer gaps (lacunas) encontrados

DO $$
DECLARE
    v_table_count INTEGER;
    v_view_count INTEGER;
    v_index_count INTEGER;
    v_missing_tables TEXT[] := ARRAY[]::TEXT[];
    v_missing_views TEXT[] := ARRAY[]::TEXT[];
    v_table_name TEXT;
    v_view_name TEXT;
    v_expected_tables TEXT[] := ARRAY[
        'clientes', 'usuarios', 'tipoequipamentos', 'tipoequipamentosclientes',
        'fabricantes', 'modelos', 'equipamentosstatus', 'fornecedores',
        'contratostatus', 'contratos', 'tipoaquisicao', 'notasfiscais',
        'notasfiscaisitens', 'estados', 'cidades', 'localidades', 'empresas',
        'centrocusto', 'colaboradores', 'telefoniaoperadoras', 'telefoniacontratos',
        'telefoniaplanos', 'telefonialinhas', 'equipamentos', 'equipamento_usuarios_compartilhados',
        'laudos', 'equipamentoanexos', 'requisicoesstatus', 'requisicoes',
        'requisicoesitens', 'equipamentohistorico', 'templatetipos', 'templates',
        'regrastemplate', 'descartecargos', 'cargosconfianca', 'protocolos_descarte',
        'protocolo_descarte_itens', 'descarteevidencias', 'politicas_elegibilidade',
        'patrimonio_contestoes', 'patrimonio_logs_acesso', 'motivos_suspeita',
        'sinalizacoes_suspeitas', 'historico_investigacoes', 'parametros',
        'processamentosservicos', 'campanhasassinaturas', 'campanhascolaboradores',
        'estoqueminimoequipamentos', 'estoqueminimolinhas', 'importacao_linha_staging',
        'importacao_colaborador_staging', 'importacao_log', 'geolocalizacao_assinatura',
        'requisicoes_itens_compartilhados', 'filiais', 'tinone_analytics',
        'categorias', 'laudoevidencias', 'tinone_config', 'tinone_conversas',
        'tinone_processos_guiados', '__efmigrationshistory'
    ];
    -- Total esperado: 64 tabelas
    v_expected_views TEXT[] := ARRAY[
        'termoscolaboradoresvm', 'equipamentovm', 'equipamentohistoricovm',
        'requisicoesvm', 'requisicaoequipamentosvm', 'termoentregavm',
        'vwnadaconsta', 'vwequipamentosstatus', 'vwestoqueequipamentosalerta',
        'vwexportacaoexcel', 'colaboradorhistoricovm', 'vwdevolucaoprogramada',
        'vwequipamentoscomcolaboradoresdesligados', 'vwequipamentosdetalhes',
        'vwtelefonia', 'vwlaudos', 'vwultimasrequisicaobyd', 'vwultimasrequisicaonaobyd',
        'colaboradoresvm', 'vw_equipamentos_compartilhados', 'vw_equipamentos_usuarios_compartilhados',
        'planosvm', 'vwplanostelefonia', 'vw_tinone_estatisticas', 'vw_campanhas_resumo',
        'vw_campanhas_colaboradores_detalhado', 'vw_nao_conformidade_elegibilidade',
        'equipamentovm', 'termoentregavm', 'vw_colaboradores_simples',
        'vw_equipamentos_simples', 'vwestoquelinhasalerta'
    ];
BEGIN
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'VALIDAÇÃO DA ESTRUTURA DO BANCO';
    RAISE NOTICE '========================================';
    RAISE NOTICE '';
    
    -- Validar cada tabela esperada
    RAISE NOTICE '🔍 Validando tabelas...';
    FOREACH v_table_name IN ARRAY v_expected_tables
    LOOP
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND LOWER(table_name) = LOWER(v_table_name)
        ) THEN
            v_missing_tables := array_append(v_missing_tables, v_table_name);
            RAISE WARNING '   ❌ Tabela faltando: %', v_table_name;
        END IF;
    END LOOP;
    
    -- Validar cada view esperada
    RAISE NOTICE '';
    RAISE NOTICE '🔍 Validando views...';
    FOREACH v_view_name IN ARRAY v_expected_views
    LOOP
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.views 
            WHERE table_schema = 'public' 
            AND LOWER(table_name) = LOWER(v_view_name)
        ) THEN
            v_missing_views := array_append(v_missing_views, v_view_name);
            RAISE WARNING '   ❌ View faltando: %', v_view_name;
        END IF;
    END LOOP;
    
    -- Contar totais
    SELECT COUNT(*) INTO v_table_count
    FROM information_schema.tables
    WHERE table_schema = 'public' AND table_type = 'BASE TABLE';
    
    SELECT COUNT(*) INTO v_view_count
    FROM information_schema.views
    WHERE table_schema = 'public';
    
    SELECT COUNT(*) INTO v_index_count
    FROM pg_indexes
    WHERE schemaname = 'public';
    
    -- Relatório final
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'RELATÓRIO DE VALIDAÇÃO';
    RAISE NOTICE '========================================';
    RAISE NOTICE '';
    RAISE NOTICE '📊 ESTATÍSTICAS:';
    RAISE NOTICE '   Tabelas esperadas: %', array_length(v_expected_tables, 1);
    RAISE NOTICE '   Tabelas encontradas: %', v_table_count;
    RAISE NOTICE '   Tabelas faltando: %', array_length(v_missing_tables, 1);
    RAISE NOTICE '';
    RAISE NOTICE '   Views esperadas: %', array_length(v_expected_views, 1);
    RAISE NOTICE '   Views encontradas: %', v_view_count;
    RAISE NOTICE '   Views faltando: %', array_length(v_missing_views, 1);
    RAISE NOTICE '';
    RAISE NOTICE '   Índices criados: %', v_index_count;
    RAISE NOTICE '';
    
    -- Status final
    IF array_length(v_missing_tables, 1) = 0 AND array_length(v_missing_views, 1) = 0 THEN
        RAISE NOTICE '✅ VALIDAÇÃO COMPLETA: Todas as tabelas e views foram criadas com sucesso!';
    ELSIF array_length(v_missing_tables, 1) <= 5 AND array_length(v_missing_views, 1) <= 5 THEN
        RAISE NOTICE '⚠️  VALIDAÇÃO PARCIAL: Algumas tabelas/views estão faltando.';
        RAISE NOTICE '    Verifique os avisos acima e execute o script novamente.';
    ELSE
        RAISE NOTICE '❌ VALIDAÇÃO FALHOU: Muitas tabelas/views estão faltando!';
        RAISE NOTICE '    Execute o script novamente ou verifique os erros acima.';
    END IF;
    
    -- Listar tabelas faltando
    IF array_length(v_missing_tables, 1) > 0 THEN
        RAISE NOTICE '';
        RAISE NOTICE '📋 TABELAS FALTANDO (%):', array_length(v_missing_tables, 1);
        FOREACH v_table_name IN ARRAY v_missing_tables
        LOOP
            RAISE NOTICE '   - %', v_table_name;
        END LOOP;
    END IF;
    
    -- Listar views faltando
    IF array_length(v_missing_views, 1) > 0 THEN
        RAISE NOTICE '';
        RAISE NOTICE '📋 VIEWS FALTANDO (%):', array_length(v_missing_views, 1);
        FOREACH v_view_name IN ARRAY v_missing_views
        LOOP
            RAISE NOTICE '   - %', v_view_name;
        END LOOP;
    END IF;
    
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'SCRIPT DE INICIALIZAÇÃO EXECUTADO!';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Banco de dados SingleOne inicializado';
    RAISE NOTICE 'Versão: 2.4 (Atualizado em 08/11/2025)';
    RAISE NOTICE '';
    RAISE NOTICE 'Dados básicos inseridos:';
    RAISE NOTICE '- Cliente Demo criado';
    RAISE NOTICE '- Usuário administrador: administrador@singleone.tech';
    RAISE NOTICE '- Status de equipamentos e requisições';
    RAISE NOTICE '- Estados brasileiros';
    RAISE NOTICE '- Tipos de aquisição';
    RAISE NOTICE '- Motivos de suspeita';
    RAISE NOTICE '';
    RAISE NOTICE 'Sistemas incluídos:';
    RAISE NOTICE '- Estrutura principal do sistema';
    RAISE NOTICE '- Sistema de descarte e protocolos';
    RAISE NOTICE '- Sistema de elegibilidade';
    RAISE NOTICE '- Sistema PassCheck e Meu Patrimônio';
    RAISE NOTICE '- Sistema de sinalizações de suspeitas';
    RAISE NOTICE '- Sistema de equipamentos compartilhados';
    RAISE NOTICE '- Sistema TinOne (IA e Analytics)';
    RAISE NOTICE '- Sistema de Categorias e Filiais';
    RAISE NOTICE '';
    RAISE NOTICE 'Próximos passos:';
    RAISE NOTICE '1. Ajustar senha do usuário administrador';
    RAISE NOTICE '2. Configurar dados do cliente real';
    RAISE NOTICE '3. Cadastrar empresas, localidades e colaboradores';
    RAISE NOTICE '========================================';
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Erro ao validar estrutura: %', SQLERRM;
        RAISE NOTICE 'Script executado, mas verifique manualmente as tabelas e views criadas.';
END $$;

-- =====================================================
-- COLUNAS DE COMPATIBILIDADE - TABELA EQUIPAMENTOS
-- =====================================================
-- Alguns ambientes tiveram colunas duplicadas/legadas (em maiúsculas ou com sufixo Id)
-- usadas por mapeamentos antigos do EF/Core. Para garantir compatibilidade e evitar
-- erros 42703 (column ... does not exist), criamos essas colunas como opcionais.
DO $$
BEGIN
    -- Só executa se a tabela existir
    IF EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'public' AND table_name = 'equipamentos'
    ) THEN
        ALTER TABLE equipamentos
            ADD COLUMN IF NOT EXISTS "ClienteId" integer NULL,
            ADD COLUMN IF NOT EXISTS clienteid integer NULL,
            ADD COLUMN IF NOT EXISTS "EmpresaId" integer NULL,
            ADD COLUMN IF NOT EXISTS "CentrocustoId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Centrocusto" integer NULL,
            ADD COLUMN IF NOT EXISTS "FilialId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Filial" integer NULL,
            ADD COLUMN IF NOT EXISTS "FilialId1" integer NULL,
            ADD COLUMN IF NOT EXISTS "LocalidadeId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Localidade" integer NULL,
            ADD COLUMN IF NOT EXISTS "FornecedorId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Fornecedor" integer NULL,
            ADD COLUMN IF NOT EXISTS "UsuarioId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Usuario" integer NULL,
            ADD COLUMN IF NOT EXISTS "Equipamentostatus" integer NULL,
            ADD COLUMN IF NOT EXISTS "EquipamentostatusId" integer NULL,
            ADD COLUMN IF NOT EXISTS "EquipamentosstatusId" integer NULL,
            ADD COLUMN IF NOT EXISTS "TipoequipamentoId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Tipoequipamento" integer NULL,
            ADD COLUMN IF NOT EXISTS "FabricanteId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Fabricante" integer NULL,
            ADD COLUMN IF NOT EXISTS "ModeloId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Modelo" integer NULL,
            ADD COLUMN IF NOT EXISTS "NotafiscalId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Notafiscal" integer NULL,
            ADD COLUMN IF NOT EXISTS "NotasfiscaiId" integer NULL,
            ADD COLUMN IF NOT EXISTS "ContratoId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Contrato" integer NULL,
            ADD COLUMN IF NOT EXISTS "TipoaquisicaoId" integer NULL,
            ADD COLUMN IF NOT EXISTS "Tipoaquisicao" integer NULL;
    END IF;
END $$;

