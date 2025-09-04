using BlazorPatients.Data;
using BlazorPatients.ViewModels;
using FluentValidation;
using LightResults;
using Microsoft.EntityFrameworkCore;

namespace BlazorPatients.Services;

public class VisitService(PatientsContext dbContext,
                          IValidator<VisitViewModel> validator)
{
    public async Task<List<VisitViewModel>> GetVisitsByPatientIdAsync(int patientId)
    {
        return await Task.FromResult(dbContext.Visits
            .Where(v => v.PatientId == patientId)
            .Include(v => v.Images)
            .Select(v => v.ToViewModel())
            .ToList());
    }

    public async Task<VisitViewModel?> GetVisitAsync(int visitId)
    {
        var visit = await dbContext.Visits
            .Include(v => v.Images)
            .FirstOrDefaultAsync(v => v.Id == visitId);
        return visit?.ToViewModel();
    }

    public async Task<Result> AddVisitAsync(VisitViewModel newVisit)
    {
        var validationResult = await validator.ValidateAsync(newVisit);
        if (!validationResult.IsValid)
        {
            return Result.Failure("Visit details aren't valid!");
        }
        await dbContext.Visits.AddAsync(newVisit.ToModel());
        await dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateVisitAsync(int visitId, VisitViewModel updatedVisit)
    {
        var validationResult = await validator.ValidateAsync(updatedVisit);
        if (!validationResult.IsValid)
        {
            return Result.Failure("Visit details aren't valid!");
        }
        var model = await dbContext.Visits.FindAsync(visitId);
        if (model == null)
        {
            return Result.Failure("Visit not found!");
        }
        model.Type = updatedVisit.Type;
        model.Date = updatedVisit.Date;
        model.DoctorsNotes = updatedVisit.DoctorsNotes;

        dbContext.Visits.Update(model);
        await dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteVisitAsync(int visitId)
    {
        var model = await dbContext.Visits.FindAsync(visitId);
        if (model == null)
        {
            return Result.Failure("Visit not found!");
        }
        dbContext.Visits.Remove(model);
        await dbContext.SaveChangesAsync();
        return Result.Success();
    }
}
