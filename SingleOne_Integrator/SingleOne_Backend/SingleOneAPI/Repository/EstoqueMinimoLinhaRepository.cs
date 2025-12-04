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
    public class EstoqueMinimoLinhaRepository : IEstoqueMinimoLinhaRepository
    {
        private readonly SingleOneDbContext _context;
        private readonly EstoqueCalculoService _estoqueCalculoService;

        public EstoqueMinimoLinhaRepository(SingleOneDbContext context)
        {
            _context = context;
            _estoqueCalculoService = new EstoqueCalculoService(context);
        }

        public async Task<List<EstoqueMinimoLinha>> ListarPorCliente(int clienteId)
        {
            var linhas = await _context.EstoqueMinimoLinhas
                .Include(e => e.ClienteNavigation)
                .Include(e => e.OperadoraNavigation)
                .Include(e => e.PlanoNavigation)
                .ThenInclude(p => p.ContratoNavigation)
                .Include(e => e.LocalidadeNavigation)
                .Include(e => e.UsuarioCriacaoNavigation)
                .Where(e => e.Cliente == clienteId && e.Ativo)
                .OrderBy(e => e.LocalidadeNavigation.Descricao)
                .ThenBy(e => e.OperadoraNavigation.Nome)
                .ThenBy(e => e.PlanoNavigation.Nome)
                .ToListAsync();

            // Enriquecer com dados agregados por plano a partir de telefonialinhas (sem depender de view)
            var planoIds = linhas.Select(l => l.Plano).Distinct().ToList();
            if (planoIds.Count > 0)
            {
                var agregados = await _context.Telefonialinhas
                    .Where(t => planoIds.Contains(t.Plano))
                    .GroupBy(t => t.Plano)
                    .Select(g => new
                    {
                        Plano = g.Key,
                        Total = g.Count(),
                        Livres = g.Count(t => t.Ativo && !t.Emuso),
                        EmUso = g.Count(t => t.Ativo && t.Emuso)
                    })
                    .ToListAsync();

                var planoIdToAgg = agregados.ToDictionary(a => a.Plano, a => a);

                foreach (var linha in linhas)
                {
                    int totalLancado = 0;
                    int estoqueAtual = 0;

                    if (planoIdToAgg.TryGetValue(linha.Plano, out var agg))
                    {
                        totalLancado = agg.Total;
                        estoqueAtual = agg.Livres;
                    }

                    // Aplicar valores e calcular sempre, mesmo sem agregados
                    linha.QuantidadeTotalLancada = totalLancado;
                    linha.EstoqueAtual = estoqueAtual;

                    // Percentual de utilização (comparando com o máximo quando definido)
                    if (linha.QuantidadeMaxima > 0)
                    {
                        linha.PercentualUtilizacao =
                            (decimal)linha.EstoqueAtual / (decimal)linha.QuantidadeMaxima * 100m;
                    }
                    else
                    {
                        linha.PercentualUtilizacao = 0m;
                    }

                    // ✅ CORREÇÃO: Usar o serviço de cálculo padronizado
                    linha.StatusEstoque = _estoqueCalculoService.DeterminarStatusEstoqueLinhas(
                        linha.EstoqueAtual, 
                        linha.QuantidadeMinima, 
                        linha.QuantidadeMaxima
                    );
                }
            }

            return linhas;
        }

        public async Task<EstoqueMinimoLinha> BuscarPorId(int id)
        {
            return await _context.EstoqueMinimoLinhas
                .Include(e => e.ClienteNavigation)
                .Include(e => e.OperadoraNavigation)
                .Include(e => e.PlanoNavigation)
                .ThenInclude(p => p.ContratoNavigation)
                .Include(e => e.LocalidadeNavigation)
                .Include(e => e.UsuarioCriacaoNavigation)
                .Include(e => e.UsuarioAtualizacaoNavigation)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<EstoqueLinhaAlertaVM>> ListarAlertasLinhas(int clienteId)
        {
            // Implementação usando consulta LINQ em vez de SQL raw
            var alertas = await _context.EstoqueMinimoLinhas
                .Include(e => e.OperadoraNavigation)
                .Include(e => e.PlanoNavigation)
                .ThenInclude(p => p.ContratoNavigation)
                .Include(e => e.LocalidadeNavigation)
                .Where(e => e.Cliente == clienteId && e.Ativo)
                .Select(e => new EstoqueLinhaAlertaVM
                {
                    Cliente = e.Cliente,
                    Localidade = e.LocalidadeNavigation.Descricao,
                    Operadora = e.OperadoraNavigation.Nome,
                    Contrato = e.PlanoNavigation.ContratoNavigation.Nome,
                    Plano = e.PlanoNavigation.Nome,
                    PerfilUso = e.PerfilUso ?? "Não definido",
                    EstoqueAtual = 0, // Será calculado via view
                    EstoqueMinimo = e.QuantidadeMinima,
                    QuantidadeFaltante = 0, // Será calculado via view
                    Status = "ALERTA" // Será calculado via view
                })
                .ToListAsync();

            return alertas;
        }

        public async Task<int> ContarAlertasLinhas(int clienteId)
        {
            return await _context.EstoqueMinimoLinhas
                .Where(e => e.Cliente == clienteId && e.Ativo)
                .CountAsync();
        }
    }
}
