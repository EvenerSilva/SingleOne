using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Models;
using SingleOneAPI.Repository.Interfaces;
using SingleOneAPI.Infra.Contexto;
using SingleOneAPI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOneAPI.Repository
{
    public class EstoqueMinimoEquipamentoRepository : IEstoqueMinimoEquipamentoRepository
    {
        private readonly SingleOneDbContext _context;
        private readonly EstoqueCalculoService _estoqueCalculoService;

        public EstoqueMinimoEquipamentoRepository(SingleOneDbContext context, EstoqueCalculoService estoqueCalculoService)
        {
            _context = context;
            _estoqueCalculoService = estoqueCalculoService;
        }

        public async Task<List<EstoqueMinimoEquipamento>> ListarPorCliente(int clienteId)
        {
            return await _context.EstoqueMinimoEquipamentos
                .Where(e => e.Cliente == clienteId && e.Ativo)
                .OrderBy(e => e.Localidade)
                .ThenBy(e => e.Modelo)
                .ToListAsync();
        }

        /// <summary>
        /// Lista registros com dados de estoque calculados dinamicamente
        /// </summary>
        public async Task<List<EstoqueMinimoEquipamentoDTO>> ListarPorClienteComDadosCalculados(int clienteId)
        {
            var registros = await _context.EstoqueMinimoEquipamentos
                .Where(e => e.Cliente == clienteId && e.Ativo)
                .OrderBy(e => e.Localidade)
                .ThenBy(e => e.Modelo)
                .ToListAsync();

            var resultado = new List<EstoqueMinimoEquipamentoDTO>();

            // Calcular dados dinâmicos de estoque para cada registro
            foreach (var registro in registros)
            {
                var dadosEstoque = await _estoqueCalculoService.CalcularDadosCompletosEstoque(registro.Modelo, registro.Localidade, clienteId);
                
                // Calcular percentual de utilização e status
                dadosEstoque.PercentualUtilizacao = _estoqueCalculoService.CalcularPercentualUtilizacao(
                    dadosEstoque.EstoqueAtual, registro.QuantidadeMaxima);
                
                dadosEstoque.StatusEstoque = _estoqueCalculoService.DeterminarStatusEstoque(
                    dadosEstoque.EstoqueAtual, registro.QuantidadeMinima, registro.QuantidadeMaxima);

                // Converter para DTO
                var dto = EstoqueMinimoEquipamentoDTO.FromEntity(registro, dadosEstoque);
                resultado.Add(dto);
            }

            return resultado;
        }

        public async Task<EstoqueMinimoEquipamento> BuscarPorId(int id)
        {
            return await _context.EstoqueMinimoEquipamentos
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<EstoqueAlertaVM>> ListarAlertasConsolidados(int clienteId)
        {
            var alertasEquipamentos = await ListarAlertasEquipamentos(clienteId);
            var alertasLinhas = await _context.EstoqueMinimoLinhas
                .Include(e => e.OperadoraNavigation)
                .Include(e => e.PlanoNavigation)
                .ThenInclude(p => p.ContratoNavigation)
                .Include(e => e.LocalidadeNavigation)
                .Where(e => e.Cliente == clienteId && e.Ativo)
                .Select(e => new EstoqueAlertaVM
                {
                    Tipo = "LINHA_TELEFONICA",
                    Cliente = e.Cliente,
                    Localidade = e.LocalidadeNavigation.Descricao,
                    Descricao = $"{e.OperadoraNavigation.Nome} - {e.PlanoNavigation.ContratoNavigation.Nome} - {e.PlanoNavigation.Nome} ({e.PerfilUso})",
                    EstoqueAtual = 0, // Será calculado via view
                    EstoqueMinimo = e.QuantidadeMinima,
                    QuantidadeFaltante = 0, // Será calculado via view
                    Status = "ALERTA" // Será calculado via view
                })
                .ToListAsync();

            var resultado = new List<EstoqueAlertaVM>();
            resultado.AddRange(alertasEquipamentos.Select(a => new EstoqueAlertaVM
            {
                Tipo = "EQUIPAMENTO",
                Cliente = a.Cliente,
                Localidade = a.Localidade,
                Descricao = $"{a.TipoEquipamento} - {a.Fabricante} {a.Modelo}",
                EstoqueAtual = a.EstoqueAtual,
                EstoqueMinimo = a.EstoqueMinimo,
                QuantidadeFaltante = a.QuantidadeFaltante,
                Status = a.Status
            }));

            resultado.AddRange(alertasLinhas);

            return resultado.Where(a => a.Status == "ALERTA").ToList();
        }

        public async Task<List<EstoqueEquipamentoAlertaVM>> ListarAlertasEquipamentos(int clienteId)
        {
            // Implementação simplificada sem navegações
            var alertas = await _context.EstoqueMinimoEquipamentos
                .Where(e => e.Cliente == clienteId && e.Ativo)
                .Select(e => new EstoqueEquipamentoAlertaVM
                {
                    Cliente = e.Cliente,
                    Localidade = $"Localidade {e.Localidade}", // Simplificado
                    TipoEquipamento = "Equipamento",
                    Fabricante = $"Fabricante {e.Modelo}", // Simplificado
                    Modelo = $"Modelo {e.Modelo}",
                    EstoqueAtual = 0,
                    EstoqueMinimo = e.QuantidadeMinima,
                    QuantidadeFaltante = 0,
                    Status = "ALERTA"
                })
                .ToListAsync();

            return alertas;
        }

        public async Task<int> ContarAlertasEquipamentos(int clienteId)
        {
            return await _context.EstoqueMinimoEquipamentos
                .Where(e => e.Cliente == clienteId && e.Ativo)
                .CountAsync();
        }
    }
}
