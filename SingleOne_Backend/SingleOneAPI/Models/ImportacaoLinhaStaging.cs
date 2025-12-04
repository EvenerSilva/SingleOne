using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    [Table("importacao_linha_staging")]
    public class ImportacaoLinhaStaging
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("cliente")]
        public int Cliente { get; set; }
        
        [Column("lote_id")]
        public Guid LoteId { get; set; }
        
        [Column("usuario_importacao")]
        public int UsuarioImportacao { get; set; }
        
        [Column("data_importacao")]
        public DateTime DataImportacao { get; set; }
        
        // Dados da linha vindos do arquivo
        [Column("operadora_nome")]
        public string OperadoraNome { get; set; }
        
        [Column("contrato_nome")]
        public string ContratoNome { get; set; }
        
        [Column("plano_nome")]
        public string PlanoNome { get; set; }
        
        [Column("plano_valor")]
        public decimal PlanoValor { get; set; }
        
        [Column("numero_linha")]
        public decimal NumeroLinha { get; set; }
        
        [Column("iccid")]
        public string Iccid { get; set; }
        
        // Validação e Status
        [Column("status")]
        public string Status { get; set; }  // P=Pendente, V=Validado, E=Erro, I=Importado
        
        [Column("mensagens_validacao")]
        public string MensagensValidacao { get; set; }  // JSON com lista de erros/avisos
        
        [Column("linha_arquivo")]
        public int LinhaArquivo { get; set; }
        
        // IDs resolvidos após validação
        [Column("operadora_id")]
        public int? OperadoraId { get; set; }
        
        [Column("contrato_id")]
        public int? ContratoId { get; set; }
        
        [Column("plano_id")]
        public int? PlanoId { get; set; }
        
        // Flags de ação
        [Column("criar_operadora")]
        public bool CriarOperadora { get; set; }
        
        [Column("criar_contrato")]
        public bool CriarContrato { get; set; }
        
        [Column("criar_plano")]
        public bool CriarPlano { get; set; }
        
        // Navegação
        public virtual Usuario UsuarioImportacaoNavigation { get; set; }
        public virtual Cliente ClienteNavigation { get; set; }
    }
}

