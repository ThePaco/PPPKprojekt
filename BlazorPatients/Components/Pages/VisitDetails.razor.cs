using BlazorPatients.Services;
using BlazorPatients.ViewModels;
using BlazorPatients.Models.Enum;
using BlazorPatients.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorPatients.Components.Pages;

public partial class VisitDetails(VisitService visitService, ImageService imageService, NavigationManager navigationManager)
{
    [Parameter]
    public int VisitId { get; set; }

    public VisitViewModel Visit { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    
    // Visit Modal state
    public bool ShowDeleteVisitModal { get; set; } = false;
    
    // Image Modal states
    public bool ShowImageModal { get; set; } = false;
    public bool ShowDeleteImageModal { get; set; } = false;
    public bool ShowUploadImageModal { get; set; } = false;
    public Guid SelectedImageGuid { get; set; }
    public int ImageToDeleteId { get; set; }

    // Image upload
    private IBrowserFile? selectedFile;
    private bool isUploading = false;
    
    // Image loading states
    private bool isLoadingImages = false;
    private Dictionary<Guid, string> imageUrls = new();
    private HashSet<Guid> loadingImages = new();

    protected override async Task OnInitializedAsync()
    {
        var visit = await visitService.GetVisitAsync(VisitId);
        if (visit != null)
        {
            Visit = visit;
            await LoadImageUrlsAsync();
        }
        else
        {
            Message = "Visit not found!";
        }
    }

    private async Task LoadImageUrlsAsync()
    {
        isLoadingImages = true;
        StateHasChanged();
        
        try
        {
            foreach (var image in Visit.Images)
            {
                loadingImages.Add(image.ImageGuid);
                StateHasChanged();
                
                var url = await imageService.GetImageUrlAsync(image.ImageGuid, VisitId, image.FileExt);
                imageUrls[image.ImageGuid] = url;
                loadingImages.Remove(image.ImageGuid);
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Message = $"Error loading images: {ex.Message}";
        }
        finally
        {
            isLoadingImages = false;
            StateHasChanged();
        }
    }

    public async Task UpdateVisitAsync()
    {
        var result = await visitService.UpdateVisitAsync(VisitId, Visit);
        Message = result.IsSuccess() ? "Visit updated successfully!" : $"Error: {result.Errors.FirstOrDefault()}";
    }

    // Visit Delete Methods
    private void OpenDeleteVisitModal() => ShowDeleteVisitModal = true;

    private void CloseDeleteVisitModal() => ShowDeleteVisitModal = false;

    private async Task ConfirmDeleteVisitAsync()
    {
        var result = await visitService.DeleteVisitAsync(VisitId);

        if (result.IsSuccess())
        {
            navigationManager.NavigateTo($"/patientdetails/{Visit.PatientId}");
        }
        else
        {
            Message = $"Error deleting visit: {result.Errors.FirstOrDefault()}";
            CloseDeleteVisitModal();
        }
    }

    // Image Methods
    private void OpenImageModal(Guid imageGuid)
    {
        SelectedImageGuid = imageGuid;
        ShowImageModal = true;
    }

    private void CloseImageModal()
    {
        ShowImageModal = false;
        SelectedImageGuid = Guid.Empty;
    }

    private void OpenDeleteImageModal(int imageId, Guid imageGuid)
    {
        ImageToDeleteId = imageId;
        SelectedImageGuid = imageGuid;
        ShowDeleteImageModal = true;
    }

    private void CloseDeleteImageModal()
    {
        ShowDeleteImageModal = false;
        ImageToDeleteId = 0;
        SelectedImageGuid = Guid.Empty;
    }

    private async Task ConfirmDeleteImageAsync()
    {
        var imageToRemove = Visit.Images.FirstOrDefault(i => i.Id == ImageToDeleteId);
        if (imageToRemove != null)
        {
            Visit.Images.Remove(imageToRemove);
            imageUrls.Remove(imageToRemove.ImageGuid);
            var result = await imageService.DeleteImageAsync(ImageToDeleteId);
            Message = result.IsSuccess() ? "Image deleted successfully!" : $"Error deleting image: {result.Errors.FirstOrDefault()}";
        }
        CloseDeleteImageModal();
        StateHasChanged();
    }

    private void OpenUploadImageModal()
    {
        ShowUploadImageModal = true;
    }

    private void CloseUploadImageModal()
    {
        ShowUploadImageModal = false;
        selectedFile = null;
    }

    private void OnFileSelected(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;
    }

    private async Task UploadImageAsync()
    {
        if (selectedFile == null) return;

        isUploading = true;
        StateHasChanged();

        try
        {
            using var stream = selectedFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB limit
            var result = await imageService.UploadImageAsync(VisitId, stream, selectedFile.Name);

            if (result.IsSuccess())
            {
                Message = "Image uploaded successfully!";

                var refreshedVisit = await visitService.GetVisitAsync(VisitId);
                if (refreshedVisit != null)
                {
                    Visit = refreshedVisit;
                    await LoadImageUrlsAsync(); // Reload image URLs
                }
                CloseUploadImageModal();
            }
            else
            {
                Message = $"Upload failed: {result.Errors.FirstOrDefault()}";
            }
        }
        catch (Exception ex)
        {
            Message = $"Upload error: {ex.Message}";
        }
        finally
        {
            isUploading = false;
            StateHasChanged();
        }
    }

    public string GetImageUrl(Guid imageGuid, string fileExtension)
    {
        // Return cached URL if available, otherwise return loading placeholder
        if (imageUrls.TryGetValue(imageGuid, out var url))
        {
            return url;
        }
        
        // Return a data URL for a loading placeholder
        return "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='200'%3E%3Crect width='100%25' height='100%25' fill='%23f8f9fa'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%236c757d'%3ELoading...%3C/text%3E%3C/svg%3E";
    }

    private bool IsImageLoading(Guid imageGuid)
    {
        return loadingImages.Contains(imageGuid);
    }

    private static string GetVisitTypeDisplayName(VisitType visitType)
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
}