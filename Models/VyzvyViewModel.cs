namespace SkautApp.Models
{
    public class VyzvaItem
    {
        public int Id { get; set; }
        public string Nazev { get; set; } = string.Empty;
        public string Ikona { get; set; } = string.Empty;
        public bool Splneno { get; set; }
    }
}