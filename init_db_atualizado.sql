-- =====================================================
-- SCRIPT DE INICIALIZAÇÃO DO BANCO DE DADOS - SINGLEONE
-- Descrição: Script completo para criar banco limpo com dados básicos
-- Versão: 2.3 (Atualizado em 07/11/2025)
-- v2.3: View colaboradoresvm estendida + staging de importação de colaboradores
-- v2.2: Adicionadas 9 tabelas faltantes + colunas 2FA + coluna logo
-- v2.1: Adicionadas 8 novas tabelas (campanhas, estoque mínimo, importação, etc)
-- v2.0: Script base com estrutura principal
-- =====================================================

-- Habilitar extensão UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =====================================================
-- TABELAS PRINCIPAIS DO SISTEMA
-- =====================================================

-- Tabela: Clientes
CREATE TABLE IF NOT EXISTS Clientes
(
	Id serial not null primary key,
	RazaoSocial varchar(200) not null,
	Cnpj varchar(20) not null,
	Ativo boolean not null,
	Logo TEXT
);

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
	constraint fkUsuarioCliente foreign key (Cliente) REFERENCES Clientes(Id)
);

-- =====================================================
-- TABELAS DE EQUIPAMENTOS
-- =====================================================

-- Tabela: TipoEquipamentos
CREATE TABLE IF NOT EXISTS TipoEquipamentos
(
	Id serial not null primary key,
	Descricao varchar(200) not null,
	Ativo BOOLEAN not null
);

-- Tabela: TipoEquipamentosClientes
CREATE TABLE IF NOT EXISTS TipoEquipamentosClientes
(
	Id serial not null primary key,
	Cliente int not null,
	Tipo int not null,
	constraint fkTipoEqpCliente foreign key (Cliente) references Clientes(Id),
	constraint fkTipoEqpClienteTIpo foreign key (Tipo) references TipoEquipamentos(Id)
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
	constraint fkFabricanteCliente foreign key (Cliente) references Clientes(Id),
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
	constraint fkModelosCliente foreign key (Cliente) references Clientes(Id),
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
CREATE TABLE IF NOT EXISTS Fornecedores
(
	Id serial not null primary key,
	Cliente int not null,
	Nome varchar(200) not null,
	CNPJ varchar(20),
	Ativo boolean not null,
	DestinadorResiduos boolean not null default false,
	MigrateID int,
	constraint fkFornecedorCliente foreign key (Cliente) references Clientes(Id)
);

-- Tabela: ContratoStatus
CREATE TABLE ContratoStatus (
	Id INT NOT NULL,
	Nome VARCHAR(100),
	CONSTRAINT PK_StatusContrato PRIMARY KEY(Id)
);

-- Tabela: Contratos
CREATE TABLE Contratos (
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
	CONSTRAINT PK_Contrato PRIMARY KEY(Id),
	CONSTRAINT FK_Contrato_Clientes FOREIGN KEY (Cliente) REFERENCES Clientes(Id),
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

-- Tabela: NotasFiscais
CREATE TABLE IF NOT EXISTS NotasFiscais 
(
	Id serial not null primary key,
	Cliente int not null,
	Fornecedor int not null,
	Numero int not null,
	DtEmissao TIMESTAMP not null,
	Descricao varchar(500),
	Valor money,
	Contrato int,
	Virtual boolean not null,
	GerouEquipamento boolean not null,
	MigrateID int,
	ArquivoNome VARCHAR(255),
	ArquivoCaminho VARCHAR(500),
	ArquivoTamanho BIGINT,
	ArquivoTipo VARCHAR(100),
	ArquivoDataUpload TIMESTAMP,
	ArquivoUsuarioUpload INT,
	constraint fkNFCliente foreign key (Cliente) references Clientes(Id),
	constraint fkNFFornecedor foreign key (Fornecedor) references Fornecedores(Id),
	constraint fkNFContrato foreign key (Contrato) references Contratos(Id),
	CONSTRAINT fk_NotasFiscais_ArquivoUsuarioUpload FOREIGN KEY (ArquivoUsuarioUpload) REFERENCES Usuarios(Id)
);

-- Tabela: NotasFiscaisItens
CREATE TABLE IF NOT EXISTS NotasFiscaisItens
(
	Id serial not null primary key,
	NotaFiscal int not null,
	TipoEquipamento int not null,
	Fabricante int not null,
	Modelo int not null,
	Quantidade int not null,
	ValorUnitario money not null,
	TipoAquisicao int not null,
	DtLimiteGarantia TIMESTAMP,
	Contrato int,
	constraint fkNFINotaFiscal foreign key (NotaFiscal) references NotasFiscais(Id),
	constraint fkNFITipoEqp foreign key (TipoEquipamento) references TipoEquipamentos(Id),
	constraint fkNFIFabricante foreign key (Fabricante) references Fabricantes(Id),
	constraint fkNFIModelo foreign key (Modelo) references Modelos(Id),
	constraint fkNFITipoAquisicao foreign key (TipoAquisicao) references TipoAquisicao(Id),
	constraint fkNFIContrato foreign key (Contrato) references Contratos(Id)
);

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
	Descricao varchar(300) not null,
	Cidade varchar(100),
	Estado varchar(2),
	Ativo boolean not null,
	MigrateID int,
	constraint fkLocalidadeCliente foreign key (Cliente) references Clientes(Id)
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
	constraint fkEmpresaCliente foreign key (Cliente) references Clientes(Id),
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
	Cliente int not null,
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
	constraint fkColaboradorCliente foreign key (Cliente) references Clientes(Id)
	-- NOTA: FK para Filiais será adicionada após criação da tabela Filiais
);

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
	constraint fkTelContratoCliente foreign key (Cliente) references Clientes(Id),
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
	Cliente int not null,
	TipoEquipamento int not null,
	Fabricante int not null,
	Modelo int not null,
	NotaFiscal int,
	Contrato int,
	EquipamentoStatus int,
	Usuario int,
	Localizacao int,
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
	constraint fkEquipamentoCliente foreign key (Cliente) references Clientes(Id),
	constraint fkEquipamentoTipoEqp foreign key (TipoEquipamento) references TipoEquipamentos(Id),
	constraint fkEquipamentoFabricante foreign key (Fabricante) references Fabricantes(Id),
	constraint fkEquipamentoModelo foreign key (Modelo) references Modelos(Id),
	constraint fkEquipamentoNotaFiscal foreign key (NotaFiscal) references NotasFiscais(Id),
	constraint fkEquipamentoContrato foreign key (Contrato) references Contratos(Id),
	constraint fkEquipamentoStatus foreign key (EquipamentoStatus) references EquipamentosStatus(Id),
	constraint fkEquipamentoUsuario foreign key (Usuario) references Usuarios(Id),
	constraint fkEquipamentoLocalidade foreign key (Localizacao) references Localidades(Id),
	constraint fkEquipamentoEmpresa foreign key (Empresa) references Empresas(Id),
	constraint fkEquipamentoCentro foreign key (CentroCusto) references centrocusto(Id),
	constraint fkEquipamentoFornecedor foreign key (Fornecedor) references Fornecedores(Id),
	constraint fkEquipamentoTipoAquisicao foreign key (TipoAquisicao) references TipoAquisicao(Id)
);

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
	DtSolicitacao timestamp not null,
	DtProcessamento timestamp,
	AssinaturaEletronica boolean not null,
	DtAssinaturaEletronica timestamp,
	DtEnvioTermo timestamp,
	HashRequisicao varchar(200) not null,
	MigrateID int,
	constraint fkRequisicaoCliente foreign key (Cliente) references Clientes(Id),
	constraint fkRequisicaoUsuario foreign key (UsuarioRequisicao) references Usuarios(Id),
	constraint fkRequisicaoTecnico foreign key (TecnicoResponsavel) references Usuarios(Id),
	constraint fkRequisicaoStatus foreign key (RequisicaoStatus) references RequisicoesStatus(Id),
	constraint fkRequisicaoColaborador foreign key (ColaboradorFinal) references Colaboradores(Id)
);

-- Tabela: RequisicoesItens
CREATE TABLE IF NOT EXISTS RequisicoesItens
(
	Id serial not null primary key,
	Requisicao int not null,
	Equipamento int not null,
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
	constraint fkTemplateCliente foreign key (Cliente) references Clientes(Id)
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
	constraint fkCargoCliente foreign key (Cliente) references Clientes(Id)
);

