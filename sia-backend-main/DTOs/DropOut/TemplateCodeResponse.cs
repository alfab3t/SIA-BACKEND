namespace astratech_apps_backend.DTOs.DropOut
{
    public class TemplateCodeResponse
    {
        public string AspxCode { get; set; } = "";
        public string AspxCsCode { get; set; } = "";
        public string AspxDesignerCode { get; set; } = "";
        public string ReportLogic { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> RequiredFiles { get; set; } = new List<string>();
        public Dictionary<string, string> ConfigurationSteps { get; set; } = new Dictionary<string, string>();
    }
}