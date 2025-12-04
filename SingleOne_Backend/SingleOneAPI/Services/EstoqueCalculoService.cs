using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Infra.Contexto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOneAPI.Services
{
    public class EstoqueCalculoService
    {
        private readonly SingleOneDbContext _context;

        public EstoqueCalculoService(SingleOneDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calcula o total de equipamentos lançados para um modelo específico em uma localidade específica
        /// </summary>
        /// <param name="modeloId">ID do modelo</param>
        /// <param name="localidadeId">ID da localidade</param>
        /// <param name="clienteId">ID do cliente</param>
        /// <returns>Total de equipamentos lançados</returns>
        public async Task<int> CalcularTotalLancado(int modeloId, int localidadeId, int clienteId)
        {
            // Total lançado = todos os equipamentos ativos deste modelo nesta localidade (independente do status)
            var totalLancado = await _context.Equipamentos
                .Where(e => e.Modelo == modeloId 
                         && e.Localidade == localidadeId
                         && e.Cliente == clienteId 
                         && e.Ativo == true)
                .CountAsync();

            return totalLancado;
        }

        /// <summary>
        /// Calcula o estoque atual para um modelo específico em uma localidade específica
        /// Estoque atual = equipamentos nos status: Novo (6), Em estoque (3), Devolvido (2)
        /// </summary>
        /// <param name="modeloId">ID do modelo</param>
        /// <param name="localidadeId">ID da localidade</param>
        /// <param name="clienteId">ID do cliente</param>
        /// <returns>Quantidade em estoque atual</returns>
        public async Task<int> CalcularEstoqueAtual(int modeloId, int localidadeId, int clienteId)
        {
            // Status que contam como estoque atual: Novo (6), Em estoque (3), Devolvido (2)
            var statusEstoque = new[] { 2, 3, 6 };

            var estoqueAtual = await _context.Equipamentos
                .Where(e => e.Modelo == modeloId 
                         && e.Localidade == localidadeId
                         && e.Cliente == clienteId 
                         && e.Ativo == true
                         && statusEstoque.Contains(e.Equipamentostatus.Value))
                .CountAsync();

            return estoqueAtual;
        }

        /// <summary>
        /// Calcula dados completos de estoque para um registro de estoque mínimo
        /// </summary>
        /// <param name="modeloId">ID do modelo</param>
        /// <param name="localidadeId">ID da localidade</param>
        /// <param name="clienteId">ID do cliente</param>
        /// <returns>Dados completos de estoque</returns>
        public async Task<DadosEstoqueModelo> CalcularDadosCompletosEstoque(int modeloId, int localidadeId, int clienteId)
        {
            var totalLancado = await CalcularTotalLancado(modeloId, localidadeId, clienteId);
            var estoqueAtual = await CalcularEstoqueAtual(modeloId, localidadeId, clienteId);

            // Buscar dados do modelo para informações adicionais
            var modelo = await _context.Modelos
                .Include(m => m.FabricanteNavigation)
                .ThenInclude(f => f.TipoequipamentoNavigation)
                .FirstOrDefaultAsync(m => m.Id == modeloId);

            // Buscar dados da localidade
            var localidade = await _context.Localidades
                .FirstOrDefaultAsync(l => l.Id == localidadeId);

            return new DadosEstoqueModelo
            {
                ModeloId = modeloId,
                LocalidadeId = localidadeId,
                ClienteId = clienteId,
                TotalLancado = totalLancado,
                EstoqueAtual = estoqueAtual,
                ModeloDescricao = modelo?.Descricao ?? $"Modelo {modeloId}",
                FabricanteDescricao = modelo?.FabricanteNavigation?.Descricao ?? "N/A",
                TipoEquipamentoDescricao = modelo?.FabricanteNavigation?.TipoequipamentoNavigation?.Descricao ?? "N/A",
                LocalidadeDescricao = localidade?.Descricao ?? $"Localidade {localidadeId}"
            };
        }

        /// <summary>
        /// Calcula dados de estoque para múltiplos modelos em uma localidade específica
        /// </summary>
        /// <param name="modelosIds">Lista de IDs dos modelos</param>
        /// <param name="localidadeId">ID da localidade</param>
        /// <param name="clienteId">ID do cliente</param>
        /// <returns>Dicionário com dados de estoque por modelo</returns>
        public async Task<Dictionary<int, DadosEstoqueModelo>> CalcularDadosEstoqueMultiplosModelos(List<int> modelosIds, int localidadeId, int clienteId)
        {
            var resultado = new Dictionary<int, DadosEstoqueModelo>();

            foreach (var modeloId in modelosIds)
            {
                var dados = await CalcularDadosCompletosEstoque(modeloId, localidadeId, clienteId);
                resultado[modeloId] = dados;
            }

            return resultado;
        }

        /// <summary>
        /// Calcula o percentual de utilização do estoque
        /// </summary>
        /// <param name="estoqueAtual">Quantidade atual em estoque</param>
        /// <param name="estoqueMaximo">Quantidade máxima configurada</param>
        /// <returns>Percentual de utilização (0-100)</returns>
        public double CalcularPercentualUtilizacao(int estoqueAtual, int estoqueMaximo)
        {
            if (estoqueMaximo <= 0) return 0;
            
            var percentual = (double)estoqueAtual / estoqueMaximo * 100;
            return Math.Min(percentual, 100); // Máximo 100%
        }

        /// <summary>
        /// Determina o status do estoque baseado nas quantidades
        /// </summary>
        /// <param name="estoqueAtual">Quantidade atual</param>
        /// <param name="estoqueMinimo">Quantidade mínima</param>
        /// <param name="estoqueMaximo">Quantidade máxima</param>
        /// <returns>Status do estoque</returns>
        public string DeterminarStatusEstoque(int estoqueAtual, int estoqueMinimo, int estoqueMaximo)
        {
            if (estoqueAtual <= estoqueMinimo)
                return "ALERTA";
            else if (estoqueMaximo > 0 && estoqueAtual >= estoqueMaximo)
                return "EXCESSO";
            else
                return "OK";
        }

        /// <summary>
        /// Determina o status do estoque para linhas telefônicas
        /// Mesmo padrão usado para recursos, mas considera que estoque máximo pode ser 0 (sem limite)
        /// </summary>
        /// <param name="estoqueAtual">Quantidade atual de linhas livres</param>
        /// <param name="estoqueMinimo">Quantidade mínima configurada</param>
        /// <param name="estoqueMaximo">Quantidade máxima configurada (0 = sem limite)</param>
        /// <returns>Status do estoque</returns>
        public string DeterminarStatusEstoqueLinhas(int estoqueAtual, int estoqueMinimo, int estoqueMaximo)
        {
            if (estoqueAtual <= estoqueMinimo)
                return "ALERTA";
            else if (estoqueMaximo > 0 && estoqueAtual >= estoqueMaximo)
                return "EXCESSO";
            else
                return "OK";
        }
    }

    /// <summary>
    /// Classe para armazenar dados calculados de estoque
    /// </summary>
    public class DadosEstoqueModelo
    {
        public int ModeloId { get; set; }
        public int LocalidadeId { get; set; }
        public int ClienteId { get; set; }
        public int TotalLancado { get; set; }
        public int EstoqueAtual { get; set; }
        public string ModeloDescricao { get; set; }
        public string FabricanteDescricao { get; set; }
        public string TipoEquipamentoDescricao { get; set; }
        public string LocalidadeDescricao { get; set; }
        public double PercentualUtilizacao { get; set; }
        public string StatusEstoque { get; set; }
    }
}
