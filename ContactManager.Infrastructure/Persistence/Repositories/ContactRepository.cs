using ContactManager.Application.Interfaces.Persistence;
using ContactManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContactManager.Infrastructure.Persistence.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly ApplicationDbContext _context;

    public ContactRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Contact?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Contact> AddAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        await _context.Contacts.AddAsync(contact, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return contact;
    }

    public async Task<IEnumerable<Contact>> AddRangeAsync(
        IEnumerable<Contact> contacts, 
        CancellationToken cancellationToken = default)
    {
        var contactList = contacts.ToList();
        
        await _context.Contacts.AddRangeAsync(contactList, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return contactList;
    }

    public async Task UpdateAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        _context.Contacts.Update(contact);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var contact = await _context.Contacts.FindAsync(new object[] { id }, cancellationToken);
        
        if (contact != null)
        {
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts.AnyAsync(c => c.Id == id, cancellationToken);
    }
}