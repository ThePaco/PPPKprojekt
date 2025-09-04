namespace BlazorPatients.ViewModels;

public class PatientViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsMale { get; set; }
    public string Oib { get; set; }
    public DateTime Birthday { get; set; }
    public List<PrescriptionViewModel> Prescriptions { get; set; } = new();
    public List<VisitViewModel> Visits { get; set; } = new();
}

public static class PatientViewModelExtensions
{
    public static PatientViewModel ToViewModel(this Models.Patient patient)
    {
        return new PatientViewModel
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            IsMale = patient.IsMale,
            Oib = patient.Oib,
            Birthday = patient.Birthday,
            Prescriptions = patient.Prescriptions?.Select(pr => pr.ToViewModel()).ToList() ?? new List<PrescriptionViewModel>(),
            Visits = patient.Visits?.Select(vi => vi.ToViewModel()).ToList() ?? new List<VisitViewModel>()
        };
    }
    public static Models.Patient ToModel(this PatientViewModel viewModel)
    {
        return new Models.Patient
        {
            Id = viewModel.Id,
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            IsMale = viewModel.IsMale,
            Oib = viewModel.Oib,
            Birthday = viewModel.Birthday
        };
    }
}
