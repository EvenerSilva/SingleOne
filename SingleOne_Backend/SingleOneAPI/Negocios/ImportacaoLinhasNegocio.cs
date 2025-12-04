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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios
{
    public class ImportacaoLinhasNegocio : IImportacaoLinhasNegocio
    {
        private readonly SingleOneDbContext _context;
        private readonly ILogger<ImportacaoLinhasNegocio> _logger;

        public ImportacaoLinhasNegocio(
            SingleOneDbContext context,
            ILogger<ImportacaoLinhasNegocio> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ResultadoValidacaoDTO> ProcessarArquivo(IFormFile arquivo, int clienteId, int usuarioId)
        {
            _logger.LogInformation($"[IMPORTACAO-LINHAS] Iniciando processamento de arquivo: {arquivo.FileName}");

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
                var linhasArquivo = await LerArquivoExcel(arquivo);
                
                if (linhasArquivo.Count == 0)
                    throw new Exception("Arquivo não contém dados válidos");

                if (linhasArquivo.Count > 5000)
                    throw new Exception("Arquivo excede o limite de 5000 linhas por importação");

                _logger.LogInformation($"[IMPORTACAO-LINHAS] {linhasArquivo.Count} linhas lidas do arquivo");

                // Inserir dados na staging
                var registrosStaging = new List<ImportacaoLinhaStaging>();
                
                foreach (var (linha, index) in linhasArquivo.Select((l, i) => (l, i)))
                {
                    registrosStaging.Add(new ImportacaoLinhaStaging
                    {
                        LoteId = loteId,
                        Cliente = clienteId,
                        UsuarioImportacao = usuarioId,
                        DataImportacao = dataImportacao,
                        OperadoraNome = linha.Operadora?.Trim(),
                        ContratoNome = linha.Contrato?.Trim(),
                        PlanoNome = linha.Plano?.Trim(),
                        PlanoValor = linha.PlanoValor,
                        NumeroLinha = linha.Numero,
                        Iccid = linha.Iccid?.Trim(),
                        Status = "P", // Pendente
                        LinhaArquivo = index + 2, // Excel começa na linha 2 (1 é header)
                        MensagensValidacao = "{}"
                    });
                }

                await _context.ImportacaoLinhasStaging.AddRangeAsync(registrosStaging);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[IMPORTACAO-LINHAS] {registrosStaging.Count} registros inseridos na staging");

                // Executar validações
                await ValidarLote(loteId, clienteId);

                // Criar log de importação
                var log = new ImportacaoLog
                {
                    LoteId = loteId,
                    Cliente = clienteId,
                    Usuario = usuarioId,
                    TipoImportacao = "LINHAS",
                    DataInicio = dataImportacao,
                    Status = "PROCESSANDO",
                    NomeArquivo = arquivo.FileName,
                    TotalRegistros = registrosStaging.Count
                };

                _context.ImportacaoLogs.Add(log);
                await _context.SaveChangesAsync();

                // Gerar resumo
                var resumo = await ObterResumoValidacao(loteId, clienteId);

                var resultadoDTO = new ResultadoValidacaoDTO
                {
                    LoteId = loteId,
                    TotalRegistros = resumo.Total,
                    TotalValidos = resumo.Validos,
                    TotalAvisos = resumo.Avisos,
                    TotalErros = resumo.Erros,
                    NovasOperadoras = resumo.NovasOperadoras,
                    NovosContratos = resumo.NovosContratos,
                    NovosPlanos = resumo.NovosPlanos,
                    PodeImportar = resumo.Erros == 0,
                    Mensagem = resumo.Erros == 0 
                        ? "Validação concluída com sucesso. Todos os registros estão prontos para importação."
                        : $"Validação concluída com {resumo.Erros} erro(s). Corrija os erros antes de importar."
                };

                _logger.LogInformation($"[IMPORTACAO-LINHAS] 📦 ResultadoDTO criado:");
                _logger.LogInformation($"[IMPORTACAO-LINHAS] 📦   - LoteId (Guid): {resultadoDTO.LoteId}");
                _logger.LogInformation($"[IMPORTACAO-LINHAS] 📦   - TotalRegistros: {resultadoDTO.TotalRegistros}");
                _logger.LogInformation($"[IMPORTACAO-LINHAS] 📦   - TotalValidos: {resultadoDTO.TotalValidos}");
                _logger.LogInformation($"[IMPORTACAO-LINHAS] 📦   - TotalErros: {resultadoDTO.TotalErros}");
                _logger.LogInformation($"[IMPORTACAO-LINHAS] 📦   - Status dos registros:");
                _logger.LogInformation($"[IMPORTACAO-LINHAS] 📦     - Pendentes (P): {resumo.Pendentes}");
                _logger.LogInformation($"[IMPORTACAO-LINHAS] 📦     - Validados (V): {resumo.Validos}");
                _logger.LogInformation($"[IMPORTACAO-LINHAS] 📦     - Erros (E): {resumo.Erros}");

                return resultadoDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[IMPORTACAO-LINHAS] Erro ao processar arquivo: {ex.Message}");
                throw new Exception($"Erro ao processar arquivo: {ex.Message}");
            }
        }

        private async Task<List<LinhaArquivoDTO>> LerArquivoExcel(IFormFile arquivo)
        {
            var linhas = new List<LinhaArquivoDTO>();

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
                            var operadora = worksheet.Cell(linhaAtual, 1).GetString();
                            var contrato = worksheet.Cell(linhaAtual, 2).GetString();
                            var plano = worksheet.Cell(linhaAtual, 3).GetString();
                            var valorStr = worksheet.Cell(linhaAtual, 4).GetString();
                            var numeroStr = worksheet.Cell(linhaAtual, 5).GetString();
                            var iccid = worksheet.Cell(linhaAtual, 6).GetString();

                            // Tentar parsear valor
                            decimal valor = 0;
                            if (!string.IsNullOrEmpty(valorStr))
                            {
                                valorStr = valorStr.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim();
                                decimal.TryParse(valorStr, System.Globalization.NumberStyles.Any, 
                                    System.Globalization.CultureInfo.InvariantCulture, out valor);
                            }

                            // Tentar parsear número da linha
                            decimal numero = 0;
                            if (!string.IsNullOrEmpty(numeroStr))
                            {
                                // Remove caracteres não numéricos
                                numeroStr = new string(numeroStr.Where(char.IsDigit).ToArray());
                                decimal.TryParse(numeroStr, out numero);
                            }

                            linhas.Add(new LinhaArquivoDTO
                            {
                                Operadora = operadora,
                                Contrato = contrato,
                                Plano = plano,
                                PlanoValor = valor,
                                Numero = numero,
                                Iccid = iccid
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"[IMPORTACAO-LINHAS] Erro ao ler linha {linhaAtual}: {ex.Message}");
                        }

                        linhaAtual++;
                        
                        // Limitar a 5000 linhas
                        if (linhaAtual > 5001)
                            break;
                    }
                }
            }

            return linhas;
        }

        private async Task ValidarLote(Guid loteId, int clienteId)
        {
            _logger.LogInformation($"[IMPORTACAO-LINHAS] Iniciando validação do lote: {loteId}");

            // IMPORTANTE: AsTracking() para garantir que o EF Core rastreie as mudanças
            var registros = await _context.ImportacaoLinhasStaging
                .AsTracking()
                .Where(x => x.LoteId == loteId && x.Cliente == clienteId)
                .ToListAsync();

            // Carregar dados necessários para validação
            var operadoras = await _context.Telefoniaoperadoras
                .Where(x => x.Ativo)
                .ToListAsync();

            var contratos = await _context.Telefoniacontratos
                .Where(x => x.Cliente == clienteId && x.Ativo)
                .Include(x => x.OperadoraNavigation)
                .ToListAsync();

            var planos = await _context.Telefoniaplanos
                .Where(x => x.Ativo)
                .Include(x => x.ContratoNavigation)
                .ToListAsync();

            var linhasExistentes = await _context.Telefonialinhas
                .Select(x => x.Numero)
                .ToListAsync();

            foreach (var registro in registros)
            {
                var erros = new List<string>();
                var avisos = new List<string>();

                // ========== VALIDAÇÃO 1: Campos obrigatórios ==========
                if (string.IsNullOrWhiteSpace(registro.OperadoraNome))
                    erros.Add("Operadora é obrigatória");
                
                if (string.IsNullOrWhiteSpace(registro.ContratoNome))
                    erros.Add("Contrato é obrigatório");
                
                if (string.IsNullOrWhiteSpace(registro.PlanoNome))
                    erros.Add("Plano é obrigatório");
                
                if (registro.PlanoValor <= 0)
                    erros.Add("Valor do plano deve ser maior que zero");
                
                if (registro.NumeroLinha == 0)
                    erros.Add("Número da linha é obrigatório");

                // Se faltam campos obrigatórios, pular validações seguintes
                if (erros.Any())
                {
                    registro.Status = "E";
                    registro.MensagensValidacao = JsonConvert.SerializeObject(new { erros, avisos });
                    continue;
                }

                // ========== VALIDAÇÃO 2: Operadora ==========
                var operadora = operadoras.FirstOrDefault(x => 
                    x.Nome.Trim().ToLower() == registro.OperadoraNome.Trim().ToLower());

                if (operadora != null)
                {
                    registro.OperadoraId = operadora.Id;
                }
                else
                {
                    registro.CriarOperadora = true;
                    avisos.Add($"✨ Operadora '{registro.OperadoraNome}' será criada automaticamente");
                }

                // ========== VALIDAÇÃO 3: Contrato ==========
                if (registro.OperadoraId.HasValue)
                {
                    var contrato = contratos.FirstOrDefault(x =>
                        x.Nome.Trim().ToLower() == registro.ContratoNome.Trim().ToLower() &&
                        x.Operadora == registro.OperadoraId.Value);

                    if (contrato != null)
                    {
                        registro.ContratoId = contrato.Id;
                    }
                    else
                    {
                        registro.CriarContrato = true;
                        avisos.Add($"✨ Contrato '{registro.ContratoNome}' será criado automaticamente");
                    }
                }
                else if (registro.CriarOperadora)
                {
                    // Se operadora será criada, contrato também será
                    registro.CriarContrato = true;
                    avisos.Add($"✨ Contrato '{registro.ContratoNome}' será criado automaticamente");
                }

                // ========== VALIDAÇÃO 4: Plano ==========
                if (registro.ContratoId.HasValue)
                {
                    var plano = planos.FirstOrDefault(x =>
                        x.Nome.Trim().ToLower() == registro.PlanoNome.Trim().ToLower() &&
                        x.Contrato == registro.ContratoId.Value);

                    if (plano != null)
                    {
                        registro.PlanoId = plano.Id;

                        // Verificar divergência de valor
                        if (Math.Abs(plano.Valor - registro.PlanoValor) > 0.01m)
                        {
                            avisos.Add($"⚠️ Valor divergente - Cadastrado: R$ {plano.Valor:N2} / Arquivo: R$ {registro.PlanoValor:N2}");
                        }
                    }
                    else
                    {
                        registro.CriarPlano = true;
                        avisos.Add($"✨ Plano '{registro.PlanoNome}' (R$ {registro.PlanoValor:N2}) será criado automaticamente");
                    }
                }
                else if (registro.CriarContrato)
                {
                    // Se contrato será criado, plano também será
                    registro.CriarPlano = true;
                    avisos.Add($"✨ Plano '{registro.PlanoNome}' (R$ {registro.PlanoValor:N2}) será criado automaticamente");
                }

                // ========== VALIDAÇÃO 5: Linha já existe? ==========
                if (linhasExistentes.Contains(registro.NumeroLinha))
                {
                    erros.Add($"❌ Linha {registro.NumeroLinha} já cadastrada no sistema");
                }

                // ========== VALIDAÇÃO 6: Duplicata no lote ==========
                var duplicataNoLote = registros
                    .Where(x => x.Id != registro.Id && x.NumeroLinha == registro.NumeroLinha)
                    .Any();

                if (duplicataNoLote)
                {
                    erros.Add("❌ Número duplicado no arquivo de importação");
                }

                // ========== VALIDAÇÃO 7: Formato do número ==========
                if (registro.NumeroLinha.ToString().Length < 10 || registro.NumeroLinha.ToString().Length > 11)
                {
                    avisos.Add("⚠️ Número da linha com formato incomum (esperado 10 ou 11 dígitos)");
                }

                // ========== VALIDAÇÃO 8: ICCID ==========
                if (string.IsNullOrWhiteSpace(registro.Iccid))
                {
                    avisos.Add("⚠️ ICCID não informado");
                }

                // ========== ATUALIZAR STATUS ==========
                registro.MensagensValidacao = JsonConvert.SerializeObject(new { erros, avisos });

                if (erros.Any())
                {
                    registro.Status = "E"; // Erro
                    _logger.LogInformation($"[IMPORTACAO-LINHAS] 📝 Linha {registro.LinhaArquivo}: Status = E (Erro) - {erros.Count} erro(s)");
                }
                else if (avisos.Any())
                {
                    registro.Status = "V"; // Validado com avisos
                    _logger.LogInformation($"[IMPORTACAO-LINHAS] 📝 Linha {registro.LinhaArquivo}: Status = V (Validado com {avisos.Count} aviso(s))");
                }
                else
                {
                    registro.Status = "V"; // Validado
                    _logger.LogInformation($"[IMPORTACAO-LINHAS] 📝 Linha {registro.LinhaArquivo}: Status = V (Validado sem avisos)");
                }
            }

            _logger.LogInformation($"[IMPORTACAO-LINHAS] ⏳ Salvando alterações de status no banco...");
            
            // O EF Core já está rastreando as mudanças automaticamente (query com AsNoTracking = false)
            // Apenas salvamos as mudanças
            var linhasAfetadas = await _context.SaveChangesAsync();
            _logger.LogInformation($"[IMPORTACAO-LINHAS] ✅ Status salvos com sucesso! {linhasAfetadas} linha(s) atualizada(s)");
            
            // IMPORTANTE: Limpar o tracking do contexto para forçar nova leitura do banco
            _context.ChangeTracker.Clear();
            
            // Verificar status após salvar (fazendo nova query no banco)
            var statusContagem = await _context.ImportacaoLinhasStaging
                .Where(x => x.LoteId == loteId && x.Cliente == clienteId)
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            
            foreach (var item in statusContagem)
            {
                _logger.LogInformation($"[IMPORTACAO-LINHAS] 📊 Status '{item.Status}' (do banco): {item.Count} registro(s)");
            }
            
            _logger.LogInformation($"[IMPORTACAO-LINHAS] Validação concluída para lote: {loteId}");
        }

        public async Task<List<DetalheLinhaStagingDTO>> ObterDetalhesValidacao(Guid loteId, int clienteId, string filtroStatus = null)
        {
            var query = _context.ImportacaoLinhasStaging
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

                return new DetalheLinhaStagingDTO
                {
                    Id = r.Id,
                    LinhaArquivo = r.LinhaArquivo,
                    OperadoraNome = r.OperadoraNome,
                    ContratoNome = r.ContratoNome,
                    PlanoNome = r.PlanoNome,
                    PlanoValor = r.PlanoValor,
                    NumeroLinha = r.NumeroLinha,
                    Iccid = r.Iccid,
                    Status = r.Status,
                    StatusDescricao = ObterStatusDescricao(r.Status),
                    Erros = mensagens.erros.ToObject<List<string>>(),
                    Avisos = mensagens.avisos.ToObject<List<string>>(),
                    CriarOperadora = r.CriarOperadora,
                    CriarContrato = r.CriarContrato,
                    CriarPlano = r.CriarPlano
                };
            }).ToList();

            return resultado;
        }

        public async Task<ResumoValidacaoDTO> ObterResumoValidacao(Guid loteId, int clienteId)
        {
            var registros = await _context.ImportacaoLinhasStaging
                .Where(x => x.LoteId == loteId && x.Cliente == clienteId)
                .ToListAsync();

            var resumo = new ResumoValidacaoDTO
            {
                LoteId = loteId,
                Total = registros.Count,
                Validos = registros.Count(x => x.Status == "V"),
                Erros = registros.Count(x => x.Status == "E"),
                Pendentes = registros.Count(x => x.Status == "P"),
                Importados = registros.Count(x => x.Status == "I"),
                NovasOperadoras = registros.Where(x => x.CriarOperadora).GroupBy(x => x.OperadoraNome.ToLower()).Count(),
                NovosContratos = registros.Where(x => x.CriarContrato).GroupBy(x => new { 
                    Operadora = x.OperadoraNome.ToLower(), 
                    Contrato = x.ContratoNome.ToLower() 
                }).Count(),
                NovosPlanos = registros.Where(x => x.CriarPlano).GroupBy(x => new { 
                    Operadora = x.OperadoraNome.ToLower(),
                    Contrato = x.ContratoNome.ToLower(),
                    Plano = x.PlanoNome.ToLower() 
                }).Count(),
                NomesOperadorasNovas = registros.Where(x => x.CriarOperadora)
                    .GroupBy(x => x.OperadoraNome)
                    .Select(g => g.First().OperadoraNome)
                    .ToList(),
                NomesContratosNovos = registros.Where(x => x.CriarContrato)
                    .GroupBy(x => x.ContratoNome)
                    .Select(g => g.First().ContratoNome)
                    .ToList(),
                NomesPlanosNovos = registros.Where(x => x.CriarPlano)
                    .GroupBy(x => x.PlanoNome)
                    .Select(g => g.First().PlanoNome)
                    .ToList()
            };

            // Contar avisos (registros validados com mensagens de aviso)
            resumo.Avisos = registros.Count(x => x.Status == "V" && !string.IsNullOrEmpty(x.MensagensValidacao) &&
                x.MensagensValidacao.Contains("avisos") && x.MensagensValidacao.Contains("["));

            return resumo;
        }

        public async Task<ResultadoImportacaoDTO> EfetivarImportacao(Guid loteId, int clienteId, int usuarioId)
        {
            _logger.LogInformation($"[IMPORTACAO-LINHAS] Iniciando efetivação da importação - Lote: {loteId}");

            // Usar estratégia de execução do contexto para suportar transações com retry
            var strategy = _context.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var registros = await _context.ImportacaoLinhasStaging
                    .Where(x => x.LoteId == loteId && x.Cliente == clienteId && x.Status == "V")
                    .ToListAsync();

                if (!registros.Any())
                    throw new Exception("Não há registros válidos para importar");

                var resultado = new ResultadoImportacaoDTO
                {
                    LoteId = loteId,
                    DataInicio = DateTime.Now
                };

                // ========== ETAPA 1: Criar Operadoras novas ==========
                var operadorasNovas = registros
                    .Where(x => x.CriarOperadora)
                    .GroupBy(x => x.OperadoraNome.Trim().ToLower())
                    .ToList();

                _logger.LogInformation($"[IMPORTACAO-LINHAS] Criando {operadorasNovas.Count} operadoras novas");

                foreach (var grupo in operadorasNovas)
                {
                    var novaOperadora = new Telefoniaoperadora
                    {
                        Nome = grupo.First().OperadoraNome,
                        Ativo = true
                    };

                    _context.Telefoniaoperadoras.Add(novaOperadora);
                    await _context.SaveChangesAsync();

                    // Atualizar ID nos registros staging
                    foreach (var reg in grupo)
                    {
                        reg.OperadoraId = novaOperadora.Id;
                    }

                    resultado.OperadorasCriadas++;
                    _logger.LogInformation($"[IMPORTACAO-LINHAS] Operadora criada: {novaOperadora.Nome} (ID: {novaOperadora.Id})");
                }

                // ========== ETAPA 2: Criar Contratos novos ==========
                var contratosNovos = registros
                    .Where(x => x.CriarContrato && x.OperadoraId.HasValue)
                    .GroupBy(x => new { 
                        Operadora = x.OperadoraId.Value, 
                        Nome = x.ContratoNome.Trim().ToLower() 
                    })
                    .ToList();

                _logger.LogInformation($"[IMPORTACAO-LINHAS] Criando {contratosNovos.Count} contratos novos");

                foreach (var grupo in contratosNovos)
                {
                    var novoContrato = new Telefoniacontrato
                    {
                        Cliente = clienteId,
                        Operadora = grupo.Key.Operadora,
                        Nome = grupo.First().ContratoNome,
                        Descricao = $"Criado via importação em {DateTime.Now:dd/MM/yyyy HH:mm}",
                        Ativo = true
                    };

                    _context.Telefoniacontratos.Add(novoContrato);
                    await _context.SaveChangesAsync();

                    foreach (var reg in grupo)
                    {
                        reg.ContratoId = novoContrato.Id;
                    }

                    resultado.ContratosCriados++;
                    _logger.LogInformation($"[IMPORTACAO-LINHAS] Contrato criado: {novoContrato.Nome} (ID: {novoContrato.Id})");
                }

                // ========== ETAPA 3: Criar Planos novos ==========
                var planosNovos = registros
                    .Where(x => x.CriarPlano && x.ContratoId.HasValue)
                    .GroupBy(x => new { 
                        Contrato = x.ContratoId.Value, 
                        Nome = x.PlanoNome.Trim().ToLower() 
                    })
                    .ToList();

                _logger.LogInformation($"[IMPORTACAO-LINHAS] Criando {planosNovos.Count} planos novos");

                foreach (var grupo in planosNovos)
                {
                    var novoPlano = new Telefoniaplano
                    {
                        Contrato = grupo.Key.Contrato,
                        Nome = grupo.First().PlanoNome,
                        Valor = grupo.First().PlanoValor,
                        Ativo = true
                    };

                    _context.Telefoniaplanos.Add(novoPlano);
                    await _context.SaveChangesAsync();

                    foreach (var reg in grupo)
                    {
                        reg.PlanoId = novoPlano.Id;
                    }

                    resultado.PlanosCriados++;
                    _logger.LogInformation($"[IMPORTACAO-LINHAS] Plano criado: {novoPlano.Nome} (ID: {novoPlano.Id})");
                }

                // ========== ETAPA 4: Criar Linhas ==========
                _logger.LogInformation($"[IMPORTACAO-LINHAS] Criando {registros.Count} linhas telefônicas");

                foreach (var registro in registros)
                {
                    if (!registro.PlanoId.HasValue)
                    {
                        _logger.LogWarning($"[IMPORTACAO-LINHAS] Registro sem PlanoId - Linha {registro.LinhaArquivo}");
                        continue;
                    }

                    var novaLinha = new Telefonialinha
                    {
                        Plano = registro.PlanoId.Value,
                        Numero = registro.NumeroLinha,
                        Iccid = registro.Iccid ?? string.Empty,
                        Emuso = false,  // ← SEMPRE FALSE NA IMPORTAÇÃO
                        Ativo = true
                    };

                    _context.Telefonialinhas.Add(novaLinha);
                    registro.Status = "I"; // Importado

                    resultado.LinhasCriadas++;
                }

                await _context.SaveChangesAsync();

                // ========== ETAPA 5: Atualizar log ==========
                var log = await _context.ImportacaoLogs
                    .FirstOrDefaultAsync(x => x.LoteId == loteId && x.Cliente == clienteId);

                if (log != null)
                {
                    log.Status = "CONCLUIDO";
                    log.DataFim = DateTime.Now;
                    log.TotalValidados = resultado.LinhasCriadas;
                    log.TotalImportados = resultado.LinhasCriadas;
                    log.Observacoes = $"Operadoras: {resultado.OperadorasCriadas}, Contratos: {resultado.ContratosCriados}, Planos: {resultado.PlanosCriados}, Linhas: {resultado.LinhasCriadas}";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                resultado.DataFim = DateTime.Now;
                resultado.TotalProcessado = resultado.LinhasCriadas;
                resultado.Sucesso = true;
                resultado.Mensagem = $"Importação concluída com sucesso! {resultado.LinhasCriadas} linhas criadas.";

                _logger.LogInformation($"[IMPORTACAO-LINHAS] Importação concluída - Lote: {loteId} - Linhas: {resultado.LinhasCriadas}");

                return resultado;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"[IMPORTACAO-LINHAS] Erro ao efetivar importação: {ex.Message}");
                throw new Exception($"Erro ao efetivar importação: {ex.Message}");
            }
            });
        }

        public async Task<bool> LimparStaging(Guid loteId, int clienteId)
        {
            try
            {
                var registros = await _context.ImportacaoLinhasStaging
                    .Where(x => x.LoteId == loteId && x.Cliente == clienteId)
                    .ToListAsync();

                _context.ImportacaoLinhasStaging.RemoveRange(registros);

                var log = await _context.ImportacaoLogs
                    .FirstOrDefaultAsync(x => x.LoteId == loteId && x.Cliente == clienteId);

                if (log != null)
                {
                    log.Status = "CANCELADO";
                    log.DataFim = DateTime.Now;
                    log.Observacoes = "Importação cancelada pelo usuário";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"[IMPORTACAO-LINHAS] Lote {loteId} cancelado e limpo");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[IMPORTACAO-LINHAS] Erro ao limpar staging: {ex.Message}");
                return false;
            }
        }

        public async Task<List<HistoricoImportacaoDTO>> ObterHistorico(int clienteId, int? limite = 50)
        {
            var historico = await _context.ImportacaoLogs
                .Where(x => x.Cliente == clienteId && x.TipoImportacao == "LINHAS")
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
                TotalImportados = h.TotalImportados ?? 0,
                UsuarioNome = h.UsuarioNavigation?.Nome ?? "Sistema",
                UsuarioEmail = h.UsuarioNavigation?.Email ?? "",
                Observacoes = h.Observacoes
            }).ToList();
        }

        public byte[] GerarTemplateExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Linhas Telefônicas");

                // Definir cabeçalhos
                worksheet.Cell(1, 1).Value = "Operadora";
                worksheet.Cell(1, 2).Value = "Contrato";
                worksheet.Cell(1, 3).Value = "Plano";
                worksheet.Cell(1, 4).Value = "Valor";
                worksheet.Cell(1, 5).Value = "Número";
                worksheet.Cell(1, 6).Value = "ICCID";

                // Estilizar cabeçalho
                var headerRange = worksheet.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Adicionar exemplos
                worksheet.Cell(2, 1).Value = "Vivo";
                worksheet.Cell(2, 2).Value = "Contrato Corporativo 2024";
                worksheet.Cell(2, 3).Value = "Plano Básico";
                worksheet.Cell(2, 4).Value = 49.90;
                worksheet.Cell(2, 5).Value = "11987654321";
                worksheet.Cell(2, 6).Value = "8955000012345678901";

                worksheet.Cell(3, 1).Value = "Claro";
                worksheet.Cell(3, 2).Value = "Contrato Empresarial";
                worksheet.Cell(3, 3).Value = "Plano Premium";
                worksheet.Cell(3, 4).Value = 89.90;
                worksheet.Cell(3, 5).Value = "11912345678";
                worksheet.Cell(3, 6).Value = "8955000087654321098";

                // Ajustar largura das colunas
                worksheet.Column(1).Width = 20;
                worksheet.Column(2).Width = 30;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 12;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 25;

                // Adicionar instruções em outra aba
                var instrucoes = workbook.Worksheets.Add("Instruções");
                instrucoes.Cell(1, 1).Value = "INSTRUÇÕES PARA IMPORTAÇÃO DE LINHAS TELEFÔNICAS";
                instrucoes.Cell(1, 1).Style.Font.Bold = true;
                instrucoes.Cell(1, 1).Style.Font.FontSize = 14;

                instrucoes.Cell(3, 1).Value = "📋 COLUNAS OBRIGATÓRIAS:";
                instrucoes.Cell(3, 1).Style.Font.Bold = true;
                instrucoes.Cell(4, 1).Value = "• Operadora: Nome da operadora (ex: Vivo, Claro, TIM)";
                instrucoes.Cell(5, 1).Value = "• Contrato: Nome do contrato";
                instrucoes.Cell(6, 1).Value = "• Plano: Nome do plano";
                instrucoes.Cell(7, 1).Value = "• Valor: Valor mensal do plano (ex: 49.90)";
                instrucoes.Cell(8, 1).Value = "• Número: Número da linha com DDD (ex: 11987654321)";

                instrucoes.Cell(10, 1).Value = "📋 COLUNAS OPCIONAIS:";
                instrucoes.Cell(10, 1).Style.Font.Bold = true;
                instrucoes.Cell(11, 1).Value = "• ICCID: Código do chip (recomendado)";

                instrucoes.Cell(13, 1).Value = "✨ O SISTEMA CRIARÁ AUTOMATICAMENTE:";
                instrucoes.Cell(13, 1).Style.Font.Bold = true;
                instrucoes.Cell(14, 1).Value = "• Operadoras que não existirem";
                instrucoes.Cell(15, 1).Value = "• Contratos que não existirem";
                instrucoes.Cell(16, 1).Value = "• Planos que não existirem";

                instrucoes.Cell(18, 1).Value = "⚠️ IMPORTANTE:";
                instrucoes.Cell(18, 1).Style.Font.Bold = true;
                instrucoes.Cell(19, 1).Value = "• Não altere o nome das colunas";
                instrucoes.Cell(20, 1).Value = "• Linhas duplicadas no arquivo serão rejeitadas";
                instrucoes.Cell(21, 1).Value = "• Linhas que já existem no sistema serão rejeitadas";
                instrucoes.Cell(22, 1).Value = "• Máximo de 5000 linhas por importação";

                instrucoes.Column(1).Width = 80;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
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
                "CONCLUIDO" => "Concluído",
                "ERRO" => "Erro",
                "CANCELADO" => "Cancelado",
                _ => "Desconhecido"
            };
        }

        // Classe interna para leitura do Excel
        private class LinhaArquivoDTO
        {
            public string Operadora { get; set; }
            public string Contrato { get; set; }
            public string Plano { get; set; }
            public decimal PlanoValor { get; set; }
            public decimal Numero { get; set; }
            public string Iccid { get; set; }
        }
    }
}

