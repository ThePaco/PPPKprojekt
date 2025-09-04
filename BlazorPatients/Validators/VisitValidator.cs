using BlazorPatients.ViewModels;
using FluentValidation;

namespace BlazorPatients.Validators;

public class VisitValidator : AbstractValidator<VisitViewModel>
{
    public VisitValidator()
    {
        RuleFor(v => v.Type)
            .IsInEnum().WithMessage("Visit type is required.");
        RuleFor(v => v.Date)
            .NotEmpty().WithMessage("Visit date is required.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Visit date cannot be in the future.");
        RuleFor(v => v.DoctorsNotes)
            .NotEmpty().WithMessage("Doctor's notes are required.")
            .MaximumLength(4096).WithMessage("Doctor's notes cannot exceed 1000 characters.");
    }
}
