using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    [Table("laudoevidencias")]
    public partial class LaudoEvidencia
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("laudo")]
        public int laudo { get; set; }

        [Required]
        [Column("nomearquivo")]
        [StringLength(255)]
        public string nomearquivo { get; set; } = string.Empty;

        [Column("ordem")]
        public int ordem { get; set; }
    }
}
