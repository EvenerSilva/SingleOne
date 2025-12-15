using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SingleOneAPI.Infra.Contexto;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SingleOne.Util;

namespace SingleOneAPI.Negocios
{
    public class ImportacaoColaboradoresNegocio : IImportacaoColaboradoresNegocio
    {
        private readonly SingleOneDbContext _context;
        private readonly ILogger<ImportacaoColaboradoresNegocio> _logger;

        public ImportacaoColaboradoresNegocio(
            SingleOneDbContext context,
            ILogger<ImportacaoColaboradoresNegocio> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ResultadoValidacaoColaboradoresDTO> ProcessarArquivo(IFormFile arquivo, int clienteId, int usuarioId)
        {
            _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Iniciando processamento de arquivo: {arquivo.FileName}");

            try
            {
                // Validação inicial do arquivo
                if (arquivo == null || arquivo.Length == 0)
                    throw new Exception("Arquivo vazio ou não enviado");

                if (!arquivo.FileName.EndsWith(".xlsx") && !arquivo.FileName.EndsWith(".xls"))
                    throw new Exception("Formato de arquivo inválido. Apenas arquivos Excel (.xlsx, .xls) são aceitos");

                if (arquivo.Length > 10 * 1024 * 1024) // 10MB
                    throw new Exception("Arquivo muito grande. Limite máximo: 10MB");

                var loteId = Guid.NewGuid();
                var dataImportacao = DateTime.Now;

                // Ler dados do Excel
                var colaboradoresArquivo = await LerArquivoExcel(arquivo);
                
                if (colaboradoresArquivo.Count == 0)
                    throw new Exception("Arquivo não contém dados válidos");

                if (colaboradoresArquivo.Count > 5000)
                    throw new Exception("Arquivo excede o limite de 5000 linhas por importação");

                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] {colaboradoresArquivo.Count} linhas lidas do arquivo");

                // Inserir dados na staging
                var registrosStaging = new List<ImportacaoColaboradorStaging>();
                
                foreach (var (colaborador, index) in colaboradoresArquivo.Select((c, i) => (c, i)))
                {
                    registrosStaging.Add(new ImportacaoColaboradorStaging
                    {
                        LoteId = loteId,
                        Cliente = clienteId,
                        UsuarioImportacao = usuarioId,
                        DataImportacao = dataImportacao,
                        
                        // Dados do colaborador
                        NomeColaborador = colaborador.Nome?.Trim(),
                        Cpf = LimparCpf(colaborador.Cpf),
                        Matricula = colaborador.Matricula?.Trim(),
                        Email = colaborador.Email?.Trim(),
                        Cargo = colaborador.Cargo?.Trim(),
                        Setor = colaborador.Setor?.Trim(),
                        DataAdmissao = colaborador.DataAdmissao,
                        TipoColaborador = colaborador.TipoColaborador?.Trim().ToUpper(),
                        DataDemissao = colaborador.DataDemissao,
                        MatriculaSuperior = colaborador.MatriculaSuperior?.Trim(),
                        
                        // Dados relacionados
                        EmpresaNome = colaborador.EmpresaNome?.Trim(),
                        EmpresaCnpj = LimparCnpj(colaborador.EmpresaCnpj),
                        LocalidadeDescricao = colaborador.LocalidadeDescricao?.Trim(),
                        LocalidadeCidade = colaborador.LocalidadeCidade?.Trim(),
                        LocalidadeEstado = colaborador.LocalidadeEstado?.Trim().ToUpper(),
                        CentroCustoCodigo = colaborador.CentroCustoCodigo?.Trim(),
                        CentroCustoNome = colaborador.CentroCustoNome?.Trim(),
                        FilialNome = colaborador.FilialNome?.Trim(),
                        FilialCnpj = LimparCnpj(colaborador.FilialCnpj),
                        
                        Status = "P", // Pendente
                        LinhaArquivo = index + 2, // Excel começa na linha 2 (1 é header)
                        MensagensValidacao = "{}"
                    });
                }

                await _context.ImportacaoColaboradoresStaging.AddRangeAsync(registrosStaging);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] {registrosStaging.Count} registros inseridos na staging");

                // Executar validações
                await ValidarLote(loteId, clienteId);

                // Gerar resumo
                var resumo = await ObterResumoValidacao(loteId, clienteId);

                // Registrar log consolidado da validação
                var statusPosValidacao = resumo.Erros == 0 ? "VALIDADO" : "PENDENTE_CORRECAO";

                var log = new ImportacaoLog
                {
                    LoteId = loteId,
                    Cliente = clienteId,
                    Usuario = usuarioId,
                    TipoImportacao = "COLABORADORES",
                    DataInicio = dataImportacao,
                    Status = statusPosValidacao,
                    NomeArquivo = arquivo.FileName,
                    TotalRegistros = resumo.Total,
                    TotalValidados = resumo.Validos,
                    TotalErros = resumo.Erros,
                    TotalImportados = 0,
                    Observacoes = statusPosValidacao == "VALIDADO"
                        ? "Validação concluída. Lote aguardando confirmação para efetivar as alterações."
                        : "Validação concluída com pendências. Corrija os erros indicados antes de confirmar."
                };

                _context.ImportacaoLogs.Add(log);
                await _context.SaveChangesAsync();

                var resultadoDTO = new ResultadoValidacaoColaboradoresDTO
                {
                    LoteId = loteId,
                    TotalRegistros = resumo.Total,
                    TotalValidos = resumo.Validos,
                    TotalAvisos = resumo.Avisos,
                    TotalErros = resumo.Erros,
                    NovasEmpresas = resumo.NovasEmpresas,
                    NovasLocalidades = resumo.NovasLocalidades,
                    NovoscentrosCusto = resumo.NovosCentrosCusto,
                    NovasFiliais = resumo.NovasFiliais,
                    TotalAtualizacoes = resumo.TotalAtualizacoes,
                    TotalSemAlteracao = resumo.TotalSemAlteracao,
                    TotalNovos = resumo.TotalNovos,
                    PodeImportar = resumo.Erros == 0,
                    Mensagem = resumo.Erros == 0 
                        ? "Validação concluída com sucesso. Todos os registros estão prontos para importação."
                        : $"Validação concluída com {resumo.Erros} erro(s). Corrija os erros antes de importar."
                };

                const int LIMITE_ERROS_RESUMO = 25;
                resultadoDTO.ErrosCriticos = registrosStaging
                    .Where(x => x.Status == "E")
                    .OrderBy(x => x.LinhaArquivo)
                    .Take(LIMITE_ERROS_RESUMO)
                    .Select(x => new ErroValidacaoResumoDTO
                    {
                        Linha = x.LinhaArquivo,
                        Nome = x.NomeColaborador,
                        Cpf = x.Cpf,
                        Matricula = x.Matricula,
                        Mensagens = ExtrairMensagens(x.MensagensValidacao)
                    })
                    .ToList();
                resultadoDTO.PossuiMaisErros = registrosStaging.Count(x => x.Status == "E") > LIMITE_ERROS_RESUMO;

                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Validação concluída - Lote: {loteId}");

                return resultadoDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[IMPORTACAO-COLABORADORES] Erro ao processar arquivo: {ex.Message}");
                throw new Exception($"Erro ao processar arquivo: {ex.Message}");
            }
        }

