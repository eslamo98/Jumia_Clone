namespace Jumia_Clone.Models.DTOs.AiChatBotDTOs
{
    public class QueryGenerationRequest
    {
        public string UserQuery { get; set; }
        public string DatabaseSchema { get; set; }
        public List<string> AllowedTables { get; set; }
        public List<string> AllowedOperations { get; set; }
    }
}
