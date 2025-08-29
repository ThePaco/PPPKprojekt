using BlazorPatients.Services;
using BlazorPatients.ViewModels;
using LightResults;

namespace BlazorPatients.Components.Pages;

public partial class PatientAdd(PatientService service)
{
    public PatientViewModel Patient { get; set; } = new();
    private Result? result;
    public string? Message { get; set; }

    public async Task AddPatient()
    {
        result = await service.AddPatientAsync(Patient);
        Message = result.HasValue && result.Value.IsSuccess() ? "Patient successfully added!" 
                      : result.Value.Errors.First().Message;
    }
}