        private async Task<List<ColaboradorArquivoDTO>> LerArquivoExcel(IFormFile arquivo)
        {
            var colaboradores = new List<ColaboradorArquivoDTO>();

            using (var stream = new MemoryStream())
            {
                await arquivo.CopyToAsync(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.First();
                    
                    // Ler a partir da linha 2 (linha 1 é o cabeçalho)
                    int linhaAtual = 2;
                    
                    while (!worksheet.Cell(linhaAtual, 1).IsEmpty())
                    {
                        try
                        {
                            colaboradores.Add(new ColaboradorArquivoDTO
                            {
                                Nome = worksheet.Cell(linhaAtual, 1).GetString(),
                                Cpf = worksheet.Cell(linhaAtual, 2).GetString(),
                                Matricula = worksheet.Cell(linhaAtual, 3).GetString(),
                                Email = worksheet.Cell(linhaAtual, 4).GetString(),
                                Cargo = worksheet.Cell(linhaAtual, 5).GetString(),
                                Setor = worksheet.Cell(linhaAtual, 6).GetString(),
                                DataAdmissao = TentarParsearData(worksheet.Cell(linhaAtual, 7)),
                                TipoColaborador = worksheet.Cell(linhaAtual, 8).GetString(),
                                EmpresaNome = worksheet.Cell(linhaAtual, 9).GetString(),
                                EmpresaCnpj = worksheet.Cell(linhaAtual, 10).GetString(),
                                LocalidadeDescricao = worksheet.Cell(linhaAtual, 11).GetString(),
                                LocalidadeCidade = worksheet.Cell(linhaAtual, 12).GetString(),
                                LocalidadeEstado = worksheet.Cell(linhaAtual, 13).GetString(),
                                CentroCustoCodigo = worksheet.Cell(linhaAtual, 14).GetString(),
                                CentroCustoNome = worksheet.Cell(linhaAtual, 15).GetString(),
                                FilialNome = worksheet.Cell(linhaAtual, 16).GetString(),
                                FilialCnpj = worksheet.Cell(linhaAtual, 17).GetString(),
                                DataDemissao = TentarParsearData(worksheet.Cell(linhaAtual, 18)),
                                MatriculaSuperior = worksheet.Cell(linhaAtual, 19).GetString()
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"[IMPORTACAO-COLABORADORES] Erro ao ler linha {linhaAtual}: {ex.Message}");
                        }

                        linhaAtual++;
                        
                        // Limitar a 5000 linhas
                        if (linhaAtual > 5001)
                            break;
                    }
                }
            }

            return colaboradores;
        }

        private async Task ValidarLote(Guid loteId, int clienteId)
        {
            _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Iniciando validação do lote: {loteId}");

            var registros = await _context.ImportacaoColaboradoresStaging
                .AsTracking()
                .Where(x => x.LoteId == loteId && x.Cliente == clienteId)
                .ToListAsync();

            // Carregar dados necessários para validação
            var empresas = await _context.Empresas
                .Where(x => x.Cliente == clienteId)
                .ToListAsync();

            var empresaIdsCliente = new HashSet<int>(empresas.Select(x => x.Id));

            var localidades = await _context.Localidades
                .Where(x => x.Cliente == clienteId && x.Ativo)
                .ToListAsync();

            var centrosCusto = await _context.Centrocustos
                .Where(x => x.Ativo && empresaIdsCliente.Contains(x.Empresa))
                .Include(x => x.EmpresaNavigation)
                .ToListAsync();

            var filiais = await _context.Filiais
                .Where(x => x.Ativo == true && empresaIdsCliente.Contains(x.EmpresaId))
                .ToListAsync();

            var colaboradoresExistentesInfo = await _context.Colaboradores
                .Where(x => x.Cliente == clienteId)
                .Select(x => new { x.Cpf, x.Matricula, x.Empresa })
                .ToListAsync();

            string NormalizarCpfBanco(string cpfCriptografado) =>
                LimparCpf(Cripto.CriptografarDescriptografar(cpfCriptografado, false));

            var cpfsExistentes = new HashSet<string>(
                colaboradoresExistentesInfo
                    .Select(x => NormalizarCpfBanco(x.Cpf))
                    .Where(cpf => !string.IsNullOrWhiteSpace(cpf)),
                StringComparer.OrdinalIgnoreCase);

            var matriculasPorEmpresa = new Dictionary<int, Dictionary<string, string>>();

            foreach (var info in colaboradoresExistentesInfo.Where(x => !string.IsNullOrWhiteSpace(x.Matricula)))
            {
                var empresaId = info.Empresa;
                var matriculaKey = info.Matricula.Trim().ToLower();
                var cpfNormalizado = NormalizarCpfBanco(info.Cpf);

                if (!matriculasPorEmpresa.TryGetValue(empresaId, out var matriculasDict))
                {
                    matriculasDict = new Dictionary<string, string>();
                    matriculasPorEmpresa[empresaId] = matriculasDict;
                }

                if (!matriculasDict.ContainsKey(matriculaKey))
                {
                    matriculasDict[matriculaKey] = cpfNormalizado;
                }
            }

            foreach (var registro in registros)
            {
                var erros = new List<string>();
                var avisos = new List<string>();

                // ========== VALIDAÇÃO 1: Campos obrigatórios ==========
                if (string.IsNullOrWhiteSpace(registro.NomeColaborador))
                    erros.Add("Nome é obrigatório");
                
                if (string.IsNullOrWhiteSpace(registro.Cpf))
                    erros.Add("CPF é obrigatório");
                
                if (string.IsNullOrWhiteSpace(registro.Matricula))
                    erros.Add("Matrícula é obrigatória");
                
                if (string.IsNullOrWhiteSpace(registro.Email))
                    erros.Add("Email é obrigatório");
                
                if (string.IsNullOrWhiteSpace(registro.Cargo))
                    erros.Add("Cargo é obrigatório");
                
                if (string.IsNullOrWhiteSpace(registro.Setor))
                    erros.Add("Setor é obrigatório");
                
                if (!registro.DataAdmissao.HasValue)
                    erros.Add("Data de Admissão é obrigatória");
                
                if (string.IsNullOrWhiteSpace(registro.TipoColaborador))
                    erros.Add("Tipo de Colaborador é obrigatório");
                
                if (string.IsNullOrWhiteSpace(registro.EmpresaNome))
                    erros.Add("Nome da Empresa é obrigatório");
                
                if (string.IsNullOrWhiteSpace(registro.EmpresaCnpj))
                    erros.Add("CNPJ da Empresa é obrigatório");
                
                if (string.IsNullOrWhiteSpace(registro.LocalidadeDescricao))
                    erros.Add("Localidade é obrigatória");
                
                if (string.IsNullOrWhiteSpace(registro.LocalidadeCidade))
                    erros.Add("Cidade da Localidade é obrigatória");
                
                if (string.IsNullOrWhiteSpace(registro.LocalidadeEstado))
                    erros.Add("Estado da Localidade é obrigatório");
                
                if (string.IsNullOrWhiteSpace(registro.CentroCustoCodigo))
                    erros.Add("Código do Centro de Custo é obrigatório");
                
                if (string.IsNullOrWhiteSpace(registro.CentroCustoNome))
                    erros.Add("Nome do Centro de Custo é obrigatório");

                // Se faltam campos obrigatórios, pular validações seguintes
                if (erros.Any())
                {
                    registro.Status = "E";
                    registro.MensagensValidacao = JsonConvert.SerializeObject(new { erros, avisos });
                    continue;
                }

                // ========== VALIDAÇÃO 2: Tipo de Colaborador ==========
                if (!new[] { "F", "T", "C" }.Contains(registro.TipoColaborador.ToUpper()))
                {
                    erros.Add("Tipo de Colaborador deve ser F (Funcionário), T (Terceiro) ou C (Consultor)");
                }

                // ========== VALIDAÇÃO 3: Estado ==========
                if (registro.LocalidadeEstado.Length != 2)
                {
                    erros.Add("Estado deve ter exatamente 2 caracteres (sigla UF)");
                }

                // ========== VALIDAÇÃO 4: CPF ==========
                var cpfNormalizado = LimparCpf(registro.Cpf);
                var cpfJaExistente = cpfsExistentes.Contains(cpfNormalizado);
                if (!ValidarCpf(registro.Cpf))
                {
                    erros.Add("CPF inválido");
                }
                else
                {
                    var cpfDuplicadoLote = registros
                        .Where(x => x.Id != registro.Id && x.Cpf == registro.Cpf)
                        .Any();

                    if (cpfDuplicadoLote)
                    {
                        erros.Add("❌ CPF duplicado no arquivo de importação");
                    }
                }

                if (cpfJaExistente)
                {
                    avisos.Add($"♻️ CPF {registro.Cpf} já cadastrado: alterações serão aplicadas");
                }

                // ========== VALIDAÇÃO 5: Email ==========
                if (!ValidarEmail(registro.Email))
                {
                    erros.Add("Email inválido");
                }

                // ========== VALIDAÇÃO 6: CNPJ ==========
                if (!ValidarCnpj(registro.EmpresaCnpj))
                {
                    erros.Add("CNPJ da Empresa inválido");
                }

                if (!string.IsNullOrWhiteSpace(registro.FilialCnpj) && !ValidarCnpj(registro.FilialCnpj))
                {
                    erros.Add("CNPJ da Filial inválido");
                }

                // ========== VALIDAÇÃO 7: Datas ==========
                if (registro.DataDemissao.HasValue && registro.DataAdmissao.HasValue)
                {
                    if (registro.DataDemissao.Value < registro.DataAdmissao.Value)
                    {
                        erros.Add("Data de Demissão deve ser posterior à Data de Admissão");
                    }
                }

                // Se há erros críticos, pular validações de relacionamentos
                if (erros.Any())
                {
                    registro.Status = "E";
                    registro.MensagensValidacao = JsonConvert.SerializeObject(new { erros, avisos });
                    continue;
                }

                // ========== VALIDAÇÃO 8: Empresa ==========
                var empresa = empresas.FirstOrDefault(x => 
                    x.Cnpj.Trim() == registro.EmpresaCnpj.Trim());

                if (empresa != null)
                {
                    registro.EmpresaId = empresa.Id;
                }
                else
                {
                    registro.CriarEmpresa = true;
                    avisos.Add($"✨ Empresa '{registro.EmpresaNome}' será criada automaticamente");
                }

                // ========== VALIDAÇÃO 9: Localidade ==========
                var localidade = localidades.FirstOrDefault(x => 
                    x.Descricao.Trim().ToLower() == registro.LocalidadeDescricao.Trim().ToLower() &&
                    (string.IsNullOrEmpty(x.Cidade) ? "" : x.Cidade.Trim().ToLower()) == (string.IsNullOrEmpty(registro.LocalidadeCidade) ? "" : registro.LocalidadeCidade.Trim().ToLower()) &&
                    (string.IsNullOrEmpty(x.Estado) ? "" : x.Estado.Trim().ToLower()) == (string.IsNullOrEmpty(registro.LocalidadeEstado) ? "" : registro.LocalidadeEstado.Trim().ToLower()));

                if (localidade != null)
                {
                    registro.LocalidadeId = localidade.Id;
                }
                else
                {
                    registro.CriarLocalidade = true;
                    avisos.Add($"✨ Localidade '{registro.LocalidadeDescricao}' será criada automaticamente");
                }

                // ========== VALIDAÇÃO 10: Filial (se informada) ==========
                if (!string.IsNullOrWhiteSpace(registro.FilialNome))
                {
                    if (registro.EmpresaId.HasValue && registro.LocalidadeId.HasValue)
                    {
                        var filial = filiais.FirstOrDefault(x =>
                            x.Nome.Trim().ToLower() == registro.FilialNome.Trim().ToLower() &&
                            x.EmpresaId == registro.EmpresaId.Value &&
                            x.LocalidadeId == registro.LocalidadeId.Value);

                        if (filial != null)
                        {
                            registro.FilialId = filial.Id;
                        }
                        else
                        {
                            registro.CriarFilial = true;
                            avisos.Add($"✨ Filial '{registro.FilialNome}' será criada automaticamente");
                        }
                    }
                    else if (registro.CriarEmpresa || registro.CriarLocalidade)
                    {
                        registro.CriarFilial = true;
                        avisos.Add($"✨ Filial '{registro.FilialNome}' será criada automaticamente");
                    }
                }

                // ========== VALIDAÇÃO 11: Centro de Custo ==========
                if (registro.EmpresaId.HasValue)
                {
                    var centroCusto = centrosCusto.FirstOrDefault(x =>
                        x.Codigo.Trim().ToLower() == registro.CentroCustoCodigo.Trim().ToLower() &&
                        x.Empresa == registro.EmpresaId.Value);

                    if (centroCusto != null)
                    {
                        registro.CentroCustoId = centroCusto.Id;
                    }
                    else
                    {
                        registro.CriarCentroCusto = true;
                        avisos.Add($"✨ Centro de Custo '{registro.CentroCustoCodigo}' será criado automaticamente");
                    }
                }
                else if (registro.CriarEmpresa)
                {
                    registro.CriarCentroCusto = true;
                    avisos.Add($"✨ Centro de Custo '{registro.CentroCustoCodigo}' será criado automaticamente");
                }

                // ========== VALIDAÇÃO 12: Matrícula única por empresa ==========
                if (registro.EmpresaId.HasValue && !string.IsNullOrWhiteSpace(registro.Matricula))
                {
                    var empresaId = registro.EmpresaId.Value;
                    var matriculaKey = registro.Matricula.Trim().ToLower();

                    if (matriculasPorEmpresa.TryGetValue(empresaId, out var matriculasEmpresa) &&
                        matriculasEmpresa.TryGetValue(matriculaKey, out var cpfAssociado))
                    {
                        var mesmaPessoa = string.Equals(cpfAssociado, cpfNormalizado, StringComparison.OrdinalIgnoreCase);
                        if (!mesmaPessoa)
                        {
                            erros.Add($"❌ Matrícula {registro.Matricula} já cadastrada para esta empresa");
                        }
                    }
                }

                var matriculaDuplicadaLote = registros
                    .Where(x => x.Id != registro.Id && 
                                x.Matricula == registro.Matricula && 
                                x.EmpresaCnpj == registro.EmpresaCnpj)
                    .Any();

                if (matriculaDuplicadaLote)
                {
                    erros.Add("❌ Matrícula duplicada no arquivo para a mesma empresa");
                }

                // ========== ATUALIZAR STATUS ==========
                registro.MensagensValidacao = JsonConvert.SerializeObject(new { erros, avisos });

                if (erros.Any())
                {
                    registro.Status = "E"; // Erro
                }
                else if (avisos.Any())
                {
                    registro.Status = "V"; // Validado com avisos
                }
                else
                {
                    registro.Status = "V"; // Validado
                }
            }

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            
            _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Validação concluída para lote: {loteId}");
        }

        public async Task<List<DetalheColaboradorStagingDTO>> ObterDetalhesValidacao(Guid loteId, int clienteId, string filtroStatus = null)
        {
            var query = _context.ImportacaoColaboradoresStaging
                .Where(x => x.LoteId == loteId && x.Cliente == clienteId);

            if (!string.IsNullOrEmpty(filtroStatus))
            {
                query = query.Where(x => x.Status == filtroStatus);
            }

            var registros = await query
                .OrderBy(x => x.LinhaArquivo)
                .ToListAsync();

            var resultado = registros.Select(r =>
            {
                var mensagens = string.IsNullOrEmpty(r.MensagensValidacao)
                    ? new { erros = new List<string>(), avisos = new List<string>() }
                    : JsonConvert.DeserializeObject<dynamic>(r.MensagensValidacao);

                return new DetalheColaboradorStagingDTO
                {
                    Id = r.Id,
                    LinhaArquivo = r.LinhaArquivo,
                    NomeColaborador = r.NomeColaborador,
                    Cpf = r.Cpf,
                    Matricula = r.Matricula,
                    Email = r.Email,
                    Cargo = r.Cargo,
                    Setor = r.Setor,
                    DataAdmissao = r.DataAdmissao,
                    TipoColaborador = r.TipoColaborador,
                    DataDemissao = r.DataDemissao,
                    EmpresaNome = r.EmpresaNome,
                    EmpresaCnpj = r.EmpresaCnpj,
                    LocalidadeDescricao = r.LocalidadeDescricao,
                    LocalidadeCidade = r.LocalidadeCidade,
                    LocalidadeEstado = r.LocalidadeEstado,
                    CentroCustoCodigo = r.CentroCustoCodigo,
                    CentroCustoNome = r.CentroCustoNome,
                    FilialNome = r.FilialNome,
                    Status = r.Status,
                    StatusDescricao = ObterStatusDescricao(r.Status),
                    Erros = mensagens.erros.ToObject<List<string>>(),
                    Avisos = mensagens.avisos.ToObject<List<string>>(),
                    CriarEmpresa = r.CriarEmpresa,
                    CriarLocalidade = r.CriarLocalidade,
                    CriarCentroCusto = r.CriarCentroCusto,
                    CriarFilial = r.CriarFilial
                };
            }).ToList();

            return resultado;
        }

        public async Task<ResumoValidacaoColaboradoresDTO> ObterResumoValidacao(Guid loteId, int clienteId)
        {
            var registros = await _context.ImportacaoColaboradoresStaging
                .Where(x => x.LoteId == loteId && x.Cliente == clienteId)
                .ToListAsync();

            var cpfsLote = registros
                .Select(x => LimparCpf(x.Cpf))
                .Where(cpf => !string.IsNullOrWhiteSpace(cpf))
                .ToHashSet();

            var colaboradoresCliente = await _context.Colaboradores
                .Where(x => x.Cliente == clienteId)
                .ToListAsync();

            var colaboradoresPorCpf = new Dictionary<string, Colaboradore>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in colaboradoresCliente)
            {
                var cpfLimpo = LimparCpf(Cripto.CriptografarDescriptografar(col.Cpf, false));
                if (string.IsNullOrWhiteSpace(cpfLimpo) || !cpfsLote.Contains(cpfLimpo))
                {
                    continue;
                }

                if (!colaboradoresPorCpf.ContainsKey(cpfLimpo))
                {
                    colaboradoresPorCpf[cpfLimpo] = col;
                }
            }

            var resumo = new ResumoValidacaoColaboradoresDTO
            {
                LoteId = loteId,
                Total = registros.Count,
                Validos = registros.Count(x => x.Status == "V"),
                Erros = registros.Count(x => x.Status == "E"),
                Pendentes = registros.Count(x => x.Status == "P"),
                Importados = registros.Count(x => x.Status == "I"),
                NovasEmpresas = registros.Where(x => x.CriarEmpresa).GroupBy(x => x.EmpresaCnpj).Count(),
                NovasLocalidades = registros.Where(x => x.CriarLocalidade).GroupBy(x => new { 
                    x.LocalidadeDescricao, 
                    x.LocalidadeCidade, 
                    x.LocalidadeEstado 
                }).Count(),
                NovosCentrosCusto = registros.Where(x => x.CriarCentroCusto).GroupBy(x => new { 
                    x.EmpresaCnpj, 
                    x.CentroCustoCodigo 
                }).Count(),
                NovasFiliais = registros.Where(x => x.CriarFilial).GroupBy(x => new { 
                    x.EmpresaCnpj, 
                    x.FilialNome 
                }).Count(),
                NomesEmpresasNovas = registros.Where(x => x.CriarEmpresa)
                    .GroupBy(x => x.EmpresaNome)
                    .Select(g => g.First().EmpresaNome)
                    .ToList(),
                NomesLocalidadesNovas = registros.Where(x => x.CriarLocalidade)
                    .GroupBy(x => x.LocalidadeDescricao)
                    .Select(g => g.First().LocalidadeDescricao)
                    .ToList(),
                NomesCentrosCustoNovos = registros.Where(x => x.CriarCentroCusto)
                    .GroupBy(x => x.CentroCustoNome)
                    .Select(g => g.First().CentroCustoNome)
                    .ToList(),
                NomesFiliaisNovas = registros.Where(x => x.CriarFilial)
                    .GroupBy(x => x.FilialNome)
                    .Select(g => g.First().FilialNome)
                    .ToList()
            };

            int totalAtualizacoes = 0;
            int totalSemAlteracao = 0;

            foreach (var registro in registros.Where(x => x.Status == "V"))
            {
                var cpfRegistro = LimparCpf(registro.Cpf);
                if (string.IsNullOrWhiteSpace(cpfRegistro))
                {
                    continue;
                }

                if (colaboradoresPorCpf.TryGetValue(cpfRegistro, out var colaboradorExistente))
                {
                    if (PossuiMudancas(registro, colaboradorExistente))
                    {
                        totalAtualizacoes++;
                    }
                    else
                    {
                        totalSemAlteracao++;
                    }
                }
            }

            resumo.TotalAtualizacoes = totalAtualizacoes;
            resumo.TotalSemAlteracao = totalSemAlteracao;
            var totalNovos = resumo.Validos - totalAtualizacoes - totalSemAlteracao;
            resumo.TotalNovos = totalNovos < 0 ? 0 : totalNovos;

            // Contar avisos
            resumo.Avisos = registros.Count(x => x.Status == "V" && !string.IsNullOrEmpty(x.MensagensValidacao) &&
                x.MensagensValidacao.Contains("avisos") && x.MensagensValidacao.Contains("["));

            return resumo;
        }

