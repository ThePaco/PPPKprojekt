using BlazorPatients.Data;
using BlazorPatients.ViewModels;
using FluentValidation;
using LightResults;
using Microsoft.EntityFrameworkCore;

namespace BlazorPatients.Services;

public class PrescriptionService(PatientsContext dbContext,
                                 IValidator<PrescriptionViewModel> validator)
{
    public async Task<List<PrescriptionViewModel>> GetAllPrescriptionsForPatientAsync(int patientId)
    {
        return await Task.FromResult(dbContext.Prescriptions
            .Where(p => p.PatientId == patientId)
            .Select(p => p.ToViewModel())
            .ToList());
    }
    public async Task<Result> AddPrescriptionAsync(PrescriptionViewModel newPrescription)
    {
        var validationResult = await validator.ValidateAsync(newPrescription);
        if (!validationResult.IsValid)
        {
            return Result.Failure("Prescription details aren't valid!");
        }
        await dbContext.Prescriptions.AddAsync(newPrescription.ToModel());
        await dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdatePrescriptionAsync(int prescriptionId, PrescriptionViewModel endedPrescription)
    {
        var validationResult = await validator.ValidateAsync(endedPrescription);
        if (!validationResult.IsValid)
        {
            return Result.Failure("Prescription details aren't valid!");
        }
        var model = await dbContext.Prescriptions.FirstOrDefaultAsync(p => p.Id == prescriptionId);
        if (model == null)
        {
            return Result.Failure("Prescription not found!");
        }
        model.DateEnding = endedPrescription.DateEnding;
        dbContext.Prescriptions.Update(model);

        await dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeletePrescriptionAsync(int prescriptionId)
    {
        var model = await dbContext.Prescriptions.FirstOrDefaultAsync(p => p.Id == prescriptionId);
        if (model == null)
        {
            return Result.Failure("Prescription not found!");
        }
        dbContext.Prescriptions.Remove(model);
        await dbContext.SaveChangesAsync();
        return Result.Success();
    }
}
