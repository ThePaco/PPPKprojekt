namespace BlazorPatients.Models;

public class Image
{
    public required int Id { get; set; }
    public required Guid ImageGuid { get; set; }
    public required string FileExt { get; set; }
    public required int VisitId { get; set; }
    public Visit Visit { get; set; }
}
