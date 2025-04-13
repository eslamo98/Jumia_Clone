namespace Jumia_Clone.Models.DTOs.UserProductInteractionDTOs
{
    public class UserProductInteractiondto
    {
        public int InteractionId { get; set; }
        public int? CustomerId { get; set; }
        public string SessionId { get; set; }
        public int ProductId { get; set; }
        public string InteractionType { get; set; }
        public DateTime? InteractionTime { get; set; }
        public int? DurationSeconds { get; set; }
        public string InteractionData { get; set; }
     
    }
}
