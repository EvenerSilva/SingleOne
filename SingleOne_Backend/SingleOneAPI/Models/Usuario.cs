using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    public partial class Usuario
    {
        public Usuario()
        {
            Colaboradores = new HashSet<Colaboradore>();
            Equipamentoanexos = new HashSet<Equipamentoanexo>();
            Equipamentohistoricos = new HashSet<Equipamentohistorico>();
            Equipamentos = new HashSet<Equipamento>();
            LaudoTecnicoNavigations = new HashSet<Laudo>();
            LaudoUsuarioNavigations = new HashSet<Laudo>();
            RequisicoTecnicoresponsavelNavigations = new HashSet<Requisico>();
            RequisicoUsuariorequisicaoNavigations = new HashSet<Requisico>();
        }

        public int Id { get; set; }
        public int Cliente { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string Palavracriptografada { get; set; }
        public bool Su { get; set; }
        public bool Adm { get; set; }
        public bool Operador { get; set; }
        public bool Consulta { get; set; }
        public bool Ativo { get; set; }
        public int? Migrateid { get; set; }
        public DateTime? Ultimologin { get; set; }

        // Campos para 2FA
        [Column("two_factor_enabled")]
        public bool? TwoFactorEnabled { get; set; }
        [Column("two_factor_secret")]
        public string TwoFactorSecret { get; set; }
        [Column("two_factor_backup_codes")]
        public string TwoFactorBackupCodes { get; set; }
        [Column("two_factor_last_used")]
        public DateTime? TwoFactorLastUsed { get; set; }
        
        

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual ICollection<Colaboradore> Colaboradores { get; set; }
        public virtual ICollection<Equipamentoanexo> Equipamentoanexos { get; set; }
        public virtual ICollection<Equipamentohistorico> Equipamentohistoricos { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
        public virtual ICollection<Laudo> LaudoTecnicoNavigations { get; set; }
        public virtual ICollection<Laudo> LaudoUsuarioNavigations { get; set; }
        public virtual ICollection<Requisico> RequisicoTecnicoresponsavelNavigations { get; set; }
        public virtual ICollection<Requisico> RequisicoUsuariorequisicaoNavigations { get; set; }
    }
}
