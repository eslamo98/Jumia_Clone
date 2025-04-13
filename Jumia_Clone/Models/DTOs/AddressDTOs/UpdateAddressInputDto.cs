using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.AddressDTOs
{
    public class UpdateAddressInputDto
    {
        public int AddressId { get; set; }

        [Required]
        [MaxLength(255)]
        public string StreetAddress { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(100)]
        public string State { get; set; }

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Country { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        public bool? IsDefault { get; set; }

        [Required]
        [MaxLength(50)]
        public string AddressName { get; set; }

        public int UserId { get; set; }
    }
}
