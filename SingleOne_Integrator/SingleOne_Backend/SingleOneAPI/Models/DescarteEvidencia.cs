using System;

namespace SingleOneAPI.Models
{
    public partial class DescarteEvidencia
    {
        public int Id { get; set; }
        public int Equipamento { get; set; }
        public string Descricao { get; set; }
        public string Tipoprocesso { get; set; }
        public string Nomearquivo { get; set; }
        public string Caminhoarquivo { get; set; }
        public string Tipoarquivo { get; set; }
        public long? Tamanhoarquivo { get; set; }
        public int Usuarioupload { get; set; }
        public DateTime Dataupload { get; set; }
        public bool Ativo { get; set; }
        public int? ProtocoloId { get; set; }

        public virtual Equipamento EquipamentoNavigation { get; set; }
        public virtual Usuario UsuarioUploadNavigation { get; set; }
        public virtual ProtocoloDescarte ProtocoloNavigation { get; set; }
    }
}

