using BlazorPatients.ViewModels;
using FluentValidation;

namespace BlazorPatients.Validators;

public class PatientValidator : AbstractValidator<PatientViewModel>
{
    public PatientValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");
        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");
        RuleFor(p => p.Oib)
            .NotEmpty().WithMessage("OIB is required.")
            .Length(11).WithMessage("OIB must be exactly 11 characters.")
            .Matches(@"^\d{11}$").WithMessage("OIB must contain only digits.");
        RuleFor(p => p.Birthday)
            .NotEmpty().WithMessage("Birthday is required.")
            .LessThan(DateTime.Now).WithMessage("Birthday must be in the past.");
    }
}
