using ContactManager.Application.Interfaces.Persistence;
using ContactManager.Domain.Common;
using ContactManager.Domain.Entities;

namespace ContactManager.Application.UseCases.Contacts.Commands;

public class CreateContactCommand
{
    private readonly IContactRepository _repository;

    public CreateContactCommand(IContactRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<int>> ExecuteAsync(
        string name,
        DateTime dateOfBirth,
        bool married,
        string phone,
        decimal salary,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contactResult = Contact.Create(name, dateOfBirth, married, phone, salary);

            if (contactResult.IsFailure)
            {
                return Result.Failure<int>(contactResult.Error);
            }

            var created = await _repository.AddAsync(contactResult.Value!, cancellationToken);
            
            return Result.Success(created.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>($"Error creating contact: {ex.Message}");
        }
    }
}