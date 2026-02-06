using System.ComponentModel.DataAnnotations;

namespace ContactManager.Web.Models;

public class ContactViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Display(Name = "Married")]
    public bool Married { get; set; }

    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Invalid phone format")]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Salary is required")]
    [Range(0, 999999999, ErrorMessage = "Salary must be between 0 and 999,999,999")]
    [DataType(DataType.Currency)]
    public decimal Salary { get; set; }

    public int Age => DateTime.Now.Year - DateOfBirth.Year - 
                      (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}