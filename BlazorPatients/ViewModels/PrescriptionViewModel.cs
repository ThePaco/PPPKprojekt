namespace BlazorPatients.ViewModels;

public class PrescriptionViewModel
{
    public int PrecriptionId { get; set; }
    public int PatientId { get; set; }
    public string MedicationName { get; set; }
    public DateTime DatePrescribed { get; set; }
    public DateTime? DateEnding { get; set; }
}

public static class PrescriptionViewModelExtensions
{
    public static PrescriptionViewModel ToViewModel(this Models.Prescription prescription)
    {
        return new PrescriptionViewModel
        {
            PrecriptionId = prescription.Id,
            PatientId = prescription.PatientId,
            MedicationName = prescription.MedicationName,
            DatePrescribed = prescription.DatePrescribed,
            DateEnding = prescription.DateEnding
        };
    }
    public static Models.Prescription ToModel(this PrescriptionViewModel viewModel)
    {
        return new Models.Prescription
        {
            Id = viewModel.PrecriptionId,
            PatientId = viewModel.PatientId,
            MedicationName = viewModel.MedicationName,
            DatePrescribed = viewModel.DatePrescribed,
            DateEnding = viewModel.DateEnding
        };
    }
}