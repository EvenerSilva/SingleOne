using Microsoft.EntityFrameworkCore;
using SingleOne.Models;
using SingleOneAPI.Models;
using System;
using System.Linq;

namespace SingleOneAPI.Infra.Contexto
{
    public class SingleOneDbContext : DbContext
    {
        public SingleOneDbContext(DbContextOptions<SingleOneDbContext> options) 
            : base(options) 
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        #region DbSets
        public virtual DbSet<Centrocusto> Centrocustos { get; set; }
        public virtual DbSet<Cliente> Clientes { get; set; }
        public virtual DbSet<Colaboradore> Colaboradores { get; set; }
        public virtual DbSet<Colaboradorhistoricovm> Colaboradorhistoricovms { get; set; }
        public virtual DbSet<Descartecargo> Descartecargos { get; set; }
        public virtual DbSet<DescarteEvidencia> DescarteEvidencias { get; set; }
        public virtual DbSet<ProtocoloDescarte> ProtocolosDescarte { get; set; }
        public virtual DbSet<ProtocoloDescarteItem> ProtocoloDescarteItens { get; set; }
        public virtual DbSet<CargoConfianca> CargosConfianca { get; set; }
        public virtual DbSet<Empresa> Empresas { get; set; }
        public virtual DbSet<Filial> Filiais { get; set; }
        public virtual DbSet<Equipamento> Equipamentos { get; set; }
        public virtual DbSet<Equipamentoanexo> Equipamentoanexos { get; set; }
        public virtual DbSet<Equipamentohistorico> Equipamentohistoricos { get; set; }
        public virtual DbSet<Equipamentohistoricovm> Equipamentohistoricovms { get; set; }
        public virtual DbSet<Equipamentosstatus> Equipamentosstatuses { get; set; }
        public virtual DbSet<Equipamentovm> Equipamentovms { get; set; }
        public virtual DbSet<Fabricante> Fabricantes { get; set; }
        public virtual DbSet<Fornecedore> Fornecedores { get; set; }
        public virtual DbSet<Laudo> Laudos { get; set; }
        public virtual DbSet<LaudoEvidencia> LaudoEvidencias { get; set; }
        public virtual DbSet<Localidade> Localidades { get; set; }
        public virtual DbSet<Modelo> Modelos { get; set; }
        public virtual DbSet<Notasfiscai> Notasfiscais { get; set; }
        public virtual DbSet<Notasfiscaisiten> Notasfiscaisitens { get; set; }
        public virtual DbSet<Parametro> Parametros { get; set; }
        public virtual DbSet<Requisicaoequipamentosvm> Requisicaoequipamentosvms { get; set; }
        public virtual DbSet<Requisico> Requisicoes { get; set; }
        public virtual DbSet<Requisicoesiten> Requisicoesitens { get; set; }
        public virtual DbSet<Requisicoesstatus> Requisicoesstatuses { get; set; }
        public virtual DbSet<Requisicoesvm> Requisicoesvms { get; set; }
        public virtual DbSet<Telefoniacontrato> Telefoniacontratos { get; set; }
        public virtual DbSet<Telefonialinha> Telefonialinhas { get; set; }
        public virtual DbSet<Telefoniaoperadora> Telefoniaoperadoras { get; set; }
        public virtual DbSet<Telefoniaplano> Telefoniaplanos { get; set; }
        public virtual DbSet<Template> Templates { get; set; }
        public virtual DbSet<Templatetipo> Templatetipos { get; set; }
        public virtual DbSet<Termoentregavm> Termoentregavms { get; set; }
        public virtual DbSet<Termoscolaboradoresvm> Termoscolaboradoresvms { get; set; }
        public virtual DbSet<Tipoequipamento> Tipoequipamento { get; set; }
        public virtual DbSet<Tipoaquisicao> Tipoaquisicaos { get; set; }
        public virtual DbSet<Tipoequipamentoscliente> Tipoequipamentosclientes { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<Vwcargo> Vwcargos { get; set; }
        public virtual DbSet<Vwdevolucaoprogramadum> Vwdevolucaoprogramada { get; set; }
        public virtual DbSet<Vwequipamentoscomcolaboradoresdesligado> Vwequipamentoscomcolaboradoresdesligados { get; set; }
        public virtual DbSet<Vwequipamentosdetalhe> Vwequipamentosdetalhes { get; set; }
        public virtual DbSet<Vwequipamentosstatus> Vwequipamentosstatuses { get; set; }
        public virtual DbSet<Vwexportacaoexcel> Vwexportacaoexcels { get; set; }
        public virtual DbSet<Vwlaudo> Vwlaudos { get; set; }
        public virtual DbSet<Vwnadaconstum> Vwnadaconsta { get; set; }
        public virtual DbSet<Vwtelefonium> Vwtelefonia { get; set; }
        public virtual DbSet<RegrasTemplate> RegrasTemplate { get; set; }
        public virtual DbSet<VwUltimasRequisicaoBYOD> VwUltimasRequisicoesBYOD { get; set; }
        public virtual DbSet<VwUltimasRequisicaoNaoBYOD> VwUltimasRequisicoesNaoBYOD { get; set; }
        public virtual DbSet<ColaboradoresVM> ColaboradoresVMs { get; set; }
        public virtual DbSet<CentrocustoVM> CentrocustosVMs { get; set; }
        public virtual DbSet<PlanosVM> PlanosVMs { get; set; }
        public virtual DbSet<GeolocalizacaoAssinatura> GeolocalizacaoAssinaturas { get; set; }
        public virtual DbSet<Categoria> Categorias { get; set; }
        public virtual DbSet<EstoqueMinimoEquipamento> EstoqueMinimoEquipamentos { get; set; }
        public virtual DbSet<EstoqueMinimoLinha> EstoqueMinimoLinhas { get; set; }
        public virtual DbSet<PatrimonioContestacao> PatrimonioContestoes { get; set; }
        public virtual DbSet<PatrimonioLogAcesso> PatrimonioLogsAcesso { get; set; }
        public virtual DbSet<RequisicaoItemCompartilhado> RequisicoesItensCompartilhados { get; set; }
        
        // Tabelas de Sinalização de Suspeitas
        public virtual DbSet<SinalizacaoSuspeita> SinalizacoesSuspeitas { get; set; }
        public virtual DbSet<HistoricoInvestigacao> HistoricoInvestigacoes { get; set; }
        public virtual DbSet<MotivoSuspeita> MotivosSuspeita { get; set; }
        
        // Tabelas de Políticas de Elegibilidade
        public virtual DbSet<PoliticaElegibilidade> PoliticasElegibilidade { get; set; }

        // 📧 Tabelas de Campanhas de Assinaturas
        public virtual DbSet<CampanhaAssinatura> CampanhasAssinaturas { get; set; }
        public virtual DbSet<CampanhaColaborador> CampanhasColaboradores { get; set; }

        // 🦉 Tabelas do Oni o Sábio (TinOne)
        public virtual DbSet<SingleOneAPI.Models.TinOne.TinOneConfigEntity> TinOneConfigs { get; set; }
        public virtual DbSet<SingleOneAPI.Models.TinOne.TinOneAnalytics> TinOneAnalytics { get; set; }
        public virtual DbSet<SingleOneAPI.Models.TinOne.TinOneConversa> TinOneConversas { get; set; }

        // 📤 Tabelas de Importação
        public virtual DbSet<ImportacaoLinhaStaging> ImportacaoLinhasStaging { get; set; }
        public virtual DbSet<ImportacaoColaboradorStaging> ImportacaoColaboradoresStaging { get; set; }
        public virtual DbSet<ImportacaoLog> ImportacaoLogs { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SingleOneDbContext).Assembly);
            modelBuilder.HasPostgresExtension("uuid-ossp");

