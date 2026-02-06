namespace ContactManager.Application.DTOs;

public class ContactDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool Married { get; set; }
    public string Phone { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    
    public int Age => DateTime.Now.Year - DateOfBirth.Year - 
                      (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}