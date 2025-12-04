using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Models;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Repository.Interfaces;
using SingleOneAPI.Infra.Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios
{
    public class EstoqueMinimoNegocio : IEstoqueMinimoNegocio
    {
        private readonly IEstoqueMinimoEquipamentoRepository _estoqueMinimoEquipamentoRepository;
        private readonly IEstoqueMinimoLinhaRepository _estoqueMinimoLinhaRepository;
        private readonly IRepository<EstoqueMinimoEquipamento> _equipamentoRepository;
        private readonly IRepository<EstoqueMinimoLinha> _linhaRepository;
        private readonly IRepository<Telefoniaplano> _planoRepository;
        private readonly IRepository<Telefoniacontrato> _contratoRepository;

        public EstoqueMinimoNegocio(
            IEstoqueMinimoEquipamentoRepository estoqueMinimoEquipamentoRepository,
            IEstoqueMinimoLinhaRepository estoqueMinimoLinhaRepository,
            IRepository<EstoqueMinimoEquipamento> equipamentoRepository,
            IRepository<EstoqueMinimoLinha> linhaRepository,
            IRepository<Telefoniaplano> planoRepository,
            IRepository<Telefoniacontrato> contratoRepository)
        {
            _estoqueMinimoEquipamentoRepository = estoqueMinimoEquipamentoRepository;
            _estoqueMinimoLinhaRepository = estoqueMinimoLinhaRepository;
            _equipamentoRepository = equipamentoRepository;
            _linhaRepository = linhaRepository;
            _planoRepository = planoRepository;
            _contratoRepository = contratoRepository;
        }

        // =====================================================
        // MÉTODOS PARA EQUIPAMENTOS
        // =====================================================

        public async Task<List<EstoqueMinimoEquipamento>> ListarEquipamentos(int clienteId)
        {
            return await _estoqueMinimoEquipamentoRepository.ListarPorCliente(clienteId);
        }

        public async Task<List<EstoqueMinimoEquipamentoDTO>> ListarEquipamentosComDadosCalculados(int clienteId)
        {
            return await _estoqueMinimoEquipamentoRepository.ListarPorClienteComDadosCalculados(clienteId);
        }

        public async Task<EstoqueMinimoEquipamento> BuscarEquipamento(int id)
        {
            return await _estoqueMinimoEquipamentoRepository.BuscarPorId(id);
        }

        public async Task SalvarEquipamento(EstoqueMinimoEquipamento estoqueMinimo)
        {
            Console.WriteLine($"[NEGOCIO] 🔍 Salvando equipamento - ID: {estoqueMinimo.Id}");
            Console.WriteLine($"[NEGOCIO] 🔍 Cliente: {estoqueMinimo.Cliente}, Modelo: {estoqueMinimo.Modelo}, Localidade: {estoqueMinimo.Localidade}");
            Console.WriteLine($"[NEGOCIO] 🔍 Quantidade Mínima: {estoqueMinimo.QuantidadeMinima}, Máxima: {estoqueMinimo.QuantidadeMaxima}");
            
            if (estoqueMinimo.Id == 0)
            {
                // Novo registro
                Console.WriteLine("[NEGOCIO] 🔍 Criando novo registro...");
                estoqueMinimo.DtCriacao = DateTime.Now;
                estoqueMinimo.Ativo = true;
                _equipamentoRepository.Adicionar(estoqueMinimo);
                Console.WriteLine("[NEGOCIO] ✅ Novo registro criado com sucesso");
            }
            else
            {
                // Atualização - buscar entidade existente primeiro
                Console.WriteLine($"[NEGOCIO] 🔍 Atualizando registro existente ID: {estoqueMinimo.Id}");
                
                var entidadeExistente = await _estoqueMinimoEquipamentoRepository.BuscarPorId(estoqueMinimo.Id);
                if (entidadeExistente == null)
                {
                    throw new InvalidOperationException($"Registro com ID {estoqueMinimo.Id} não encontrado");
                }
                
                // Atualizar apenas os campos necessários
                entidadeExistente.Modelo = estoqueMinimo.Modelo;
                entidadeExistente.Localidade = estoqueMinimo.Localidade;
                entidadeExistente.QuantidadeMinima = estoqueMinimo.QuantidadeMinima;
                entidadeExistente.QuantidadeMaxima = estoqueMinimo.QuantidadeMaxima;
                entidadeExistente.Observacoes = estoqueMinimo.Observacoes;
                entidadeExistente.DtAtualizacao = DateTime.Now;
                entidadeExistente.UsuarioAtualizacao = estoqueMinimo.UsuarioAtualizacao;
                
                Console.WriteLine("[NEGOCIO] 🔍 Campos atualizados, salvando...");
                _equipamentoRepository.Atualizar(entidadeExistente);
                Console.WriteLine("[NEGOCIO] ✅ Registro atualizado com sucesso");
            }
        }

        public async Task ExcluirEquipamento(int id)
        {
            var estoqueMinimo = await _estoqueMinimoEquipamentoRepository.BuscarPorId(id);
            if (estoqueMinimo != null)
            {
                estoqueMinimo.Ativo = false;
                estoqueMinimo.DtAtualizacao = DateTime.Now;
                _equipamentoRepository.Atualizar(estoqueMinimo);
            }
        }

        // =====================================================
        // MÉTODOS PARA LINHAS TELEFÔNICAS
        // =====================================================

        public async Task<List<EstoqueMinimoLinha>> ListarLinhas(int clienteId)
        {
            return await _estoqueMinimoLinhaRepository.ListarPorCliente(clienteId);
        }

        public async Task<EstoqueMinimoLinha> BuscarLinha(int id)
        {
            return await _estoqueMinimoLinhaRepository.BuscarPorId(id);
        }

        public async Task SalvarLinha(EstoqueMinimoLinha estoqueMinimo)
        {
            try
            {
                Console.WriteLine($"[NEGOCIO] 🔍 Salvando linha - ID: {estoqueMinimo.Id}");
                Console.WriteLine($"[NEGOCIO] 🔍 Cliente: {estoqueMinimo.Cliente}, Plano: {estoqueMinimo.Plano}, Localidade: {estoqueMinimo.Localidade}");
                Console.WriteLine($"[NEGOCIO] 🔍 Quantidade Mínima: {estoqueMinimo.QuantidadeMinima}, Máxima: {estoqueMinimo.QuantidadeMaxima}");

                // Validar campos obrigatórios
                if (estoqueMinimo.Cliente <= 0)
                    throw new InvalidOperationException("Cliente inválido");
                if (estoqueMinimo.Plano <= 0)
                    throw new InvalidOperationException("Plano inválido");
                if (estoqueMinimo.Localidade <= 0)
                    throw new InvalidOperationException("Localidade inválida");

                // Preencher Operadora automaticamente a partir do Plano → Contrato → Operadora
                if (estoqueMinimo.Operadora == 0)
                {
                    var planoEntidade = _planoRepository.ObterPorId(estoqueMinimo.Plano);
                    if (planoEntidade == null)
                        throw new InvalidOperationException($"Plano {estoqueMinimo.Plano} não encontrado");

                    var contratoEntidade = _contratoRepository.ObterPorId(planoEntidade.Contrato);
                    if (contratoEntidade == null)
                        throw new InvalidOperationException($"Contrato {planoEntidade.Contrato} não encontrado para o plano {planoEntidade.Id}");

                    estoqueMinimo.Operadora = contratoEntidade.Operadora;
                    Console.WriteLine($"[NEGOCIO] 🔍 Operadora inferida: {estoqueMinimo.Operadora}");
                }

                if (estoqueMinimo.Id == 0)
                {
                    // Novo registro
                    estoqueMinimo.DtCriacao = DateTime.Now;
                    estoqueMinimo.Ativo = true;
                    _linhaRepository.Adicionar(estoqueMinimo);
                }
                else
                {
                    // Atualização
                    estoqueMinimo.DtAtualizacao = DateTime.Now;
                    _linhaRepository.Atualizar(estoqueMinimo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NEGOCIO] ❌ Erro ao salvar linha: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[NEGOCIO] ❌ Inner: {ex.InnerException.Message}");
                    Console.WriteLine($"[NEGOCIO] ❌ Inner Stack: {ex.InnerException.StackTrace}");
                }
                throw;
            }
        }

        public async Task ExcluirLinha(int id)
        {
            var estoqueMinimo = await _estoqueMinimoLinhaRepository.BuscarPorId(id);
            if (estoqueMinimo != null)
            {
                estoqueMinimo.Ativo = false;
                estoqueMinimo.DtAtualizacao = DateTime.Now;
                _linhaRepository.Atualizar(estoqueMinimo);
            }
        }

        // =====================================================
        // MÉTODOS PARA RELATÓRIOS E ALERTAS
        // =====================================================

        public async Task<List<EstoqueAlertaVM>> ListarAlertas(int clienteId)
        {
            return await _estoqueMinimoEquipamentoRepository.ListarAlertasConsolidados(clienteId);
        }

        public async Task<List<EstoqueEquipamentoAlertaVM>> ListarAlertasEquipamentos(int clienteId)
        {
            return await _estoqueMinimoEquipamentoRepository.ListarAlertasEquipamentos(clienteId);
        }

        public async Task<List<EstoqueLinhaAlertaVM>> ListarAlertasLinhas(int clienteId)
        {
            return await _estoqueMinimoLinhaRepository.ListarAlertasLinhas(clienteId);
        }

        public async Task<int> ContarAlertas(int clienteId)
        {
            var alertasEquipamentos = await _estoqueMinimoEquipamentoRepository.ContarAlertasEquipamentos(clienteId);
            var alertasLinhas = await _estoqueMinimoLinhaRepository.ContarAlertasLinhas(clienteId);
            return alertasEquipamentos + alertasLinhas;
        }
    }
}
