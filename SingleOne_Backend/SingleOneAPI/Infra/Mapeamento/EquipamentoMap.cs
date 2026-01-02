using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;
using System;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class EquipamentoMap : IEntityTypeConfiguration<Equipamento>
    {
        public void Configure(EntityTypeBuilder<Equipamento> builder)
        {
            // Mapear para a tabela correta no banco (equipamentos - plural)
            builder.ToTable("equipamentos");
            
            // Log para debug
            Console.WriteLine("[EQUIPAMENTO-MAP] Configurando mapeamento para tabela 'equipamentos'");

            // Configurar chave primária
            builder.HasKey(e => e.Id);
            
            // Configurar convenção de nomenclatura para evitar ClienteId
            builder.Property(e => e.Cliente)
                .HasColumnName("cliente");
            
            // Configurar propriedades de navegação
            builder.HasOne(e => e.CentrocustoNavigation)
                .WithMany()
                .HasForeignKey(e => e.Centrocusto)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ClienteNavigation)
                .WithMany()
                .HasForeignKey(e => e.Cliente)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.EmpresaNavigation)
                .WithMany()
                .HasForeignKey(e => e.Empresa)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.EquipamentostatusNavigation)
                .WithMany()
                .HasForeignKey(e => e.Equipamentostatus)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.FabricanteNavigation)
                .WithMany()
                .HasForeignKey(e => e.Fabricante)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ModeloNavigation)
                .WithMany()
                .HasForeignKey(e => e.Modelo)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.NotafiscalNavigation)
                .WithMany()
                .HasForeignKey(e => e.Notafiscal)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.TipoequipamentoNavigation)
                .WithMany()
                .HasForeignKey(e => e.Tipoequipamento)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.TipoaquisicaoNavigation)
                .WithMany()
                .HasForeignKey(e => e.Tipoaquisicao)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.UsuarioNavigation)
                .WithMany()
                .HasForeignKey(e => e.Usuario)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ContratoNavigation)
                .WithMany()
                .HasForeignKey(e => e.Contrato)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Filial)
                .WithMany()
                .HasForeignKey(e => e.FilialId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar propriedades obrigatórias
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(e => e.Tipoequipamento)
                .HasColumnName("tipoequipamento")
                .IsRequired();

            builder.Property(e => e.Fabricante)
                .HasColumnName("fabricante")
                .IsRequired();

            builder.Property(e => e.Modelo)
                .HasColumnName("modelo")
                .IsRequired();

            builder.Property(e => e.Tipoaquisicao)
                .HasColumnName("tipoaquisicao")
                .IsRequired();

            builder.Property(e => e.Possuibo)
                .HasColumnName("possuibo")
                .IsRequired();

            builder.Property(e => e.Numeroserie)
                .HasColumnName("numeroserie")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.Dtcadastro)
                .HasColumnName("dtcadastro")
                .IsRequired();

            builder.Property(e => e.Ativo)
                .HasColumnName("ativo")
                .IsRequired();


            // Configurar propriedades opcionais
            builder.Property(e => e.Notafiscal)
                .HasColumnName("notafiscal");

            builder.Property(e => e.Usuario)
                .HasColumnName("usuario");

            builder.Property(e => e.Fornecedor)
                .HasColumnName("fornecedor");

            builder.Property(e => e.Descricaobo)
                .HasColumnName("descricaobo");

            builder.Property(e => e.Patrimonio)
                .HasColumnName("patrimonio")
                .HasMaxLength(255);

            builder.Property(e => e.Dtlimitegarantia)
                .HasColumnName("dtlimitegarantia");

            builder.Property(e => e.Migrateid)
                .HasColumnName("migrateid");

            builder.Property(e => e.Enviouemailreporte)
                .HasColumnName("enviouemailreporte");

            builder.Property(e => e.Centrocusto)
                .HasColumnName("centrocusto");

            builder.Property(e => e.Empresa)
                .HasColumnName("empresa");

            builder.Property(e => e.FilialId)
                .HasColumnName("filial_id");

            builder.Property(e => e.Equipamentostatus)
                .HasColumnName("equipamentostatus");

            builder.Property(e => e.Contrato)
                .HasColumnName("contrato");

            // Configurar campos de localização
            builder.Property(e => e.Localidade)
                .HasColumnName("localidade_id");

            // ✅ REMOVIDO: Coluna localizacao não existe mais no banco, foi substituída por localidade_id
            // builder.Property(e => e.Localizacao)
            //     .HasColumnName("localizacao");
            
            // Ignorar a propriedade Localizacao para evitar que o EF tente mapeá-la
            builder.Ignore(e => e.Localizacao);

            // Configurar propriedades de navegação para localização (evita shadow property LocalidadeNavigationId)
            builder.HasOne(e => e.LocalidadeNavigation)
                .WithMany()
                .HasForeignKey(e => e.Localidade)
                .HasPrincipalKey(l => l.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar índices
            builder.HasIndex(e => e.Numeroserie)
                .IsUnique()
                .HasDatabaseName("IX_equipamentos_numeroserie");

            builder.HasIndex(e => e.Cliente)
                .HasDatabaseName("IX_equipamentos_cliente");

            builder.HasIndex(e => e.Equipamentostatus)
                .HasDatabaseName("IX_equipamentos_equipamentostatus");

            builder.HasIndex(e => e.Ativo)
                .HasDatabaseName("IX_equipamentos_ativo");
        }
    }
}