            // Configuração específica para PatrimonioLogAcesso
            modelBuilder.Entity<PatrimonioLogAcesso>(entity =>
            {
                entity.ToTable("patrimonio_logs_acesso");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TipoAcesso).HasColumnName("tipo_acesso");
                entity.Property(e => e.ColaboradorId).HasColumnName("colaborador_id");
                entity.Property(e => e.CpfConsultado).HasColumnName("cpf_consultado");
                entity.Property(e => e.IpAddress).HasColumnName("ip_address");
                entity.Property(e => e.UserAgent).HasColumnName("user_agent");
                entity.Property(e => e.DadosConsultados).HasColumnName("dados_consultados");
                entity.Property(e => e.Sucesso).HasColumnName("sucesso");
                entity.Property(e => e.MensagemErro).HasColumnName("mensagem_erro");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            // Configuração específica para PatrimonioContestacao
            modelBuilder.Entity<PatrimonioContestacao>(entity =>
            {
                entity.ToTable("patrimonio_contestoes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ColaboradorId).HasColumnName("colaborador_id");
                entity.Property(e => e.EquipamentoId).HasColumnName("equipamento_id");
                entity.Property(e => e.Motivo).HasColumnName("motivo");
                entity.Property(e => e.Descricao).HasColumnName("descricao");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.EvidenciaUrl).HasColumnName("evidencia_url");
                entity.Property(e => e.DataContestacao).HasColumnName("data_contestacao");
                entity.Property(e => e.DataResolucao).HasColumnName("data_resolucao");
                entity.Property(e => e.UsuarioResolucao).HasColumnName("usuario_resolucao");
                entity.Property(e => e.ObservacaoResolucao).HasColumnName("observacao_resolucao");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.TipoContestacao).HasColumnName("tipo_contestacao");

                // Configurar relacionamentos
                entity.HasOne(e => e.Colaborador)
                    .WithMany()
                    .HasForeignKey(e => e.ColaboradorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ✅ CORREÇÃO: Remover chave estrangeira rígida para equipamento_id
                // Agora pode referenciar tanto equipamentos quanto linhas telefônicas
                // A validação é feita no código da aplicação (PatrimonioNegocio)
                // entity.HasOne(e => e.Equipamento)
                //     .WithMany()
                //     .HasForeignKey(e => e.EquipamentoId)
                //     .OnDelete(DeleteBehavior.Restrict);

                // ✅ CORREÇÃO: Remover foreign key de usuario_resolucao
                // Este campo pode conter tanto ID de Usuario (equipe técnica) quanto ID de Colaborador (autoatendimento)
                // A validação e busca são feitas no código da aplicação
                entity.Ignore(e => e.UsuarioResolucaoNavigation);
            });

            // Configuração para ProtocoloDescarte
            modelBuilder.Entity<ProtocoloDescarte>(entity =>
            {
                entity.ToTable("protocolos_descarte");
                
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Protocolo).HasColumnName("protocolo");
                entity.Property(e => e.Cliente).HasColumnName("cliente");
                entity.Property(e => e.TipoDescarte).HasColumnName("tipo_descarte");
                entity.Property(e => e.MotivoDescarte).HasColumnName("motivo_descarte");
                entity.Property(e => e.DestinoFinal).HasColumnName("destino_final");
                entity.Property(e => e.EmpresaDestinoFinal).HasColumnName("empresa_destino_final");
                entity.Property(e => e.CnpjDestinoFinal).HasColumnName("cnpj_destino_final");
                entity.Property(e => e.CertificadoDescarte).HasColumnName("certificado_descarte");
                
                // Campos MTR
                entity.Property(e => e.MtrObrigatorio).HasColumnName("mtr_obrigatorio");
                entity.Property(e => e.MtrNumero).HasColumnName("mtr_numero");
                entity.Property(e => e.MtrEmitidoPor).HasColumnName("mtr_emitido_por");
                entity.Property(e => e.MtrDataEmissao).HasColumnName("mtr_data_emissao");
                entity.Property(e => e.MtrValidade).HasColumnName("mtr_validade");
                entity.Property(e => e.MtrArquivo).HasColumnName("mtr_arquivo");
                entity.Property(e => e.MtrEmpresaTransportadora).HasColumnName("mtr_empresa_transportadora");
                entity.Property(e => e.MtrCnpjTransportadora).HasColumnName("mtr_cnpj_transportadora");
                entity.Property(e => e.MtrPlacaVeiculo).HasColumnName("mtr_placa_veiculo");
                entity.Property(e => e.MtrMotorista).HasColumnName("mtr_motorista");
                entity.Property(e => e.MtrCpfMotorista).HasColumnName("mtr_cpf_motorista");
                entity.Property(e => e.ResponsavelProtocolo).HasColumnName("responsavel_protocolo");
                entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");
                entity.Property(e => e.DataConclusao).HasColumnName("data_conclusao");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.ValorTotalEstimado).HasColumnName("valor_total_estimado");
                entity.Property(e => e.DocumentoGerado).HasColumnName("documento_gerado");
                entity.Property(e => e.CaminhoDocumento).HasColumnName("caminho_documento");
                entity.Property(e => e.Observacoes).HasColumnName("observacoes");
                entity.Property(e => e.Ativo).HasColumnName("ativo");

                // Configurar relacionamentos
                entity.HasOne(e => e.ClienteNavigation)
                    .WithMany()
                    .HasForeignKey(e => e.Cliente)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ResponsavelNavigation)
                    .WithMany()
                    .HasForeignKey(e => e.ResponsavelProtocolo)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                // Ignorar coleção de evidências para evitar referência circular
                entity.Ignore(e => e.Evidencias);
            });

            // Configuração para ProtocoloDescarteItem
            modelBuilder.Entity<ProtocoloDescarteItem>(entity =>
            {
                entity.ToTable("protocolo_descarte_itens");
                
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ProtocoloId).HasColumnName("protocolo_id");
                entity.Property(e => e.Equipamento).HasColumnName("equipamento");
                entity.Property(e => e.ProcessosObrigatorios).HasColumnName("processos_obrigatorios");
                entity.Property(e => e.ObrigarSanitizacao).HasColumnName("obrigar_sanitizacao");
                entity.Property(e => e.ObrigarDescaracterizacao).HasColumnName("obrigar_descaracterizacao");
                entity.Property(e => e.ObrigarPerfuracaoDisco).HasColumnName("obrigar_perfuracao_disco");
                entity.Property(e => e.EvidenciasObrigatorias).HasColumnName("evidencias_obrigatorias");
                entity.Property(e => e.ProcessoSanitizacao).HasColumnName("processo_sanitizacao");
                entity.Property(e => e.ProcessoDescaracterizacao).HasColumnName("processo_descaracterizacao");
                entity.Property(e => e.ProcessoPerfuracaoDisco).HasColumnName("processo_perfuracao_disco");
                entity.Property(e => e.EvidenciasExecutadas).HasColumnName("evidencias_executadas");
                entity.Property(e => e.MetodoSanitizacao).HasColumnName("metodo_sanitizacao");
                entity.Property(e => e.FerramentaUtilizada).HasColumnName("ferramenta_utilizada");
                entity.Property(e => e.ObservacoesSanitizacao).HasColumnName("observacoes_sanitizacao");
                entity.Property(e => e.ValorEstimado).HasColumnName("valor_estimado");
                entity.Property(e => e.ObservacoesItem).HasColumnName("observacoes_item");
                entity.Property(e => e.DataProcessoIniciado).HasColumnName("data_processo_iniciado");
                entity.Property(e => e.DataProcessoConcluido).HasColumnName("data_processo_concluido");
                entity.Property(e => e.StatusItem).HasColumnName("status_item");
                entity.Property(e => e.Ativo).HasColumnName("ativo");

                // Configurar relacionamentos
                entity.HasOne(e => e.Protocolo)
                    .WithMany(p => p.Itens)
                    .HasForeignKey(e => e.ProtocoloId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EquipamentoNavigation)
                    .WithMany()
                    .HasForeignKey(e => e.Equipamento)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuração para DescarteEvidencia
            modelBuilder.Entity<DescarteEvidencia>(entity =>
            {
                entity.ToTable("descarteevidencias");
                
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Equipamento).HasColumnName("equipamento");
                entity.Property(e => e.Descricao).HasColumnName("descricao");
                entity.Property(e => e.Tipoprocesso).HasColumnName("tipoprocesso");
                entity.Property(e => e.Nomearquivo).HasColumnName("nomearquivo");
                entity.Property(e => e.Caminhoarquivo).HasColumnName("caminhoarquivo");
                entity.Property(e => e.Tipoarquivo).HasColumnName("tipoarquivo");
                entity.Property(e => e.Tamanhoarquivo).HasColumnName("tamanhoarquivo");
                entity.Property(e => e.Usuarioupload).HasColumnName("usuarioupload");
                entity.Property(e => e.Dataupload).HasColumnName("dataupload");
                entity.Property(e => e.Ativo).HasColumnName("ativo");
                entity.Property(e => e.ProtocoloId).HasColumnName("protocolo_id");

                // Configurar relacionamento com Protocolo (sem navegação inversa)
                entity.HasOne(e => e.ProtocoloNavigation)
                    .WithMany()
                    .HasForeignKey(e => e.ProtocoloId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Configurar relacionamento com Equipamento
                entity.HasOne(e => e.EquipamentoNavigation)
                    .WithMany()
                    .HasForeignKey(e => e.Equipamento)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configurar relacionamento com Usuario
                entity.HasOne(e => e.UsuarioUploadNavigation)
                    .WithMany()
                    .HasForeignKey(e => e.Usuarioupload)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 🦉 Configuração para TinOne (usa annotations dos modelos)
            // Os modelos já possuem [Table] e [Column] attributes configurados
            
            // 🔥 FORÇAR IGNORAR metadata em TinOneConversa (problema de tipo jsonb vs text)
            modelBuilder.Entity<SingleOneAPI.Models.TinOne.TinOneConversa>(entity =>
            {
                entity.Ignore(e => e.Metadata);
            });

            // ========================================
            // 📧 Configuração para Campanhas de Assinaturas
            // ========================================
            modelBuilder.Entity<CampanhaAssinatura>(entity =>
            {
                entity.ToTable("campanhasassinaturas");
                entity.HasKey(e => e.Id);
                
                // Mapeamento de colunas
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Cliente).HasColumnName("cliente");
                entity.Property(e => e.UsuarioCriacao).HasColumnName("usuariocriacao");
                entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
                entity.Property(e => e.Descricao).HasColumnName("descricao");
                entity.Property(e => e.DataCriacao).HasColumnName("datacriacao");
                entity.Property(e => e.DataInicio).HasColumnName("datainicio");
                entity.Property(e => e.DataFim).HasColumnName("datafim");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.FiltrosJson).HasColumnName("filtrosjson");
                entity.Property(e => e.TotalColaboradores).HasColumnName("totalcolaboradores");
                entity.Property(e => e.TotalEnviados).HasColumnName("totalenviados");
                entity.Property(e => e.TotalAssinados).HasColumnName("totalassinados");
                entity.Property(e => e.TotalPendentes).HasColumnName("totalpendentes");
                entity.Property(e => e.PercentualAdesao).HasColumnName("percentualadesao");
                entity.Property(e => e.DataUltimoEnvio).HasColumnName("dataultimoenvio");
                entity.Property(e => e.DataConclusao).HasColumnName("dataconclusao");
                
                // Configurar relacionamentos
                entity.HasOne(e => e.ClienteNavigation)
                    .WithMany()
                    .HasForeignKey(e => e.Cliente)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.UsuarioCriacaoNavigation)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioCriacao)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasMany(e => e.CampanhaColaboradores)
                    .WithOne(cc => cc.Campanha)
                    .HasForeignKey(cc => cc.CampanhaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração para Campanha Colaborador
            modelBuilder.Entity<CampanhaColaborador>(entity =>
            {
                entity.ToTable("campanhascolaboradores");
                entity.HasKey(e => e.Id);
                
                // Mapeamento de colunas
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CampanhaId).HasColumnName("campanhaid");
                entity.Property(e => e.ColaboradorId).HasColumnName("colaboradorid");
                entity.Property(e => e.DataInclusao).HasColumnName("datainclusao");
                entity.Property(e => e.StatusAssinatura).HasColumnName("statusassinatura");
                entity.Property(e => e.DataEnvio).HasColumnName("dataenvio");
                entity.Property(e => e.DataAssinatura).HasColumnName("dataassinatura");
                entity.Property(e => e.TotalEnvios).HasColumnName("totalenvios");
                entity.Property(e => e.DataUltimoEnvio).HasColumnName("dataultimoenvio");
                entity.Property(e => e.IpEnvio).HasColumnName("ipenvio").HasMaxLength(50);
                entity.Property(e => e.LocalizacaoEnvio).HasColumnName("localizacaoenvio").HasMaxLength(500);
                
                // Configurar relacionamentos
                entity.HasOne(e => e.Campanha)
                    .WithMany(c => c.CampanhaColaboradores)
                    .HasForeignKey(e => e.CampanhaId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Colaborador)
                    .WithMany()
                    .HasForeignKey(e => e.ColaboradorId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Índice único para evitar duplicatas
                entity.HasIndex(e => new { e.CampanhaId, e.ColaboradorId })
                    .IsUnique()
                    .HasDatabaseName("uk_campanhacolaborador");
            });
        }

        public override int SaveChanges()
        {
            // Atualizar campos de timestamp antes de salvar
            AtualizarTimestamps();
            return base.SaveChanges();
        }

        private void AtualizarTimestamps()
        {
            var entidades = ChangeTracker.Entries()
                .Where(e => e.Entity is Empresa && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entidade in entidades)
            {
                var empresa = (Empresa)entidade.Entity;
                var agora = DateTime.Now;

                if (entidade.State == EntityState.Added)
                {
                    empresa.CreatedAt = agora;
                    empresa.UpdatedAt = agora;
                    Console.WriteLine($"[DBCONTEXT]  Timestamps definidos para nova empresa - CreatedAt: {empresa.CreatedAt}, UpdatedAt: {empresa.UpdatedAt}");
                }
                else if (entidade.State == EntityState.Modified)
                {
                    empresa.UpdatedAt = agora;
                    Console.WriteLine($"[DBCONTEXT]  UpdatedAt definido para empresa editada: {empresa.UpdatedAt}");
                }
            }
        }
    }
}
