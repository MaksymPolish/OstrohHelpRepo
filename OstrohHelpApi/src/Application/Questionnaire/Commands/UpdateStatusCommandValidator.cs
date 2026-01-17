using FluentValidation;

namespace Application.Questionnaire.Commands;

public class UpdateStatusCommandValidator : AbstractValidator<UpdateStatusCommand>
{
    public UpdateStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(x => x.StatusId).NotEmpty().WithMessage("StatusId is required");
    }
}
