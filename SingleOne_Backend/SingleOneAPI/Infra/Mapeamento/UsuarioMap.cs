using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class UsuarioMap : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> entity)
        {
            entity.ToTable("usuarios");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Adm).HasColumnName("adm");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Consulta).HasColumnName("consulta");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("email");

            entity.Property(e => e.Migrateid).HasColumnName("migrateid");

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("nome");

            entity.Property(e => e.Operador).HasColumnName("operador");

            entity.Property(e => e.Palavracriptografada)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("palavracriptografada");

            entity.Property(e => e.Senha)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("senha");

            entity.Property(e => e.Su).HasColumnName("su");

            entity.Property(e => e.Ultimologin)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ultimologin");

            // Mapeamento dos campos 2FA
            entity.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            entity.Property(e => e.TwoFactorSecret).HasColumnName("two_factor_secret");
            entity.Property(e => e.TwoFactorBackupCodes).HasColumnName("two_factor_backup_codes");
            entity.Property(e => e.TwoFactorLastUsed).HasColumnName("two_factor_last_used");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkusuariocliente");
        }
    }
}
