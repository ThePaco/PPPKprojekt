namespace BlazorPatients.ViewModels;

public class PatientViewModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsMale { get; set; }
    public string Oib { get; set; }
    public DateTime Birthday { get; set; }
}

public static class PatientViewModelExtensions
{
    public static PatientViewModel ToViewModel(this Models.Patient patient)
    {
        return new PatientViewModel
        {
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            IsMale = patient.IsMale,
            Oib = patient.Oib,
            Birthday = patient.Birthday
        };
    }
    public static Models.Patient ToModel(this PatientViewModel viewModel)
    {
        return new Models.Patient
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            IsMale = viewModel.IsMale,
            Oib = viewModel.Oib,
            Birthday = viewModel.Birthday
        };
    }
}
