namespace astratech_apps_backend.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public List<Menu> Children { get; set; } = [];
    }
}
