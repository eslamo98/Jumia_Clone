namespace Jumia_Clone.Models.DTOs.UserDTOs
{
    public class CustomerDto : UserDto
    {
        public int CustomerId { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
