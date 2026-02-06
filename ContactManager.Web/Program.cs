using ContactManager.Application.Interfaces.Persistence;
using ContactManager.Application.Interfaces.Services;
using ContactManager.Application.UseCases.Contacts.Commands;
using ContactManager.Application.UseCases.Contacts.Queries;
using ContactManager.Application.Validators;
using ContactManager.Infrastructure.Persistence;
using ContactManager.Infrastructure.Persistence.Repositories;
using ContactManager.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Contact Manager API",
        Version = "v1",
        Description = "RESTful API for Contact Management System built with Clean Architecture",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com"
        }
    });

});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

builder.Services.AddScoped<IContactRepository, ContactRepository>();

builder.Services.AddScoped<ICsvService, CsvService>();
builder.Services.AddScoped<CsvService>(); 

builder.Services.AddValidatorsFromAssemblyContaining<ContactValidator>();
builder.Services.AddScoped<ContactValidator>();

builder.Services.AddScoped<GetAllContactsQuery>();
builder.Services.AddScoped<CreateContactCommand>();
builder.Services.AddScoped<UpdateContactCommand>();
builder.Services.AddScoped<DeleteContactCommand>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Contact Manager API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Contact Manager API";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Contacts}/{action=Index}/{id?}");

app.MapControllers();

app.Run();