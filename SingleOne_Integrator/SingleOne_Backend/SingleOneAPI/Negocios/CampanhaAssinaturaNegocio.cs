using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingleOneAPI.Negocios
{
    public class CampanhaAssinaturaNegocio : ICampanhaAssinaturaNegocio
    {
        private readonly IRepository<CampanhaAssinatura> _campanhaRepository;
        private readonly IRepository<CampanhaColaborador> _campanhaColaboradorRepository;
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IRepository<Empresa> _empresaRepository;
        private readonly IRepository<Localidade> _localidadeRepository;
        private readonly IColaboradorNegocio _colaboradorNegocio;
        private readonly IRepository<Equipamentohistorico> _equipamentoHistoricoRepository;

        public CampanhaAssinaturaNegocio(
            IRepository<CampanhaAssinatura> campanhaRepository,
            IRepository<CampanhaColaborador> campanhaColaboradorRepository,
            IRepository<Colaboradore> colaboradorRepository,
            IRepository<Usuario> usuarioRepository,
            IRepository<Empresa> empresaRepository,
            IRepository<Localidade> localidadeRepository,
            IColaboradorNegocio colaboradorNegocio,
            IRepository<Equipamentohistorico> equipamentoHistoricoRepository)
        {
            _campanhaRepository = campanhaRepository;
            _campanhaColaboradorRepository = campanhaColaboradorRepository;
            _colaboradorRepository = colaboradorRepository;
            _usuarioRepository = usuarioRepository;
            _empresaRepository = empresaRepository;
            _localidadeRepository = localidadeRepository;
            _colaboradorNegocio = colaboradorNegocio;
            _equipamentoHistoricoRepository = equipamentoHistoricoRepository;
        }

        // ==================== CRUD BÁSICO ====================

        public CampanhaAssinatura CriarCampanha(CampanhaAssinatura campanha, List<int> colaboradoresIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(campanha.Nome))
                    throw new ArgumentException("Nome da campanha é obrigatório");

                if (colaboradoresIds == null || colaboradoresIds.Count == 0)
                    throw new ArgumentException("É necessário selecionar pelo menos um colaborador");

                // Criar campanha
                campanha.DataCriacao = DateTime.Now;
                campanha.Status = 'A'; // Ativa
                campanha.TotalColaboradores = colaboradoresIds.Count;
                campanha.TotalEnviados = 0;
                campanha.TotalAssinados = 0;
                campanha.TotalPendentes = colaboradoresIds.Count;
                campanha.PercentualAdesao = 0;

                var resultado = _campanhaRepository.ExecuteInTransaction(() =>
                {
                    _campanhaRepository.AdicionarSemSalvar(campanha);
                    _campanhaRepository.SalvarAlteracoes();
                    
                    foreach (var colaboradorId in colaboradoresIds)
                    {
                        var campanhaColaborador = new CampanhaColaborador
                        {
                            CampanhaId = campanha.Id,
                            ColaboradorId = colaboradorId,
                            DataInclusao = DateTime.Now,
                            StatusAssinatura = 'P',
                            TotalEnvios = 0
                        };
                        _campanhaColaboradorRepository.AdicionarSemSalvar(campanhaColaborador);
                    }
                    
                    return campanha;
                });

                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CAMPANHA-NEGOCIO] Erro ao criar campanha: {ex.Message}");
                throw;
            }
        }

        public CampanhaAssinatura ObterCampanhaPorId(int id)
        {
            return _campanhaRepository.ObterPorId(id);
        }

        public List<CampanhaAssinatura> ListarCampanhasPorCliente(int clienteId, char? status = null)
        {
            var query = _campanhaRepository.Query().Where(c => c.Cliente == clienteId);

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return query.OrderByDescending(c => c.DataCriacao).ToList();
        }

        public void AtualizarCampanha(CampanhaAssinatura campanha)
        {
            _campanhaRepository.Atualizar(campanha);
        }

        public void InativarCampanha(int id)
        {
            var campanha = _campanhaRepository.ObterPorId(id);
            if (campanha != null)
            {
                campanha.Status = 'I'; // Inativa
                _campanhaRepository.Atualizar(campanha);
            }
        }

        public void ConcluirCampanha(int id)
        {
            var campanha = _campanhaRepository.ObterPorId(id);
            if (campanha != null)
            {
                campanha.Status = 'C'; // Concluída
                campanha.DataConclusao = DateTime.Now;
                _campanhaRepository.Atualizar(campanha);
            }
        }

        // ==================== GERENCIAMENTO DE COLABORADORES ====================

        public void AdicionarColaboradoresNaCampanha(int campanhaId, List<int> colaboradoresIds)
        {
            foreach (var colaboradorId in colaboradoresIds)
            {
                // Verificar se já existe
                var existe = _campanhaColaboradorRepository.Query()
                    .Any(cc => cc.CampanhaId == campanhaId && cc.ColaboradorId == colaboradorId);

                if (!existe)
                {
                    var campanhaColaborador = new CampanhaColaborador
                    {
                        CampanhaId = campanhaId,
                        ColaboradorId = colaboradorId,
                        DataInclusao = DateTime.Now,
                        StatusAssinatura = 'P',
                        TotalEnvios = 0
                    };
                    _campanhaColaboradorRepository.Adicionar(campanhaColaborador);
                }
            }

            AtualizarEstatisticasCampanha(campanhaId);
        }

        public void RemoverColaboradorDaCampanha(int campanhaId, int colaboradorId)
        {
            var campanhaColaborador = _campanhaColaboradorRepository.Query()
                .FirstOrDefault(cc => cc.CampanhaId == campanhaId && cc.ColaboradorId == colaboradorId);

            if (campanhaColaborador != null)
            {
                _campanhaColaboradorRepository.Remover(campanhaColaborador);
                AtualizarEstatisticasCampanha(campanhaId);
            }
        }

        public List<CampanhaColaborador> ObterColaboradoresDaCampanha(int campanhaId, char? statusAssinatura = null)
        {
            var query = _campanhaColaboradorRepository.Query()
                .Include(cc => cc.Colaborador)
                    .ThenInclude(c => c.EmpresaNavigation)
                .Include(cc => cc.Colaborador)
                    .ThenInclude(c => c.LocalidadeNavigation)
                .Include(cc => cc.Colaborador)
                    .ThenInclude(c => c.Filial)
                .Include(cc => cc.Colaborador)
                    .ThenInclude(c => c.CentrocustoNavigation)
                .Where(cc => cc.CampanhaId == campanhaId);

            if (statusAssinatura.HasValue)
            {
                query = query.Where(cc => cc.StatusAssinatura == statusAssinatura.Value);
            }

            return query.ToList();
        }

        // ==================== ENVIO DE TERMOS ====================

        public bool EnviarTermoParaColaborador(int campanhaId, int colaboradorId, int usuarioEnvioId, string ip, string localizacao)
        {
            try
            {
                var campanhaColaborador = _campanhaColaboradorRepository.Query()
                    .FirstOrDefault(cc => cc.CampanhaId == campanhaId && cc.ColaboradorId == colaboradorId);

                if (campanhaColaborador == null)
                    return false;

                var campanha = _campanhaRepository.Query()
                    .FirstOrDefault(c => c.Id == campanhaId);

                if (campanha == null)
                    return false;

                var resultado = _colaboradorNegocio.TermoPorEmail(
                    campanha.Cliente, 
                    colaboradorId, 
                    usuarioEnvioId, 
                    false
                ).GetAwaiter().GetResult();

                if (string.IsNullOrEmpty(resultado))
                    return false;

                campanhaColaborador.StatusAssinatura = 'E';
                campanhaColaborador.DataEnvio = campanhaColaborador.DataEnvio ?? DateTime.Now;
                campanhaColaborador.DataUltimoEnvio = DateTime.Now;
                campanhaColaborador.TotalEnvios = (campanhaColaborador.TotalEnvios ?? 0) + 1;
                campanhaColaborador.IpEnvio = ip;
                campanhaColaborador.LocalizacaoEnvio = localizacao;

                _campanhaColaboradorRepository.Atualizar(campanhaColaborador);

                AtualizarEstatisticasCampanha(campanhaId);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CAMPANHA-ENVIO] Erro ao enviar termo para colaborador {colaboradorId}: {ex.Message}");
                return false;
            }
        }

        public bool EnviarTermosEmMassa(int campanhaId, List<int> colaboradoresIds, int usuarioEnvioId, string ip, string localizacao)
        {
            int falhas = 0;

            foreach (var colaboradorId in colaboradoresIds)
            {
                if (!EnviarTermoParaColaborador(campanhaId, colaboradorId, usuarioEnvioId, ip, localizacao))
                {
                    falhas++;
                }
            }

            return falhas == 0;
        }

        // ==================== ATUALIZAÇÃO DE STATUS ====================

        public void MarcarComoAssinado(int campanhaId, int colaboradorId)
        {
            var campanhaColaborador = _campanhaColaboradorRepository.Query()
                .FirstOrDefault(cc => cc.CampanhaId == campanhaId && cc.ColaboradorId == colaboradorId);

            if (campanhaColaborador != null)
            {
                campanhaColaborador.StatusAssinatura = 'A'; // Assinado
                campanhaColaborador.DataAssinatura = DateTime.Now;
                _campanhaColaboradorRepository.Atualizar(campanhaColaborador);

                AtualizarEstatisticasCampanha(campanhaId);
            }
        }

        public void AtualizarEstatisticasCampanha(int campanhaId)
        {
            var campanha = _campanhaRepository.ObterPorId(campanhaId);
            if (campanha == null) return;

            var colaboradores = _campanhaColaboradorRepository.Query()
                .Where(cc => cc.CampanhaId == campanhaId)
                .ToList();

            var colaboradoresComRecursosAtivos = colaboradores
                .Where(cc => ColaboradorPossuiRecursosAtivos(cc.ColaboradorId))
                .ToList();

            campanha.TotalColaboradores = colaboradoresComRecursosAtivos.Count;
            campanha.TotalEnviados = colaboradoresComRecursosAtivos.Count(cc => cc.StatusAssinatura == 'E' || cc.StatusAssinatura == 'A');
            campanha.TotalAssinados = colaboradoresComRecursosAtivos.Count(cc => cc.StatusAssinatura == 'A');
            campanha.TotalPendentes = colaboradoresComRecursosAtivos.Count(cc => cc.StatusAssinatura == 'P' || cc.StatusAssinatura == 'E');

            if (campanha.TotalColaboradores > 0)
            {
                campanha.PercentualAdesao = Math.Round(((decimal)campanha.TotalAssinados / campanha.TotalColaboradores) * 100, 2);
            }
            else
            {
                campanha.PercentualAdesao = null;
            }

            campanha.DataUltimoEnvio = colaboradores
                .Where(cc => cc.DataUltimoEnvio.HasValue)
                .OrderByDescending(cc => cc.DataUltimoEnvio)
                .Select(cc => cc.DataUltimoEnvio)
                .FirstOrDefault();

            _campanhaRepository.Atualizar(campanha);
        }

        // ==================== RELATÓRIOS ====================

        public CampanhaResumoDTO ObterResumoCampanha(int campanhaId)
        {
            var campanha = _campanhaRepository.Query()
                .Include(c => c.UsuarioCriacaoNavigation)
                .FirstOrDefault(c => c.Id == campanhaId);

            if (campanha == null) return null;

            return new CampanhaResumoDTO
            {
                Id = campanha.Id,
                Nome = campanha.Nome,
                Descricao = campanha.Descricao,
                DataCriacao = campanha.DataCriacao,
                DataInicio = campanha.DataInicio,
                DataFim = campanha.DataFim,
                Status = campanha.Status,
                StatusDescricao = ObterDescricaoStatus(campanha.Status),
                UsuarioCriacaoNome = campanha.UsuarioCriacaoNavigation?.Nome,
                TotalColaboradores = campanha.TotalColaboradores,
                TotalEnviados = campanha.TotalEnviados,
                TotalAssinados = campanha.TotalAssinados,
                TotalPendentes = campanha.TotalPendentes,
                PercentualAdesao = campanha.PercentualAdesao,
                DataUltimoEnvio = campanha.DataUltimoEnvio,
                DataConclusao = campanha.DataConclusao,
                FiltrosJson = campanha.FiltrosJson
            };
        }

        public List<CampanhaResumoDTO> ObterResumoCampanhasPorCliente(int clienteId)
        {
            var campanhas = _campanhaRepository.Query()
                .Include(c => c.UsuarioCriacaoNavigation)
                .Where(c => c.Cliente == clienteId)
                .OrderByDescending(c => c.DataCriacao)
                .ToList();

            return campanhas.Select(c => new CampanhaResumoDTO
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao,
                DataCriacao = c.DataCriacao,
                DataInicio = c.DataInicio,
                DataFim = c.DataFim,
                Status = c.Status,
                StatusDescricao = ObterDescricaoStatus(c.Status),
                UsuarioCriacaoNome = c.UsuarioCriacaoNavigation?.Nome,
                TotalColaboradores = c.TotalColaboradores,
                TotalEnviados = c.TotalEnviados,
                TotalAssinados = c.TotalAssinados,
                TotalPendentes = c.TotalPendentes,
                PercentualAdesao = c.PercentualAdesao,
                DataUltimoEnvio = c.DataUltimoEnvio,
                DataConclusao = c.DataConclusao,
                FiltrosJson = c.FiltrosJson
            }).ToList();
        }

        public RelatorioAderenciaDTO ObterRelatorioAderencia(int campanhaId)
        {
            var campanha = _campanhaRepository.ObterPorId(campanhaId);
            if (campanha == null) return null;

            var colaboradoresCampanha = _campanhaColaboradorRepository.Query()
                .Include(cc => cc.Colaborador)
                    .ThenInclude(c => c.EmpresaNavigation)
                .Include(cc => cc.Colaborador)
                    .ThenInclude(c => c.LocalidadeNavigation)
                .Where(cc => cc.CampanhaId == campanhaId)
                .ToList();

            var colaboradores = colaboradoresCampanha
                .Where(cc => ColaboradorPossuiRecursosAtivos(cc.ColaboradorId))
                .ToList();

            var totalRecusados = colaboradores.Count(cc => cc.StatusAssinatura == 'R');
            var total = colaboradores.Count;

            var relatorio = new RelatorioAderenciaDTO
            {
                CampanhaId = campanha.Id,
                CampanhaNome = campanha.Nome,
                DataCriacao = campanha.DataCriacao,
                TotalColaboradores = total,
                TotalEnviados = campanha.TotalEnviados,
                TotalAssinados = campanha.TotalAssinados,
                TotalPendentes = campanha.TotalPendentes,
                TotalRecusados = totalRecusados,
                PercentualAdesao = campanha.PercentualAdesao ?? 0,
                PercentualPendente = total > 0 ? Math.Round(((decimal)campanha.TotalPendentes / total) * 100, 2) : 0,
                PercentualRecusado = total > 0 ? Math.Round(((decimal)totalRecusados / total) * 100, 2) : 0,
                AderenciaPorEmpresa = new List<AderenciaPorEmpresaDTO>(),
                AderenciaPorLocalidade = new List<AderenciaPorLocalidadeDTO>(),
                AderenciaPorTipo = new List<AderenciaPorTipoDTO>(),
                TimelineEnvios = new List<EnvioPorDiaDTO>()
            };

            // Estatísticas por empresa
            var porEmpresa = colaboradores
                .GroupBy(cc => cc.Colaborador.EmpresaNavigation?.Nome ?? "Sem Empresa")
                .Select(g => new AderenciaPorEmpresaDTO
                {
                    EmpresaNome = g.Key,
                    Total = g.Count(),
                    Assinados = g.Count(cc => cc.StatusAssinatura == 'A'),
                    Pendentes = g.Count(cc => cc.StatusAssinatura == 'P' || cc.StatusAssinatura == 'E'),
                    PercentualAdesao = g.Count() > 0 ? Math.Round(((decimal)g.Count(cc => cc.StatusAssinatura == 'A') / g.Count()) * 100, 2) : 0
                }).ToList();

            relatorio.AderenciaPorEmpresa = porEmpresa;

            // Estatísticas por localidade
            var porLocalidade = colaboradores
                .GroupBy(cc => cc.Colaborador.LocalidadeNavigation?.Descricao ?? "Sem Localidade")
                .Select(g => new AderenciaPorLocalidadeDTO
                {
                    LocalidadeNome = g.Key,
                    Total = g.Count(),
                    Assinados = g.Count(cc => cc.StatusAssinatura == 'A'),
                    Pendentes = g.Count(cc => cc.StatusAssinatura == 'P' || cc.StatusAssinatura == 'E'),
                    PercentualAdesao = g.Count() > 0 ? Math.Round(((decimal)g.Count(cc => cc.StatusAssinatura == 'A') / g.Count()) * 100, 2) : 0
                }).ToList();

            relatorio.AderenciaPorLocalidade = porLocalidade;

            // Estatísticas por tipo
            var porTipo = colaboradores
                .GroupBy(cc => ObterDescricaoTipoColaborador(cc.Colaborador.Tipocolaborador))
                .Select(g => new AderenciaPorTipoDTO
                {
                    TipoColaborador = g.Key,
                    Total = g.Count(),
                    Assinados = g.Count(cc => cc.StatusAssinatura == 'A'),
                    Pendentes = g.Count(cc => cc.StatusAssinatura == 'P' || cc.StatusAssinatura == 'E'),
                    PercentualAdesao = g.Count() > 0 ? Math.Round(((decimal)g.Count(cc => cc.StatusAssinatura == 'A') / g.Count()) * 100, 2) : 0
                }).ToList();

            relatorio.AderenciaPorTipo = porTipo;

            return relatorio;
        }

        public List<ColaboradorPendenteDTO> ObterColaboradoresPendentes(int campanhaId)
        {
            var colaboradoresCampanha = _campanhaColaboradorRepository.Query()
                .Include(cc => cc.Colaborador)
                    .ThenInclude(c => c.EmpresaNavigation)
                .Include(cc => cc.Colaborador)
                    .ThenInclude(c => c.LocalidadeNavigation)
                .Where(cc => cc.CampanhaId == campanhaId && (cc.StatusAssinatura == 'P' || cc.StatusAssinatura == 'E'))
                .ToList();

            var colaboradores = colaboradoresCampanha
                .Where(cc => ColaboradorPossuiRecursosAtivos(cc.ColaboradorId))
                .ToList();

            return colaboradores.Select(cc => new ColaboradorPendenteDTO
            {
                ColaboradorId = cc.ColaboradorId,
                ColaboradorNome = cc.Colaborador.Nome,
                ColaboradorCpf = cc.Colaborador.Cpf,
                ColaboradorEmail = cc.Colaborador.Email,
                ColaboradorCargo = cc.Colaborador.Cargo,
                EmpresaNome = cc.Colaborador.EmpresaNavigation?.Nome,
                LocalidadeNome = cc.Colaborador.LocalidadeNavigation?.Descricao,
                StatusAssinatura = cc.StatusAssinatura,
                StatusAssinaturaDescricao = ObterDescricaoStatusAssinatura(cc.StatusAssinatura),
                DataInclusao = cc.DataInclusao,
                DataEnvio = cc.DataEnvio,
                DataUltimoEnvio = cc.DataUltimoEnvio,
                TotalEnvios = cc.TotalEnvios,
                DiasDesdeEnvio = cc.DataUltimoEnvio.HasValue ? (DateTime.Now - cc.DataUltimoEnvio.Value).Days : 0
            }).ToList();
        }

        // ==================== MÉTODOS AUXILIARES ====================

        /// <summary>
        /// Verifica se um colaborador possui recursos ativos (não devolvidos) usando equipamentohistorico
        /// Status 4 = Entregue (ATIVO)
        /// Status 5 = Devolvido (INATIVO)
        /// </summary>
        private bool ColaboradorPossuiRecursosAtivos(int colaboradorId)
        {
            try
            {
                var equipamentosDoColaborador = _equipamentoHistoricoRepository.Query()
                    .Where(eh => eh.Colaborador == colaboradorId)
                    .Select(eh => eh.Equipamento)
                    .Distinct()
                    .ToList();

                if (!equipamentosDoColaborador.Any())
                    return false;

                var equipamentosAtivos = 0;

                foreach (var equipamentoId in equipamentosDoColaborador)
                {
                    var ultimoRegistro = _equipamentoHistoricoRepository.Query()
                        .Where(eh => eh.Equipamento == equipamentoId && eh.Colaborador == colaboradorId)
                        .OrderByDescending(eh => eh.Dtregistro)
                        .FirstOrDefault();

                    if (ultimoRegistro != null && ultimoRegistro.Equipamentostatus == 4)
                    {
                        equipamentosAtivos++;
                    }
                }

                return equipamentosAtivos > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CAMPANHA-NEGOCIO] Erro ao verificar recursos do colaborador {colaboradorId}: {ex.Message}");
                return true;
            }
        }

        private string ObterDescricaoStatus(char status)
        {
            return status switch
            {
                'A' => "Ativa",
                'I' => "Inativa",
                'C' => "Concluída",
                'G' => "Agendada",
                _ => "Desconhecido"
            };
        }

        private string ObterDescricaoStatusAssinatura(char status)
        {
            return status switch
            {
                'P' => "Pendente",
                'E' => "Enviado",
                'A' => "Assinado",
                'R' => "Recusado",
                _ => "Desconhecido"
            };
        }

        private string ObterDescricaoTipoColaborador(char tipo)
        {
            return tipo switch
            {
                'E' => "Efetivo",
                'T' => "Terceiro",
                'P' => "Prestador de Serviço",
                'A' => "Autônomo",
                _ => "Outro"
            };
        }
    }
}

