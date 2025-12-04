using System.Collections.Generic;

namespace SingleOneAPI.DTOs.TinOne
{
    /// <summary>
    /// DTO para perguntas feitas ao TinOne
    /// </summary>
    public class TinOnePerguntaDTO
    {
        public string Pergunta { get; set; } = string.Empty;
        public string? PaginaContexto { get; set; }
        public string? SessaoId { get; set; }
        public int? UsuarioId { get; set; }
        public int? ClienteId { get; set; }
    }

    /// <summary>
    /// DTO para respostas do TinOne
    /// </summary>
    public class TinOneRespostaDTO
    {
        public string Resposta { get; set; } = string.Empty;
        public string Tipo { get; set; } = "texto"; // texto, guia, navegacao, erro
        public object? Dados { get; set; } // Dados adicionais (JSON)
        public bool Sucesso { get; set; } = true;
        public string? ErroMensagem { get; set; }
    }

    /// <summary>
    /// DTO para informações de campo
    /// </summary>
    public class TinOneCampoInfoDTO
    {
        public string CampoId { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? Exemplo { get; set; }
        public string? Tipo { get; set; }
        public bool Obrigatorio { get; set; }
        public string? Formato { get; set; }
        public string? RegraNegocio { get; set; }
        public List<string>? Dicas { get; set; }
    }

    /// <summary>
    /// DTO para processos guiados
    /// </summary>
    public class TinOneProcessoDTO
    {
        public int ProcessoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("Tempo-estimado")]
        public string? TempoEstimado { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("Palavras-chave")]
        public List<string>? PalavrasChave { get; set; }
        
        public List<TinOnePassoDTO> Passos { get; set; } = new();
    }

    /// <summary>
    /// DTO para passos de processos
    /// </summary>
    public class TinOnePassoDTO
    {
        public int Numero { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? Rota { get; set; }
        public string? Acao { get; set; }
        public string? ElementoDestaque { get; set; }
        public string? Dica { get; set; }
    }

    /// <summary>
    /// DTO para feedback
    /// </summary>
    public class TinOneFeedbackDTO
    {
        public int? AnalyticsId { get; set; }
        public bool FoiUtil { get; set; }
        public string? Comentario { get; set; }
    }
}