-- Tabela: CargosConfianca
CREATE TABLE IF NOT EXISTS cargosconfianca (
    id SERIAL PRIMARY KEY,
    cliente INTEGER NOT NULL,
    cargo VARCHAR(200) NOT NULL,
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
    ativo BOOLEAN NOT NULL DEFAULT true,
    dt_cadastro TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    dt_atualizacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    usuario_cadastro INTEGER,
    
    CONSTRAINT fk_politica_cliente FOREIGN KEY (cliente) REFERENCES clientes(id) ON DELETE CASCADE,
    CONSTRAINT fk_politica_tipo_equipamento FOREIGN KEY (tipo_equipamento_id) REFERENCES tipoequipamentos(id) ON DELETE CASCADE,
    CONSTRAINT fk_politica_usuario FOREIGN KEY (usuario_cadastro) REFERENCES usuarios(id) ON DELETE SET NULL,
    CONSTRAINT uk_politica_elegibilidade UNIQUE (cliente, tipo_colaborador, cargo, tipo_equipamento_id)
);

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
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

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
	constraint fkParametrosCliente foreign key (Cliente) references Clientes(Id)
);

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
    CONSTRAINT fk_campanha_cliente FOREIGN KEY (Cliente) REFERENCES Clientes(Id) ON DELETE CASCADE,
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
    CONSTRAINT fk_estoquemin_equip_cliente FOREIGN KEY (cliente) REFERENCES Clientes(Id) ON DELETE CASCADE,
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
    CONSTRAINT fk_estoquemin_linha_cliente FOREIGN KEY (cliente) REFERENCES Clientes(Id) ON DELETE CASCADE,
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
    CONSTRAINT fk_importacao_staging_cliente FOREIGN KEY (cliente) REFERENCES Clientes(Id) ON DELETE CASCADE,
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
    CONSTRAINT fk_colab_staging_cliente FOREIGN KEY (cliente) REFERENCES Clientes(Id) ON DELETE CASCADE
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
    CONSTRAINT fk_importacao_log_cliente FOREIGN KEY (cliente) REFERENCES Clientes(Id) ON DELETE CASCADE,
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

-- Tabela: Requisiço Item Compartilhado
CREATE TABLE IF NOT EXISTS RequisicaoItemCompartilhado (
    Id SERIAL PRIMARY KEY,
    RequisicaoItemId INTEGER NOT NULL,
    ColaboradorId INTEGER NOT NULL,
    TipoAcesso VARCHAR(50) NOT NULL DEFAULT 'usuario_compartilhado',
    DataInicio TIMESTAMP NOT NULL DEFAULT NOW(),
    DataFim TIMESTAMP,
    Observacao TEXT,
    Ativo BOOLEAN NOT NULL DEFAULT true,
    CriadoPor INTEGER NOT NULL,
    CriadoEm TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_req_comp_item FOREIGN KEY (RequisicaoItemId) REFERENCES RequisicoesItens(Id) ON DELETE CASCADE,
    CONSTRAINT fk_req_comp_colaborador FOREIGN KEY (ColaboradorId) REFERENCES Colaboradores(Id) ON DELETE CASCADE,
    CONSTRAINT fk_req_comp_usuario FOREIGN KEY (CriadoPor) REFERENCES Usuarios(Id)
);

-- =====================================================
-- VIEWS DO SISTEMA
-- =====================================================

-- View: TermosColaboradoresVM
CREATE OR REPLACE VIEW TermosColaboradoresVM AS 
SELECT r.ColaboradorFinal as ColaboradorFinalId, c.Nome as ColaboradorFinal, 
	(SELECT max(DtEnvioTermo) FROM requisicoes rq WHERE rq.ColaboradorFinal = r.ColaboradorFinal) as DtEnvioTermo, 
	(SELECT CASE WHEN count(*) > 0 THEN 'Em aberto' ELSE 'Assinado' END 
	FROM Requisicoes rq WHERE rq.ColaboradorFinal = r.ColaboradorFinal AND rq.AssinaturaEletronica = false) as Situacao
FROM Requisicoes r
	JOIN Colaboradores c ON r.ColaboradorFinal = c.Id
GROUP BY r.ColaboradorFinal, c.Nome;

-- View: EquipamentoVM
CREATE OR REPLACE VIEW EquipamentoVM AS
SELECT e.id,
    e.tipoequipamento AS tipoequipamentoid,
    COALESCE(te.descricao, 'Nao definido'::character varying) AS tipoequipamento,
    e.fabricante AS fabricanteid,
    COALESCE(f.descricao, 'Nao definido'::character varying) AS fabricante,
    e.modelo AS modeloid,
    COALESCE(m.descricao, 'Nao definido'::character varying) AS modelo,
    e.notafiscal AS notafiscalid,
        CASE
            WHEN e.notafiscal IS NOT NULL THEN nf.numero::character varying
            ELSE 'Nao definido'::character varying
        END AS "Notafiscal",
    e.equipamentostatus AS equipamentostatusid,
    COALESCE(es.descricao, 'Nao definido'::character varying) AS equipamentostatus,
    e.usuario AS usuarioid,
    COALESCE(u.nome, 'Nao definido'::character varying) AS usuario,
    e.localidade_id AS localizacaoid,
        CASE
            WHEN e.localidade_id = 1 THEN 'Nao definido'::character varying
            ELSE COALESCE(l.descricao, 'Nao definido'::character varying)
        END AS localizacao,
    e.possuibo,
    e.descricaobo,
    e.numeroserie,
    e.patrimonio,
    e.dtlimitegarantia,
    e.dtcadastro,
    e.tipoaquisicao,
    COALESCE(ta.nome, 'Nao definido'::character varying) AS "TipoAquisicao",
    e.fornecedor,
        CASE
            WHEN e.fornecedor IS NOT NULL THEN forn.nome
            ELSE 'Nao definido'::character varying
        END AS "FornecedorNome",
    e.cliente,
    NULL::text AS colaboradorid,
    NULL::text AS colaboradornome,
    NULL::text AS requisicaoid,
    e.ativo,
    COALESCE(e.empresa, cc.empresa) AS empresaid,
    COALESCE(emp.nome, emp_cc.nome, 'Nao definido'::character varying) AS empresa,
    e.centrocusto AS centrocustoid,
    COALESCE(cc.nome, 'Nao definido'::character varying) AS centrocusto,
    e.contrato AS contratoid,
    COALESCE(con.descricao, 'Nao definido'::character varying) AS contrato,
    e.filial_id AS "Filialid",
    COALESCE(fil.nome, 'Nao definido'::character varying) AS "Filial"
   FROM equipamentos e
     LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     LEFT JOIN fabricantes f ON e.fabricante = f.id
     LEFT JOIN modelos m ON e.modelo = m.id
     LEFT JOIN notasfiscais nf ON e.notafiscal = nf.id
     LEFT JOIN fornecedores forn ON e.fornecedor = forn.id
     LEFT JOIN equipamentosstatus es ON e.equipamentostatus = es.id
     LEFT JOIN usuarios u ON e.usuario = u.id
     LEFT JOIN localidades l ON e.localidade_id = l.id
     LEFT JOIN empresas emp ON e.empresa = emp.id
     LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
     LEFT JOIN empresas emp_cc ON cc.empresa = emp_cc.id
     LEFT JOIN contratos con ON e.contrato = con.id
     LEFT JOIN filiais fil ON e.filial_id = fil.id
     LEFT JOIN tipoaquisicao ta ON e.tipoaquisicao = ta.id
  WHERE e.ativo = true;

-- View: EquipamentoHistoricoVM
CREATE OR REPLACE VIEW EquipamentoHistoricoVM AS
SELECT e.id, te.id TipoequipamentoID, te.descricao TipoEquipamento, f.Id FabricanteId, f.Descricao Fabricante, m.Id ModeloId, m.Descricao Modelo, e.NumeroSerie, e.Patrimonio, es.Id EquipamentoStatusId, es.Descricao EquipamentoStatus, c.Id ColaboradorId, c.Nome Colaborador, eh.DtRegistro,
	u.Id UsuarioId, u.Nome Usuario
FROM EquipamentoHistorico eh
	JOIN Equipamentos e ON eh.Equipamento = e.Id
	JOIN Usuarios u ON eh.Usuario = u.Id
	JOIN EquipamentosStatus es ON eh.EquipamentoStatus = es.Id
	JOIN Fabricantes f ON e.Fabricante = f.Id
	JOIN Modelos m ON e.Modelo = m.Id
	JOIN TipoEquipamentos te ON e.tipoequipamento = te.id
	LEFT JOIN Colaboradores c ON eh.Colaborador = c.Id;

-- View: RequisicoesVM
CREATE OR REPLACE VIEW RequisicoesVM AS
SELECT r.id, u.Id UsuarioRequisicaoId, u.Nome UsuarioRequisicao, t.Id TecnicoResponsavelId, t.Nome TecnicoResponsavel, c.Id ColaboradorFinalId, c.Nome ColaboradorFinal,
	r.DtSolicitacao, r.DtProcessamento, r.RequisicaoStatus RequisicaoStatusId, rs.Descricao RequisicaoStatus, r.AssinaturaEletronica, r.DtAssinaturaEletronica, r.DtEnvioTermo, r.HashRequisicao,
	(SELECT count(*) FROM requisicoesItens ri JOIN equipamentos e ON ri.Equipamento = e.id WHERE ri.Requisicao = r.Id AND DtEntrega IS NOT NULL AND DtDevolucao IS NULL AND e.EquipamentoStatus <> 8) EquipamentosPendentes, r.cliente
