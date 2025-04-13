namespace Jumia_Clone.Models.DTOs.AddressDTO
{
    public class AddressFilterRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public bool? IsDefault { get; set; }
        public string Country { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";
    }

}
