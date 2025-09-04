using BlazorPatients.Services;
using BlazorPatients.ViewModels;
using Microsoft.AspNetCore.Components;
using LightResults;

namespace BlazorPatients.Components.Pages;

public partial class Home(PatientService patientService)
{
    public List<PatientViewModel> Patients { get; set; } = [];
    public List<PatientViewModel> FilteredPatients { get; set; } = [];
    public string SearchTerm { get; set; } = string.Empty;
    
    // Add Patient Modal properties
    public bool ShowAddPatientModal { get; set; } = false;
    public PatientViewModel NewPatient { get; set; } = new();
    private Result? addPatientResult;
    public string? AddPatientMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Patients = await patientService.GetAllPatients();
        FilteredPatients = Patients;
    }

    private void OnSearchChanged(ChangeEventArgs e)
    {
        SearchTerm = e.Value?.ToString() ?? string.Empty;
        FilterPatients();
    }

    private void FilterPatients()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            FilteredPatients = Patients;
        }
        else
        {
            FilteredPatients = Patients.Where(p =>
                                                  p.Oib.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                  p.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                  p.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                                             ).ToList();
        }
    }

    private void OpenAddPatientModal()
    {
        NewPatient = new PatientViewModel();
        AddPatientMessage = null;
        ShowAddPatientModal = true;
    }

    private void CloseAddPatientModal()
    {
        ShowAddPatientModal = false;
        NewPatient = new PatientViewModel();
        AddPatientMessage = null;
    }

    public async Task AddPatientAsync()
    {
        addPatientResult = await patientService.AddPatientAsync(NewPatient);
        
        if (addPatientResult.HasValue && addPatientResult.Value.IsSuccess())
        {
            AddPatientMessage = "Patient successfully added!";
            // Refresh the patient list
            Patients = await patientService.GetAllPatients();
            FilterPatients();
            StateHasChanged();
            
            // Close the modal after a short delay to show success message
            await Task.Delay(1000);
            CloseAddPatientModal();
        }
        else
        {
            AddPatientMessage = addPatientResult.Value.Errors.First().Message;
            StateHasChanged();
        }
    }
}