FROM requisicoes r
	JOIN RequisicoesStatus rs ON r.RequisicaoStatus = rs.id
	JOIN Usuarios u ON r.UsuarioRequisicao = u.Id
	JOIN Usuarios t ON r.TecnicoResponsavel = t.Id
	LEFT JOIN Colaboradores c ON r.ColaboradorFinal = c.Id;

-- View: RequisicaoEquipamentosVM
CREATE OR REPLACE VIEW RequisicaoEquipamentosVM AS  
SELECT ri.Id, r.Id Requisicao, ri.Equipamento EquipamentoId, concat(te.Descricao, ' ', f.Descricao, ' ', m.Descricao) Equipamento, e.NumeroSerie, e.Patrimonio, ue.Id UsuarioEntregaId, ue.Nome UsuarioEntrega,   
 ud.Id UsuarioDevolucaoId, ud.Nome UsuarioDevolucao, ri.DtEntrega, ri.DtDevolucao, ri.ObservacaoEntrega, ri.DtProgramadaRetorno, e.EquipamentoStatus, tl.Numero, ri.linhatelefonica linhaid, e.TipoAquisicao
FROM Requisicoes r   
	JOIN RequisicoesItens ri ON r.Id = ri.Requisicao  
	JOIN Equipamentos e ON ri.Equipamento = e.Id
	JOIN TipoEquipamentos te ON e.TipoEquipamento = te.Id
	JOIN Fabricantes f ON e.Fabricante = f.id  
	JOIN Modelos m ON e.Modelo = m.Id   
	LEFT JOIN Usuarios ue ON ri.UsuarioEntrega = ue.Id  
	LEFT JOIN Usuarios ud ON ri.UsuarioDevolucao = ud.Id  
	LEFT JOIN TelefoniaLinhas tl ON ri.LinhaTelefonica = tl.Id  
WHERE e.Id <> 1  
UNION ALL  
SELECT ri.Id, r.Id Requisicao, ri.Equipamento EquipamentoId, concat(f.Descricao, ' ', cast(tl.Numero as varchar)) Equipamento, '' NumeroSerie, e.Patrimonio, ue.Id UsuarioEntregaId, ue.Nome UsuarioEntrega,   
 ud.Id UsuarioDevolucaoId, ud.Nome UsuarioDevolucao, ri.DtEntrega, ri.DtDevolucao, ri.ObservacaoEntrega, ri.DtProgramadaRetorno, e.EquipamentoStatus, tl.Numero, ri.linhatelefonica linhaid, e.TipoAquisicao
FROM Requisicoes r   
	JOIN RequisicoesItens ri ON r.Id = ri.Requisicao  
	JOIN Equipamentos e ON ri.Equipamento = e.Id  
	JOIN TipoEquipamentos te ON e.TipoEquipamento = te.Id
	JOIN Fabricantes f ON e.Fabricante = f.id  
	JOIN Modelos m ON e.Modelo = m.Id   
	LEFT JOIN Usuarios ue ON ri.UsuarioEntrega = ue.Id  
	LEFT JOIN Usuarios ud ON ri.UsuarioDevolucao = ud.Id  
	JOIN TelefoniaLinhas tl ON ri.LinhaTelefonica = tl.Id  
WHERE e.Id = 1;

-- View: TermoEntregaVM
CREATE OR REPLACE VIEW TermoEntregaVM AS 
SELECT te.Descricao TipoEquipamento, fab.Descricao fabricante, mdl.Descricao modelo, eqp.NumeroSerie, COALESCE(eqp.Patrimonio, '') Patrimonio, ri.DtEntrega, ri.ObservacaoEntrega, ri.DtProgramadaRetorno, req.HashRequisicao, req.colaboradorfinal, req.cliente, eqp.tipoaquisicao
FROM Requisicoes req 
	JOIN requisicoesItens ri ON req.id = ri.Requisicao 
	JOIN Equipamentos eqp ON ri.Equipamento = eqp.Id 
	JOIN TipoEquipamentos te ON eqp.TipoEquipamento = te.Id 
	JOIN Fabricantes fab ON eqp.Fabricante = fab.Id 
	JOIN Modelos mdl ON eqp.Modelo = mdl.Id 
WHERE ri.DtDevolucao IS NULL AND req.RequisicaoStatus IN (1,3) AND eqp.EquipamentoStatus <> 8 AND eqp.Id <> 1
UNION ALL
SELECT te.Descricao TipoEquipamento, 'Número' fabricante, cast(tl.Numero as varchar) modelo, '' NumeroSerie, COALESCE(eqp.Patrimonio, '') Patrimonio, ri.DtEntrega, ri.ObservacaoEntrega, ri.DtProgramadaRetorno, req.HashRequisicao, req.colaboradorfinal, req.cliente, eqp.tipoaquisicao
FROM Requisicoes req 
	JOIN requisicoesItens ri ON req.id = ri.Requisicao 
	JOIN Equipamentos eqp ON ri.Equipamento = eqp.Id 
	JOIN TipoEquipamentos te ON eqp.TipoEquipamento = te.Id 
	JOIN Fabricantes fab ON eqp.Fabricante = fab.Id 
	JOIN Modelos mdl ON eqp.Modelo = mdl.Id 
	LEFT JOIN telefoniaLinhas tl ON ri.LinhaTelefonica = tl.Id 
WHERE ri.DtDevolucao IS NULL AND req.RequisicaoStatus IN (1,3) AND eqp.EquipamentoStatus <> 8 AND eqp.Id = 1;

-- View: vwNadaConsta
CREATE OR REPLACE VIEW vwNadaConsta AS
SELECT 
	c.id, c.Nome, c.Cpf, cc.Nome CentroCusto, e.Nome Empresa, c.Matricula, c.Cargo,
(SELECT count(*)
FROM Requisicoes r
	LEFT JOIN RequisicoesItens ri ON r.Id = ri.Requisicao
WHERE r.ColaboradorFinal = c.id AND ri.DtDevolucao IS NULL) MaquinasComColaborador, e.cliente
FROM colaboradores c
	JOIN CentroCusto cc ON c.CentroCusto = cc.Id
	JOIN Empresas e ON c.Empresa = e.Id;

-- View: vwEquipamentosStatus
CREATE OR REPLACE VIEW vwEquipamentosStatus AS
SELECT tec.cliente, descricao TipoEquipamento, 
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 1 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Danificado,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 2 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Devolvido,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 3 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) EmEstoque,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 4 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Entregue,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 5 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Extraviado,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 6 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Novo,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 7 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Requisitado,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 8 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Roubado,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 9 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) SemConserto,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 10 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Migrado,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 11 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Descartado
FROM TipoEquipamentos te
	JOIN tipoequipamentosclientes tec ON te.Id = tec.tipo
WHERE te.ativo = true;

-- View: vwestoqueequipamentosalerta
CREATE OR REPLACE VIEW vwestoqueequipamentosalerta AS
SELECT e.cliente,
    l.descricao AS localidade,
    te.descricao AS tipoequipamento,
    f.descricao AS fabricante,
    m.descricao AS modelo,
    count(
        CASE
            WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1
            ELSE NULL::integer
        END) AS estoqueatual,
    eme.quantidademinima AS estoqueminimo,
        CASE
            WHEN count(
            CASE
                WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1
                ELSE NULL::integer
            END) < eme.quantidademinima THEN 'ALERTA'::text
            ELSE 'OK'::text
        END AS status,
        CASE
            WHEN count(
            CASE
                WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1
                ELSE NULL::integer
            END) < eme.quantidademinima THEN eme.quantidademinima - count(
            CASE
                WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1
                ELSE NULL::integer
            END)
            ELSE 0::bigint
        END AS quantidadefaltante
   FROM equipamentos e
     JOIN modelos m ON e.modelo = m.id
     JOIN fabricantes f ON e.fabricante = f.id
     JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     JOIN localidades l ON e.localidade_id = l.id
     JOIN estoqueminimoequipamentos eme ON e.modelo = eme.modelo AND e.localidade_id = eme.localidade AND e.cliente = eme.cliente
  WHERE eme.ativo = true
  GROUP BY e.cliente, l.descricao, te.descricao, f.descricao, m.descricao, eme.quantidademinima;

-- View: vwExportacaoExcel
CREATE OR REPLACE VIEW vwExportacaoExcel AS
SELECT DISTINCT eqp.id, c.Nome Colaborador, c.Cargo, te.Descricao TipoEquipamento, fab.Descricao Fabricante, mdl.Descricao Modelo, concat(' ', cast(nf.Numero as varchar), nf.Descricao) NotaFiscal, es.Descricao EquipamentoStatus, es.id EquipamentostatusId,
	usu.Nome UsuarioCadastro, Loc.Descricao Localizacao, CASE eqp.PossuiBO WHEN false THEN 'Não' ELSE 'Sim' END PossuiBO, eqp.DescricaoBO, eqp.NumeroSerie, eqp.Patrimonio,
	eqp.DtCadastro, CASE eqp.TipoAquisicao WHEN 1 THEN 'Alugado' WHEN 2 THEN 'Próprio' WHEN 3 THEN 'Corporativo' ELSE 'Não Definido' END TipoAquisicao, eqp.cliente, eqp.ativo, emp.nome empresa, cc.nome centrocusto
