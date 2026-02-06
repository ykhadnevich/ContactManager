using ContactManager.Domain.Entities;
using FluentValidation;

namespace ContactManager.Application.Validators;

public class ContactValidator : AbstractValidator<Contact>
{
    public ContactValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Now).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("Invalid date of birth");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^\+?[\d\s\-\(\)]+$").WithMessage("Invalid phone format")
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters");

        RuleFor(x => x.Salary)
            .GreaterThanOrEqualTo(0).WithMessage("Salary cannot be negative")
            .LessThan(1000000000).WithMessage("Salary value is too large");
    }
}