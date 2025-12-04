using SingleOneAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Services
{
    public interface ICategoriaService
    {
        Task<CategoriaListResponseDTO> ListarCategoriasAsync(string filtro = null);
        Task<CategoriaResponseDTO> BuscarCategoriaPorIdAsync(int id);
        Task<CategoriaResponseDTO> CriarCategoriaAsync(CategoriaCreateDTO categoriaDto);
        Task<CategoriaResponseDTO> AtualizarCategoriaAsync(CategoriaUpdateDTO categoriaDto);
        Task<CategoriaResponseDTO> DesativarCategoriaAsync(int id);
        Task<CategoriaResponseDTO> ReativarCategoriaAsync(int id);
        Task<bool> VerificarNomeExistenteAsync(string nome, int? idExcluir = null);
        Task<int> ContarTiposEquipamentoPorCategoriaAsync(int categoriaId);
    }
}
