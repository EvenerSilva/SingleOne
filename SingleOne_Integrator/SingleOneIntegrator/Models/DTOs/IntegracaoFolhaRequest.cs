using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SingleOneIntegrator.Models.DTOs
{
    /// <summary>
    /// Request para integração de folha de pagamento
    /// </summary>
    public class IntegracaoFolhaRequest
    {
        /// <summary>
        /// Timestamp da geração dos dados no sistema do cliente
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Tipo de operação: FULL_SYNC (sincronização completa) ou INCREMENTAL (apenas mudanças)
        /// </summary>
        [Required]
        [RegularExpression("^(FULL_SYNC|INCREMENTAL)$", ErrorMessage = "TipoOperacao deve ser FULL_SYNC ou INCREMENTAL")]
        public string TipoOperacao { get; set; }

        /// <summary>
        /// Lista de colaboradores
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "Deve conter pelo menos 1 colaborador")]
        [MaxLength(1000, ErrorMessage = "Máximo de 1000 colaboradores por requisição")]
        public List<ColaboradorDTO> Colaboradores { get; set; }
    }

    /// <summary>
    /// Dados de um colaborador
    /// </summary>
    public class ColaboradorDTO
    {
        /// <summary>
        /// Identificador único no sistema externo (usado para rastreabilidade)
        /// </summary>
        [MaxLength(100)]
        public string? IdentificadorExterno { get; set; }

        /// <summary>
        /// Nome completo do colaborador
        /// </summary>
        [Required(ErrorMessage = "NomeCompleto é obrigatório")]
        [MaxLength(200)]
        public string NomeCompleto { get; set; }

        /// <summary>
        /// CPF (apenas números)
        /// </summary>
        [Required(ErrorMessage = "CPF é obrigatório")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter exatamente 11 dígitos")]
        public string Cpf { get; set; }

        /// <summary>
        /// Email corporativo
        /// </summary>
        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? Email { get; set; }

        /// <summary>
        /// Matrícula do colaborador
        /// </summary>
        [MaxLength(50)]
        public string? Matricula { get; set; }

        /// <summary>
        /// Cargo
        /// </summary>
        [MaxLength(200)]
        public string? Cargo { get; set; }

        /// <summary>
        /// Código do centro de custo
        /// </summary>
        [MaxLength(50)]
        public string? CentroCusto { get; set; }

        /// <summary>
        /// Descrição do centro de custo
        /// </summary>
        [MaxLength(200)]
        public string? TxtCentroCusto { get; set; }

        /// <summary>
        /// Razão social da empresa
        /// </summary>
        [MaxLength(200)]
        public string? Empresa { get; set; }

        /// <summary>
        /// CNPJ da empresa (apenas números)
        /// </summary>
        [RegularExpression(@"^\d{14}$", ErrorMessage = "CNPJ deve conter exatamente 14 dígitos")]
        [MaxLength(14)]
        public string? CnpjEmpresa { get; set; }

        /// <summary>
        /// Nome fantasia da empresa
        /// </summary>
        [MaxLength(200)]
        public string? NomeFantasia { get; set; }

        /// <summary>
        /// Data de admissão
        /// </summary>
        public DateTime? DataAdmissao { get; set; }

        /// <summary>
        /// Data de demissão (se aplicável)
        /// </summary>
        public DateTime? DataDemissao { get; set; }

        /// <summary>
        /// Status: ATIVO, DEMITIDO, FERIAS, AFASTADO, etc
        /// </summary>
        [MaxLength(50)]
        public string? Status { get; set; }

        /// <summary>
        /// Tipo de colaborador: CLT, PJ, ESTAGIARIO, TEMPORARIO, etc
        /// </summary>
        [MaxLength(50)]
        public string? TipoColaborador { get; set; }

        /// <summary>
        /// Cidade
        /// </summary>
        [MaxLength(100)]
        public string? Cidade { get; set; }

        /// <summary>
        /// Estado (sigla)
        /// </summary>
        [MaxLength(2)]
        public string? Estado { get; set; }

        /// <summary>
        /// Nome do superior direto
        /// </summary>
        [MaxLength(200)]
        public string? Superior { get; set; }

        /// <summary>
        /// Nome de usuário no sistema (login)
        /// </summary>
        [MaxLength(100)]
        public string? NomeDeUsuario { get; set; }
    }
}


