using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Infra.Contexto;
using SingleOneAPI.DTOs;
using SingleOneAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOneAPI.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly SingleOneDbContext _context;

        public CategoriaService(SingleOneDbContext context)
        {
            _context = context;
        }

        public async Task<CategoriaListResponseDTO> ListarCategoriasAsync(string filtro = null)
        {
            try
            {
                var query = _context.Categorias.AsQueryable();

                // Listar todas as categorias (ativas e inativas) para permitir reativação
                // Não filtrar por ativo para que categorias desativadas possam ser visualizadas e reativadas

                // Aplicar filtro se fornecido
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    var filtroLower = filtro.ToLower();
                    query = query.Where(c => 
                        c.Nome.ToLower().Contains(filtroLower) ||
                        (c.Descricao != null && c.Descricao.ToLower().Contains(filtroLower))
                    );
                }

                // Ordenar por nome
                query = query.OrderBy(c => c.Nome);

                var categorias = await query.ToListAsync();

                // Converter para DTOs e contar tipos de equipamento
                var categoriasDto = new List<CategoriaDTO>();
                foreach (var categoria in categorias)
                {
                    var totalTipos = await ContarTiposEquipamentoPorCategoriaAsync(categoria.Id);
                    categoriasDto.Add(new CategoriaDTO
                    {
                        Id = categoria.Id,
                        Nome = categoria.Nome,
                        Descricao = categoria.Descricao,
                        Ativo = categoria.Ativo,
                        DataCriacao = categoria.DataCriacao,
                        DataAtualizacao = categoria.DataAtualizacao,
                        TotalTiposEquipamento = totalTipos
                    });
                }

                return new CategoriaListResponseDTO
                {
                    Sucesso = true,
                    Mensagem = "Categorias listadas com sucesso",
                    Dados = categoriasDto,
                    Status = 200,
                    Total = categoriasDto.Count
                };
            }
            catch (Exception ex)
            {
                return new CategoriaListResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao listar categorias: {ex.Message}",
                    Dados = new List<CategoriaDTO>(),
                    Status = 500,
                    Total = 0
                };
            }
        }

        public async Task<CategoriaResponseDTO> BuscarCategoriaPorIdAsync(int id)
        {
            try
            {
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null)
                {
                    return new CategoriaResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Categoria não encontrada",
                        Dados = null,
                        Status = 404
                    };
                }

                var totalTipos = await ContarTiposEquipamentoPorCategoriaAsync(categoria.Id);
                var categoriaDto = new CategoriaDTO
                {
                    Id = categoria.Id,
                    Nome = categoria.Nome,
                    Descricao = categoria.Descricao,
                    Ativo = categoria.Ativo,
                    DataCriacao = categoria.DataCriacao,
                    DataAtualizacao = categoria.DataAtualizacao,
                    TotalTiposEquipamento = totalTipos
                };

                return new CategoriaResponseDTO
                {
                    Sucesso = true,
                    Mensagem = "Categoria encontrada com sucesso",
                    Dados = categoriaDto,
                    Status = 200
                };
            }
            catch (Exception ex)
            {
                return new CategoriaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao buscar categoria: {ex.Message}",
                    Dados = null,
                    Status = 500
                };
            }
        }

        public async Task<CategoriaResponseDTO> CriarCategoriaAsync(CategoriaCreateDTO categoriaDto)
        {
            try
            {
                // Verificar se nome já existe
                if (await VerificarNomeExistenteAsync(categoriaDto.Nome))
                {
                    return new CategoriaResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Já existe uma categoria com este nome",
                        Dados = null,
                        Status = 400
                    };
                }

                var categoria = new Categoria
                {
                    Nome = categoriaDto.Nome.Trim(),
                    Descricao = categoriaDto.Descricao?.Trim(),
                    Ativo = true,
                    DataCriacao = DateTime.Now,
                    DataAtualizacao = DateTime.Now
                };

                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();

                var categoriaCriadaDto = new CategoriaDTO
                {
                    Id = categoria.Id,
                    Nome = categoria.Nome,
                    Descricao = categoria.Descricao,
                    Ativo = categoria.Ativo,
                    DataCriacao = categoria.DataCriacao,
                    DataAtualizacao = categoria.DataAtualizacao,
                    TotalTiposEquipamento = 0
                };

                return new CategoriaResponseDTO
                {
                    Sucesso = true,
                    Mensagem = "Categoria criada com sucesso",
                    Dados = categoriaCriadaDto,
                    Status = 201
                };
            }
            catch (Exception ex)
            {
                return new CategoriaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao criar categoria: {ex.Message}",
                    Dados = null,
                    Status = 500
                };
            }
        }

        public async Task<CategoriaResponseDTO> AtualizarCategoriaAsync(CategoriaUpdateDTO categoriaDto)
        {
            try
            {
                var categoria = await _context.Categorias.FindAsync(categoriaDto.Id);
                if (categoria == null)
                {
                    return new CategoriaResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Categoria não encontrada",
                        Dados = null,
                        Status = 404
                    };
                }

                // Verificar se nome já existe (excluindo a categoria atual)
                if (await VerificarNomeExistenteAsync(categoriaDto.Nome, categoriaDto.Id))
                {
                    return new CategoriaResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Já existe uma categoria com este nome",
                        Dados = null,
                        Status = 400
                    };
                }

                Console.WriteLine($"[BACKEND] Atualizando categoria ID {categoria.Id}");
                Console.WriteLine($"[BACKEND] Nome recebido: '{categoriaDto.Nome}'");
                Console.WriteLine($"[BACKEND] Descrição recebida: '{categoriaDto.Descricao}'");
                Console.WriteLine($"[BACKEND] Ativo recebido: {categoriaDto.Ativo}");

                // Atualizar diretamente no banco para evitar problemas de tracking
                var sql = @"
                    UPDATE categorias 
                    SET nome = {0}, descricao = {1}, ativo = {2}, data_atualizacao = {3} 
                    WHERE id = {4}";

                var nome = categoriaDto.Nome.Trim();
                var descricao = categoriaDto.Descricao?.Trim() ?? "";
                var ativo = categoriaDto.Ativo;
                var dataAtualizacao = DateTime.Now;
                var id = categoria.Id;

                Console.WriteLine($"[BACKEND] Executando SQL direto:");
                Console.WriteLine($"[BACKEND] - Nome: '{nome}'");
                Console.WriteLine($"[BACKEND] - Descrição: '{descricao}'");
                Console.WriteLine($"[BACKEND] - Ativo: {ativo}");
                Console.WriteLine($"[BACKEND] - Data: {dataAtualizacao}");
                Console.WriteLine($"[BACKEND] - ID: {id}");

                try
                {
                    var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, nome, descricao, ativo, dataAtualizacao, id);
                    Console.WriteLine($"[BACKEND] SQL executado com sucesso. Linhas afetadas: {rowsAffected}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BACKEND] ERRO no SQL: {ex.Message}");
                    throw;
                }

                // Recarregar a categoria do banco
                await _context.Entry(categoria).ReloadAsync();

                Console.WriteLine($"[BACKEND] Categoria salva no banco. Verificando dados salvos...");
                var categoriaSalva = await _context.Categorias.FindAsync(categoria.Id);
                Console.WriteLine($"[BACKEND] Dados após SaveChanges - Nome: '{categoriaSalva.Nome}', Descrição: '{categoriaSalva.Descricao}'");

                var totalTipos = await ContarTiposEquipamentoPorCategoriaAsync(categoria.Id);
                var categoriaAtualizadaDto = new CategoriaDTO
                {
                    Id = categoria.Id,
                    Nome = categoria.Nome,
                    Descricao = categoria.Descricao,
                    Ativo = categoria.Ativo,
                    DataCriacao = categoria.DataCriacao,
                    DataAtualizacao = categoria.DataAtualizacao,
                    TotalTiposEquipamento = totalTipos
                };

                return new CategoriaResponseDTO
                {
                    Sucesso = true,
                    Mensagem = "Categoria atualizada com sucesso",
                    Dados = categoriaAtualizadaDto,
                    Status = 200
                };
            }
            catch (Exception ex)
            {
                return new CategoriaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao atualizar categoria: {ex.Message}",
                    Dados = null,
                    Status = 500
                };
            }
        }

        public async Task<CategoriaResponseDTO> DesativarCategoriaAsync(int id)
        {
            try
            {
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null)
                {
                    return new CategoriaResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Categoria não encontrada",
                        Dados = null,
                        Status = 404
                    };
                }

                // Atualizar diretamente no banco para garantir que a mudança seja persistida
                var sql = @"
                    UPDATE categorias 
                    SET ativo = {0}, data_atualizacao = {1} 
                    WHERE id = {2}";

                var ativo = false;
                var dataAtualizacao = DateTime.Now;

                Console.WriteLine($"[BACKEND] Desativando categoria ID {id}");
                Console.WriteLine($"[BACKEND] Executando SQL direto para desativar categoria");

                try
                {
                    var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, ativo, dataAtualizacao, id);
                    Console.WriteLine($"[BACKEND] SQL executado com sucesso. Linhas afetadas: {rowsAffected}");
                    
                    if (rowsAffected == 0)
                    {
                        return new CategoriaResponseDTO
                        {
                            Sucesso = false,
                            Mensagem = "Nenhuma categoria foi atualizada. Verifique se o ID existe.",
                            Dados = null,
                            Status = 404
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BACKEND] ERRO no SQL: {ex.Message}");
                    throw;
                }

                // Recarregar a categoria do banco para obter os dados atualizados
                await _context.Entry(categoria).ReloadAsync();
                var categoriaAtualizada = await _context.Categorias.FindAsync(id);

                var totalTipos = await ContarTiposEquipamentoPorCategoriaAsync(categoriaAtualizada.Id);
                var categoriaDesativadaDto = new CategoriaDTO
                {
                    Id = categoriaAtualizada.Id,
                    Nome = categoriaAtualizada.Nome,
                    Descricao = categoriaAtualizada.Descricao,
                    Ativo = categoriaAtualizada.Ativo,
                    DataCriacao = categoriaAtualizada.DataCriacao,
                    DataAtualizacao = categoriaAtualizada.DataAtualizacao,
                    TotalTiposEquipamento = totalTipos
                };

                return new CategoriaResponseDTO
                {
                    Sucesso = true,
                    Mensagem = "Categoria desativada com sucesso",
                    Dados = categoriaDesativadaDto,
                    Status = 200
                };
            }
            catch (Exception ex)
            {
                return new CategoriaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao desativar categoria: {ex.Message}",
                    Dados = null,
                    Status = 500
                };
            }
        }

        public async Task<CategoriaResponseDTO> ReativarCategoriaAsync(int id)
        {
            try
            {
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null)
                {
                    return new CategoriaResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Categoria não encontrada",
                        Dados = null,
                        Status = 404
                    };
                }

                // Atualizar diretamente no banco para garantir que a mudança seja persistida
                var sql = @"
                    UPDATE categorias 
                    SET ativo = {0}, data_atualizacao = {1} 
                    WHERE id = {2}";

                var ativo = true;
                var dataAtualizacao = DateTime.Now;

                Console.WriteLine($"[BACKEND] Reativando categoria ID {id}");
                Console.WriteLine($"[BACKEND] Executando SQL direto para reativar categoria");

                try
                {
                    var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, ativo, dataAtualizacao, id);
                    Console.WriteLine($"[BACKEND] SQL executado com sucesso. Linhas afetadas: {rowsAffected}");
                    
                    if (rowsAffected == 0)
                    {
                        return new CategoriaResponseDTO
                        {
                            Sucesso = false,
                            Mensagem = "Nenhuma categoria foi atualizada. Verifique se o ID existe.",
                            Dados = null,
                            Status = 404
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BACKEND] ERRO no SQL: {ex.Message}");
                    throw;
                }

                // Recarregar a categoria do banco para obter os dados atualizados
                await _context.Entry(categoria).ReloadAsync();
                var categoriaAtualizada = await _context.Categorias.FindAsync(id);

                var totalTipos = await ContarTiposEquipamentoPorCategoriaAsync(categoriaAtualizada.Id);
                var categoriaReativadaDto = new CategoriaDTO
                {
                    Id = categoriaAtualizada.Id,
                    Nome = categoriaAtualizada.Nome,
                    Descricao = categoriaAtualizada.Descricao,
                    Ativo = categoriaAtualizada.Ativo,
                    DataCriacao = categoriaAtualizada.DataCriacao,
                    DataAtualizacao = categoriaAtualizada.DataAtualizacao,
                    TotalTiposEquipamento = totalTipos
                };

                return new CategoriaResponseDTO
                {
                    Sucesso = true,
                    Mensagem = "Categoria reativada com sucesso",
                    Dados = categoriaReativadaDto,
                    Status = 200
                };
            }
            catch (Exception ex)
            {
                return new CategoriaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao reativar categoria: {ex.Message}",
                    Dados = null,
                    Status = 500
                };
            }
        }

        public async Task<bool> VerificarNomeExistenteAsync(string nome, int? idExcluir = null)
        {
            var query = _context.Categorias.Where(c => c.Nome.ToLower() == nome.ToLower());
            
            if (idExcluir.HasValue)
            {
                query = query.Where(c => c.Id != idExcluir.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> ContarTiposEquipamentoPorCategoriaAsync(int categoriaId)
        {
            return await _context.Tipoequipamento
                .Where(t => t.CategoriaId == categoriaId && t.Ativo)
                .CountAsync();
        }
    }
}
