using BlazorPatients.Components;
using BlazorPatients.Data;
using BlazorPatients.Services;
using BlazorPatients.Validators;
using BlazorPatients.ViewModels;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextPool<PatientsContext>(options => options
                                                   .UseNpgsql(builder
                                                     .Configuration
                                                     .GetConnectionString("DefaultConnection")));

builder.Services
       .AddScoped<IValidator<PatientViewModel>, PatientValidator>()
       .AddScoped<PatientService>(); 

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