        public async Task<ResultadoImportacaoColaboradoresDTO> EfetivarImportacao(Guid loteId, int clienteId, int usuarioId)
        {
            _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Iniciando efetivação da importação - Lote: {loteId}");

            // Verificar status do log antes de processar
            var logVerificacao = await _context.ImportacaoLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LoteId == loteId && x.Cliente == clienteId);

            if (logVerificacao != null)
            {
                var statusUpper = (logVerificacao.Status ?? "").ToUpper().Trim();
                if (statusUpper == "CANCELADO")
                {
                    throw new Exception("Não é possível confirmar um lote que foi cancelado anteriormente.");
                }
                
                if (statusUpper == "CONCLUIDO")
                {
                    throw new Exception("Este lote já foi concluído anteriormente.");
                }
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var registros = await _context.ImportacaoColaboradoresStaging
                    .Where(x => x.LoteId == loteId && x.Cliente == clienteId && x.Status == "V")
                    .ToListAsync();

                if (!registros.Any())
                    throw new Exception("Não há registros válidos para importar. O lote pode ter sido cancelado ou não possui dados válidos.");

                var logExecucao = await _context.ImportacaoLogs
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.LoteId == loteId && x.Cliente == clienteId);

                if (logExecucao != null)
                {
                    logExecucao.Status = "PROCESSANDO";
                    logExecucao.DataFim = null;
                    logExecucao.TotalImportados = 0;
                    logExecucao.Observacoes = "Importação em andamento...";
                    await _context.SaveChangesAsync();
                }

