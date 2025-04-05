using System.ComponentModel.DataAnnotations;

public class RegisterUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }

    [Required]
    [RegularExpression("^(customer|seller)$", ErrorMessage = "User type must be either 'customer' or 'seller'")]
    public string UserType { get; set; }
}