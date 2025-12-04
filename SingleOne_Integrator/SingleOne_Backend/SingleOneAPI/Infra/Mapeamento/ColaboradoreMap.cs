using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class ColaboradoreMap : IEntityTypeConfiguration<Colaboradore>
    {
        public void Configure(EntityTypeBuilder<Colaboradore> entity)
        {
            entity.ToTable("colaboradores");

            entity.HasIndex(e => e.Cpf, "colaboradores_cpf_key")
                .IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Antigaempresa).HasColumnName("antigaempresa");

            entity.Property(e => e.Antigalocalidade).HasColumnName("antigalocalidade");

            entity.Property(e => e.Antigocentrocusto).HasColumnName("antigocentrocusto");

            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .HasColumnName("cargo");

            entity.Property(e => e.Centrocusto).HasColumnName("centrocusto");

            // Cliente agora é opcional (herda da empresa automaticamente)
            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Cpf)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("cpf");

            entity.Property(e => e.Dtadmissao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtadmissao");

            entity.Property(e => e.Dtatualizacao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtatualizacao");

            entity.Property(e => e.Dtatualizacaocentrocusto)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtatualizacaocentrocusto");

            entity.Property(e => e.Dtatualizacaoempresa)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtatualizacaoempresa");

            entity.Property(e => e.Dtatualizacaolocalidade)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtatualizacaolocalidade");

            entity.Property(e => e.Dtcadastro)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtcadastro");

            entity.Property(e => e.Dtdemissao)
               .HasColumnType("timestamp without time zone")
               .HasColumnName("dtdemissao");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnName("email");

            entity.Property(e => e.Empresa).HasColumnName("empresa");

            entity.Property(e => e.Localidade).HasColumnName("localidade");

            entity.Property(e => e.Matricula)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("matricula");

            entity.Property(e => e.Matriculasuperior)
                .HasMaxLength(50)
                .HasColumnName("matriculasuperior");

            entity.Property(e => e.Migrateid).HasColumnName("migrateid");

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnName("nome");

            entity.Property(e => e.Setor)
                .HasMaxLength(100)
                .HasColumnName("setor");

            entity.Property(e => e.Situacao)
                .HasMaxLength(1)
                .HasColumnName("situacao");

            entity.Property(e => e.Situacaoantiga)
                .HasMaxLength(1)
                .HasColumnName("situacaoantiga");

            entity.Property(e => e.Tipocolaborador)
                .IsRequired()
                .HasMaxLength(1)
                .HasColumnName("tipocolaborador");

            entity.Property(e => e.Usuario).HasColumnName("usuario");

            // Novos campos opcionais
            entity.Property(e => e.FilialId).HasColumnName("filial_id");
            entity.Property(e => e.LocalidadeId).HasColumnName("localidade_id");

            // Relacionamentos obrigatórios
            entity.HasOne(d => d.CentrocustoNavigation)
                .WithMany(p => p.Colaboradores)
                .HasForeignKey(d => d.Centrocusto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkcolaboradorcentrocusto");

            // Cliente agora é opcional
            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Colaboradores)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.SetNull) // Mudou para SetNull
                .HasConstraintName("fkcolaboradorcliente");

            entity.HasOne(d => d.EmpresaNavigation)
                .WithMany(p => p.Colaboradores)
                .HasForeignKey(d => d.Empresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkcolaboradorempresa");

            entity.HasOne(d => d.UsuarioNavigation)
                .WithMany(p => p.Colaboradores)
                .HasForeignKey(d => d.Usuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkcolaboradorusuario");

            // Novos relacionamentos opcionais
            entity.HasOne(d => d.Filial)
                .WithMany(p => p.Colaboradores)
                .HasForeignKey(d => d.FilialId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_colaboradores_filial");

            // Relacionamento com localidade (obrigatório) - usando campo Localidade
            entity.HasOne(d => d.LocalidadeNavigation)
                .WithMany(p => p.Colaboradores)
                .HasForeignKey(d => d.Localidade)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkcolaboradorlocalidade")
                .IsRequired(true); // Obrigatório conforme estrutura da tabela
        }
    }
}