FROM Equipamentos eqp
	JOIN TipoEquipamentos te ON eqp.TipoEquipamento = te.Id
	JOIN Fabricantes fab ON eqp.Fabricante = fab.Id
	JOIN Modelos mdl ON eqp.Modelo = mdl.Id
	JOIN EquipamentosStatus es ON eqp.EquipamentoStatus = es.Id
	JOIN Usuarios usu ON eqp.Usuario = usu.Id
	JOIN Localidades loc ON eqp.Localizacao = loc.Id
	LEFT JOIN NotasFiscais nf ON eqp.NotaFiscal = nf.Id
	LEFT JOIN RequisicoesItens ri ON eqp.id = ri.Equipamento AND ri.DtDevolucao IS NULL AND ri.DtEntrega IS NOT NULL
	LEFT JOIN Requisicoes r ON ri.Requisicao = r.Id
	LEFT JOIN Colaboradores c ON r.ColaboradorFinal = c.Id
	LEFT JOIN Empresas emp ON eqp.empresa = emp.id
	LEFT JOIN CentroCusto cc ON eqp.centrocusto = cc.id
WHERE eqp.Id > 1 AND eqp.Ativo = true;

-- View: ColaboradorHistoricoVM
CREATE OR REPLACE VIEW ColaboradorHistoricoVM AS
SELECT c.Id, c.Nome, Cpf, Matricula, Email, Cargo, 
	CASE
		WHEN Situacao = 'P' THEN 'Provisionado'
		WHEN Situacao = 'A' THEN 'Ativo'
		WHEN Situacao = 'I' THEN 'Inativo' 
		WHEN situacao IS NULL THEN 'N/I' END Situacao, 
	CASE 
		WHEN SituacaoAntiga = 'P' THEN 'Provisionado'
		WHEN SituacaoAntiga = 'A' THEN 'Ativo'
		WHEN SituacaoAntiga = 'I' THEN 'Inativo' 
		WHEN SituacaoAntiga IS NULL THEN 'N/I' END SituacaoAntiga, DtAtualizacao, c.Empresa EmpresaAtualId, e.Nome EmpresaAtual, c.AntigaEmpresa EmpresaAntigaId, ea.Nome EmpresaAntiga, DtAtualizacaoEmpresa, 
	c.Localidade LocalidadeAtualId, l.Descricao LocalidadeAtual, c.AntigaLocalidade LocalidadeAntigaId, la.Descricao LocalidadeAntiga, c.DtAtualizacaoLocalidade,
	c.CentroCusto CentroCustoAtualId, cc.Codigo CodigoCCAtual, cc.Nome NomeCCAtual, c.AntigoCentroCusto CentroCustoAntigoId, cca.Codigo CodigoCCAntigo, cca.Nome NomeCCAntigo, c.DtAtualizacaoCentroCusto, e.cliente
FROM colaboradores c
	JOIN Empresas e ON c.Empresa = e.Id
	LEFT JOIN Empresas ea ON c.AntigaEmpresa = ea.Id
	JOIN Localidades l ON c.Localidade = l.Id
	LEFT JOIN Localidades la ON c.AntigaLocalidade = la.Id
	JOIN CentroCusto cc ON c.CentroCusto = cc.Id
	LEFT JOIN CentroCusto cca ON c.AntigoCentroCusto = cca.Id
WHERE c.DtAtualizacao IS NOT NULL OR c.DtAtualizacaoCentroCusto IS NOT NULL OR c.DtAtualizacaoEmpresa IS NOT NULL OR c.DtAtualizacaoLocalidade IS NOT NULL;

-- View: vwDevolucaoProgramada
CREATE OR REPLACE VIEW vwDevolucaoProgramada AS
SELECT req.cliente, col.Nome nomeColaborador, ri.dtProgramadaRetorno
FROM requisicoes req
	JOIN RequisicoesItens ri ON req.id = ri.Requisicao
	JOIN colaboradores col ON req.ColaboradorFinal = col.Id
WHERE ri.DtProgramadaRetorno IS NOT NULL AND ri.DtDevolucao IS NULL AND req.RequisicaoStatus IN (1,3)
	AND req.id NOT IN(SELECT requisicao FROM requisicoesItens WHERE equipamento IN(SELECT id FROM equipamentos WHERE equipamentoStatus = 8));

-- View: vwequipamentoscomcolaboradoresdesligados
CREATE OR REPLACE VIEW vwequipamentoscomcolaboradoresdesligados AS
SELECT r.cliente,
	c.nome, c.dtdemissao,
	count(ri.equipamento) AS qtde
FROM ((requisicoes r
	JOIN requisicoesitens ri ON ((r.id = ri.requisicao)))
	JOIN colaboradores c ON ((r.colaboradorfinal = c.id)))
WHERE ((ri.dtdevolucao IS NULL) AND (c.dtdemissao IS NOT NULL AND c.dtdemissao < NOW()) AND (ri.equipamento IN ( SELECT equipamentos.id
		FROM equipamentos
		WHERE (equipamentos.equipamentostatus <> 8))))
GROUP BY r.cliente, c.nome, c.dtdemissao;

-- View: vwEquipamentosDetalhes
CREATE OR REPLACE VIEW vwEquipamentosDetalhes AS
SELECT e.id, e.cliente, te.id tipoEquipamentoID, te.descricao tipoequipamento, e.fabricante fabricanteId, f.descricao fabricante, m.id modeloid, m.descricao modelo,
	es.id equipamentoStatusID, es.descricao equipamentostatus, l.id localidadeid, l.descricao localidade, e.numeroserie, e.patrimonio, e.empresa empresaid, emp.nome empresa, e.centrocusto centrocustoid, cc.nome centrocusto
FROM equipamentos e
	JOIN tipoequipamentos te ON e.tipoequipamento = te.id
	JOIN fabricantes f ON e.fabricante = f.id
	JOIN modelos m ON e.modelo = m.id
	LEFT JOIN localidades l ON e.localizacao = l.id
	LEFT JOIN empresas emp ON e.empresa = emp.id
	LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
	JOIN equipamentosstatus es ON e.equipamentostatus = es.id
WHERE te.ativo = TRUE AND e.ativo = true;

-- View: vwTelefonia
CREATE OR REPLACE VIEW vwTelefonia AS 
SELECT o.nome operadora, c.nome contrato, p.nome plano, p.valor, l.numero, l.iccid, l.emuso, l.ativo, c.cliente
FROM telefoniaoperadoras o 
	JOIN telefoniacontratos c ON o.id = c.operadora
	JOIN telefoniaplanos p ON c.id = p.contrato
	JOIN telefonialinhas l ON p.id = l.plano
WHERE o.ativo = true AND c.ativo = true AND p.ativo = true AND l.ativo = true;

-- View: vwLaudos
CREATE OR REPLACE VIEW vwLaudos AS
SELECT l.id, l.cliente, concat(te.descricao, ' ', f.descricao, ' ', m.descricao) equipamento, e.numeroserie, e.patrimonio, l.descricao, l.laudo, l.dtentrada, l.dtlaudo, l.mauuso, l.temconserto, l.usuario, u.nome usuarionome, l.tecnico, u.nome tecniconome, l.valormanutencao, e.empresa, emp.nome empresanome, e.centrocusto, cc.nome centrocustonome
FROM laudos l
	JOIN equipamentos e ON l.equipamento = e.id
	JOIN tipoequipamentos te ON e.tipoequipamento = te.id
	JOIN fabricantes f ON e.fabricante = f.id
	JOIN modelos m ON e.modelo = m.id
	JOIN usuarios t ON l.tecnico = t.id
	JOIN usuarios u ON l.usuario = u.id
	LEFT JOIN empresas emp ON e.empresa = emp.id
	LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
WHERE l.ativo = true;

-- View: vwUltimasRequisicaoBYOD
CREATE OR REPLACE VIEW "vwUltimasRequisicaoBYOD" AS
SELECT r.id AS requisicaoid,
	r.cliente,
	r.usuariorequisicao,
	r.tecnicoresponsavel,
	r.requisicaostatus,
	r.colaboradorfinal,
	cf.nome AS nomecolaboradorfinal,
	r.dtsolicitacao,
	r.dtprocessamento,
	r.assinaturaeletronica,
	r.dtassinaturaeletronica,
	r.dtenviotermo,
	r.hashrequisicao,
	ri.id AS requisicaoitemid,
	ri.equipamento,
	ri.linhatelefonica,
	ri.usuarioentrega,
	ri.usuariodevolucao,
	ri.dtentrega,
	ri.dtdevolucao,
	ri.observacaoentrega,
	ri.dtprogramadaretorno,
	e.id AS equipamentoid,
	e.tipoaquisicao,
	e.equipamentostatus,
	e.numeroserie,
	e.patrimonio
