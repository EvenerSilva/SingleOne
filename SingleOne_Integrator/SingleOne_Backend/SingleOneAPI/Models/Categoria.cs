using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    [Table("categorias")]
    public class Categoria
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("nome")]
        [StringLength(100)]
        public string Nome { get; set; }

        [Column("descricao")]
        public string Descricao { get; set; }

        [Column("ativo")]
        public bool Ativo { get; set; } = true;

        [Column("data_criacao")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Column("data_atualizacao")]
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;

        // Propriedades de navegação
        public virtual ICollection<Tipoequipamento> TiposEquipamento { get; set; }
    }
}
