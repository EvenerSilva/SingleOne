using SingleOneAPI.Models;
using SingleOne.Models;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Templatetipo
    {
        public Templatetipo()
        {
            Templates = new HashSet<Template>();
        }

        public int Id { get; set; }
        public string Descricao { get; set; }

        public virtual ICollection<Template> Templates { get; set; }
        public virtual ICollection<RegrasTemplate> Regras { get; set; }
    }
}
