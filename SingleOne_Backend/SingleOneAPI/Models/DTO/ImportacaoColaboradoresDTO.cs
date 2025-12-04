using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.DTO
{
    /// <summary>
    /// DTO para resultado da validação do arquivo de importação de colaboradores
    /// </summary>
    public class ResultadoValidacaoColaboradoresDTO
    {
        public Guid LoteId { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalValidos { get; set; }
        public int TotalAvisos { get; set; }
        public int TotalErros { get; set; }
        public int NovasEmpresas { get; set; }
        public int NovasLocalidades { get; set; }
        public int NovoscentrosCusto { get; set; }
        public int NovasFiliais { get; set; }
        public int TotalAtualizacoes { get; set; }
        public int TotalSemAlteracao { get; set; }
        public int TotalNovos { get; set; }
        public bool PodeImportar { get; set; }
        public string Mensagem { get; set; }
        public List<ErroValidacaoResumoDTO> ErrosCriticos { get; set; } = new List<ErroValidacaoResumoDTO>();
        public bool PossuiMaisErros { get; set; }
    }

    /// <summary>
    /// DTO para detalhes de um registro na staging de colaboradores
    /// </summary>
    public class DetalheColaboradorStagingDTO
    {
        public int Id { get; set; }
        public int LinhaArquivo { get; set; }
        
        // Dados do colaborador
        public string NomeColaborador { get; set; }
        public string Cpf { get; set; }
        public string Matricula { get; set; }
        public string Email { get; set; }
        public string Cargo { get; set; }
        public string Setor { get; set; }
        public DateTime? DataAdmissao { get; set; }
        public string TipoColaborador { get; set; }
        public DateTime? DataDemissao { get; set; }
        
        // Dados relacionados
        public string EmpresaNome { get; set; }
        public string EmpresaCnpj { get; set; }
        public string LocalidadeDescricao { get; set; }
        public string LocalidadeCidade { get; set; }
        public string LocalidadeEstado { get; set; }
        public string CentroCustoCodigo { get; set; }
        public string CentroCustoNome { get; set; }
        public string FilialNome { get; set; }
        
        // Status
        public string Status { get; set; }
        public string StatusDescricao { get; set; }
        public List<string> Erros { get; set; }
        public List<string> Avisos { get; set; }
        
        // Flags
        public bool CriarEmpresa { get; set; }
        public bool CriarLocalidade { get; set; }
        public bool CriarCentroCusto { get; set; }
        public bool CriarFilial { get; set; }
    }

    /// <summary>
    /// DTO para resultado da importação efetivada de colaboradores
    /// </summary>
    public class ResultadoImportacaoColaboradoresDTO
    {
        public Guid LoteId { get; set; }
        public int EmpresasCriadas { get; set; }
        public int LocalidadesCriadas { get; set; }
        public int CentrosCustoCriados { get; set; }
        public int FiliaisCriadas { get; set; }
        public int ColaboradoresCriados { get; set; }
        public int ColaboradoresAtualizados { get; set; }
        public int ColaboradoresSemAlteracao { get; set; }
        public int TotalProcessado { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string Mensagem { get; set; }
        public bool Sucesso { get; set; }
    }

    /// <summary>
    /// DTO para resumo de validação por lote de colaboradores
    /// </summary>
    public class ResumoValidacaoColaboradoresDTO
    {
        public Guid LoteId { get; set; }
        public int Total { get; set; }
        public int Validos { get; set; }
        public int Avisos { get; set; }
        public int Erros { get; set; }
        public int Pendentes { get; set; }
        public int Importados { get; set; }
        
        // Contadores de novos registros
        public int NovasEmpresas { get; set; }
        public int NovasLocalidades { get; set; }
        public int NovosCentrosCusto { get; set; }
        public int NovasFiliais { get; set; }
        public int TotalAtualizacoes { get; set; }
        public int TotalSemAlteracao { get; set; }
        public int TotalNovos { get; set; }
        
        // Lista dos nomes das novas entidades
        public List<string> NomesEmpresasNovas { get; set; }
        public List<string> NomesLocalidadesNovas { get; set; }
        public List<string> NomesCentrosCustoNovos { get; set; }
        public List<string> NomesFiliaisNovas { get; set; }
    }

    public class ErroValidacaoResumoDTO
    {
        public int Linha { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Matricula { get; set; }
        public List<string> Mensagens { get; set; }
    }

    public class RecriptografarDocumentosResultadoDTO
    {
        public int TotalAtualizados { get; set; }
        public int TotalCpfAtualizados { get; set; }
        public int TotalEmailsAtualizados { get; set; }
    }
}

