using SingleOneAPI.Models;

namespace SingleOneAPI.Models
{
    public partial class RegrasTemplate
    {
        public int Id { get; set; }
        public int TipoTemplate { get; set; }
        public int TipoAquisicao { get; set; }

        public virtual Templatetipo TemplatetipoNavigation { get; set; }
    }
}
