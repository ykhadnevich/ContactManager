using ContactManager.Application.Common;
using ContactManager.Application.Interfaces.Persistence;

namespace ContactManager.Application.UseCases.Contacts.Commands;

public class DeleteContactCommand
{
    private readonly IContactRepository _repository;

    public DeleteContactCommand(IContactRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _repository.ExistsAsync(id, cancellationToken);
            if (!exists)
            {
                return Result.Failure("Contact not found");
            }

            await _repository.DeleteAsync(id, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting contact: {ex.Message}");
        }
    }
}