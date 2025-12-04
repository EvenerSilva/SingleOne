using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SingleOneAPI.Models
{
    public partial class Notasfiscai
    {
        public Notasfiscai()
        {
            Equipamentos = new HashSet<Equipamento>();
            Notasfiscaisitens = new HashSet<Notasfiscaisiten>();
        }

        public int Id { get; set; }
        public int Cliente { get; set; }
        public int Fornecedor { get; set; }
        public int Numero { get; set; }
        public DateTime Dtemissao { get; set; }
        public string Descricao { get; set; }
        public decimal? Valor { get; set; }
        public bool Virtual { get; set; }
        public bool Gerouequipamento { get; set; }
        public int? Migrateid { get; set; }
        public string ArquivoNotaFiscal { get; set; }
        public string NomeArquivoOriginal { get; set; }
        public DateTime? DataUploadArquivo { get; set; }
        public int? UsuarioUploadArquivo { get; set; }
        public int? UsuarioRemocaoArquivo { get; set; }
        public DateTime? DataRemocaoArquivo { get; set; }

        // Propriedades para controlar a visibilidade dos botões de ação
        [JsonPropertyName("podeVisualizar")]
        public bool PodeVisualizar => true;
        
        [JsonPropertyName("podeAdicionarRecursos")]
        public bool PodeAdicionarRecursos => !Gerouequipamento;
        
        [JsonPropertyName("podeExcluir")]
        public bool PodeExcluir => !Gerouequipamento;

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Fornecedore FornecedorNavigation { get; set; }
        public virtual Usuario UsuarioUploadArquivoNavigation { get; set; }
        public virtual Usuario UsuarioRemocaoArquivoNavigation { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
        public virtual ICollection<Notasfiscaisiten> Notasfiscaisitens { get; set; }

        public decimal CalcularValorNota()
        {
            decimal valor = 0;
            foreach (var item in Notasfiscaisitens)
            {
                valor += (item.Valorunitario * item.Quantidade);
            }
            return valor;
        }
    }
}
