using ContactManager.Domain.Common;

namespace ContactManager.Domain.Entities;

public partial class Contact
{
    public static Result<Contact> Create(
        string name,
        DateTime dateOfBirth,
        bool married,
        string phone,
        decimal salary)
    {
        var validationResult = Validate(name, dateOfBirth, phone, salary);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Contact>(validationResult.Error);
        }
        
        var contact = new Contact
        {
            Name = name.Trim(),
            DateOfBirth = dateOfBirth,
            Married = married,
            Phone = phone.Trim(),
            Salary = salary
        };

        return Result.Success(contact);
    }
    
    public Result Update(
        string name,
        DateTime dateOfBirth,
        bool married,
        string phone,
        decimal salary)
    {
        var validationResult = Validate(name, dateOfBirth, phone, salary);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        Name = name.Trim();
        DateOfBirth = dateOfBirth;
        Married = married;
        Phone = phone.Trim();
        Salary = salary;

        return Result.Success();
    }
}