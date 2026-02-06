using ContactManager.Application.Common;
using ContactManager.Application.DTOs;
using ContactManager.Application.Interfaces.Persistence;

namespace ContactManager.Application.UseCases.Contacts.Queries;

public class GetAllContactsQuery
{
    private readonly IContactRepository _repository;

    public GetAllContactsQuery(IContactRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ContactDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var contacts = await _repository.GetAllAsync(cancellationToken);
            
            var contactDtos = contacts.Select(c => new ContactDto
            {
                Id = c.Id,
                Name = c.Name,
                DateOfBirth = c.DateOfBirth,
                Married = c.Married,
                Phone = c.Phone,
                Salary = c.Salary
            });

            return Result.Success(contactDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<ContactDto>>($"Error retrieving contacts: {ex.Message}");
        }
    }
}