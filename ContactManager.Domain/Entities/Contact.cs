using ContactManager.Domain.Common;

namespace ContactManager.Domain.Entities;

public partial class Contact
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public bool Married { get; private set; }
    public string Phone { get; private set; } = string.Empty;
    public decimal Salary { get; private set; }

    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }
    
    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
    
    private Contact() { }
}