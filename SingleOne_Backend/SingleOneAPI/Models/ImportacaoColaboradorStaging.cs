using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    [Table("importacao_colaborador_staging")]
    public class ImportacaoColaboradorStaging
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("lote_id")]
        public Guid LoteId { get; set; }
        
        [Column("cliente")]
        public int Cliente { get; set; }
        
        [Column("usuario_importacao")]
        public int UsuarioImportacao { get; set; }
        
        [Column("data_importacao")]
        public DateTime DataImportacao { get; set; }
        
        // ========== Dados do colaborador vindos do arquivo ==========
        [Column("nome_colaborador")]
        public string NomeColaborador { get; set; }
        
        [Column("cpf")]
        public string Cpf { get; set; }
        
        [Column("matricula")]
        public string Matricula { get; set; }
        
        [Column("email")]
        public string Email { get; set; }
        
        [Column("cargo")]
        public string Cargo { get; set; }
        
        [Column("setor")]
        public string Setor { get; set; }
        
        [Column("data_admissao")]
        public DateTime? DataAdmissao { get; set; }
        
        [Column("tipo_colaborador")]
        public string TipoColaborador { get; set; }  // F, T ou C
        
        [Column("data_demissao")]
        public DateTime? DataDemissao { get; set; }  // Opcional
        
        [Column("matricula_superior")]
        public string MatriculaSuperior { get; set; }  // Opcional
        
        // ========== Dados relacionados (do arquivo) ==========
        [Column("empresa_nome")]
        public string EmpresaNome { get; set; }
        
        [Column("empresa_cnpj")]
        public string EmpresaCnpj { get; set; }
        
        [Column("localidade_descricao")]
        public string LocalidadeDescricao { get; set; }
        
        [Column("localidade_cidade")]
        public string LocalidadeCidade { get; set; }
        
        [Column("localidade_estado")]
        public string LocalidadeEstado { get; set; }
        
        [Column("centro_custo_codigo")]
        public string CentroCustoCodigo { get; set; }
        
        [Column("centro_custo_nome")]
        public string CentroCustoNome { get; set; }
        
        [Column("filial_nome")]
        public string FilialNome { get; set; }  // Opcional
        
        [Column("filial_cnpj")]
        public string FilialCnpj { get; set; }  // Opcional
        
        // ========== Validação e Status ==========
        [Column("status")]
        public string Status { get; set; }  // P=Pendente, V=Validado, E=Erro, I=Importado
        
        [Column("mensagens_validacao")]
        public string MensagensValidacao { get; set; }  // JSON com erros/avisos
        
        [Column("linha_arquivo")]
        public int LinhaArquivo { get; set; }
        
        // ========== IDs resolvidos após validação ==========
        [Column("empresa_id")]
        public int? EmpresaId { get; set; }
        
        [Column("localidade_id")]
        public int? LocalidadeId { get; set; }
        
        [Column("centro_custo_id")]
        public int? CentroCustoId { get; set; }
        
        [Column("filial_id")]
        public int? FilialId { get; set; }
        
        // ========== Flags de ação ==========
        [Column("criar_empresa")]
        public bool CriarEmpresa { get; set; }
        
        [Column("criar_localidade")]
        public bool CriarLocalidade { get; set; }
        
        [Column("criar_centro_custo")]
        public bool CriarCentroCusto { get; set; }
        
        [Column("criar_filial")]
        public bool CriarFilial { get; set; }
        
        // ========== Navegação ==========
        public virtual Usuario UsuarioImportacaoNavigation { get; set; }
        public virtual Cliente ClienteNavigation { get; set; }
    }
}