FROM requisicoes r
	JOIN requisicoesitens ri ON r.id = ri.requisicao
	JOIN equipamentos e ON ri.equipamento = e.id
	LEFT JOIN colaboradores cf ON r.colaboradorfinal = cf.id
WHERE e.tipoaquisicao = 2
ORDER BY r.dtsolicitacao DESC
LIMIT 1000;

-- View: vwUltimasRequisicaoNaoBYOD
CREATE OR REPLACE VIEW "vwUltimasRequisicaoNaoBYOD" AS
SELECT r.id AS requisicaoid,
	r.cliente,
	r.usuariorequisicao,
	r.tecnicoresponsavel,
	r.requisicaostatus,
	r.colaboradorfinal,
	cf.nome AS nomecolaboradorfinal,
	r.dtsolicitacao,
	r.dtprocessamento,
	r.assinaturaeletronica,
	r.dtassinaturaeletronica,
	r.dtenviotermo,
	r.hashrequisicao,
	ri.id AS requisicaoitemid,
	ri.equipamento,
	ri.linhatelefonica,
	ri.usuarioentrega,
	ri.usuariodevolucao,
	ri.dtentrega,
	ri.dtdevolucao,
	ri.observacaoentrega,
	ri.dtprogramadaretorno,
	e.id AS equipamentoid,
	e.tipoaquisicao,
	e.equipamentostatus,
	e.numeroserie,
	e.patrimonio,
	tl.numero
FROM requisicoes r
	JOIN requisicoesitens ri ON r.id = ri.requisicao
	JOIN equipamentos e ON ri.equipamento = e.id
	LEFT JOIN colaboradores cf ON r.colaboradorfinal = cf.id
	LEFT JOIN telefonialinhas tl ON ri.linhatelefonica = tl.id
WHERE e.tipoaquisicao <> 2
ORDER BY r.dtsolicitacao DESC
LIMIT 1000;

-- View: colaboradoresvm
DROP VIEW IF EXISTS public.colaboradoresvm;

CREATE VIEW public.colaboradoresvm AS
SELECT
	c.id,
	c.cliente,
	e.nome AS empresa,
	cc.nome AS nomecentrocusto,
	cc.codigo AS codigocentrocusto,
	c.nome,
	c.cpf,
	c.matricula,
	c.email,
	c.tipocolaborador::text AS tipocolaborador,
	(
		CASE
			WHEN COALESCE(NULLIF(c.situacao, ''), 'A') IN ('A','D','I','F') THEN COALESCE(NULLIF(c.situacao, ''), 'A')
			WHEN c.dtdemissao IS NULL THEN 'A'
			WHEN c.dtdemissao < (CURRENT_DATE)::timestamp THEN 'D'
			ELSE 'A'
		END
	)::text AS situacao,
	c.cargo,
	c.setor,
	COALESCE(l.descricao, '') AS localidadedescricao,
	COALESCE(l.cidade, '') AS localidadecidade,
	COALESCE(l.estado, '') AS localidadeestado,
	c.dtadmissao,
	c.dtdemissao,
	c.dtcadastro,
	COALESCE(c.matriculasuperior, '') AS matriculasuperior
FROM colaboradores c
	JOIN empresas e ON c.empresa = e.id
	JOIN centrocusto cc ON c.centrocusto = cc.id
	LEFT JOIN localidades l ON l.id = c.localidade;

-- View: vw_equipamentos_compartilhados
CREATE OR REPLACE VIEW vw_equipamentos_compartilhados AS
SELECT 
	e.id AS equipamento_id,
	e.patrimonio,
	e.numeroserie,
	e.compartilhado,
	e.usuario AS responsavel_principal_id,
	u_resp.nome AS responsavel_principal_nome,
	te.descricao AS tipo_equipamento,
	m.descricao AS modelo,
	f.descricao AS fabricante,
	es.descricao AS status,
	l.descricao AS localidade,
	emp.nome AS empresa,
	(SELECT COUNT(*) 
	 FROM equipamento_usuarios_compartilhados euc 
	 WHERE euc.equipamento_id = e.id AND euc.ativo = TRUE
	) AS total_usuarios_compartilhados
FROM equipamentos e
LEFT JOIN usuarios u_resp ON e.usuario = u_resp.id
LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
LEFT JOIN modelos m ON e.modelo = m.id
LEFT JOIN fabricantes f ON e.fabricante = f.id
LEFT JOIN equipamentosstatus es ON e.equipamentostatus = es.id
LEFT JOIN localidades l ON e.localizacao = l.id
LEFT JOIN empresas emp ON e.empresa = emp.id
WHERE e.ativo = TRUE AND e.compartilhado = TRUE;

-- View: vw_equipamentos_usuarios_compartilhados
CREATE OR REPLACE VIEW vw_equipamentos_usuarios_compartilhados AS
SELECT 
	euc.id,
	euc.equipamento_id,
	e.patrimonio,
	e.numeroserie,
	euc.colaborador_id,
	c.nome AS colaborador_nome,
	c.matricula AS colaborador_matricula,
	c.email AS colaborador_email,
	c.cargo AS colaborador_cargo,
	euc.data_inicio,
	euc.data_fim,
	euc.ativo,
	euc.tipo_acesso,
	euc.observacao,
	euc.criado_por,
	u_criador.nome AS criado_por_nome,
	euc.criado_em,
	CASE 
		WHEN euc.ativo = FALSE THEN 'Inativo'
		WHEN euc.data_fim IS NULL THEN 'Ativo - Indefinido'
		WHEN euc.data_fim < CURRENT_TIMESTAMP THEN 'Expirado'
		ELSE 'Ativo - Temporário'
	END AS status_acesso
FROM equipamento_usuarios_compartilhados euc
INNER JOIN equipamentos e ON euc.equipamento_id = e.id
INNER JOIN colaboradores c ON euc.colaborador_id = c.id
LEFT JOIN usuarios u_criador ON euc.criado_por = u_criador.id
WHERE e.ativo = TRUE;

-- View: planosvm
CREATE OR REPLACE VIEW planosvm AS
SELECT 
    p.id,
    p.nome AS plano,
    p.ativo,
    p.valor,
    c.nome AS contrato,
    c.id AS contratoid,
    o.nome AS operadora,
    o.id AS operadoraid,
    COALESCE(COUNT(l.id), 0) AS contlinhas,
    COALESCE(COUNT(CASE WHEN l.emuso = true THEN l.id END), 0) AS contlinhasemuso,
    COALESCE(COUNT(CASE WHEN l.emuso = false THEN l.id END), 0) AS contlinhaslivres
FROM telefoniaplanos p
LEFT JOIN telefoniacontratos c ON p.contrato = c.id
LEFT JOIN telefoniaoperadoras o ON c.operadora = o.id
LEFT JOIN telefonialinhas l ON l.plano = p.id
WHERE p.ativo = true
GROUP BY p.id, p.nome, p.ativo, p.valor, c.nome, c.id, o.nome, o.id;

COMMENT ON VIEW planosvm IS 'View para listar planos de telefonia com informações agregadas de linhas';

-- View: vwplanostelefonia
CREATE OR REPLACE VIEW vwplanostelefonia AS
SELECT 
    p.id,
    p.nome AS plano,
    p.ativo,
    p.valor,
    c.nome AS contrato,
    c.id AS contratoid,
    o.nome AS operadora,
    o.id AS operadoraid,
    COALESCE(COUNT(l.id), 0) AS contlinhas,
    COALESCE(COUNT(CASE WHEN l.emuso = true THEN l.id END), 0) AS contlinhasemuso,
    COALESCE(COUNT(CASE WHEN l.emuso = false THEN l.id END), 0) AS contlinhaslivres
FROM telefoniaplanos p
LEFT JOIN telefoniacontratos c ON p.contrato = c.id
LEFT JOIN telefoniaoperadoras o ON c.operadora = o.id
LEFT JOIN telefonialinhas l ON l.plano = p.id
WHERE p.ativo = true
GROUP BY p.id, p.nome, p.ativo, p.valor, c.nome, c.id, o.nome, o.id;

COMMENT ON VIEW vwplanostelefonia IS 'View para listar planos de telefonia com informações agregadas de linhas';

-- View: vw_tinone_estatisticas
CREATE OR REPLACE VIEW vw_tinone_estatisticas AS
SELECT count(*) AS total_interacoes,
    count(DISTINCT usuario_id) AS usuarios_unicos,
    count(DISTINCT sessao_id) AS sessoes_unicas,
    avg(tempo_resposta_ms) AS tempo_medio_resposta,
    count(
        CASE
            WHEN foi_util = true THEN 1
            ELSE NULL::integer
        END) AS feedbacks_positivos,
    count(
        CASE
            WHEN foi_util = false THEN 1
            ELSE NULL::integer
        END) AS feedbacks_negativos,
    date(created_at) AS data
   FROM tinone_analytics
  GROUP BY (date(created_at))
  ORDER BY (date(created_at)) DESC;

COMMENT ON VIEW vw_tinone_estatisticas IS 'Estatísticas diárias de uso do TinOne';

