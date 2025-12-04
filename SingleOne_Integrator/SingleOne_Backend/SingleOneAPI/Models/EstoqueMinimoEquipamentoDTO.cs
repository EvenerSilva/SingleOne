using System;

namespace SingleOneAPI.Models
{
    /// <summary>
    /// DTO para EstoqueMinimoEquipamento com dados calculados dinamicamente
    /// </summary>
    public class EstoqueMinimoEquipamentoDTO
    {
        public int Id { get; set; }
        public int Cliente { get; set; }
        public int Modelo { get; set; }
        public int Localidade { get; set; }
        public int QuantidadeMinima { get; set; }
        public int QuantidadeMaxima { get; set; }
        public int QuantidadeTotalLancada { get; set; } // Calculado dinamicamente
        public int EstoqueAtual { get; set; } // Calculado dinamicamente
        public bool Ativo { get; set; }
        public DateTime DtCriacao { get; set; }
        public int UsuarioCriacao { get; set; }
        public DateTime? DtAtualizacao { get; set; }
        public int? UsuarioAtualizacao { get; set; }
        public string? Observacoes { get; set; }

        // Campos calculados
        public double PercentualUtilizacao { get; set; }
        public string StatusEstoque { get; set; } = "OK";
        public int QuantidadeFaltante { get; set; }
        public int QuantidadeExcesso { get; set; }

        // Informações de navegação (para exibição)
        public string? ModeloDescricao { get; set; }
        public string? FabricanteDescricao { get; set; }
        public string? TipoEquipamentoDescricao { get; set; }
        public string? LocalidadeDescricao { get; set; }

        /// <summary>
        /// Converte de EstoqueMinimoEquipamento para DTO
        /// </summary>
        public static EstoqueMinimoEquipamentoDTO FromEntity(EstoqueMinimoEquipamento entity, DadosEstoqueModelo dadosEstoque)
        {
            var dto = new EstoqueMinimoEquipamentoDTO
            {
                Id = entity.Id,
                Cliente = entity.Cliente,
                Modelo = entity.Modelo,
                Localidade = entity.Localidade,
                QuantidadeMinima = entity.QuantidadeMinima,
                QuantidadeMaxima = entity.QuantidadeMaxima,
                QuantidadeTotalLancada = dadosEstoque.TotalLancado,
                EstoqueAtual = dadosEstoque.EstoqueAtual,
                Ativo = entity.Ativo,
                DtCriacao = entity.DtCriacao,
                UsuarioCriacao = entity.UsuarioCriacao,
                DtAtualizacao = entity.DtAtualizacao,
                UsuarioAtualizacao = entity.UsuarioAtualizacao,
                Observacoes = entity.Observacoes,
                ModeloDescricao = dadosEstoque.ModeloDescricao,
                FabricanteDescricao = dadosEstoque.FabricanteDescricao,
                TipoEquipamentoDescricao = dadosEstoque.TipoEquipamentoDescricao,
                LocalidadeDescricao = dadosEstoque.LocalidadeDescricao
            };

            // Calcular campos derivados
            dto.PercentualUtilizacao = dadosEstoque.PercentualUtilizacao;
            dto.StatusEstoque = dadosEstoque.StatusEstoque;
            
            // Calcular quantidade faltante/excesso
            if (dto.EstoqueAtual <= dto.QuantidadeMinima)
                dto.QuantidadeFaltante = dto.QuantidadeMinima - dto.EstoqueAtual;
            
            if (dto.EstoqueAtual >= dto.QuantidadeMaxima)
                dto.QuantidadeExcesso = dto.EstoqueAtual - dto.QuantidadeMaxima;

            return dto;
        }
    }
}
