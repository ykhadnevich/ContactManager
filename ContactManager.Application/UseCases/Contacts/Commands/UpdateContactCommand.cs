using ContactManager.Application.Common;
using ContactManager.Application.Interfaces.Persistence;

public class UpdateContactCommand
{
    private readonly IContactRepository _repository;

    public UpdateContactCommand(IContactRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> ExecuteAsync(
        int id,
        string name,
        DateTime dateOfBirth,
        bool married,
        string phone,
        decimal salary,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contact = await _repository.GetByIdAsync(id, cancellationToken);
            
            if (contact == null)
            {
                return Result.Failure("Contact not found");
            }

            var updateResult = contact.Update(name, dateOfBirth, married, phone, salary);

            if (updateResult.IsFailure)
            {
                return Result.Failure(updateResult.Error);
            }

            await _repository.UpdateAsync(contact, cancellationToken);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error updating contact: {ex.Message}");
        }
    }
}