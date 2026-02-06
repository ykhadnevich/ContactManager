namespace ContactManager.Application.DTOs.Api;


public record CreateContactRequest(
    string Name,
    DateTime DateOfBirth,
    bool Married,
    string Phone,
    decimal Salary
);