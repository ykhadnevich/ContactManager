using ContactManager.Application.UseCases.Contacts.Commands;
using ContactManager.Application.UseCases.Contacts.Queries;
using ContactManager.Infrastructure.Services;
using ContactManager.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.Web.Controllers;

public class ContactsController : Controller
{
    private readonly GetAllContactsQuery _getAllQuery;
    private readonly CreateContactCommand _createCommand;
    private readonly UpdateContactCommand _updateCommand;
    private readonly DeleteContactCommand _deleteCommand;
    private readonly CsvService _csvService;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(
        GetAllContactsQuery getAllQuery,
        CreateContactCommand createCommand,
        UpdateContactCommand updateCommand,
        DeleteContactCommand deleteCommand,
        CsvService csvService,
        ILogger<ContactsController> logger)
    {
        _getAllQuery = getAllQuery;
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _deleteCommand = deleteCommand;
        _csvService = csvService;
        _logger = logger;
    }
    
    public async Task<IActionResult> Index()
    {
        try
        {
            var result = await _getAllQuery.ExecuteAsync();

            if (result.IsFailure)
            {
                TempData["Error"] = result.Error;
                return View(new List<ContactViewModel>());
            }

            var viewModels = result.Value.Select(dto => new ContactViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                DateOfBirth = dto.DateOfBirth,
                Married = dto.Married,
                Phone = dto.Phone,
                Salary = dto.Salary
            }).ToList();

            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading contacts");
            TempData["Error"] = "An error occurred while loading contacts.";
            return View(new List<ContactViewModel>());
        }
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a CSV file to upload.";
            return RedirectToAction(nameof(Index));
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Only CSV files are allowed.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _csvService.ProcessCsvFileAsync(stream);

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Successfully imported {result.SuccessfulRecords} contacts.";
            }
            else
            {
                TempData["Warning"] = $"Imported {result.SuccessfulRecords} contacts. {result.FailedRecords} records failed.";
                TempData["Errors"] = string.Join("<br/>", result.Errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading CSV file");
            TempData["Error"] = "An error occurred while processing the CSV file.";
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] ContactViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return Json(new { success = false, message = string.Join("; ", errors) });
        }

        try
        {
            var result = await _updateCommand.ExecuteAsync(
                model.Id,
                model.Name,
                model.DateOfBirth,
                model.Married,
                model.Phone,
                model.Salary);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Contact updated successfully." });
            }

            return Json(new { success = false, message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact {ContactId}", model.Id);
            return Json(new { success = false, message = "An error occurred while updating the contact." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _deleteCommand.ExecuteAsync(id);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Contact deleted successfully." });
            }

            return Json(new { success = false, message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contact {ContactId}", id);
            return Json(new { success = false, message = "An error occurred while deleting the contact." });
        }
    }
}