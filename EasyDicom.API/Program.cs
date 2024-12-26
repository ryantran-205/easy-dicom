using EasyDicom.API.Services;
using EasyDicom.API.Services.Interfaces;
using EasyDicom.API.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Bind the configuration section to the DicomSettings class
builder.Services.Configure<DicomSettings>(builder.Configuration.GetSection("DicomSettings"));
builder.Services.AddSingleton<IDicomService, DicomService>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DICOM API",
        Version = "v1",
        Description = "An API for converting DICOM Structured Reports to PDF"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DICOM API V1");
        c.RoutePrefix = string.Empty; 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
