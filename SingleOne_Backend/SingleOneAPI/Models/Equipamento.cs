using SingleOneAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    public partial class Equipamento
    {
        public Equipamento()
        {
            Equipamentoanexos = new HashSet<Equipamentoanexo>();
            Equipamentohistoricos = new HashSet<Equipamentohistorico>();
            Laudos = new HashSet<Laudo>();
            Requisicoesitens = new HashSet<Requisicoesiten>();
        }

        public int Id { get; set; }
        
        // Campo cliente agora é opcional (herda da empresa automaticamente)
        [Column("cliente")]
        public int? Cliente { get; set; }
        
        public int Tipoequipamento { get; set; }
        public int Fabricante { get; set; }
        public int Modelo { get; set; }
        public int? Notafiscal { get; set; }
        public int? Equipamentostatus { get; set; }
        public int? Usuario { get; set; }
        public int Tipoaquisicao { get; set; }
        public int? Fornecedor { get; set; }
        public bool Possuibo { get; set; }
        public string Descricaobo { get; set; }
        public string Numeroserie { get; set; }
        public string Patrimonio { get; set; }
        public DateTime? Dtlimitegarantia { get; set; }
        public DateTime Dtcadastro { get; set; }
        public bool Ativo { get; set; }
        public int? Migrateid { get; set; }
        public bool? Enviouemailreporte { get; set; }
        
        // Campos opcionais para flexibilidade
        public int? Empresa { get; set; }
        public int? Centrocusto { get; set; }
        public int? Contrato { get; set; }
        
        
        [Column("filial_id")]
        public int? FilialId { get; set; }
        
        [Column("localidade_id")]
        public int? Localidade { get; set; }
        
        // Campo para compatibilidade com dados antigos
        [Column("localizacao")]
        public int? Localizacao { get; set; }

        // Propriedades de navegação
        public virtual Centrocusto? CentrocustoNavigation { get; set; }
        public virtual Cliente? ClienteNavigation { get; set; }
        public virtual Empresa? EmpresaNavigation { get; set; }
        public virtual Equipamentosstatus? EquipamentostatusNavigation { get; set; }
        public virtual Fabricante? FabricanteNavigation { get; set; }
        public virtual Modelo? ModeloNavigation { get; set; }
        public virtual Notasfiscai? NotafiscalNavigation { get; set; }
        public virtual Tipoequipamento? TipoequipamentoNavigation { get; set; }
        public virtual Tipoaquisicao? TipoaquisicaoNavigation { get; set; }
        public virtual Usuario? UsuarioNavigation { get; set; }
        public virtual Contrato? ContratoNavigation { get; set; }
        public virtual Filial? Filial { get; set; }
        public virtual Localidade? LocalidadeNavigation { get; set; }

        public virtual ICollection<Equipamentoanexo> Equipamentoanexos { get; set; }
        public virtual ICollection<Equipamentohistorico> Equipamentohistoricos { get; set; }
        public virtual ICollection<Laudo> Laudos { get; set; }
        public virtual ICollection<Requisicoesiten> Requisicoesitens { get; set; }
    }
}
