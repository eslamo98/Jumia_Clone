using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.AddressDTO
{
    public class CreateAddressRequest
    {
        [Required]
        [MaxLength(200)]
        public string StreetAddress { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(100)]
        public string State { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression(@"^[a-zA-Z0-9\s-]+$", ErrorMessage = "Postal code can only contain letters, numbers, spaces, and hyphens")]
        public string PostalCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Country { get; set; }

        [MaxLength(20)]
        [Phone]
        public string PhoneNumber { get; set; }

        public bool? IsDefault { get; set; }

        [Required]
        [MaxLength(100)]
        public string AddressName { get; set; }
    }

    public class UpdateAddressRequest
    {
        [Required]
        [MaxLength(200)]
        public string StreetAddress { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(100)]
        public string State { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression(@"^[a-zA-Z0-9\s-]+$", ErrorMessage = "Postal code can only contain letters, numbers, spaces, and hyphens")]
        public string PostalCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Country { get; set; }

        [MaxLength(20)]
        [Phone]
        public string PhoneNumber { get; set; }

        public bool? IsDefault { get; set; }

        [Required]
        [MaxLength(100)]
        public string AddressName { get; set; }
    }

}
