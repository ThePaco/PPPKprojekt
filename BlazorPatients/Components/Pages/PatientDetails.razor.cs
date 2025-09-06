using BlazorPatients.Models.Enum;
using BlazorPatients.Services;
using BlazorPatients.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

namespace BlazorPatients.Components.Pages;

public partial class PatientDetails(PatientService patientService,
                                    PrescriptionService perscriptionService,
                                    VisitService visitService,
                                    NavigationManager navigationManager,
                                    IJSRuntime jsRuntime)
{
    [Parameter]
    public int Id { get; set; }
    public PatientViewModel Patient { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    
    // Modal state
    public bool ShowAddPrescriptionModal { get; set; } = false;
    public PrescriptionViewModel NewPrescription { get; set; } = new();
    public string PrescriptionMessage { get; set; } = string.Empty;
    
    // End Prescription Modal state
    public bool ShowEndPrescriptionModal { get; set; } = false;
    public int PrescriptionToEndId { get; set; }
    public DateTime EndDate { get; set; } = DateTime.Today;
    public string EndPrescriptionMessage { get; set; } = string.Empty;
    
    // Delete Prescription Modal state
    public bool ShowDeletePrescriptionModal { get; set; } = false;
    public int PrescriptionToDeleteId { get; set; }
    public string PrescriptionToDeleteName { get; set; } = string.Empty;

    // Visit Modal state
    public bool ShowAddVisitModal { get; set; } = false;
    public VisitViewModel NewVisit { get; set; } = new();
    public string VisitMessage { get; set; } = string.Empty;

    // Delete Patient Modal state
    public bool ShowDeletePatientModal { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        Patient = await patientService.GetPatientAsync(Id);
        await LoadPrescriptionsAndVisits();
    }
    
    public async Task UpdatePatientAsync()
    {
        var result = await patientService.UpdatePatientAsync(Patient);
        Message = result.IsSuccess() ? "Patient updated successfully!" : $"Error: {result.Errors.FirstOrDefault()}";
    }

    private async Task LoadPrescriptionsAndVisits()
    {
        Patient.Prescriptions = await perscriptionService.GetAllPrescriptionsForPatientAsync(Id);
        Patient.Visits = await visitService.GetVisitsByPatientIdAsync(Id);
    }

    // Prescription Modal Methods
    private void OpenAddPrescriptionModal()
    {
        NewPrescription = new PrescriptionViewModel
        {
            PatientId = Id,
            DatePrescribed = DateTime.Today
        };
        PrescriptionMessage = string.Empty;
        ShowAddPrescriptionModal = true;
    }

    private void CloseAddPrescriptionModal()
    {
        ShowAddPrescriptionModal = false;
        PrescriptionMessage = string.Empty;
    }

    private async Task AddPrescriptionAsync()
    {
        var result = await perscriptionService.AddPrescriptionAsync(NewPrescription);
        
        if (result.IsSuccess())
        {
            PrescriptionMessage = "Prescription added successfully!";
            await LoadPrescriptionsAndVisits();
            await Task.Delay(1500);
            CloseAddPrescriptionModal();
        }
        else
        {
            PrescriptionMessage = $"Error: {result.Errors.FirstOrDefault()}";
        }
    }
    
    // End Prescription Methods
    private void OpenEndPrescriptionModal(int prescriptionId)
    {
        PrescriptionToEndId = prescriptionId;
        EndDate = DateTime.Today;
        EndPrescriptionMessage = string.Empty;
        ShowEndPrescriptionModal = true;
    }

    private void CloseEndPrescriptionModal()
    {
        ShowEndPrescriptionModal = false;
        EndPrescriptionMessage = string.Empty;
    }

    private async Task ConfirmEndPrescriptionAsync()
    {
        var prescriptionToUpdate = Patient.Prescriptions.FirstOrDefault(p => p.PrecriptionId == PrescriptionToEndId);
        if (prescriptionToUpdate != null)
        {
            prescriptionToUpdate.DateEnding = EndDate;
            var result = await perscriptionService.UpdatePrescriptionAsync(PrescriptionToEndId, prescriptionToUpdate);
            
            if (result.IsSuccess())
            {
                EndPrescriptionMessage = "Prescription ended successfully!";
                await LoadPrescriptionsAndVisits();
                await Task.Delay(1500);
                CloseEndPrescriptionModal();
            }
            else
            {
                EndPrescriptionMessage = $"Error: {result.Errors.FirstOrDefault()}";
            }
        }
    }

    // Delete Prescription Methods
    private void OpenDeletePrescriptionModal(int prescriptionId, string medicationName)
    {
        PrescriptionToDeleteId = prescriptionId;
        PrescriptionToDeleteName = medicationName;
        ShowDeletePrescriptionModal = true;
    }

    private void CloseDeletePrescriptionModal()
    {
        ShowDeletePrescriptionModal = false;
    }

    private async Task ConfirmDeletePrescriptionAsync()
    {
        var result = await perscriptionService.DeletePrescriptionAsync(PrescriptionToDeleteId);
        
        if (result.IsSuccess())
        {
            await LoadPrescriptionsAndVisits();
            CloseDeletePrescriptionModal();
        }
        else
        {
            // You might want to show an error message here
            Message = $"Error deleting prescription: {result.Errors.FirstOrDefault()}";
            CloseDeletePrescriptionModal();
        }
    }

    // Visit Modal Methods
    private void OpenAddVisitModal()
    {
        NewVisit = new VisitViewModel
                   {
                       PatientId = Id,
                       Date = DateTime.Today,
                       Type = VisitType.GP
                   };
        VisitMessage = string.Empty;
        ShowAddVisitModal = true;
    }

    private void CloseAddVisitModal()
    {
        ShowAddVisitModal = false;
        VisitMessage = string.Empty;
    }

    private async Task AddVisitAsync()
    {
        var result = await visitService.AddVisitAsync(NewVisit);

        if (result.IsSuccess())
        {
            VisitMessage = "Visit added successfully!";
            await LoadPrescriptionsAndVisits();
            await Task.Delay(1500);
            CloseAddVisitModal();
        }
        else
        {
            VisitMessage = $"Error: {result.Errors.FirstOrDefault()}";
        }
    }

    private void ViewVisitDetails(int visitId)
    {
        navigationManager.NavigateTo($"/visitdetails/{visitId}");
    }

    // Delete Patient Methods
    private void OpenDeletePatientModal()
    {
        ShowDeletePatientModal = true;
    }

    private void CloseDeletePatientModal()
    {
        ShowDeletePatientModal = false;
    }

    private async Task ConfirmDeletePatientAsync()
    {
        var result = await patientService.DeletePatientAsync(Id);
        
        if (result.IsSuccess())
        {
            // Navigate back to the home page after successful deletion
            navigationManager.NavigateTo("/");
        }
        else
        {
            Message = $"Error deleting patient: {result.Errors.FirstOrDefault()}";
            CloseDeletePatientModal();
        }
    }
    private string GetVisitTypeDisplayName(VisitType visitType)
    {
        return visitType switch
        {
            VisitType.GP => "General Practitioner",
            VisitType.KRV => "Blood Work",
            VisitType.XRAY => "X-Ray",
            VisitType.CT => "CT Scan",
            VisitType.MR => "MR Scan",
            VisitType.ULTRA => "Ultrasound",
            VisitType.EKG => "Electrocardiogram",
            VisitType.ECHO => "Echocardiogram",
            VisitType.EYE => "Eye Examination",
            VisitType.DERM => "Dermatology",
            VisitType.DENTA => "Dental",
            VisitType.MAMMO => "Mammography",
            VisitType.NEURO => "Neurology",
            _ => visitType.ToString()
        };
    }
    private async Task ExportToCsvAsync()
    {
        var csv = new StringBuilder();

        csv.AppendLine("PATIENT DETAILS");
        csv.AppendLine("Id,FirstName,LastName,IsMale,Oib,Birthday");
        csv.AppendLine($"{Patient.Id},{EscapeCsvField(Patient.FirstName)},{EscapeCsvField(Patient.LastName)},{Patient.IsMale},{EscapeCsvField(Patient.Oib)},{Patient.Birthday:yyyy-MM-dd}");
        csv.AppendLine();

        csv.AppendLine("PRESCRIPTIONS");
        csv.AppendLine("PrescriptionId,PatientId,MedicationName,DatePrescribed,DateEnding");
        foreach (var prescription in Patient.Prescriptions)
        {
            csv.AppendLine($"{prescription.PrecriptionId},{prescription.PatientId},{EscapeCsvField(prescription.MedicationName)},{prescription.DatePrescribed:yyyy-MM-dd},{prescription.DateEnding?.ToString("yyyy-MM-dd") ?? ""}");
        }
        csv.AppendLine();

        csv.AppendLine("VISITS");
        csv.AppendLine("VisitId,PatientId,Type,Date,DoctorsNotes");
        foreach (var visit in Patient.Visits)
        {
            csv.AppendLine($"{visit.VisitId},{visit.PatientId},{visit.Type},{visit.Date:yyyy-MM-dd},{EscapeCsvField(visit.DoctorsNotes)}");
        }

        var fileName = $"patient_{Patient.Id}_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvContent = csv.ToString();

        await jsRuntime.InvokeVoidAsync("downloadFile", fileName, csvContent, "text/csv");
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