-- View: vw_campanhas_resumo
CREATE OR REPLACE VIEW vw_campanhas_resumo AS
SELECT 
    c.id,
    c.cliente,
    c.nome,
    c.descricao,
    c.datacriacao,
    c.datainicio,
    c.datafim,
    c.status,
    c.totalcolaboradores,
    c.totalenviados,
    c.totalassinados,
    c.totalpendentes,
    c.percentualadesao,
    c.dataultimoenvio,
    c.dataconclusao,
    u.nome AS usuariocriacao_nome,
    CASE c.status
        WHEN 'A' THEN 'Ativa'
        WHEN 'I' THEN 'Inativa'
        WHEN 'C' THEN 'Concluída'
        WHEN 'G' THEN 'Agendada'
    END AS status_descricao,
    COUNT(cc.id) AS total_colaboradores_cadastrados,
    COUNT(CASE WHEN cc.statusassinatura = 'A' THEN 1 END) AS total_assinados_real,
    COUNT(CASE WHEN cc.statusassinatura IN ('P', 'E') THEN 1 END) AS total_pendentes_real
FROM campanhasassinaturas c
LEFT JOIN usuarios u ON c.usuariocriacao = u.id
LEFT JOIN campanhascolaboradores cc ON c.id = cc.campanhaid
GROUP BY c.id, c.cliente, c.nome, c.descricao, c.datacriacao, c.datainicio, c.datafim, 
         c.status, c.totalcolaboradores, c.totalenviados, c.totalassinados, 
         c.totalpendentes, c.percentualadesao, c.dataultimoenvio, c.dataconclusao, u.nome;

COMMENT ON VIEW vw_campanhas_resumo IS 'Visão resumida das campanhas com estatísticas atualizadas';

-- View: vw_campanhas_colaboradores_detalhado
CREATE OR REPLACE VIEW vw_campanhas_colaboradores_detalhado AS
SELECT 
    c.id AS campanha_id,
    c.nome AS campanha_nome,
    c.status AS campanha_status,
    cc.id AS associacao_id,
    cc.colaboradorid,
    col.nome AS colaborador_nome,
    col.cpf AS colaborador_cpf,
    col.email AS colaborador_email,
    col.cargo AS colaborador_cargo,
    e.nome AS empresa_nome,
    l.descricao AS localidade_nome,
    cc.statusassinatura,
    CASE cc.statusassinatura
        WHEN 'P' THEN 'Pendente'
        WHEN 'E' THEN 'Enviado'
        WHEN 'A' THEN 'Assinado'
        WHEN 'R' THEN 'Recusado'
    END AS status_descricao,
    cc.datainclusao,
    cc.dataenvio,
    cc.dataassinatura,
    cc.totalenvios,
    cc.dataultimoenvio,
    cc.ipenvio,
    cc.localizacaoenvio
FROM campanhasassinaturas c
INNER JOIN campanhascolaboradores cc ON c.id = cc.campanhaid
INNER JOIN colaboradores col ON cc.colaboradorid = col.id
LEFT JOIN empresas e ON col.empresa = e.id
LEFT JOIN localidades l ON col.localidade = l.id;

COMMENT ON VIEW vw_campanhas_colaboradores_detalhado IS 'Visão detalhada de colaboradores por campanha';

-- View: vw_nao_conformidade_elegibilidade
CREATE OR REPLACE VIEW vw_nao_conformidade_elegibilidade AS
WITH equipamentos_alocados AS (
    SELECT DISTINCT ON (c.id, e.id)
        c.id AS colaborador_id,
        c.nome AS colaborador_nome,
        c.cpf AS colaborador_cpf,
        c.email AS colaborador_email,
        c.cargo AS colaborador_cargo,
        c.tipocolaborador AS tipo_colaborador,
        CASE c.tipocolaborador
            WHEN 'F'::bpchar THEN 'Funcionário'::text
            WHEN 'T'::bpchar THEN 'Terceirizado'::text
            WHEN 'C'::bpchar THEN 'Consultor'::text
            ELSE 'Desconhecido'::text
        END AS tipo_colaborador_descricao,
        emp.nome AS empresa_nome,
        COALESCE((cc.codigo::text || ' - '::text) || cc.nome::text, ''::text) AS centro_custo,
        loc.descricao AS localidade,
        e.id AS equipamento_id,
        e.patrimonio AS equipamento_patrimonio,
        e.numeroserie AS equipamento_serie,
        te.id AS tipo_equipamento_id,
        te.descricao AS tipo_equipamento_descricao,
        te.categoria_id AS categoria_equipamento,
        e.fabricante AS fabricante_id,
        f.descricao AS fabricante,
        e.modelo AS modelo_id,
        m.descricao AS modelo,
        e.equipamentostatus AS equipamento_status,
        c.cliente
    FROM colaboradores c
    JOIN requisicoes r ON r.colaboradorfinal = c.id
    JOIN requisicoesitens ri ON ri.requisicao = r.id
    JOIN equipamentos e ON e.id = ri.equipamento
    JOIN tipoequipamentos te ON te.id = e.tipoequipamento
    LEFT JOIN fabricantes f ON f.id = e.fabricante
    LEFT JOIN modelos m ON m.id = e.modelo
    LEFT JOIN empresas emp ON emp.id = c.empresa
    LEFT JOIN centrocusto cc ON cc.id = c.centrocusto
    LEFT JOIN localidades loc ON loc.id = c.localidade
    WHERE ri.dtdevolucao IS NULL AND ri.equipamento IS NOT NULL AND (c.dtdemissao IS NULL OR c.dtdemissao > now()) AND (e.tipoaquisicao IS NULL OR e.tipoaquisicao <> 2) AND e.equipamentostatus = 4
),
contagem_equipamentos AS (
    SELECT equipamentos_alocados.colaborador_id,
        equipamentos_alocados.tipo_equipamento_id,
        count(*) AS quantidade_atual
       FROM equipamentos_alocados
      GROUP BY equipamentos_alocados.colaborador_id, equipamentos_alocados.tipo_equipamento_id
),
politicas_aplicaveis AS (
    SELECT DISTINCT ON (ea.colaborador_id, ea.tipo_equipamento_id)
        ea.colaborador_id,
        ea.tipo_colaborador,
        ea.colaborador_cargo,
        ea.tipo_equipamento_id,
        pe.id AS politica_id,
        pe.permite_acesso AS permite_acesso,
        pe.quantidade_maxima AS quantidade_maxima,
        pe.observacoes AS politica_observacoes,
        pe.usarpadrao,
        pe.cargo AS politica_cargo
    FROM equipamentos_alocados ea
    LEFT JOIN politicas_elegibilidade pe ON 
        pe.tipo_colaborador = ea.tipo_colaborador
        AND pe.tipo_equipamento_id = ea.tipo_equipamento_id
        AND pe.cliente = ea.cliente
        AND pe.ativo = true
        AND (
            pe.cargo IS NULL 
            OR pe.cargo = ''
            OR (
                pe.cargo IS NOT NULL 
                AND pe.cargo <> ''
                AND ea.colaborador_cargo IS NOT NULL
                AND (
                    (pe.usarpadrao = false AND UPPER(TRIM(ea.colaborador_cargo)) = UPPER(TRIM(pe.cargo)))
                    OR (pe.usarpadrao = true AND UPPER(ea.colaborador_cargo) LIKE '%' || UPPER(TRIM(pe.cargo)) || '%')
                )
            )
        )
)
SELECT ea.colaborador_id,
    ea.colaborador_nome,
    ea.colaborador_cpf,
    ea.colaborador_email,
    ea.colaborador_cargo,
    ea.tipo_colaborador,
    ea.tipo_colaborador_descricao,
    ea.empresa_nome,
    ea.centro_custo,
    ea.localidade,
    ea.equipamento_id,
    ea.equipamento_patrimonio,
    ea.equipamento_serie,
    ea.tipo_equipamento_id,
    ea.tipo_equipamento_descricao,
    ea.categoria_equipamento,
    ea.fabricante_id,
    ea.fabricante,
    ea.modelo_id,
    ea.modelo,
    ea.equipamento_status,
    ea.cliente,
    pa.politica_id,
    COALESCE(pa.permite_acesso, true) AS permite_acesso,
    pa.quantidade_maxima,
    pa.politica_observacoes,
    ce.quantidade_atual,
    now() AS dt_geracao_relatorio
   FROM equipamentos_alocados ea
     LEFT JOIN politicas_aplicaveis pa ON pa.colaborador_id = ea.colaborador_id AND pa.tipo_equipamento_id = ea.tipo_equipamento_id
     LEFT JOIN contagem_equipamentos ce ON ce.colaborador_id = ea.colaborador_id AND ce.tipo_equipamento_id = ea.tipo_equipamento_id
  WHERE pa.politica_id IS NOT NULL AND pa.permite_acesso = false OR pa.politica_id IS NOT NULL AND pa.permite_acesso = true AND pa.quantidade_maxima IS NOT NULL AND ce.quantidade_atual > pa.quantidade_maxima
  ORDER BY ea.colaborador_nome, ea.tipo_equipamento_descricao;

