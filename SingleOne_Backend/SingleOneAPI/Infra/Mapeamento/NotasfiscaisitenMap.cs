using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class NotasfiscaisitenMap : IEntityTypeConfiguration<Notasfiscaisiten>
    {
        public void Configure(EntityTypeBuilder<Notasfiscaisiten> entity)
        {
            entity.ToTable("notasfiscaisitens");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Fabricante).HasColumnName("fabricante");

            entity.Property(e => e.Modelo).HasColumnName("modelo");

            entity.Property(e => e.Notafiscal).HasColumnName("notafiscal");

            entity.Property(e => e.Quantidade).HasColumnName("quantidade");

            entity.Property(e => e.Tipoequipamento).HasColumnName("tipoequipamento");

            entity.Property(e => e.TipoAquisicao).HasColumnName("tipoaquisicao");

            entity.Property(e => e.Dtlimitegarantia).HasColumnName("dtlimitegarantia");
            
            entity.Property(e => e.Contrato).HasColumnName("contrato");
                                                                    
            entity.Property(e => e.Valorunitario)
                .HasPrecision(10, 2)
                .HasColumnName("valorunitario");


            entity.HasOne(d => d.FabricanteNavigation)
                .WithMany(p => p.Notasfiscaisitens)
                .HasForeignKey(d => d.Fabricante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fknfifabricante");

            entity.HasOne(d => d.ModeloNavigation)
                .WithMany(p => p.Notasfiscaisitens)
                .HasForeignKey(d => d.Modelo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fknfimodelo");

            entity.HasOne(d => d.NotafiscalNavigation)
                .WithMany(p => p.Notasfiscaisitens)
                .HasForeignKey(d => d.Notafiscal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fknfinotafiscal");

            entity.HasOne(d => d.TipoequipamentoNavigation)
                .WithMany(p => p.Notasfiscaisitens)
                .HasForeignKey(d => d.Tipoequipamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fknfitipoeqp");

            entity.HasOne(d => d.ContratoNavigation)
                .WithMany(p => p.Notasfiscaisitens)
                .HasForeignKey(d => d.Contrato)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fknfcontrato");
        }
    }
}
