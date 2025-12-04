using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.ViewModels
{
    public class NotaFiscalListagemVM
    {
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
        
        // Campos de arquivo
        public string ArquivoNotaFiscal { get; set; }
        public string NomeArquivoOriginal { get; set; }
        public DateTime? DataUploadArquivo { get; set; }
        public bool TemArquivo => !string.IsNullOrEmpty(ArquivoNotaFiscal);

        // Propriedades para controlar a visibilidade dos botões de ação
        public bool PodeVisualizar { get; set; }
        public bool PodeAdicionarRecursos { get; set; }
        public bool PodeExcluir { get; set; }

        // Propriedades de navegação
        public string FornecedorNome { get; set; }
        public string FornecedorCnpj { get; set; }
        public int QuantidadeItens { get; set; }
        
        // Objeto completo do fornecedor para uso no frontend
        public FornecedorNavigationVM FornecedorNavigation { get; set; }
    }
}
