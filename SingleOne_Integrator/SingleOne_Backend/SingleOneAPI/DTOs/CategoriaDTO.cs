using System;
using System.Collections.Generic;

namespace SingleOneAPI.DTOs
{
    public class CategoriaDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public int TotalTiposEquipamento { get; set; }
    }

    public class CategoriaCreateDTO
    {
        public string Nome { get; set; }
        public string Descricao { get; set; }
    }

    public class CategoriaUpdateDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
    }

    public class CategoriaResponseDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public CategoriaDTO Dados { get; set; }
        public int Status { get; set; }
    }

    public class CategoriaListResponseDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public List<CategoriaDTO> Dados { get; set; }
        public int Status { get; set; }
        public int Total { get; set; }
    }
}
