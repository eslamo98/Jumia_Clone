namespace Jumia_Clone.Models.DTOs.SellerDTOs
{
    public class Sellerdto
    {
        public int SellerId { get; set; }

        public int UserId { get; set; }

        public string BusinessName { get; set; }

        public string BusinessDescription { get; set; }

        public string BusinessLogo { get; set; }

        public bool? IsVerified { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public double? Rating { get; set; }
    }
}
