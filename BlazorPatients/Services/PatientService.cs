using BlazorPatients.Data;
using BlazorPatients.Validators;
using BlazorPatients.ViewModels;
using FluentValidation;
using LightResults;
using Microsoft.EntityFrameworkCore;

namespace BlazorPatients.Services;

public class PatientService
{
    private readonly PatientsContext dbContext;
    private readonly IValidator<PatientViewModel> validator;
    public PatientService(PatientsContext dbContext, IValidator<PatientViewModel> validator)
    {
        this.dbContext = dbContext;
        this.validator = validator;
    }

    public async Task<List<PatientViewModel>> GetAllPatients()
    {
        return await Task.FromResult(dbContext.Patients.Select(p => p.ToViewModel()).ToList());
    }

    public async Task<PatientViewModel?> GetPatientByOib(string oib)
    {
        var patient = await dbContext.Patients.FindAsync(oib);
        return patient?.ToViewModel();
    }

    public async Task<Result> AddPatientAsync(PatientViewModel newPatient)
    {
        if (await dbContext.Patients.AnyAsync(n => n.Oib == newPatient.Oib))
        {
            return Result.Failure("Patient is already registered in the system!");
        }

        var validationResult = await validator.ValidateAsync(newPatient);

        if (!validationResult.IsValid)
        {
            return Result.Failure("Patient details aren't valid!");
        }

        await dbContext.Patients.AddAsync(newPatient.ToModel());
        await dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdatePatientAsync(PatientViewModel updatedPatient)
    {
        var validationResult = await validator.ValidateAsync(updatedPatient);
        if (!validationResult.IsValid)
        {
            return Result.Failure("Patient details aren't valid!");
        }
        
        var model = await dbContext.Patients.FirstOrDefaultAsync(p => p.Oib == updatedPatient.Oib);
        if (model == null)
        {
            return Result.Failure("Patient not found!");
        }

        dbContext.Entry(model).CurrentValues.SetValues(updatedPatient.ToModel());

        await dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeletePatientAsync(string oib)
    {
        var patient = await dbContext.Patients.FindAsync(oib);
        if (patient == null)
        {
            return Result.Failure("Patient not found!");
        }

        dbContext.Patients.Remove(patient);

        await dbContext.SaveChangesAsync();
        return Result.Success();
    }


}
