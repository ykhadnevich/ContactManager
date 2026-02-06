using ContactManager.Domain.Common;
using System.Text.RegularExpressions;

namespace ContactManager.Domain.Entities;

public partial class Contact
{

    private static Result Validate(string name, DateTime dateOfBirth, string phone, decimal salary)
    {
        var errors = new List<string>();

        var nameError = ValidateName(name);
        if (nameError != null) errors.Add(nameError);

        var dobError = ValidateDateOfBirth(dateOfBirth);
        if (dobError != null) errors.Add(dobError);
        
        var phoneError = ValidatePhone(phone);
        if (phoneError != null) errors.Add(phoneError);

        var salaryError = ValidateSalary(salary);
        if (salaryError != null) errors.Add(salaryError);

        if (errors.Any())
            return Result.Failure(string.Join("; ", errors));

        return Result.Success();
    }

    private static string? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Name is required";
        
        if (name.Length > 100)
            return "Name cannot exceed 100 characters";

        return null;
    }

    private static string? ValidateDateOfBirth(DateTime dateOfBirth)
    {
        if (dateOfBirth >= DateTime.Today)
            return "Date of birth must be in the past";
        
        if (dateOfBirth < DateTime.Today.AddYears(-120))
            return "Invalid date of birth";

        return null;
    }

    private static string? ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return "Phone is required";
        
        if (phone.Length > 20)
            return "Phone cannot exceed 20 characters";
        
        if (!IsValidPhoneFormat(phone))
            return "Invalid phone format";

        return null;
    }

    private static string? ValidateSalary(decimal salary)
    {
        if (salary < 0)
            return "Salary cannot be negative";
        
        if (salary > 1_000_000_000)
            return "Salary value is too large";

        return null;
    }

    private static bool IsValidPhoneFormat(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        return Regex.IsMatch(phone, @"^\+?[\d\s\-\(\)]+$");
    }
}