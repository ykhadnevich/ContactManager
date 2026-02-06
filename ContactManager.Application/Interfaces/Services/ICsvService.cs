using ContactManager.Application.DTOs;

namespace ContactManager.Application.Interfaces.Services;

public interface ICsvService
{
    Task<UploadResultDto> ProcessCsvFileAsync(Stream fileStream, CancellationToken cancellationToken = default);
    Task<IEnumerable<CsvContactDto>> ParseCsvAsync(Stream fileStream, CancellationToken cancellationToken = default);
}