using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SingleOne.Models;

namespace SingleOneAPI.Models
{
    [Table("geolocalizacao_assinatura")]
    public class GeolocalizacaoAssinatura
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("colaborador_id")]
        public int ColaboradorId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("colaborador_nome")]
        public string ColaboradorNome { get; set; }

        [Required]
        [Column("usuario_logado_id")]
        public int UsuarioLogadoId { get; set; }

        [Required]
        [StringLength(45)]
        [Column("ip_address")]
        public string IpAddress { get; set; }

        [StringLength(100)]
        [Column("country")]
        public string Country { get; set; }

        [StringLength(100)]
        [Column("city")]
        public string City { get; set; }

        [StringLength(100)]
        [Column("region")]
        public string Region { get; set; }

        [Column("latitude", TypeName = "decimal(10,8)")]
        public decimal? Latitude { get; set; }

        [Column("longitude", TypeName = "decimal(11,8)")]
        public decimal? Longitude { get; set; }

        [Column("accuracy_meters", TypeName = "decimal(10,2)")]
        public decimal? AccuracyMeters { get; set; }

        [Required]
        [Column("timestamp_captura")]
        public DateTime TimestampCaptura { get; set; }

        [Required]
        [StringLength(50)]
        [Column("acao")]
        public string Acao { get; set; }

        [Column("data_criacao")]
        public DateTime DataCriacao { get; set; }

        // Navegação para relacionamentos (opcional)
        [ForeignKey("ColaboradorId")]
        public virtual Colaboradore Colaborador { get; set; }

        [ForeignKey("UsuarioLogadoId")]
        public virtual Usuario UsuarioLogado { get; set; }
    }
}

