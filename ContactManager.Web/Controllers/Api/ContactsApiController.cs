using ContactManager.Application.DTOs;
using ContactManager.Application.DTOs.Api;
using ContactManager.Application.UseCases.Contacts.Commands;
using ContactManager.Application.UseCases.Contacts.Queries;
using ContactManager.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContactsApiController : ControllerBase
{
    private readonly GetAllContactsQuery _getAllQuery;
    private readonly CreateContactCommand _createCommand;
    private readonly UpdateContactCommand _updateCommand;
    private readonly DeleteContactCommand _deleteCommand;
    private readonly CsvService _csvService;
    private readonly ILogger<ContactsApiController> _logger;

    public ContactsApiController(
        GetAllContactsQuery getAllQuery,
        CreateContactCommand createCommand,
        UpdateContactCommand updateCommand,
        DeleteContactCommand deleteCommand,
        CsvService csvService,
        ILogger<ContactsApiController> logger)
    {
        _getAllQuery = getAllQuery;
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _deleteCommand = deleteCommand;
        _csvService = csvService;
        _logger = logger;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ContactDto>>> GetAll()
    {
        try
        {
            var result = await _getAllQuery.ExecuteAsync();

            if (result.IsFailure)
            {
                return StatusCode(500, new { error = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contacts");
            return StatusCode(500, new { error = "An error occurred while retrieving contacts." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateContactResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateContactResponse>> Create([FromBody] CreateContactRequest request)
    {
        try
        {
            var result = await _createCommand.ExecuteAsync(
                request.Name,
                request.DateOfBirth,
                request.Married,
                request.Phone,
                request.Salary);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return CreatedAtAction(
                nameof(GetAll), 
                new { id = result.Value }, 
                new CreateContactResponse(result.Value, "Contact created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact");
            return StatusCode(500, new { error = "An error occurred while creating the contact." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContactRequest request)
    {
        try
        {
            var result = await _updateCommand.ExecuteAsync(
                id,
                request.Name,
                request.DateOfBirth,
                request.Married,
                request.Phone,
                request.Salary);

            if (result.IsFailure)
            {
                if (result.Error.Contains("not found"))
                {
                    return NotFound(new { error = result.Error });
                }
                return BadRequest(new { error = result.Error });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact {ContactId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the contact." });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _deleteCommand.ExecuteAsync(id);

            if (result.IsFailure)
            {
                if (result.Error.Contains("not found"))
                {
                    return NotFound(new { error = result.Error });
                }
                return BadRequest(new { error = result.Error });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contact {ContactId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the contact." });
        }
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UploadResultDto>> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "Please provide a CSV file." });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only CSV files are allowed." });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _csvService.ProcessCsvFileAsync(stream);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CSV file");
            return StatusCode(500, new { error = "An error occurred while processing the CSV file." });
        }
    }
}