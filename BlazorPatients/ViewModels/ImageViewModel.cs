using System.Security.Principal;

namespace BlazorPatients.ViewModels;

public class ImageViewModel
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string FileExt { get; set; } = string.Empty;
    public int VisitId { get; set; }
}

public static class ImageViewModelExtensions
{
    public static ImageViewModel ToViewModel(this Models.Image image) => new()
    {
        Id = image.Id,
        Guid = image.ImageGuid,
        FileExt = image.FileExt,
        VisitId = image.VisitId
    };
    public static Models.Image ToModel(this ImageViewModel viewModel) => new()
    {
        Id = viewModel.Id,
        ImageGuid = viewModel.Guid,
        FileExt = viewModel.FileExt,
        VisitId = viewModel.VisitId
    };
}
