using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    /// <summary>
    /// Modelo que representa um protocolo de descarte
    /// Um protocolo pode conter múltiplos equipamentos
    /// </summary>
    public partial class ProtocoloDescarte
    {
        public ProtocoloDescarte()
        {
            Itens = new HashSet<ProtocoloDescarteItem>();
            Evidencias = new HashSet<DescarteEvidencia>();
        }

        /// <summary>
        /// ID único do protocolo
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Número único do protocolo (ex: DESC-2025-001234)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Protocolo { get; set; }

        /// <summary>
        /// ID do cliente
        /// </summary>
        [Required]
        public int Cliente { get; set; }

        /// <summary>
        /// Tipo de descarte (DOACAO, VENDA, DEVOLUCAO, etc.)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TipoDescarte { get; set; }

        /// <summary>
        /// Motivo do descarte
        /// </summary>
        public string MotivoDescarte { get; set; }

        /// <summary>
        /// Destino final dos equipamentos
        /// </summary>
        [StringLength(500)]
        public string DestinoFinal { get; set; }

        /// <summary>
        /// Empresa responsável pelo destino final (logística reversa, reciclagem, etc)
        /// </summary>
        [StringLength(200)]
        public string EmpresaDestinoFinal { get; set; }

        /// <summary>
        /// CNPJ da empresa de destino final
        /// </summary>
        [StringLength(20)]
        public string CnpjDestinoFinal { get; set; }

        /// <summary>
        /// Número do certificado de descarte ambiental
        /// </summary>
        [StringLength(100)]
        public string CertificadoDescarte { get; set; }

        /// <summary>
        /// Indica se MTR (Manifesto de Transporte de Resíduos) é obrigatório
        /// </summary>
        public bool MtrObrigatorio { get; set; }

        /// <summary>
        /// Número do MTR emitido
        /// </summary>
        [StringLength(50)]
        public string MtrNumero { get; set; }

        /// <summary>
        /// Quem emitiu o MTR (GERADOR, TRANSPORTADOR, DESTINADOR)
        /// </summary>
        [StringLength(20)]
        public string MtrEmitidoPor { get; set; }

        /// <summary>
        /// Data de emissão do MTR
        /// </summary>
        public DateTime? MtrDataEmissao { get; set; }

        /// <summary>
        /// Data de validade do MTR
        /// </summary>
        public DateTime? MtrValidade { get; set; }

        /// <summary>
        /// Caminho do arquivo MTR (PDF)
        /// </summary>
        [StringLength(500)]
        public string MtrArquivo { get; set; }

        /// <summary>
        /// Empresa transportadora (quando MTR emitido por transportador)
        /// </summary>
        [StringLength(200)]
        public string MtrEmpresaTransportadora { get; set; }

        /// <summary>
        /// CNPJ da empresa transportadora
        /// </summary>
        [StringLength(20)]
        public string MtrCnpjTransportadora { get; set; }

        /// <summary>
        /// Placa do veículo de transporte
        /// </summary>
        [StringLength(10)]
        public string MtrPlacaVeiculo { get; set; }

        /// <summary>
        /// Nome do motorista responsável pelo transporte
        /// </summary>
        [StringLength(100)]
        public string MtrMotorista { get; set; }

        /// <summary>
        /// CPF do motorista
        /// </summary>
        [StringLength(14)]
        public string MtrCpfMotorista { get; set; }

        /// <summary>
        /// ID do usuário responsável pelo protocolo
        /// </summary>
        [Required]
        public int ResponsavelProtocolo { get; set; }

        /// <summary>
        /// Data de criação do protocolo
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data de conclusão do protocolo
        /// </summary>
        public DateTime? DataConclusao { get; set; }

        /// <summary>
        /// Status do protocolo (EM_ANDAMENTO, CONCLUIDO, CANCELADO)
        /// </summary>
        [Required]
        [StringLength(30)]
        public string Status { get; set; }

        /// <summary>
        /// Valor total estimado (principalmente para vendas)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorTotalEstimado { get; set; }

        /// <summary>
        /// Indica se o documento oficial foi gerado
        /// </summary>
        public bool DocumentoGerado { get; set; }

        /// <summary>
        /// Caminho do documento gerado
        /// </summary>
        [StringLength(500)]
        public string CaminhoDocumento { get; set; }

        /// <summary>
        /// Observações gerais do protocolo
        /// </summary>
        public string Observacoes { get; set; }

        /// <summary>
        /// Indica se o registro está ativo
        /// </summary>
        public bool Ativo { get; set; }

        // Navegação
        /// <summary>
        /// Cliente relacionado
        /// </summary>
        [ForeignKey("Cliente")]
        public virtual Cliente ClienteNavigation { get; set; }

        /// <summary>
        /// Usuário responsável pelo protocolo
        /// </summary>
        [ForeignKey("ResponsavelProtocolo")]
        public virtual Usuario ResponsavelNavigation { get; set; }

        /// <summary>
        /// Lista de equipamentos do protocolo
        /// </summary>
        public virtual ICollection<ProtocoloDescarteItem> Itens { get; set; }

        /// <summary>
        /// Lista de evidências vinculadas ao protocolo
        /// </summary>
        public virtual ICollection<DescarteEvidencia> Evidencias { get; set; }
    }
}
