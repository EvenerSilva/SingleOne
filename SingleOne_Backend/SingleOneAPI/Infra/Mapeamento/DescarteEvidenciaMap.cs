using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class DescarteEvidenciaMap : IEntityTypeConfiguration<DescarteEvidencia>
    {
        public void Configure(EntityTypeBuilder<DescarteEvidencia> builder)
        {
            builder.ToTable("descarteevidencias");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Equipamento)
                .HasColumnName("equipamento")
                .IsRequired();

            builder.Property(e => e.Descricao)
                .HasColumnName("descricao")
                .HasMaxLength(500);

            builder.Property(e => e.Tipoprocesso)
                .HasColumnName("tipoprocesso")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Nomearquivo)
                .HasColumnName("nomearquivo")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Caminhoarquivo)
                .HasColumnName("caminhoarquivo")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.Tipoarquivo)
                .HasColumnName("tipoarquivo")
                .HasMaxLength(100);

            builder.Property(e => e.Tamanhoarquivo)
                .HasColumnName("tamanhoarquivo");

            builder.Property(e => e.Usuarioupload)
                .HasColumnName("usuarioupload")
                .IsRequired();

            builder.Property(e => e.Dataupload)
                .HasColumnName("dataupload")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.Ativo)
                .HasColumnName("ativo")
                .HasDefaultValue(true);

            builder.Property(e => e.ProtocoloId)
                .HasColumnName("protocolo_id");

            // Relacionamentos
            builder.HasOne(e => e.EquipamentoNavigation)
                .WithMany()
                .HasForeignKey(e => e.Equipamento)
                .HasConstraintName("fk_descarteevidencias_equipamento");

            builder.HasOne(e => e.UsuarioUploadNavigation)
                .WithMany()
                .HasForeignKey(e => e.Usuarioupload)
                .HasConstraintName("fk_descarteevidencias_usuario");

            builder.HasOne(e => e.ProtocoloNavigation)
                .WithMany(p => p.Evidencias)
                .HasForeignKey(e => e.ProtocoloId)
                .HasConstraintName("fk_descarteevidencias_protocolo")
                .IsRequired(false);
        }
    }
}

