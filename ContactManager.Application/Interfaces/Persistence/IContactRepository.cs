using ContactManager.Domain.Entities;

namespace ContactManager.Application.Interfaces.Persistence;

public interface IContactRepository
{
    Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Contact?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Contact> AddAsync(Contact contact, CancellationToken cancellationToken = default);
    Task<IEnumerable<Contact>> AddRangeAsync(IEnumerable<Contact> contacts, CancellationToken cancellationToken = default);
    Task UpdateAsync(Contact contact, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}