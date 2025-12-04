using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class CargoConfiancaMap : IEntityTypeConfiguration<CargoConfianca>
    {
        public void Configure(EntityTypeBuilder<CargoConfianca> builder)
        {
            builder.ToTable("cargosconfianca");
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            
            builder.Property(e => e.Cliente)
                .HasColumnName("cliente")
                .IsRequired();

            builder.Property(e => e.Cargo)
                .HasColumnName("cargo")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Usarpadrao)
                .HasColumnName("usarpadrao")
                .IsRequired();

            builder.Property(e => e.Nivelcriticidade)
                .HasColumnName("nivelcriticidade")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.Obrigarsanitizacao)
                .HasColumnName("obrigarsanitizacao")
                .IsRequired();

            builder.Property(e => e.Obrigardescaracterizacao)
                .HasColumnName("obrigardescaracterizacao")
                .IsRequired();

            builder.Property(e => e.Obrigarperfuracaodisco)
                .HasColumnName("obrigarperfuracaodisco")
                .IsRequired();

            builder.Property(e => e.Obrigarevidencias)
                .HasColumnName("obrigarevidencias")
                .IsRequired();

            builder.Property(e => e.Ativo)
                .HasColumnName("ativo")
                .HasDefaultValue(true);

            builder.Property(e => e.Usuariocriacao)
                .HasColumnName("usuariocriacao")
                .IsRequired();

            builder.Property(e => e.Datacriacao)
                .HasColumnName("datacriacao")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.Usuarioalteracao)
                .HasColumnName("usuarioalteracao");

            builder.Property(e => e.Dataalteracao)
                .HasColumnName("dataalteracao");

            // Relacionamentos
            builder.HasOne(e => e.ClienteNavigation)
                .WithMany(c => c.CargosConfianca)
                .HasForeignKey(e => e.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cargoconfianca_cliente");

            builder.HasOne(e => e.UsuarioCriacaoNavigation)
                .WithMany()
                .HasForeignKey(e => e.Usuariocriacao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cargoconfianca_usuario_criacao");

            builder.HasOne(e => e.UsuarioAlteracaoNavigation)
                .WithMany()
                .HasForeignKey(e => e.Usuarioalteracao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cargoconfianca_usuario_alteracao");
        }
    }
}

