using BlazorPatients.Data;
using BlazorPatients.Models;
using BlazorPatients.ViewModels;
using LightResults;
using Microsoft.EntityFrameworkCore;
using Supabase;
using Supabase.Storage;
using Client = Supabase.Client;

namespace BlazorPatients.Services;

public class ImageService(PatientsContext dContext, Client supabaseClient)
{
    private const string BucketName = "images";

    public async Task<Result> UploadImageAsync(int visitId, Stream imageStream, string fileName)
    {
        try
        {
            var imageGuid = Guid.NewGuid();
            var fileExtension = Path.GetExtension(fileName);
            var supabaseFileName = $"{visitId}/{imageGuid}{fileExtension}";

            var bucket = supabaseClient.Storage.From(BucketName);

            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            await bucket.Upload(imageBytes, supabaseFileName, 
                                new Supabase.Storage.FileOptions
                                {
                                    ContentType = GetContentType(fileExtension),
                                    Upsert = false
                                });

            var image = new ImageViewModel()
            {
                Guid = imageGuid,
                FileExt = fileExtension,
                VisitId = visitId
            };

            dContext.Images.Add(image.ToModel());
            await dContext.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to upload image: {ex.Message}");
        }
    }

    public async Task<Result> DeleteImageAsync(int imageId)
    {
        try
        {
            var image = await dContext.Images
                .Include(i => i.Visit)
                .FirstOrDefaultAsync(i => i.Id == imageId);

            if (image == null)
                return Result.Failure("Image not found");

            var supabaseFileName = $"{image.VisitId}/{image.ImageGuid}{image.FileExt}";
            var bucket = supabaseClient.Storage.From(BucketName);
            await bucket.Remove(supabaseFileName);

            dContext.Images.Remove(image);
            await dContext.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete image: {ex.Message}");
        }
    }

    public async Task<string> GetImageUrlAsync(Guid imageGuid, int visitId, string fileExtension = ".jpg")
    {
        try
        {
            var supabaseFileName = $"{visitId}/{imageGuid}{fileExtension}";
            var bucket = supabaseClient.Storage.From(BucketName);
            
            var signedUrl = await bucket.CreateSignedUrl(supabaseFileName, 3600);
            return signedUrl;
        }
        catch (Exception)
        {
            return GetImageUrl(imageGuid, visitId, fileExtension);
        }
    }

    public string GetImageUrl(Guid imageGuid, int visitId, string fileExtension = ".jpg")
    {
        var supabaseFileName = $"{visitId}/{imageGuid}{fileExtension}";
        var bucket = supabaseClient.Storage.From(BucketName);
        return bucket.GetPublicUrl(supabaseFileName);
    }

    public async Task<List<string>> GetImageUrlsForVisitAsync(int visitId)
    {
        var images = await dContext.Images
            .Where(i => i.VisitId == visitId)
            .ToListAsync();

        var imageUrls = new List<string>();
        foreach (var img in images)
        {
            var url = await GetImageUrlAsync(img.ImageGuid, visitId, img.FileExt);
            imageUrls.Add(url);
        }
        
        return imageUrls;
    }

    private static string GetContentType(string fileExtension)
    {
        return fileExtension.ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