                var resultado = new ResultadoImportacaoColaboradoresDTO
                {
                    LoteId = loteId,
                    DataInicio = DateTime.Now
                };

                // ========== ETAPA 1: Criar Empresas novas ==========
                var empresasNovas = registros
                    .Where(x => x.CriarEmpresa)
                    .GroupBy(x => x.EmpresaCnpj.Trim())
                    .ToList();

                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Criando {empresasNovas.Count} empresas novas");

                foreach (var grupo in empresasNovas)
                {
                    var novaEmpresa = new Empresa
                    {
                        Cliente = clienteId,
                        Nome = grupo.First().EmpresaNome,
                        Cnpj = grupo.First().EmpresaCnpj,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.Empresas.Add(novaEmpresa);
                    await _context.SaveChangesAsync();

                    foreach (var reg in grupo)
                    {
                        reg.EmpresaId = novaEmpresa.Id;
                    }

                    resultado.EmpresasCriadas++;
                }

                // ========== ETAPA 2: Criar Localidades novas ==========
                var localidadesNovas = registros
                    .Where(x => x.CriarLocalidade)
                    .GroupBy(x => new { 
                        Descricao = x.LocalidadeDescricao.Trim().ToLower(),
                        Cidade = x.LocalidadeCidade.Trim().ToLower(),
                        Estado = x.LocalidadeEstado.Trim().ToUpper()
                    })
                    .ToList();

                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Criando {localidadesNovas.Count} localidades novas");

                foreach (var grupo in localidadesNovas)
                {
                    var primeiroRegistro = registros.First(x => 
                        x.LocalidadeDescricao.Trim().ToLower() == grupo.Key.Descricao &&
                        x.LocalidadeCidade.Trim().ToLower() == grupo.Key.Cidade &&
                        x.LocalidadeEstado.Trim().ToUpper() == grupo.Key.Estado);

                    var novaLocalidade = new Localidade
                    {
                        Cliente = clienteId,
                        Descricao = primeiroRegistro.LocalidadeDescricao,
                        Cidade = primeiroRegistro.LocalidadeCidade,
                        Estado = primeiroRegistro.LocalidadeEstado,
                        Ativo = true
                    };

                    _context.Localidades.Add(novaLocalidade);
                    await _context.SaveChangesAsync();

                    foreach (var reg in registros.Where(x => 
                        x.LocalidadeDescricao.Trim().ToLower() == grupo.Key.Descricao &&
                        x.LocalidadeCidade.Trim().ToLower() == grupo.Key.Cidade &&
                        x.LocalidadeEstado.Trim().ToUpper() == grupo.Key.Estado))
                    {
                        reg.LocalidadeId = novaLocalidade.Id;
                    }

                    resultado.LocalidadesCriadas++;
                }

                // ========== ETAPA 3: Criar Filiais novas (se necessário) ==========
                var filiaisNovas = registros
                    .Where(x => x.CriarFilial && x.EmpresaId.HasValue && x.LocalidadeId.HasValue)
                    .GroupBy(x => new { 
                        EmpresaId = x.EmpresaId.Value,
                        LocalidadeId = x.LocalidadeId.Value,
                        Nome = x.FilialNome.Trim().ToLower()
                    })
                    .ToList();

                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Criando {filiaisNovas.Count} filiais novas");

                foreach (var grupo in filiaisNovas)
                {
                    var primeiroRegistro = registros.First(x => 
                        x.EmpresaId == grupo.Key.EmpresaId &&
                        x.LocalidadeId == grupo.Key.LocalidadeId &&
                        x.FilialNome.Trim().ToLower() == grupo.Key.Nome);

                    var novaFilial = new Filial
                    {
                        EmpresaId = grupo.Key.EmpresaId,
                        LocalidadeId = grupo.Key.LocalidadeId,
                        Nome = primeiroRegistro.FilialNome,
                        Cnpj = primeiroRegistro.FilialCnpj ?? string.Empty,
                        Endereco = string.Empty,
                        Telefone = string.Empty,
                        Email = string.Empty,
                        Ativo = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.Filiais.Add(novaFilial);
                    await _context.SaveChangesAsync();

                    foreach (var reg in registros.Where(x => 
                        x.EmpresaId == grupo.Key.EmpresaId &&
                        x.LocalidadeId == grupo.Key.LocalidadeId &&
                        x.FilialNome?.Trim().ToLower() == grupo.Key.Nome))
                    {
                        reg.FilialId = novaFilial.Id;
                    }

                    resultado.FiliaisCriadas++;
                }

                // ========== ETAPA 4: Criar Centros de Custo novos ==========
                var centrosCustoNovos = registros
                    .Where(x => x.CriarCentroCusto && x.EmpresaId.HasValue)
                    .GroupBy(x => new { 
                        EmpresaId = x.EmpresaId.Value,
                        Codigo = x.CentroCustoCodigo.Trim().ToLower()
                    })
                    .ToList();

                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Criando {centrosCustoNovos.Count} centros de custo novos");

                foreach (var grupo in centrosCustoNovos)
                {
                    var primeiroRegistro = registros.First(x => 
                        x.EmpresaId == grupo.Key.EmpresaId &&
                        x.CentroCustoCodigo.Trim().ToLower() == grupo.Key.Codigo);

                    var novoCentroCusto = new Centrocusto
                    {
                        Empresa = grupo.Key.EmpresaId,
                        Codigo = primeiroRegistro.CentroCustoCodigo,
                        Nome = primeiroRegistro.CentroCustoNome,
                        FilialId = primeiroRegistro.FilialId,
                        Ativo = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.Centrocustos.Add(novoCentroCusto);
                    await _context.SaveChangesAsync();

                    foreach (var reg in registros.Where(x => 
                        x.EmpresaId == grupo.Key.EmpresaId &&
                        x.CentroCustoCodigo.Trim().ToLower() == grupo.Key.Codigo))
                    {
                        reg.CentroCustoId = novoCentroCusto.Id;
                    }

                    resultado.CentrosCustoCriados++;
                }

                // ========== ETAPA 5: Criar ou Atualizar Colaboradores ==========
                // Carregar colaboradores com tracking para garantir que as alterações sejam salvas
                var colaboradoresExistentes = await _context.Colaboradores
                    .AsTracking()
                    .Where(x => x.Cliente == clienteId)
                    .ToListAsync();

                var colaboradoresPorCpf = new Dictionary<string, Colaboradore>(StringComparer.OrdinalIgnoreCase);

                foreach (var col in colaboradoresExistentes)
                {
                    var cpfLimpo = LimparCpf(Cripto.CriptografarDescriptografar(col.Cpf, false));
                    if (string.IsNullOrWhiteSpace(cpfLimpo))
                    {
                        continue;
                    }

                    if (!colaboradoresPorCpf.ContainsKey(cpfLimpo))
                    {
                        colaboradoresPorCpf[cpfLimpo] = col;
                    }
                }
                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Processando {registros.Count} colaboradores (criação/atualização)");

                foreach (var registro in registros)
                {
                    var cpfRegistro = LimparCpf(registro.Cpf);
                    registro.Cpf = cpfRegistro;

                    var nomeRegistro = (registro.NomeColaborador ?? string.Empty).Trim();
                    var matriculaRegistro = (registro.Matricula ?? string.Empty).Trim();
                    var emailRegistro = (registro.Email ?? string.Empty).Trim();
                    var cargoRegistro = (registro.Cargo ?? string.Empty).Trim();
                    var setorRegistro = (registro.Setor ?? string.Empty).Trim();
                    var matriculaSuperiorRegistro = (registro.MatriculaSuperior ?? string.Empty).Trim();
                    var tipoColaboradorRegistro = (registro.TipoColaborador ?? string.Empty).Trim().ToUpperInvariant();

                    var possuiIdsObrigatorios = registro.EmpresaId.HasValue && registro.LocalidadeId.HasValue && registro.CentroCustoId.HasValue;
                    var possuiColaboradorExistente = colaboradoresPorCpf.TryGetValue(cpfRegistro, out var colaboradorExistente);

                    if (!possuiColaboradorExistente && !possuiIdsObrigatorios)
                    {
                        _logger.LogWarning($"[IMPORTACAO-COLABORADORES] Registro sem IDs obrigatórios - Linha {registro.LinhaArquivo}");
                        continue;
                    }

                    if (!possuiColaboradorExistente)
                    {
                        var novoColaborador = new Colaboradore
                        {
                            Cliente = clienteId,
                            Usuario = usuarioId,
                            Empresa = registro.EmpresaId!.Value,
                            Localidade = registro.LocalidadeId!.Value,
                            LocalidadeId = registro.LocalidadeId,
                            Centrocusto = registro.CentroCustoId!.Value,
                            FilialId = registro.FilialId,
                            Nome = nomeRegistro,
                            Cpf = CriptografarCpf(cpfRegistro),
                            Matricula = matriculaRegistro,
                            Email = CriptografarEmail(emailRegistro),
                            Cargo = cargoRegistro,
                            Setor = setorRegistro,
                            Dtadmissao = registro.DataAdmissao ?? DateTime.Now,
                            Tipocolaborador = string.IsNullOrEmpty(tipoColaboradorRegistro) ? 'F' : char.ToUpperInvariant(tipoColaboradorRegistro[0]),
                            Dtdemissao = registro.DataDemissao,
                            Matriculasuperior = matriculaSuperiorRegistro,
                            Dtcadastro = DateTime.Now,
                            Dtatualizacao = DateTime.Now,
                            Situacao = CalcularSituacao(registro.DataDemissao)
                        };

                        _context.Colaboradores.Add(novoColaborador);
                        resultado.ColaboradoresCriados++;
                        registro.Status = "I"; // Importado

                        if (!string.IsNullOrWhiteSpace(cpfRegistro))
                        {
                            colaboradoresPorCpf[cpfRegistro] = novoColaborador;
                        }
                        continue;
                    }

                    // Atualizar colaborador existente
                    var houveMudanca = false;
                    var agora = DateTime.Now;

                    var cpfAtualTexto = LimparCpf(Cripto.CriptografarDescriptografar(colaboradorExistente.Cpf, false));
                    var cpfCriptografado = CriptografarCpf(cpfRegistro);

                    if (!string.Equals(cpfAtualTexto, cpfRegistro, StringComparison.Ordinal))
                    {
                        colaboradorExistente.Cpf = cpfCriptografado;
                        houveMudanca = true;
                    }
                    else if (!string.Equals(colaboradorExistente.Cpf ?? string.Empty, cpfCriptografado, StringComparison.Ordinal))
                    {
                        colaboradorExistente.Cpf = cpfCriptografado;
                        houveMudanca = true;
                    }

                    if (!string.IsNullOrEmpty(nomeRegistro) &&
                        !string.Equals((colaboradorExistente.Nome ?? string.Empty).Trim(), nomeRegistro, StringComparison.Ordinal))
                    {
                        colaboradorExistente.Nome = nomeRegistro;
                        houveMudanca = true;
                    }

                    if (!string.IsNullOrEmpty(matriculaRegistro) &&
                        !string.Equals((colaboradorExistente.Matricula ?? string.Empty).Trim(), matriculaRegistro, StringComparison.Ordinal))
                    {
                        colaboradorExistente.Matricula = matriculaRegistro;
                        houveMudanca = true;
                    }

                    if (registro.Email != null)
                    {
                        var emailExistenteTexto = (Cripto.CriptografarDescriptografar(colaboradorExistente.Email, false) ?? string.Empty).Trim();

                        if (string.IsNullOrEmpty(emailRegistro))
                        {
                            if (!string.IsNullOrEmpty(emailExistenteTexto) || !string.IsNullOrEmpty(colaboradorExistente.Email))
                            {
                                colaboradorExistente.Email = string.Empty;
                                houveMudanca = true;
                            }
                        }
                        else if (!string.Equals(emailRegistro, emailExistenteTexto, StringComparison.OrdinalIgnoreCase))
                        {
                            colaboradorExistente.Email = CriptografarEmail(emailRegistro);
                            houveMudanca = true;
                        }
                        else
                        {
                            var emailCriptografado = CriptografarEmail(emailRegistro);
                            if (!string.Equals(colaboradorExistente.Email ?? string.Empty, emailCriptografado, StringComparison.Ordinal))
                            {
                                colaboradorExistente.Email = emailCriptografado;
                                houveMudanca = true;
                            }
                        }
                    }

                    if (registro.Cargo != null &&
                        !string.Equals((colaboradorExistente.Cargo ?? string.Empty).Trim(), cargoRegistro, StringComparison.Ordinal))
                    {
                        colaboradorExistente.Cargo = cargoRegistro;
                        houveMudanca = true;
                    }

                    if (registro.Setor != null &&
                        !string.Equals((colaboradorExistente.Setor ?? string.Empty).Trim(), setorRegistro, StringComparison.Ordinal))
                    {
                        colaboradorExistente.Setor = setorRegistro;
                        houveMudanca = true;
                    }

                    if (registro.MatriculaSuperior != null &&
                        !string.Equals((colaboradorExistente.Matriculasuperior ?? string.Empty).Trim(), matriculaSuperiorRegistro, StringComparison.Ordinal))
                    {
                        colaboradorExistente.Matriculasuperior = matriculaSuperiorRegistro;
                        houveMudanca = true;
                    }

                    if (registro.DataAdmissao.HasValue && !DatasIguais(registro.DataAdmissao, colaboradorExistente.Dtadmissao))
                    {
                        colaboradorExistente.Dtadmissao = registro.DataAdmissao.Value;
                        houveMudanca = true;
                    }

                    // Atualizar data de demissão - permite remover (null) ou atualizar
                    if (!DatasIguais(registro.DataDemissao, colaboradorExistente.Dtdemissao))
                    {
                        var dataAntiga = colaboradorExistente.Dtdemissao;
                        colaboradorExistente.Dtdemissao = registro.DataDemissao;
                        houveMudanca = true;
                        _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Atualizando data de demissão - CPF: {cpfRegistro}, De: {dataAntiga} Para: {registro.DataDemissao}");
                    }

                    // Atualizar tipo de colaborador
                    if (!string.IsNullOrEmpty(tipoColaboradorRegistro))
                    {
                        var novoTipo = char.ToUpperInvariant(tipoColaboradorRegistro[0]);
                        var tipoAtual = char.ToUpperInvariant(colaboradorExistente.Tipocolaborador);
                        
                        if (tipoAtual != novoTipo)
                        {
                            colaboradorExistente.Tipocolaborador = novoTipo;
                            houveMudanca = true;
                        }
                    }

                    if (registro.EmpresaId.HasValue && colaboradorExistente.Empresa != registro.EmpresaId.Value)
                    {
                        colaboradorExistente.Antigaempresa = colaboradorExistente.Empresa;
                        colaboradorExistente.Empresa = registro.EmpresaId.Value;
                        colaboradorExistente.Dtatualizacaoempresa = agora;
                        houveMudanca = true;
                    }

                    if (registro.CentroCustoId.HasValue && colaboradorExistente.Centrocusto != registro.CentroCustoId.Value)
                    {
                        colaboradorExistente.Antigocentrocusto = colaboradorExistente.Centrocusto;
                        colaboradorExistente.Centrocusto = registro.CentroCustoId.Value;
                        colaboradorExistente.Dtatualizacaocentrocusto = agora;
                        houveMudanca = true;
                    }

                    if (registro.LocalidadeId.HasValue && colaboradorExistente.Localidade != registro.LocalidadeId.Value)
                    {
                        colaboradorExistente.Antigalocalidade = colaboradorExistente.Localidade;
                        colaboradorExistente.Localidade = registro.LocalidadeId.Value;
                        colaboradorExistente.LocalidadeId = registro.LocalidadeId.Value;
                        colaboradorExistente.Dtatualizacaolocalidade = agora;
                        houveMudanca = true;
                    }

                    if (colaboradorExistente.FilialId != registro.FilialId)
                    {
                        colaboradorExistente.FilialId = registro.FilialId;
                        houveMudanca = true;
                    }

                    var situacaoAtual = colaboradorExistente.Situacao ?? string.Empty;
                    var novaSituacao = CalcularSituacao(registro.DataDemissao);
                    if (!string.Equals(situacaoAtual, novaSituacao, StringComparison.OrdinalIgnoreCase))
                    {
                        colaboradorExistente.Situacaoantiga = !string.IsNullOrEmpty(situacaoAtual) ? situacaoAtual[0] : (char?)null;
                        colaboradorExistente.Situacao = novaSituacao;
                        colaboradorExistente.Dtatualizacao = agora;
                        houveMudanca = true;
                    }

                    if (houveMudanca)
                    {
                        colaboradorExistente.Dtatualizacao = agora;
                        resultado.ColaboradoresAtualizados++;
                        registro.Status = "I"; // Importado
                    }
                    else
                    {
                        resultado.ColaboradoresSemAlteracao++;
                        registro.Status = "S"; // Sem alterações
                    }

                    if (!string.IsNullOrWhiteSpace(cpfRegistro))
                    {
                        colaboradoresPorCpf[cpfRegistro] = colaboradorExistente;
                    }
                }

                // Salvar alterações dos colaboradores
                var entidadesAlteradas = _context.ChangeTracker.Entries<Colaboradore>()
                    .Where(e => e.State == EntityState.Modified)
                    .Count();
                
                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Salvando {entidadesAlteradas} colaboradores alterados");
                await _context.SaveChangesAsync();
                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Colaboradores salvos com sucesso");

                // ========== ETAPA 6: Atualizar log ==========
                // Buscar o log novamente com tracking para garantir que será atualizado
                var log = await _context.ImportacaoLogs
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.LoteId == loteId && x.Cliente == clienteId);

                if (log != null)
                {
                    log.Status = "CONCLUIDO";
                    log.DataFim = DateTime.Now;
                    // Total processado inclui criados, atualizados e sem alteração
                    var totalProcessado = resultado.ColaboradoresCriados + resultado.ColaboradoresAtualizados + resultado.ColaboradoresSemAlteracao;
                    log.TotalValidados = totalProcessado;
                    log.TotalImportados = resultado.ColaboradoresCriados + resultado.ColaboradoresAtualizados; // Apenas os que tiveram mudanças
                    log.Observacoes = $"Empresas: {resultado.EmpresasCriadas}, Localidades: {resultado.LocalidadesCriadas}, " +
                                     $"Filiais: {resultado.FiliaisCriadas}, Centros Custo: {resultado.CentrosCustoCriados}, " +
                                     $"Colaboradores Criados: {resultado.ColaboradoresCriados}, " +
                                     $"Colaboradores Atualizados: {resultado.ColaboradoresAtualizados}, " +
                                     $"Sem Alteração: {resultado.ColaboradoresSemAlteracao}";
                    
                    _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Atualizando log - Status: CONCLUIDO, TotalImportados: {log.TotalImportados}, TotalValidados: {log.TotalValidados}");
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Log atualizado com sucesso");
                }
                else
                {
                    _logger.LogWarning($"[IMPORTACAO-COLABORADORES] Log não encontrado para atualização - Lote: {loteId}");
                }

                try
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Transação commitada com sucesso - Lote: {loteId}");
                }
                catch (Exception commitEx)
                {
                    _logger.LogError($"[IMPORTACAO-COLABORADORES] Erro ao fazer commit da transação: {commitEx.Message}");
                    _logger.LogError($"[IMPORTACAO-COLABORADORES] StackTrace do commit: {commitEx.StackTrace}");
                    throw;
                }

                resultado.DataFim = DateTime.Now;
                resultado.TotalProcessado = resultado.ColaboradoresCriados + resultado.ColaboradoresAtualizados;
                resultado.Sucesso = true;
                resultado.Mensagem = $"Importação concluída com sucesso! {resultado.ColaboradoresCriados} criado(s), {resultado.ColaboradoresAtualizados} atualizado(s) e {resultado.ColaboradoresSemAlteracao} sem movimentação.";

                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Importação concluída - Lote: {loteId} | Criados: {resultado.ColaboradoresCriados} | Atualizados: {resultado.ColaboradoresAtualizados} | Sem alterações: {resultado.ColaboradoresSemAlteracao}");

                return resultado;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"[IMPORTACAO-COLABORADORES] Erro ao efetivar importação: {ex.Message}");
                _logger.LogError($"[IMPORTACAO-COLABORADORES] StackTrace: {ex.StackTrace}");

