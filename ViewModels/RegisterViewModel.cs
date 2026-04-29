using System.ComponentModel.DataAnnotations;

namespace LibraryApp.ViewModels;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "First Name")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Numbers and special characters are not allowed in names")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Numbers and special characters are not allowed in names")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
