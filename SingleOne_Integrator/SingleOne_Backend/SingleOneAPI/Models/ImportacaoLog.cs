using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    [Table("importacao_log")]
    public class ImportacaoLog
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("lote_id")]
        public Guid LoteId { get; set; }
        
        [Column("cliente")]
        public int Cliente { get; set; }
        
        [Column("usuario")]
        public int Usuario { get; set; }
        
        [Column("tipo_importacao")]
        public string TipoImportacao { get; set; }  // "LINHAS" ou "COLABORADORES"
        
        [Column("data_inicio")]
        public DateTime DataInicio { get; set; }
        
        [Column("data_fim")]
        public DateTime? DataFim { get; set; }
        
        [Column("status")]
        public string Status { get; set; }  // "PROCESSANDO", "CONCLUIDO", "ERRO"
        
        [Column("total_registros")]
        public int TotalRegistros { get; set; }
        
        [Column("total_validados")]
        public int TotalValidados { get; set; }
        
        [Column("total_erros")]
        public int TotalErros { get; set; }
        
        [Column("total_importados")]
        public int TotalImportados { get; set; }
        
        [Column("nome_arquivo")]
        public string NomeArquivo { get; set; }
        
        [Column("observacoes")]
        public string Observacoes { get; set; }
        
        // Navegação
        public virtual Usuario UsuarioNavigation { get; set; }
        public virtual Cliente ClienteNavigation { get; set; }
    }
}

