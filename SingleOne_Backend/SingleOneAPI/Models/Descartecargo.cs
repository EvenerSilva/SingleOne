namespace SingleOneAPI.Models
{
    public partial class Descartecargo
    {
        public int Id { get; set; }
        public int Cliente { get; set; }
        public string Cargo { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
    }
}
