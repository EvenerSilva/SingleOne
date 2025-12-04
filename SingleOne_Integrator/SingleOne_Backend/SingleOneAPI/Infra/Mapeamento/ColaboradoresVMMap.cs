using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class ColaboradoresVMMap : IEntityTypeConfiguration<ColaboradoresVM>
    {
        public void Configure(EntityTypeBuilder<ColaboradoresVM> builder)
        {
            builder.HasNoKey();
            builder.ToView("colaboradoresvm");
            builder.Property(c => c.Id).HasColumnName("id");
            builder.Property(c => c.Cliente).HasColumnName("cliente");
            builder.Property(c => c.Empresa).HasColumnName("empresa");
            builder.Property(c => c.NomeCentroCusto).HasColumnName("nomecentrocusto");
            builder.Property(c => c.CodigoCentroCusto).HasColumnName("codigocentrocusto");
            builder.Property(c => c.Nome).HasColumnName("nome");
            builder.Property(c => c.Cpf).HasColumnName("cpf");
            builder.Property(c => c.Matricula).HasColumnName("matricula");
            builder.Property(c => c.Email).HasColumnName("email");
            builder.Property(c => c.TipoColaborador).HasColumnName("tipocolaborador");
            builder.Property(c => c.Situacao).HasColumnName("situacao");
            builder.Property(c => c.Dtdemissao).HasColumnName("dtdemissao").HasColumnType("timestamp without time zone");
        }
    }
}