COMMENT ON VIEW vw_nao_conformidade_elegibilidade IS 'Identifica colaboradores que possuem equipamentos mas não são elegíveis conforme políticas';

-- View: equipamentovm (minúsculo)
CREATE OR REPLACE VIEW equipamentovm AS
SELECT e.id,
    e.tipoequipamento AS tipoequipamentoid,
    COALESCE(te.descricao, 'Nao definido'::character varying) AS tipoequipamento,
    e.fabricante AS fabricanteid,
    COALESCE(f.descricao, 'Nao definido'::character varying) AS fabricante,
    e.modelo AS modeloid,
    COALESCE(m.descricao, 'Nao definido'::character varying) AS modelo,
    e.notafiscal AS notafiscalid,
        CASE
            WHEN e.notafiscal IS NOT NULL THEN nf.numero::character varying
            ELSE 'Nao definido'::character varying
        END AS "Notafiscal",
    e.equipamentostatus AS equipamentostatusid,
    COALESCE(es.descricao, 'Nao definido'::character varying) AS equipamentostatus,
    e.usuario AS usuarioid,
    COALESCE(u.nome, 'Nao definido'::character varying) AS usuario,
    e.localidade_id AS localizacaoid,
        CASE
            WHEN e.localidade_id = 1 THEN 'Nao definido'::character varying
            ELSE COALESCE(l.descricao, 'Nao definido'::character varying)
        END AS localizacao,
    e.possuibo,
    e.descricaobo,
    e.numeroserie,
    e.patrimonio,
    e.dtlimitegarantia,
    e.dtcadastro,
    e.tipoaquisicao,
    COALESCE(ta.nome, 'Nao definido'::character varying) AS "TipoAquisicao",
    e.fornecedor,
        CASE
            WHEN e.fornecedor IS NOT NULL THEN forn.nome
            ELSE 'Nao definido'::character varying
        END AS "FornecedorNome",
    e.cliente,
    NULL::text AS colaboradorid,
    NULL::text AS colaboradornome,
    NULL::text AS requisicaoid,
    e.ativo,
    COALESCE(e.empresa, cc.empresa) AS empresaid,
    COALESCE(emp.nome, emp_cc.nome, 'Nao definido'::character varying) AS empresa,
    e.centrocusto AS centrocustoid,
    COALESCE(cc.nome, 'Nao definido'::character varying) AS centrocusto,
    e.contrato AS contratoid,
    COALESCE(con.descricao, 'Nao definido'::character varying) AS contrato,
    e.filial_id AS "Filialid",
    COALESCE(fil.nome, 'Nao definido'::character varying) AS "Filial"
   FROM equipamentos e
     LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     LEFT JOIN fabricantes f ON e.fabricante = f.id
     LEFT JOIN modelos m ON e.modelo = m.id
     LEFT JOIN notasfiscais nf ON e.notafiscal = nf.id
     LEFT JOIN fornecedores forn ON e.fornecedor = forn.id
     LEFT JOIN equipamentosstatus es ON e.equipamentostatus = es.id
     LEFT JOIN usuarios u ON e.usuario = u.id
     LEFT JOIN localidades l ON e.localidade_id = l.id
     LEFT JOIN empresas emp ON e.empresa = emp.id
     LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
     LEFT JOIN empresas emp_cc ON cc.empresa = emp_cc.id
     LEFT JOIN contratos con ON e.contrato = con.id
     LEFT JOIN filiais fil ON e.filial_id = fil.id
     LEFT JOIN tipoaquisicao ta ON e.tipoaquisicao = ta.id
  WHERE e.ativo = true;

-- View: termoentregavm (minúsculo)
CREATE OR REPLACE VIEW termoentregavm AS
SELECT te.descricao AS tipoequipamento,
    f.descricao AS fabricante,
    m.descricao AS modelo,
    e.numeroserie,
    e.patrimonio,
    ri.dtentrega,
    ri.observacaoentrega,
    ri.dtprogramadaretorno,
    r.hashrequisicao,
    r.colaboradorfinal,
    r.cliente,
        CASE
            WHEN e.tipoaquisicao = 2 THEN 2
            ELSE 1
        END AS tipoaquisicao
   FROM equipamentos e
     JOIN requisicoesitens ri ON e.id = ri.equipamento
     JOIN requisicoes r ON ri.requisicao = r.id
     JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     LEFT JOIN fabricantes f ON e.fabricante = f.id
     LEFT JOIN modelos m ON e.modelo = m.id
  WHERE r.requisicaostatus = 3 AND ri.dtdevolucao IS NULL;

-- View: vw_colaboradores_simples
CREATE OR REPLACE VIEW vw_colaboradores_simples AS
SELECT c.id,
    c.nome,
    c.cpf,
    c.matricula,
    c.email,
    c.cargo,
    c.setor,
    c.dtadmissao,
    c.situacao,
    c.empresa,
    e.nome AS empresa_nome,
    c.centrocusto,
    cc.nome AS centro_custo_nome,
    c.filial_id,
    f.nome AS filial_nome,
    c.localidade_id,
    l.descricao AS localidade_nome,
    COALESCE(c.cliente, e.cliente) AS cliente,
    cl.razaosocial AS cliente_nome
   FROM colaboradores c
     JOIN empresas e ON c.empresa = e.id
     JOIN centrocusto cc ON c.centrocusto = cc.id
     LEFT JOIN filiais f ON c.filial_id = f.id
     LEFT JOIN localidades l ON c.localidade_id = l.id
     JOIN clientes cl ON COALESCE(c.cliente, e.cliente) = cl.id;

-- View: vw_equipamentos_simples
CREATE OR REPLACE VIEW vw_equipamentos_simples AS
SELECT e.id,
    e.numeroserie,
    e.patrimonio,
    e.dtcadastro,
    e.ativo,
    e.empresa,
    emp.nome AS empresa_nome,
    e.centrocusto,
    cc.nome AS centro_custo_nome,
    e.filial_id,
    f.nome AS filial_nome,
    e.localidade_id,
    l.descricao AS localidade_nome,
    COALESCE(e.cliente, emp.cliente) AS cliente,
    cl.razaosocial AS cliente_nome
   FROM equipamentos e
     LEFT JOIN empresas emp ON e.empresa = emp.id
     LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
     LEFT JOIN filiais f ON e.filial_id = f.id
     LEFT JOIN localidades l ON e.localidade_id = l.id
     LEFT JOIN clientes cl ON COALESCE(e.cliente, emp.cliente) = cl.id;

-- View: vwestoquelinhasalerta
CREATE OR REPLACE VIEW vwestoquelinhasalerta AS
SELECT c.cliente,
    l.descricao AS localidade,
    o.nome AS operadora,
    c.nome AS contrato,
    p.nome AS plano,
    eml.perfiluso,
    count(
        CASE
            WHEN tl.emuso = false AND tl.ativo = true THEN 1
            ELSE NULL::integer
        END) AS estoqueatual,
    eml.quantidademinima AS estoqueminimo,
        CASE
            WHEN count(
            CASE
                WHEN tl.emuso = false AND tl.ativo = true THEN 1
                ELSE NULL::integer
            END) < eml.quantidademinima THEN 'ALERTA'::text
            ELSE 'OK'::text
        END AS status,
        CASE
            WHEN count(
            CASE
                WHEN tl.emuso = false AND tl.ativo = true THEN 1
                ELSE NULL::integer
            END) < eml.quantidademinima THEN eml.quantidademinima - count(
            CASE
                WHEN tl.emuso = false AND tl.ativo = true THEN 1
                ELSE NULL::integer
            END)
            ELSE 0::bigint
        END AS quantidadefaltante
   FROM telefonialinhas tl
     JOIN telefoniaplanos p ON tl.plano = p.id
     JOIN telefoniacontratos c ON p.contrato = c.id
     JOIN telefoniaoperadoras o ON c.operadora = o.id
     JOIN estoqueminimolinhas eml ON c.operadora = eml.operadora AND p.id = eml.plano AND c.cliente = eml.cliente
     JOIN localidades l ON eml.localidade = l.id
  WHERE eml.ativo = true
  GROUP BY c.cliente, l.descricao, o.nome, c.nome, p.nome, eml.perfiluso, eml.quantidademinima;

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
INSERT INTO clientes(razaosocial, cnpj, ativo) VALUES('SingleOne', '51908470000199', true)
ON CONFLICT DO NOTHING;

-- Inserir Status de Requisições
INSERT INTO RequisicoesStatus(Descricao, ativo) VALUES('Ativa', true) ON CONFLICT DO NOTHING;
INSERT INTO RequisicoesStatus(Descricao, ativo) VALUES('Cancelada', true) ON CONFLICT DO NOTHING;
INSERT INTO RequisicoesStatus(Descricao, ativo) VALUES('Processada', true) ON CONFLICT DO NOTHING;

