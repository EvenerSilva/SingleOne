namespace SingleOneAPI.DTOs.TinOne
{
    /// <summary>
    /// DTO para um item de configuração do TinOne
    /// </summary>
    public class TinOneConfigItemDTO
    {
        public int? Id { get; set; }
        public int? Cliente { get; set; }
        public string Chave { get; set; }
        public string Valor { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
    }
}

