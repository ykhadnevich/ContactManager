namespace ContactManager.Application.DTOs.Api;

public record UpdateContactRequest(
    string Name,
    DateTime DateOfBirth,
    bool Married,
    string Phone,
    decimal Salary
);