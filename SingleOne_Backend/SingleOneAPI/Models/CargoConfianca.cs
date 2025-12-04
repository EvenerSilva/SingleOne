using System;

namespace SingleOneAPI.Models
{
    public partial class CargoConfianca
    {
        public int Id { get; set; }
        public int Cliente { get; set; }
        public string Cargo { get; set; }
        public bool Usarpadrao { get; set; }  // Se true, usa LIKE '%cargo%'; se false, usa match exato
        public string Nivelcriticidade { get; set; }
        public bool Obrigarsanitizacao { get; set; }
        public bool Obrigardescaracterizacao { get; set; }
        public bool Obrigarperfuracaodisco { get; set; }
        public bool Obrigarevidencias { get; set; }
        public bool Ativo { get; set; }
        public int Usuariocriacao { get; set; }
        public DateTime Datacriacao { get; set; }
        public int? Usuarioalteracao { get; set; }
        public DateTime? Dataalteracao { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Usuario UsuarioCriacaoNavigation { get; set; }
        public virtual Usuario UsuarioAlteracaoNavigation { get; set; }
    }
}

