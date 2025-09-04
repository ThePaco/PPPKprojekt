using BlazorPatients.Models;
using BlazorPatients.Models.Enum;

namespace BlazorPatients.ViewModels;

public class VisitViewModel
{
    public int VisitId { get; set; }
    public int PatientId { get; set; }
    public VisitType Type { get; set; }
    public DateTime Date { get; set; }
    public string DoctorsNotes { get; set; } = string.Empty;
    public List<Image> Images { get; set; } = [];
}

public static class VisitViewModelExtensions
{
    public static VisitViewModel ToViewModel(this Models.Visit visit)
    {
        return new VisitViewModel
        {
            VisitId = visit.Id,
            PatientId = visit.PatientId,
            Type = visit.Type,
            Date = visit.Date,
            DoctorsNotes = visit.DoctorsNotes,
            Images = visit.Images?.ToList() ?? []
        };
    }
    public static Models.Visit ToModel(this VisitViewModel viewModel)
    {
        return new Models.Visit
        {
            Id = viewModel.VisitId,
            PatientId = viewModel.PatientId,
            Type = viewModel.Type,
            Date = viewModel.Date,
            DoctorsNotes = viewModel.DoctorsNotes
        };
    }
}