                // Buscar o log com tracking para garantir que será atualizado
                var logErro = await _context.ImportacaoLogs
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.LoteId == loteId && x.Cliente == clienteId);

                if (logErro != null)
                {
                    logErro.Status = "ERRO";
                    logErro.Observacoes = $"Falha ao efetivar importação: {ex.Message}";
                    logErro.DataFim = DateTime.Now;
                    logErro.TotalImportados = 0;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Log atualizado com status ERRO");
                }
                else
                {
                    _logger.LogWarning($"[IMPORTACAO-COLABORADORES] Log não encontrado para atualização de erro - Lote: {loteId}");
                }

                throw new Exception($"Erro ao efetivar importação: {ex.Message}");
            }
            });
        }

        public async Task<bool> LimparStaging(Guid loteId, int clienteId)
        {
            try
            {
                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Iniciando cancelamento - Lote: {loteId}, Cliente: {clienteId}");

                // Remover registros de staging
                var registros = await _context.ImportacaoColaboradoresStaging
                    .Where(x => x.LoteId == loteId && x.Cliente == clienteId)
                    .ToListAsync();

                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Encontrados {registros.Count} registros de staging para remover");

                if (registros.Any())
                {
                    _context.ImportacaoColaboradoresStaging.RemoveRange(registros);
                }

                // Buscar e atualizar o log com tracking explícito
                var log = await _context.ImportacaoLogs
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.LoteId == loteId && x.Cliente == clienteId);

                if (log != null)
                {
                    _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Log encontrado - Status atual: {log.Status}, ID: {log.Id}");
                    
                    log.Status = "CANCELADO";
                    log.DataFim = DateTime.Now;
                    log.TotalImportados = 0; // Usar 0 ao invés de null pois a coluna não permite NULL
                    log.Observacoes = "Importação cancelada pelo usuário";

                    _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Log atualizado - Novo status: {log.Status}");
                }
                else
                {
                    _logger.LogWarning($"[IMPORTACAO-COLABORADORES] Log não encontrado para Lote: {loteId}, Cliente: {clienteId}");
                }

                var linhasAfetadas = await _context.SaveChangesAsync();
                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] SaveChangesAsync concluído - Linhas afetadas: {linhasAfetadas}");

                // Verificar se o status foi realmente salvo
                var logVerificacao = await _context.ImportacaoLogs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.LoteId == loteId && x.Cliente == clienteId);

                if (logVerificacao != null)
                {
                    _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Verificação pós-salvamento - Status no banco: {logVerificacao.Status}");
                }

                _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Lote {loteId} cancelado e limpo com sucesso");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[IMPORTACAO-COLABORADORES] Erro ao limpar staging: {ex.Message}");
                _logger.LogError($"[IMPORTACAO-COLABORADORES] StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<List<HistoricoImportacaoDTO>> ObterHistorico(int clienteId, int? limite = 50)
        {
            var historico = await _context.ImportacaoLogs
                .Where(x => x.Cliente == clienteId && x.TipoImportacao == "COLABORADORES")
                .Include(x => x.UsuarioNavigation)
                .OrderByDescending(x => x.DataInicio)
                .Take(limite ?? 50)
                .ToListAsync();

            return historico.Select(h => new HistoricoImportacaoDTO
            {
                Id = h.Id,
                LoteId = h.LoteId,
                TipoImportacao = h.TipoImportacao,
                NomeArquivo = h.NomeArquivo,
                DataInicio = h.DataInicio,
                DataFim = h.DataFim,
                Status = h.Status,
                StatusDescricao = ObterStatusDescricaoLog(h.Status),
                TotalRegistros = h.TotalRegistros,
                TotalValidados = h.TotalValidados,
                TotalErros = h.TotalErros,
                TotalImportados = h.TotalImportados,
                UsuarioNome = h.UsuarioNavigation?.Nome ?? "Sistema",
                UsuarioEmail = h.UsuarioNavigation?.Email ?? "",
                Observacoes = h.Observacoes
            }).ToList();
        }

        public byte[] GerarTemplateExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Colaboradores");

                // Definir cabeçalhos
                worksheet.Cell(1, 1).Value = "Nome";
                worksheet.Cell(1, 2).Value = "CPF";
                worksheet.Cell(1, 3).Value = "Matrícula";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Cargo";
                worksheet.Cell(1, 6).Value = "Setor";
                worksheet.Cell(1, 7).Value = "Data Admissão";
                worksheet.Cell(1, 8).Value = "Tipo Colaborador";
                worksheet.Cell(1, 9).Value = "Empresa";
                worksheet.Cell(1, 10).Value = "CNPJ Empresa";
                worksheet.Cell(1, 11).Value = "Localidade";
                worksheet.Cell(1, 12).Value = "Cidade";
                worksheet.Cell(1, 13).Value = "Estado";
                worksheet.Cell(1, 14).Value = "Centro Custo Código";
                worksheet.Cell(1, 15).Value = "Centro Custo Nome";
                worksheet.Cell(1, 16).Value = "Filial (Opcional)";
                worksheet.Cell(1, 17).Value = "CNPJ Filial";
                worksheet.Cell(1, 18).Value = "Data Demissão";
                worksheet.Cell(1, 19).Value = "Matrícula Superior";

                // Estilizar cabeçalho
                var headerRange = worksheet.Range(1, 1, 1, 19);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Adicionar exemplos
                worksheet.Cell(2, 1).Value = "João Silva";
                worksheet.Cell(2, 2).Value = "123.456.789-00";
                worksheet.Cell(2, 3).Value = "MAT001";
                worksheet.Cell(2, 4).Value = "joao.silva@empresa.com";
                worksheet.Cell(2, 5).Value = "Analista de TI";
                worksheet.Cell(2, 6).Value = "Tecnologia";
                worksheet.Cell(2, 7).Value = "01/01/2024";
                worksheet.Cell(2, 8).Value = "F";
                worksheet.Cell(2, 9).Value = "Empresa A";
                worksheet.Cell(2, 10).Value = "12.345.678/0001-90";
                worksheet.Cell(2, 11).Value = "Sede";
                worksheet.Cell(2, 12).Value = "São Paulo";
                worksheet.Cell(2, 13).Value = "SP";
                worksheet.Cell(2, 14).Value = "CC001";
                worksheet.Cell(2, 15).Value = "TI - Infraestrutura";
                worksheet.Cell(2, 16).Value = "Filial SP";
                worksheet.Cell(2, 17).Value = "12.345.678/0002-71";
                worksheet.Cell(2, 18).Value = "";
                worksheet.Cell(2, 19).Value = "";

                worksheet.Cell(3, 1).Value = "Maria Santos";
                worksheet.Cell(3, 2).Value = "987.654.321-00";
                worksheet.Cell(3, 3).Value = "MAT002";
                worksheet.Cell(3, 4).Value = "maria.santos@empresa.com";
                worksheet.Cell(3, 5).Value = "Consultora RH";
                worksheet.Cell(3, 6).Value = "Recursos Humanos";
                worksheet.Cell(3, 7).Value = "15/03/2024";
                worksheet.Cell(3, 8).Value = "C";
                worksheet.Cell(3, 9).Value = "Empresa A";
                worksheet.Cell(3, 10).Value = "12.345.678/0001-90";
                worksheet.Cell(3, 11).Value = "Sede";
                worksheet.Cell(3, 12).Value = "São Paulo";
                worksheet.Cell(3, 13).Value = "SP";
                worksheet.Cell(3, 14).Value = "CC002";
                worksheet.Cell(3, 15).Value = "RH - Gestão de Pessoas";
                worksheet.Cell(3, 16).Value = "";
                worksheet.Cell(3, 17).Value = "";
                worksheet.Cell(3, 18).Value = "";
                worksheet.Cell(3, 19).Value = "";

                // Ajustar largura das colunas
                worksheet.Column(1).Width = 25;
                worksheet.Column(2).Width = 15;
                worksheet.Column(3).Width = 12;
                worksheet.Column(4).Width = 30;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 15;
                worksheet.Column(8).Width = 18;
                worksheet.Column(9).Width = 25;
                worksheet.Column(10).Width = 20;
                worksheet.Column(11).Width = 20;
                worksheet.Column(12).Width = 15;
                worksheet.Column(13).Width = 8;
                worksheet.Column(14).Width = 20;
                worksheet.Column(15).Width = 30;
                worksheet.Column(16).Width = 20;
                worksheet.Column(17).Width = 20;
                worksheet.Column(18).Width = 15;
                worksheet.Column(19).Width = 18;

                // Adicionar instruções em outra aba
                var instrucoes = workbook.Worksheets.Add("Instruções");
                instrucoes.Cell(1, 1).Value = "INSTRUÇÕES PARA IMPORTAÇÃO DE COLABORADORES";
                instrucoes.Cell(1, 1).Style.Font.Bold = true;
                instrucoes.Cell(1, 1).Style.Font.FontSize = 14;

                int linha = 3;

                instrucoes.Cell(linha++, 1).Value = "📋 CAMPOS OBRIGATÓRIOS:";
                instrucoes.Cell(linha - 1, 1).Style.Font.Bold = true;
                instrucoes.Cell(linha++, 1).Value = "• Nome - Nome completo do colaborador";
                instrucoes.Cell(linha++, 1).Value = "• CPF - CPF sem formatação ou com pontos/traço";
                instrucoes.Cell(linha++, 1).Value = "• Matrícula - Código único do colaborador na empresa";
                instrucoes.Cell(linha++, 1).Value = "• Email - Email válido para contato";
                instrucoes.Cell(linha++, 1).Value = "• Cargo - Cargo/função do colaborador";
                instrucoes.Cell(linha++, 1).Value = "• Setor - Setor/departamento";
                instrucoes.Cell(linha++, 1).Value = "• Data Admissão - Data de contratação (DD/MM/AAAA)";
                instrucoes.Cell(linha++, 1).Value = "• Tipo Colaborador - F (Funcionário), T (Terceiro) ou C (Consultor)";
                instrucoes.Cell(linha++, 1).Value = "• Empresa - Nome da empresa contratante";
                instrucoes.Cell(linha++, 1).Value = "• CNPJ Empresa - CNPJ da empresa (com ou sem formatação)";
                instrucoes.Cell(linha++, 1).Value = "• Localidade - Descrição da localidade (ex: Sede, Filial SP)";
                instrucoes.Cell(linha++, 1).Value = "• Cidade - Cidade da localidade";
                instrucoes.Cell(linha++, 1).Value = "• Estado - UF (sigla com 2 letras)";
                instrucoes.Cell(linha++, 1).Value = "• Centro Custo Código - Código do centro de custo";
                instrucoes.Cell(linha++, 1).Value = "• Centro Custo Nome - Nome/descrição do centro de custo";

                linha++;
                instrucoes.Cell(linha++, 1).Value = "📋 CAMPOS OPCIONAIS:";
                instrucoes.Cell(linha - 1, 1).Style.Font.Bold = true;
                instrucoes.Cell(linha++, 1).Value = "• Filial - Nome da filial (se aplicável)";
                instrucoes.Cell(linha++, 1).Value = "• CNPJ Filial - CNPJ da filial (se aplicável)";
                instrucoes.Cell(linha++, 1).Value = "• Data Demissão - Data de desligamento (DD/MM/AAAA)";
                instrucoes.Cell(linha++, 1).Value = "  * Se vazia: colaborador estará ATIVO";
                instrucoes.Cell(linha++, 1).Value = "  * Data anterior a hoje: será marcado como DESLIGADO";
                instrucoes.Cell(linha++, 1).Value = "  * Data futura: será marcado como PROGRAMADO para desligamento";
                instrucoes.Cell(linha++, 1).Value = "• Matrícula Superior - Matrícula do gestor direto";

                linha++;
                instrucoes.Cell(linha++, 1).Value = "✨ TIPOS DE COLABORADOR:";
                instrucoes.Cell(linha - 1, 1).Style.Font.Bold = true;
                instrucoes.Cell(linha++, 1).Value = "• F = Funcionário (vínculo CLT)";
                instrucoes.Cell(linha++, 1).Value = "• T = Terceiro (prestador de serviço)";
                instrucoes.Cell(linha++, 1).Value = "• C = Consultor (consultoria especializada)";

                linha++;
                instrucoes.Cell(linha++, 1).Value = "✨ SITUAÇÃO DO COLABORADOR:";
                instrucoes.Cell(linha - 1, 1).Style.Font.Bold = true;
                instrucoes.Cell(linha++, 1).Value = "A situação é calculada AUTOMATICAMENTE pelo sistema:";
                instrucoes.Cell(linha++, 1).Value = "• Data Demissão vazia → ATIVO";
                instrucoes.Cell(linha++, 1).Value = "• Data Demissão < Hoje → DESLIGADO";
                instrucoes.Cell(linha++, 1).Value = "• Data Demissão > Hoje → PROGRAMADO para desligamento";

                linha++;
                instrucoes.Cell(linha++, 1).Value = "✨ O SISTEMA CRIARÁ AUTOMATICAMENTE:";
                instrucoes.Cell(linha - 1, 1).Style.Font.Bold = true;
                instrucoes.Cell(linha++, 1).Value = "• Empresas que não existirem no sistema";
                instrucoes.Cell(linha++, 1).Value = "• Localidades que não existirem no sistema";
                instrucoes.Cell(linha++, 1).Value = "• Filiais que não existirem no sistema";
                instrucoes.Cell(linha++, 1).Value = "• Centros de Custo que não existirem no sistema";

                linha++;
                instrucoes.Cell(linha++, 1).Value = "⚠️ VALIDAÇÕES IMPORTANTES:";
                instrucoes.Cell(linha - 1, 1).Style.Font.Bold = true;
                instrucoes.Cell(linha++, 1).Value = "• CPF deve ser único (não pode haver duplicatas)";
                instrucoes.Cell(linha++, 1).Value = "• Matrícula deve ser única por empresa";
                instrucoes.Cell(linha++, 1).Value = "• Email deve ter formato válido";
                instrucoes.Cell(linha++, 1).Value = "• Centro de Custo será vinculado à Empresa especificada";
                instrucoes.Cell(linha++, 1).Value = "• Se informar Filial, ela será vinculada à Empresa + Localidade";
                instrucoes.Cell(linha++, 1).Value = "• Data Demissão deve ser posterior à Data Admissão (se preenchida)";
                instrucoes.Cell(linha++, 1).Value = "• Tipo Colaborador deve ser F, T ou C";
                instrucoes.Cell(linha++, 1).Value = "• Estado deve ter 2 caracteres (sigla)";
                instrucoes.Cell(linha++, 1).Value = "• Máximo de 5000 linhas por importação";

                linha++;
                instrucoes.Cell(linha++, 1).Value = "💡 DICAS:";
                instrucoes.Cell(linha - 1, 1).Style.Font.Bold = true;
                instrucoes.Cell(linha++, 1).Value = "• Remova as linhas de exemplo antes de importar seus dados";
                instrucoes.Cell(linha++, 1).Value = "• Não altere o nome das colunas";
                instrucoes.Cell(linha++, 1).Value = "• Você pode deixar colunas opcionais vazias";
                instrucoes.Cell(linha++, 1).Value = "• CNPJ pode ser com ou sem formatação (sistema aceita ambos)";
                instrucoes.Cell(linha++, 1).Value = "• CPF pode ser com ou sem formatação (sistema aceita ambos)";
                instrucoes.Cell(linha++, 1).Value = "• Datas devem estar no formato DD/MM/AAAA";

                instrucoes.Column(1).Width = 90;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        // ========== MÉTODOS AUXILIARES ==========

        private static bool DatasIguais(DateTime? primeira, DateTime? segunda)
        {
            if (!primeira.HasValue && !segunda.HasValue)
                return true;

            if (primeira.HasValue && segunda.HasValue)
                return primeira.Value.Date == segunda.Value.Date;

            return false;
        }

        private string CalcularSituacao(DateTime? dataDemissao)
        {
            if (!dataDemissao.HasValue)
                return "A";  // Ativo
            
            if (dataDemissao.Value < DateTime.Today)
                return "D";  // Desligado
            
            return "A";  // Ativo (programado para desligamento no futuro)
        }

        private string LimparCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;
            
            return Regex.Replace(cpf, @"[^\d]", "");
        }

        private string LimparCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return string.Empty;
            
            return Regex.Replace(cnpj, @"[^\d]", "");
        }

        private bool ValidarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = Regex.Replace(cpf, @"[^\d]", "");

            if (cpf.Length != 11)
                return false;

            // Verificar se todos os dígitos são iguais
            if (cpf.Distinct().Count() == 1)
                return false;

            var multiplicador1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicador2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCpf = cpf.Substring(0, 9);
            var soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            }

            var resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            var digito = resto.ToString();
            tempCpf += digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            digito += resto.ToString();

            return cpf.EndsWith(digito, StringComparison.Ordinal);
        }

        private bool ValidarCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            cnpj = Regex.Replace(cnpj, @"[^\d]", "");

            if (cnpj.Length != 14)
                return false;

            return true;
        }

        private bool ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private DateTime? TentarParsearData(IXLCell cell)
        {
            if (cell.IsEmpty())
                return null;

            try
            {
                // Tentar pegar como DateTime diretamente
                if (cell.DataType == XLDataType.DateTime)
                {
                    return cell.GetDateTime();
                }

                // Tentar parsear string
                var valorStr = cell.GetString();
                if (string.IsNullOrWhiteSpace(valorStr))
                    return null;

                // Formatos aceitos: DD/MM/AAAA, DD/MM/AA, AAAA-MM-DD
                var formatos = new[] { "dd/MM/yyyy", "dd/MM/yy", "yyyy-MM-dd", "dd-MM-yyyy" };
                
                if (DateTime.TryParseExact(valorStr, formatos, CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out DateTime data))
                {
                    return data;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private string ObterStatusDescricao(string status)
        {
            return status switch
            {
                "P" => "Pendente",
                "V" => "Validado",
                "E" => "Erro",
                "I" => "Importado",
                _ => "Desconhecido"
            };
        }

        private string ObterStatusDescricaoLog(string status)
        {
            return status switch
            {
                "PROCESSANDO" => "Processando",
                "VALIDADO" => "Validado",
                "PENDENTE_CORRECAO" => "Pendente de correção",
                "CONCLUIDO" => "Concluído",
                "ERRO" => "Erro",
                "CANCELADO" => "Cancelado",
                _ => "Desconhecido"
            };
        }

        private bool PossuiMudancas(ImportacaoColaboradorStaging registro, Colaboradore existente)
        {
            string Normalize(string value) => (value ?? string.Empty).Trim();
            bool StringsIguais(string a, string b) => string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);

            var nomeExistente = Normalize(existente.Nome);
            var matriculaExistente = Normalize(existente.Matricula);
            var emailExistente = Normalize(Cripto.CriptografarDescriptografar(existente.Email, false));
            var cargoExistente = Normalize(existente.Cargo);
            var setorExistente = Normalize(existente.Setor);
            var matriculaSuperiorExistente = Normalize(existente.Matriculasuperior);

            if (!StringsIguais(registro.NomeColaborador, nomeExistente)) return true;
            if (!StringsIguais(registro.Matricula, matriculaExistente)) return true;
            if (!string.IsNullOrWhiteSpace(registro.Email) && !StringsIguais(registro.Email, emailExistente)) return true;
            if (!string.IsNullOrWhiteSpace(registro.Cargo) && !StringsIguais(registro.Cargo, cargoExistente)) return true;
            if (!string.IsNullOrWhiteSpace(registro.Setor) && !StringsIguais(registro.Setor, setorExistente)) return true;
            if (!string.IsNullOrWhiteSpace(registro.MatriculaSuperior) && !StringsIguais(registro.MatriculaSuperior, matriculaSuperiorExistente)) return true;

            if (registro.DataAdmissao.HasValue && !DatasIguais(registro.DataAdmissao, existente.Dtadmissao)) return true;
            if (registro.DataDemissao.HasValue || existente.Dtdemissao.HasValue)
            {
                if (!DatasIguais(registro.DataDemissao, existente.Dtdemissao)) return true;
            }

            if (!string.IsNullOrWhiteSpace(registro.TipoColaborador))
            {
                var tipoRegistro = char.ToUpperInvariant(registro.TipoColaborador.Trim()[0]);
                if (existente.Tipocolaborador != tipoRegistro) return true;
            }

            if (registro.EmpresaId.HasValue)
            {
                if (existente.Empresa != registro.EmpresaId.Value) return true;
            }
            else if (registro.CriarEmpresa)
            {
                return true;
            }

            if (registro.CentroCustoId.HasValue)
            {
                if (existente.Centrocusto != registro.CentroCustoId.Value) return true;
            }
            else if (registro.CriarCentroCusto)
            {
                return true;
            }

            if (registro.LocalidadeId.HasValue)
            {
                if (existente.Localidade != registro.LocalidadeId.Value) return true;
            }
            else if (registro.CriarLocalidade)
            {
                return true;
            }

            var filialRegistro = registro.FilialId ?? 0;
            var filialExistente = existente.FilialId ?? 0;
            if (filialRegistro != filialExistente) return true;

            return false;
        }

        private List<string> ExtrairMensagens(string mensagensJson)
        {
            var mensagens = new List<string>();

            if (string.IsNullOrWhiteSpace(mensagensJson))
                return mensagens;

            try
            {
                var objeto = JsonConvert.DeserializeObject<dynamic>(mensagensJson);
                if (objeto == null)
                    return mensagens;

                if (objeto.erros != null)
                {
                    foreach (var erro in objeto.erros)
                    {
                        var mensagem = (string?)erro;
                        if (!string.IsNullOrWhiteSpace(mensagem))
                            mensagens.Add(mensagem);
                    }
                }

                if (objeto.avisos != null)
                {
                    foreach (var aviso in objeto.avisos)
                    {
                        var mensagem = (string?)aviso;
                        if (!string.IsNullOrWhiteSpace(mensagem))
                            mensagens.Add($"Aviso: {mensagem}");
                    }
                }
            }
            catch
            {
                mensagens.Add(mensagensJson);
            }

            return mensagens;
        }

        public async Task<RecriptografarDocumentosResultadoDTO> RecriptografarDocumentosCliente(int clienteId, int usuarioId, bool incluirEmails = true)
        {
            var colaboradores = await _context.Colaboradores
                .Where(x => x.Cliente == clienteId)
                .ToListAsync();

            var resultado = new RecriptografarDocumentosResultadoDTO();
            var agora = DateTime.Now;

            foreach (var colaborador in colaboradores)
            {
                var alterou = false;

                if (!string.IsNullOrWhiteSpace(colaborador.Cpf))
                {
                    var cpfAtual = colaborador.Cpf.Trim();
                    var cpfLimpo = LimparCpf(Cripto.CriptografarDescriptografar(cpfAtual, false));

                    if (!string.IsNullOrWhiteSpace(cpfLimpo))
                    {
                        var cpfCriptografado = CriptografarCpf(cpfLimpo);

                        if (!string.Equals(cpfCriptografado, cpfAtual, StringComparison.Ordinal))
                        {
                            colaborador.Cpf = cpfCriptografado;
                            resultado.TotalCpfAtualizados++;
                            alterou = true;
                        }
                    }
                }

                if (incluirEmails && !string.IsNullOrWhiteSpace(colaborador.Email))
                {
                    var emailAtual = colaborador.Email.Trim();
                    var emailTexto = Cripto.CriptografarDescriptografar(emailAtual, false);

                    if (!string.IsNullOrWhiteSpace(emailTexto))
                    {
                        var emailCriptografado = CriptografarEmail(emailTexto);
                        if (!string.Equals(emailCriptografado, emailAtual, StringComparison.Ordinal))
                        {
                            colaborador.Email = emailCriptografado;
                            resultado.TotalEmailsAtualizados++;
                            alterou = true;
                        }
                    }
                }

                if (alterou)
                {
                    colaborador.Dtatualizacao = agora;
                    resultado.TotalAtualizados++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"[IMPORTACAO-COLABORADORES] Recriptografia concluída - Cliente: {clienteId} - CPFs: {resultado.TotalCpfAtualizados} - Emails: {resultado.TotalEmailsAtualizados}");

            return resultado;
        }

        private string CriptografarCpf(string cpf)
        {
            var limpo = LimparCpf(cpf);
            if (string.IsNullOrWhiteSpace(limpo))
                return string.Empty;

            return Cripto.CriptografarDescriptografar(limpo, true);
        }

        private string CriptografarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            return Cripto.CriptografarDescriptografar(email.Trim(), true);
        }

        // Classe interna para leitura do Excel
        private class ColaboradorArquivoDTO
        {
            public string Nome { get; set; }
            public string Cpf { get; set; }
            public string Matricula { get; set; }
            public string Email { get; set; }
            public string Cargo { get; set; }
            public string Setor { get; set; }
            public DateTime? DataAdmissao { get; set; }
            public string TipoColaborador { get; set; }
            public string EmpresaNome { get; set; }
            public string EmpresaCnpj { get; set; }
            public string LocalidadeDescricao { get; set; }
            public string LocalidadeCidade { get; set; }
            public string LocalidadeEstado { get; set; }
            public string CentroCustoCodigo { get; set; }
            public string CentroCustoNome { get; set; }
            public string FilialNome { get; set; }
            public string FilialCnpj { get; set; }
            public DateTime? DataDemissao { get; set; }
            public string MatriculaSuperior { get; set; }
        }
    }
}