-- Inserir Status de Equipamentos
INSERT INTO EquipamentosStatus(Descricao, ativo) VALUES('Danificado', true) ON CONFLICT DO NOTHING;
INSERT INTO EquipamentosStatus(Descricao, ativo) VALUES('Devolvido', true) ON CONFLICT DO NOTHING;
INSERT INTO EquipamentosStatus(Descricao, ativo) VALUES('Em estoque', true) ON CONFLICT DO NOTHING;
INSERT INTO EquipamentosStatus(Descricao, ativo) VALUES('Entregue', true) ON CONFLICT DO NOTHING;
INSERT INTO EquipamentosStatus(Descricao, ativo) VALUES('Extraviado', true) ON CONFLICT DO NOTHING;
INSERT INTO EquipamentosStatus(Descricao, ativo) VALUES('Novo', true) ON CONFLICT DO NOTHING;
INSERT INTO EquipamentosStatus(Descricao, ativo) VALUES('Requisitado', true) ON CONFLICT DO NOTHING;
INSERT INTO EquipamentosStatus(Descricao, ativo) VALUES('Roubado', true) ON CONFLICT DO NOTHING;
INSERT INTO EquipamentosStatus(Descricao, ativo) VALUES('Sinistrado', true) ON CONFLICT DO NOTHING;
INSERT INTO EquipamentosStatus(Descricao, ativo) VALUES('Descartado', true) ON CONFLICT DO NOTHING;

-- Inserir Usuário Administrador
INSERT INTO Usuarios(Cliente, Nome, Email, Senha, PalavraCriptografada, Su, Adm, Operador, consulta, Ativo) 
VALUES(1, 'Adminstrador', 'administrador@singleone.tech', 'MTQyNTM2QEFkbWlu', '', true, true, false, false, true)
ON CONFLICT (Email) DO NOTHING;

-- Inserir Localidades Padrão
INSERT INTO Localidades(Descricao, Ativo, Cliente) VALUES('Padrão', FALSE, 1) ON CONFLICT DO NOTHING;

-- Inserir Tipo de Equipamento para Telefonia (necessário para recursos de telefonia)
INSERT INTO TipoEquipamentos(Descricao, ativo) VALUES('Linha Telefonica', true) ON CONFLICT DO NOTHING;
INSERT INTO Fabricantes(TipoEquipamento, Descricao, Ativo, Cliente) VALUES(1, 'Linha Telefonica', false, 1) ON CONFLICT DO NOTHING;
INSERT INTO Modelos(Fabricante, Descricao, Ativo, Cliente) VALUES(1, 'Linha Telefonica', false, 1) ON CONFLICT DO NOTHING;

-- Inserir Tipos de Aquisiço
INSERT INTO TipoAquisicao (Id, Nome) VALUES (1, 'Alugado') ON CONFLICT (Id) DO UPDATE SET Nome = 'Alugado';
INSERT INTO TipoAquisicao (Id, Nome) VALUES (2, 'Próprio') ON CONFLICT (Id) DO UPDATE SET Nome = 'Próprio';
INSERT INTO TipoAquisicao (Id, Nome) VALUES (3, 'Corporativo') ON CONFLICT (Id) DO UPDATE SET Nome = 'Corporativo';

-- Inserir Equipamento Dummy (necessário para telefonia)
INSERT INTO Equipamentos(Cliente, TipoEquipamento, Fabricante, Modelo, EquipamentoStatus, Usuario, Localizacao, PossuiBO, NumeroSerie, DtCadastro, Ativo, TipoAquisicao)
VALUES(1, 1, 1, 1, 6, 1, 1, false, 'Não cadastrado', now(), false, 3)
ON CONFLICT DO NOTHING;

-- Inserir Tipos de Templates
INSERT INTO TemplateTipos(Id, Descricao) VALUES(1, 'Termo de nada consta') ON CONFLICT DO NOTHING;
INSERT INTO TemplateTipos(Id, Descricao) VALUES(2, 'Termo de responsabilidade') ON CONFLICT DO NOTHING;
INSERT INTO TemplateTipos(Id, Descricao) VALUES(3, 'Termo de responsabilidade - BYOD') ON CONFLICT DO NOTHING;
INSERT INTO TemplateTipos(Id, Descricao) VALUES(4, 'Termo de sinistros') ON CONFLICT DO NOTHING;
INSERT INTO TemplateTipos(Id, Descricao) VALUES(5, 'Termo de descarte, doação, logística reversa') ON CONFLICT DO NOTHING;
INSERT INTO TemplateTipos(Id, Descricao) VALUES(6, 'Levantamento de Recursos - Inventário forçado') ON CONFLICT DO NOTHING;

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
INSERT INTO regrasTemplate (TipoTemplate, TipoAquisicao) VALUES (2, 1) ON CONFLICT DO NOTHING;
INSERT INTO regrasTemplate (TipoTemplate, TipoAquisicao) VALUES (2, 3) ON CONFLICT DO NOTHING;
INSERT INTO regrasTemplate (TipoTemplate, TipoAquisicao) VALUES (3, 2) ON CONFLICT DO NOTHING;

-- Inserir Status de Contratos
INSERT INTO contratostatus(id, nome) VALUES 
	(1, 'Aguardando Inicio Vigência'),
	(2, 'Vigente'),
	(3, 'Vencido')
ON CONFLICT DO NOTHING;

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
CREATE TABLE IF NOT EXISTS Categorias
(
    Id serial not null primary key,
    Nome varchar(100) not null,
    Descricao text,
    Ativo boolean default true,
    DataCriacao timestamp default CURRENT_TIMESTAMP,
    DataAtualizacao timestamp default CURRENT_TIMESTAMP
);

-- Tabela: Filiais
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

-- Tabela: Requisicoes_Itens_Compartilhados
CREATE TABLE IF NOT EXISTS Requisicoes_Itens_Compartilhados
(
    Id serial not null primary key,
    Requisicao_Item_Id int not null,
    Colaborador_Id int not null,
    Tipo_Acesso varchar(50) default 'usuario_compartilhado' not null,
    Data_Inicio timestamp default CURRENT_TIMESTAMP not null,
    Data_Fim timestamp,
    Observacao text,
    Ativo boolean default true not null,
    Criado_Por int not null,
    Criado_Em timestamp default CURRENT_TIMESTAMP not null,
    constraint fkReqItemComp_ReqItem foreign key (Requisicao_Item_Id) references RequisicoesItens(Id),
    constraint fkReqItemComp_Colaborador foreign key (Colaborador_Id) references Colaboradores(Id),
    constraint fkReqItemComp_CriadoPor foreign key (Criado_Por) references Usuarios(Id)
);

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
    constraint fkTinOneConfigCliente foreign key (Cliente) references Clientes(Id)
);

-- Tabela: TinOne_Analytics
CREATE TABLE IF NOT EXISTS TinOne_Analytics
(
    Id serial not null primary key,
    Usuario_Id int,
    Cliente_Id int,
    Sessao_Id varchar(100),
    Pagina_Url varchar(500),
    Pagina_Nome varchar(200),
    Acao_Tipo varchar(100),
    Pergunta text,
    Resposta text,
    Tempo_Resposta_Ms int,
    Foi_Util boolean,
    Feedback_Texto text,
    Created_At timestamp default CURRENT_TIMESTAMP,
    Updated_At timestamp default CURRENT_TIMESTAMP,
    constraint fkTinOneAnalyticsUsuario foreign key (Usuario_Id) references Usuarios(Id),
    constraint fkTinOneAnalyticsCliente foreign key (Cliente_Id) references Clientes(Id)
);

-- Tabela: TinOne_Conversas
CREATE TABLE IF NOT EXISTS TinOne_Conversas
(
    Id serial not null primary key,
    Usuario_Id int,
    Sessao_Id varchar(100),
    Tipo_Mensagem varchar(20),
    Mensagem text not null,
    Pagina_Contexto varchar(200),
    Metadata jsonb,
    Created_At timestamp default CURRENT_TIMESTAMP,
    constraint fkTinOneConversasUsuario foreign key (Usuario_Id) references Usuarios(Id)
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
-- MENSAGEM FINAL
-- =====================================================

DO $$
BEGIN
    RAISE NOTICE '========================================';
    RAISE NOTICE 'SCRIPT DE INICIALIZAÇÃO EXECUTADO COM SUCESSO!';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Banco de dados SingleOne inicializado';
    RAISE NOTICE 'Versão: 2.2 (Atualizado em 03/11/2025)';
    RAISE NOTICE '';
    RAISE NOTICE 'Dados básicos inseridos:';
    RAISE NOTICE '- Cliente Demo criado';
    RAISE NOTICE '- Usuário administrador: administrador@singleone.tech';
    RAISE NOTICE '- Status de equipamentos e requisições';
    RAISE NOTICE '- Estados brasileiros';
    RAISE NOTICE '- Tipos de aquisição';
    RAISE NOTICE '- Motivos de suspeita';
    RAISE NOTICE '';
    RAISE NOTICE 'Tabelas criadas:';
    RAISE NOTICE '- Estrutura principal do sistema (64 tabelas)';
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
END $$;

