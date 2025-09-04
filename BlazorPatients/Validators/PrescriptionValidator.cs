using BlazorPatients.ViewModels;
using FluentValidation;

namespace BlazorPatients.Validators;

public class PrescriptionValidator : AbstractValidator<PrescriptionViewModel>
{
    public PrescriptionValidator()
    {
        RuleFor(p => p.MedicationName)
            .NotEmpty().WithMessage("Medication name is required.")
            .MaximumLength(200).WithMessage("Medication name cannot exceed 200 characters.");
        RuleFor(p => p.DatePrescribed)
            .NotEmpty().WithMessage("Date prescribed is required.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Date prescribed cannot be in the future.");
        RuleFor(p => p.DateEnding)
            .GreaterThan(p => p.DatePrescribed).When(p => p.DateEnding.HasValue)
            .WithMessage("Date ending must be after date prescribed.");
    }
}
