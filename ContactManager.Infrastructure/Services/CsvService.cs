using System.Globalization;
using ContactManager.Application.DTOs;
using ContactManager.Application.Interfaces.Persistence;
using ContactManager.Application.Interfaces.Services;
using ContactManager.Domain.Entities;
using CsvHelper;
using CsvHelper.Configuration;

namespace ContactManager.Infrastructure.Services;

public class CsvService : ICsvService
{
    private readonly IContactRepository _repository;

    public CsvService(IContactRepository repository)
    {
        _repository = repository;
    }

    public async Task<UploadResultDto> ProcessCsvFileAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        var result = new UploadResultDto();

        try
        {
            var csvContacts = await ParseCsvAsync(fileStream, cancellationToken);
            var contactsList = csvContacts.ToList();
            result.TotalRecords = contactsList.Count;

            var validContacts = new List<Contact>();

            foreach (var (csvContact, index) in contactsList.Select((c, i) => (c, i)))
            {
                try
                {
                    var name = csvContact.Name?.Trim() ?? string.Empty;
                    var dateOfBirth = ParseDate(csvContact.DateOfBirth);
                    var married = ParseBoolean(csvContact.Married);
                    var phone = csvContact.Phone?.Trim() ?? string.Empty;
                    var salary = ParseDecimal(csvContact.Salary);
                    
                    var contactResult = Contact.Create(name, dateOfBirth, married, phone, salary);
                    
                    if (contactResult.IsSuccess)
                    {
                        validContacts.Add(contactResult.Value!);
                    }
                    else
                    {
                        result.FailedRecords++;
                        result.Errors.Add($"Row {index + 2}: {contactResult.Error}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedRecords++;
                    result.Errors.Add($"Row {index + 2}: {ex.Message}");
                }
            }

            if (validContacts.Any())
            {
                await _repository.AddRangeAsync(validContacts, cancellationToken);
                result.SuccessfulRecords = validContacts.Count;
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error processing CSV file: {ex.Message}");
            result.FailedRecords = result.TotalRecords;
        }

        return result;
    }

    public async Task<IEnumerable<CsvContactDto>> ParseCsvAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim
        });

        var records = new List<CsvContactDto>();

        await foreach (var record in csv.GetRecordsAsync<CsvContactDto>(cancellationToken))
        {
            records.Add(record);
        }

        return records;
    }
    

    private DateTime ParseDate(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            throw new FormatException("Date of birth is required");
        
        string[] formats = {
            "yyyy-MM-dd",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "dd-MM-yyyy",
            "MM-dd-yyyy",
            "yyyy/MM/dd"
        };

        if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, 
            DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (DateTime.TryParse(dateString, out date))
        {
            return date;
        }

        throw new FormatException($"Invalid date format: {dateString}");
    }

    private bool ParseBoolean(string boolString)
    {
        if (string.IsNullOrWhiteSpace(boolString))
            return false;

        var normalized = boolString.Trim().ToLower();
        
        return normalized switch
        {
            "true" or "yes" or "1" or "y" => true,
            "false" or "no" or "0" or "n" => false,
            _ => throw new FormatException($"Invalid boolean value: {boolString}")
        };
    }

    private decimal ParseDecimal(string decimalString)
    {
        if (string.IsNullOrWhiteSpace(decimalString))
            throw new FormatException("Salary is required");
        
        var cleaned = decimalString.Trim().Replace("$", "").Replace("€", "").Replace(",", "");

        if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw new FormatException($"Invalid decimal value: {decimalString}");
    }
}