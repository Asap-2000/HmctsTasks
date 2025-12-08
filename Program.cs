using HmctsTasks.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.IO;
using System.Linq;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Robust runtime search for XML documentation file next to the running assembly
    var assembly = Assembly.GetExecutingAssembly();
    var assemblyName = assembly.GetName().Name ?? string.Empty;
    var assemblyDir = Path.GetDirectoryName(assembly.Location) ?? AppContext.BaseDirectory;

    // Prefer exact {AssemblyName}.xml, otherwise pick a sensible fallback
    var preferred = Path.Combine(assemblyDir, $"{assemblyName}.xml");
    string? xmlPath = null;

    if (File.Exists(preferred))
    {
        xmlPath = preferred;
    }
    else if (Directory.Exists(assemblyDir))
    {
        var xmlFiles = Directory.GetFiles(assemblyDir, "*.xml", SearchOption.TopDirectoryOnly);
        xmlPath = xmlFiles.FirstOrDefault(f => Path.GetFileName(f).StartsWith(assemblyName, StringComparison.OrdinalIgnoreCase))
                  ?? xmlFiles.FirstOrDefault();
    }

    if (!string.IsNullOrEmpty(xmlPath) && File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    else
    {
        // Logging via console here to avoid using ILoggingBuilder incorrectly during startup configuration.
        Console.WriteLine($"Warning: XML comments file not found for assembly '{assemblyName}' in '{assemblyDir}'. Swagger will run without XML comments.");
    }
});

// Configure EF Core with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Ensure database exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve index.html from wwwroot by default
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();          