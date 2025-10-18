using HR_Platform.Data;
using HR_Platform.Helpers;
using HR_Platform.Services.Candidates;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This converter handles DateOnly type serialization/deserialization
        // Without this, DateOnly wouldn't work properly with JSON
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Custom configuration for DateOnly type: tells Swagger to treat DateOnly as a string with date format
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",                                                // DateOnly will appear as a string type
        Format = "date",                                                // Format is "date"
        Example = new Microsoft.OpenApi.Any.OpenApiString("1995-10-19") // Example format shown in UI
    });
});
// This configures the connection string and database provider (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICandidateService, CandidateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
