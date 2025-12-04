using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class ImportacaoColaboradorStagingMap : IEntityTypeConfiguration<ImportacaoColaboradorStaging>
    {
        public void Configure(EntityTypeBuilder<ImportacaoColaboradorStaging> builder)
        {
            builder.ToTable("importacao_colaborador_staging");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            
            builder.Property(e => e.LoteId)
                .HasColumnName("lote_id")
                .IsRequired();
            
            builder.Property(e => e.Cliente)
                .HasColumnName("cliente")
                .IsRequired();
            
            builder.Property(e => e.UsuarioImportacao)
                .HasColumnName("usuario_importacao")
                .IsRequired();
            
            builder.Property(e => e.DataImportacao)
                .HasColumnName("data_importacao")
                .IsRequired();
            
            // Dados do colaborador
            builder.Property(e => e.NomeColaborador)
                .HasColumnName("nome_colaborador")
                .HasMaxLength(255);
            
            builder.Property(e => e.Cpf)
                .HasColumnName("cpf")
                .HasMaxLength(14);
            
            builder.Property(e => e.Matricula)
                .HasColumnName("matricula")
                .HasMaxLength(50);
            
            builder.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255);
            
            builder.Property(e => e.Cargo)
                .HasColumnName("cargo")
                .HasMaxLength(100);
            
            builder.Property(e => e.Setor)
                .HasColumnName("setor")
                .HasMaxLength(100);
            
            builder.Property(e => e.DataAdmissao)
                .HasColumnName("data_admissao");
            
            builder.Property(e => e.TipoColaborador)
                .HasColumnName("tipo_colaborador")
                .HasMaxLength(1);
            
            builder.Property(e => e.DataDemissao)
                .HasColumnName("data_demissao");
            
            builder.Property(e => e.MatriculaSuperior)
                .HasColumnName("matricula_superior")
                .HasMaxLength(50);
            
            // Dados relacionados
            builder.Property(e => e.EmpresaNome)
                .HasColumnName("empresa_nome")
                .HasMaxLength(255);
            
            builder.Property(e => e.EmpresaCnpj)
                .HasColumnName("empresa_cnpj")
                .HasMaxLength(18);
            
            builder.Property(e => e.LocalidadeDescricao)
                .HasColumnName("localidade_descricao")
                .HasMaxLength(255);
            
            builder.Property(e => e.LocalidadeCidade)
                .HasColumnName("localidade_cidade")
                .HasMaxLength(100);
            
            builder.Property(e => e.LocalidadeEstado)
                .HasColumnName("localidade_estado")
                .HasMaxLength(2);
            
            builder.Property(e => e.CentroCustoCodigo)
                .HasColumnName("centro_custo_codigo")
                .HasMaxLength(50);
            
            builder.Property(e => e.CentroCustoNome)
                .HasColumnName("centro_custo_nome")
                .HasMaxLength(255);
            
            builder.Property(e => e.FilialNome)
                .HasColumnName("filial_nome")
                .HasMaxLength(255);
            
            builder.Property(e => e.FilialCnpj)
                .HasColumnName("filial_cnpj")
                .HasMaxLength(18);
            
            // Validação
            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(1)
                .IsRequired();
            
            builder.Property(e => e.MensagensValidacao)
                .HasColumnName("mensagens_validacao")
                .HasColumnType("text");
            
            builder.Property(e => e.LinhaArquivo)
                .HasColumnName("linha_arquivo");
            
            // IDs resolvidos
            builder.Property(e => e.EmpresaId)
                .HasColumnName("empresa_id");
            
            builder.Property(e => e.LocalidadeId)
                .HasColumnName("localidade_id");
            
            builder.Property(e => e.CentroCustoId)
                .HasColumnName("centro_custo_id");
            
            builder.Property(e => e.FilialId)
                .HasColumnName("filial_id");
            
            // Flags
            builder.Property(e => e.CriarEmpresa)
                .HasColumnName("criar_empresa")
                .HasDefaultValue(false);
            
            builder.Property(e => e.CriarLocalidade)
                .HasColumnName("criar_localidade")
                .HasDefaultValue(false);
            
            builder.Property(e => e.CriarCentroCusto)
                .HasColumnName("criar_centro_custo")
                .HasDefaultValue(false);
            
            builder.Property(e => e.CriarFilial)
                .HasColumnName("criar_filial")
                .HasDefaultValue(false);
            
            // Índices
            builder.HasIndex(e => e.LoteId)
                .HasDatabaseName("idx_colaborador_staging_lote");
            
            builder.HasIndex(e => e.Status)
                .HasDatabaseName("idx_colaborador_staging_status");
            
            // Relacionamentos (sem FK real, apenas navegação)
            builder.HasOne(e => e.UsuarioImportacaoNavigation)
                .WithMany()
                .HasForeignKey(e => e.UsuarioImportacao)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_colaborador_staging_usuario");
            
            builder.HasOne(e => e.ClienteNavigation)
                .WithMany()
                .HasForeignKey(e => e.Cliente)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_colaborador_staging_cliente");
        }
    }
}

