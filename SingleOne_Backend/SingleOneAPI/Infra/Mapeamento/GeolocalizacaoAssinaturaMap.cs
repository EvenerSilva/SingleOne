using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class GeolocalizacaoAssinaturaMap : IEntityTypeConfiguration<GeolocalizacaoAssinatura>
    {
        public void Configure(EntityTypeBuilder<GeolocalizacaoAssinatura> builder)
        {
            builder.ToTable("geolocalizacao_assinatura");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ColaboradorId)
                .HasColumnName("colaborador_id")
                .IsRequired();

            builder.Property(e => e.ColaboradorNome)
                .HasColumnName("colaborador_nome")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.UsuarioLogadoId)
                .HasColumnName("usuario_logado_id")
                .IsRequired();

            builder.Property(e => e.IpAddress)
                .HasColumnName("ip_address")
                .HasColumnType("inet")
                .IsRequired();

            builder.Property(e => e.Country)
                .HasColumnName("country")
                .HasMaxLength(100);

            builder.Property(e => e.City)
                .HasColumnName("city")
                .HasMaxLength(100);

            builder.Property(e => e.Region)
                .HasColumnName("region")
                .HasMaxLength(100);

            builder.Property(e => e.Latitude)
                .HasColumnName("latitude")
                .HasColumnType("decimal(10,8)");

            builder.Property(e => e.Longitude)
                .HasColumnName("longitude")
                .HasColumnType("decimal(11,8)");

            builder.Property(e => e.AccuracyMeters)
                .HasColumnName("accuracy_meters")
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.TimestampCaptura)
                .HasColumnName("timestamp_captura")
                .IsRequired();

            builder.Property(e => e.Acao)
                .HasColumnName("acao")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.DataCriacao)
                .HasColumnName("data_criacao")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Índices
            builder.HasIndex(e => e.ColaboradorId)
                .HasDatabaseName("idx_geolocalizacao_colaborador");

            builder.HasIndex(e => e.UsuarioLogadoId)
                .HasDatabaseName("idx_geolocalizacao_usuario");

            builder.HasIndex(e => e.TimestampCaptura)
                .HasDatabaseName("idx_geolocalizacao_timestamp");

            builder.HasIndex(e => e.Acao)
                .HasDatabaseName("idx_geolocalizacao_acao");

            builder.HasIndex(e => e.IpAddress)
                .HasDatabaseName("idx_geolocalizacao_ip");

            // Relacionamentos (opcionais, dependendo da estrutura do seu banco)
            // builder.HasOne(d => d.Colaborador)
            //     .WithMany()
            //     .HasForeignKey(d => d.ColaboradorId)
            //     .HasConstraintName("fk_colaborador");

            // builder.HasOne(d => d.UsuarioLogado)
            //     .WithMany()
            //     .HasForeignKey(d => d.UsuarioLogadoId)
            //     .HasConstraintName("fk_usuario");
        }
    }
}






































