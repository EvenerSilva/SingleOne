using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class SinalizacaoSuspeitaMap : IEntityTypeConfiguration<SinalizacaoSuspeita>
    {
        public void Configure(EntityTypeBuilder<SinalizacaoSuspeita> builder)
        {
            builder.ToTable("sinalizacoes_suspeitas");
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.ColaboradorId).HasColumnName("colaborador_id");
            builder.Property(e => e.CpfConsultado).HasColumnName("cpf_consultado");
            builder.Property(e => e.MotivoSuspeita).HasColumnName("motivo_suspeita").HasMaxLength(50);
            builder.Property(e => e.DescricaoDetalhada).HasColumnName("descricao_detalhada");
            builder.Property(e => e.ObservacoesVigilante).HasColumnName("observacoes_vigilante");
            builder.Property(e => e.NomeVigilante).HasColumnName("nome_vigilante").HasMaxLength(100);
            builder.Property(e => e.NumeroProtocolo).HasColumnName("numero_protocolo").HasMaxLength(20);
            builder.Property(e => e.Prioridade).HasColumnName("prioridade").HasMaxLength(20);
            builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            builder.Property(e => e.DadosConsulta)
                .HasColumnName("dados_consulta")
                .HasColumnType("jsonb");
            builder.Property(e => e.IpAddress)
                .HasColumnName("ip_address")
                .HasColumnType("inet");
            builder.Property(e => e.UserAgent).HasColumnName("user_agent");
            builder.Property(e => e.VigilanteId).HasColumnName("vigilante_id");
            builder.Property(e => e.InvestigadorId).HasColumnName("investigador_id");
            builder.Property(e => e.DataSinalizacao).HasColumnName("data_sinalizacao");
            builder.Property(e => e.DataInvestigacao).HasColumnName("data_investigacao");
            builder.Property(e => e.DataResolucao).HasColumnName("data_resolucao");
            builder.Property(e => e.ResultadoInvestigacao).HasColumnName("resultado_investigacao");
            builder.Property(e => e.ObservacoesFinais).HasColumnName("observacoes_finais");
            builder.Property(e => e.CreatedAt).HasColumnName("created_at");
            builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            // Relacionamentos
            builder.HasOne(e => e.Colaborador)
                .WithMany()
                .HasForeignKey(e => e.ColaboradorId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(e => e.Vigilante)
                .WithMany()
                .HasForeignKey(e => e.VigilanteId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(e => e.Investigador)
                .WithMany()
                .HasForeignKey(e => e.InvestigadorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
