using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.ViewModels
{
    /// <summary>
    /// ViewModel para Protocolo de Descarte
    /// Facilita o trabalho no frontend e API
    /// </summary>
    public class ProtocoloDescarteVM
    {
        /// <summary>
        /// ID único do protocolo
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número único do protocolo (ex: DESC-2025-001234)
        /// </summary>
        public string Protocolo { get; set; }

        /// <summary>
        /// ID do cliente
        /// </summary>
        public int Cliente { get; set; }

        /// <summary>
        /// Nome do cliente
        /// </summary>
        public string NomeCliente { get; set; }

        /// <summary>
        /// Tipo de descarte (DOACAO, VENDA, DEVOLUCAO, etc.)
        /// </summary>
        public string TipoDescarte { get; set; }

        /// <summary>
        /// Descrição amigável do tipo de descarte
        /// </summary>
        public string TipoDescarteDescricao { get; set; }

        /// <summary>
        /// Motivo do descarte
        /// </summary>
        public string MotivoDescarte { get; set; }

        /// <summary>
        /// Destino final dos equipamentos
        /// </summary>
        public string DestinoFinal { get; set; }

        /// <summary>
        /// Empresa responsável pelo destino final
        /// </summary>
        public string EmpresaDestinoFinal { get; set; }

        /// <summary>
        /// CNPJ da empresa de destino final
        /// </summary>
        public string CnpjDestinoFinal { get; set; }

        /// <summary>
        /// Número do certificado de descarte ambiental
        /// </summary>
        public string CertificadoDescarte { get; set; }

        // ========== INFORMAÇÕES DO MTR (Manifesto de Transporte de Resíduos) ==========

        /// <summary>
        /// Indica se MTR é obrigatório para este descarte
        /// </summary>
        public bool MtrObrigatorio { get; set; }

        /// <summary>
        /// Número do MTR emitido
        /// </summary>
        public string MtrNumero { get; set; }

        /// <summary>
        /// Quem emitiu o MTR (GERADOR, TRANSPORTADOR, DESTINADOR)
        /// </summary>
        public string MtrEmitidoPor { get; set; }

        /// <summary>
        /// Data de emissão do MTR
        /// </summary>
        public DateTime? MtrDataEmissao { get; set; }

        /// <summary>
        /// Data de validade do MTR
        /// </summary>
        public DateTime? MtrValidade { get; set; }

        /// <summary>
        /// Caminho do arquivo MTR (PDF)
        /// </summary>
        public string MtrArquivo { get; set; }

        /// <summary>
        /// Empresa transportadora
        /// </summary>
        public string MtrEmpresaTransportadora { get; set; }

        /// <summary>
        /// CNPJ da transportadora
        /// </summary>
        public string MtrCnpjTransportadora { get; set; }

        /// <summary>
        /// Placa do veículo transportador
        /// </summary>
        public string MtrPlacaVeiculo { get; set; }

        /// <summary>
        /// Nome do motorista
        /// </summary>
        public string MtrMotorista { get; set; }

        /// <summary>
        /// CPF do motorista
        /// </summary>
        public string MtrCpfMotorista { get; set; }

        // ========== FIM DAS INFORMAÇÕES DO MTR ==========

        /// <summary>
        /// ID do usuário responsável pelo protocolo
        /// </summary>
        public int ResponsavelProtocolo { get; set; }

        /// <summary>
        /// Nome do usuário responsável
        /// </summary>
        public string NomeResponsavel { get; set; }

        /// <summary>
        /// Data de criação do protocolo
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data de conclusão do protocolo
        /// </summary>
        public DateTime? DataConclusao { get; set; }

        /// <summary>
        /// Status do protocolo (EM_ANDAMENTO, CONCLUIDO, CANCELADO)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Descrição amigável do status
        /// </summary>
        public string StatusDescricao { get; set; }

        /// <summary>
        /// Valor total estimado
        /// </summary>
        public decimal? ValorTotalEstimado { get; set; }

        /// <summary>
        /// Indica se o documento oficial foi gerado
        /// </summary>
        public bool DocumentoGerado { get; set; }

        /// <summary>
        /// Caminho do documento gerado
        /// </summary>
        public string CaminhoDocumento { get; set; }

        /// <summary>
        /// Observações gerais do protocolo
        /// </summary>
        public string Observacoes { get; set; }

        /// <summary>
        /// Indica se o registro está ativo
        /// </summary>
        public bool Ativo { get; set; }

        /// <summary>
        /// Lista de equipamentos do protocolo
        /// </summary>
        public List<ProtocoloDescarteItemVM> Itens { get; set; }

        /// <summary>
        /// Quantidade total de equipamentos no protocolo
        /// </summary>
        public int QuantidadeEquipamentos { get; set; }

        /// <summary>
        /// Quantidade de equipamentos concluídos
        /// </summary>
        public int QuantidadeConcluidos { get; set; }

        /// <summary>
        /// Percentual de conclusão do protocolo
        /// </summary>
        public decimal PercentualConclusao { get; set; }

        /// <summary>
        /// Indica se o protocolo pode ser finalizado
        /// </summary>
        public bool PodeFinalizar { get; set; }

        public ProtocoloDescarteVM()
        {
            Itens = new List<ProtocoloDescarteItemVM>();
            Status = "EM_ANDAMENTO";
            Ativo = true;
        }
    }

    /// <summary>
    /// ViewModel para Item do Protocolo de Descarte
    /// </summary>
    public class ProtocoloDescarteItemVM
    {
        /// <summary>
        /// ID único do item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID do protocolo
        /// </summary>
        public int ProtocoloId { get; set; }

        /// <summary>
        /// ID do equipamento
        /// </summary>
        public int Equipamento { get; set; }

        /// <summary>
        /// Dados do equipamento
        /// </summary>
        public SingleOne.Models.Equipamentovm EquipamentoNavigation { get; set; }

        /// <summary>
        /// Indica se este equipamento tem processos obrigatórios
        /// </summary>
        public bool ProcessosObrigatorios { get; set; }

        /// <summary>
        /// Indica se é obrigatório executar sanitização
        /// </summary>
        public bool ObrigarSanitizacao { get; set; }

        /// <summary>
        /// Indica se é obrigatório executar descaracterização
        /// </summary>
        public bool ObrigarDescaracterizacao { get; set; }

        /// <summary>
        /// Indica se é obrigatório executar perfuração de disco
        /// </summary>
        public bool ObrigarPerfuracaoDisco { get; set; }

        /// <summary>
        /// Indica se são obrigatórias evidências para este equipamento
        /// </summary>
        public bool EvidenciasObrigatorias { get; set; }

        /// <summary>
        /// Indica se o processo de sanitização foi executado
        /// </summary>
        public bool ProcessoSanitizacao { get; set; }

        /// <summary>
        /// Indica se o processo de descaracterização foi executado
        /// </summary>
        public bool ProcessoDescaracterizacao { get; set; }

        /// <summary>
        /// Indica se o processo de perfuração de disco foi executado
        /// </summary>
        public bool ProcessoPerfuracaoDisco { get; set; }

        /// <summary>
        /// Indica se as evidências foram executadas
        /// </summary>
        public bool EvidenciasExecutadas { get; set; }

        /// <summary>
        /// Método de sanitização utilizado
        /// </summary>
        public string MetodoSanitizacao { get; set; }

        /// <summary>
        /// Ferramenta utilizada na sanitização
        /// </summary>
        public string FerramentaUtilizada { get; set; }

        /// <summary>
        /// Observações sobre o processo de sanitização
        /// </summary>
        public string ObservacoesSanitizacao { get; set; }

        /// <summary>
        /// Quantidade de evidências anexadas
        /// </summary>
        public int QuantidadeEvidencias { get; set; }

        /// <summary>
        /// Lista de evidências do equipamento
        /// </summary>
        public List<DescarteEvidenciaVM> Evidencias { get; set; }

        /// <summary>
        /// Valor estimado do equipamento
        /// </summary>
        public decimal? ValorEstimado { get; set; }

        /// <summary>
        /// Observações específicas deste equipamento
        /// </summary>
        public string ObservacoesItem { get; set; }

        /// <summary>
        /// Data em que o processo foi iniciado
        /// </summary>
        public DateTime? DataProcessoIniciado { get; set; }

        /// <summary>
        /// Data em que o processo foi concluído
        /// </summary>
        public DateTime? DataProcessoConcluido { get; set; }

        /// <summary>
        /// Status do item (PENDENTE, EM_PROCESSO, CONCLUIDO)
        /// </summary>
        public string StatusItem { get; set; }

        /// <summary>
        /// Descrição amigável do status do item
        /// </summary>
        public string StatusItemDescricao { get; set; }

        /// <summary>
        /// Indica se o registro está ativo
        /// </summary>
        public bool Ativo { get; set; }

        /// <summary>
        /// Indica se o item está pronto para conclusão
        /// </summary>
        public bool ProntoParaConclusao { get; set; }

        public ProtocoloDescarteItemVM()
        {
            Evidencias = new List<DescarteEvidenciaVM>();
            // ✅ CORREÇÃO: Não definir StatusItem aqui, pois o backend já define corretamente
            // Status é "PENDENTE" apenas para equipamentos com processos obrigatórios
            // Status é "CONCLUIDO" automaticamente para equipamentos sem processos obrigatórios
            Ativo = true;
        }
    }

    /// <summary>
    /// ViewModel para Evidência de Descarte
    /// </summary>
    public class DescarteEvidenciaVM
    {
        /// <summary>
        /// ID da evidência
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID do equipamento
        /// </summary>
        public int Equipamento { get; set; }

        /// <summary>
        /// ID do protocolo (opcional)
        /// </summary>
        public int? ProtocoloId { get; set; }

        /// <summary>
        /// Descrição da evidência
        /// </summary>
        public string Descricao { get; set; }

        /// <summary>
        /// Tipo do processo (SANITIZACAO, DESCARACTERIZACAO, etc.)
        /// </summary>
        public string TipoProcesso { get; set; }

        /// <summary>
        /// Nome original do arquivo
        /// </summary>
        public string NomeArquivo { get; set; }

        /// <summary>
        /// Caminho do arquivo
        /// </summary>
        public string CaminhoArquivo { get; set; }

        /// <summary>
        /// Tipo do arquivo (MIME type)
        /// </summary>
        public string TipoArquivo { get; set; }

        /// <summary>
        /// Tamanho do arquivo em bytes
        /// </summary>
        public long? TamanhoArquivo { get; set; }

        /// <summary>
        /// ID do usuário que fez o upload
        /// </summary>
        public int UsuarioUpload { get; set; }

        /// <summary>
        /// Nome do usuário que fez o upload
        /// </summary>
        public string NomeUsuarioUpload { get; set; }

        /// <summary>
        /// Data do upload
        /// </summary>
        public DateTime DataUpload { get; set; }

        /// <summary>
        /// Indica se está ativo
        /// </summary>
        public bool Ativo { get; set; }
    }
}
